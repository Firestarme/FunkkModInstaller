using FunkkModInstaller.Installer;
using FunkkModInstaller.JSON;
using FunkkModInstaller.Properties;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FunkkModInstaller
{
    /// <summary>
    /// Interaction logic for InstallerView.xaml
    /// </summary>
    public partial class InstallerView : UserControl, INotifyPropertyChanged
    {

        private readonly ObservableCollection<PackInfo> _Packs = new ObservableCollection<PackInfo>();
        public ObservableCollection<PackInfo> Packs => _Packs;

        private readonly PackInstaller _installer;
        private PackInfo? _SelectedPack;

        public event PropertyChangedEventHandler? PropertyChanged;

        private InstallerButtonState _ButtonState;
        public InstallerButtonState ButtonState => _ButtonState;

        public PackInfo? SelectedPack
        {
            get => _SelectedPack;
            set
            {
                _SelectedPack = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedPack)));
            }
        }

        public InstallerView()
        {
            this.DataContext = this;
            InitializeComponent();
            PopulateModspacks();

            //Setup property changed handler
            Settings.Default.PropertyChanged += Settings_PropertyChanged;

            //Create Installer
            _installer = new PackInstaller();
            InitializeInstaller();

            //Setup DragTarget
            PackDropTgt.Desired_Extension = App.ARCHIVE_EXTENSION;

            SetButtonState();
            this.Loaded += OnLoaded;
            RetargetBepinexInfo();
        }

        public void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            if(args.PropertyName == nameof(Settings.GameInstallPath))
            {
                _installer.SetGameDir(App.TgtGamePath);
            }
        }

        public void OnLoaded(object? sender, RoutedEventArgs args)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedPack)));
        }

        private void PopulateModspacks()
        {

            //Standard directory implementation ...
            var json_io = new JSONReader();
            var pack_directory = App.PackPath;

            if (!Directory.Exists(pack_directory)) 
            {
                App.Console.PrintLn("Can't find modpack directory, attempting to create one ... ");

                try
                {
                    Directory.CreateDirectory(pack_directory);
                    App.Console.Print("OK");
                }
                catch (Exception e)
                {
                    App.Console.Print("FAIL: Unspecified");
                    App.Console.PrintLn(1,e.Message);
                    return;
                }
            }

            App.Console.PrintLn("Discovering Modpacks ... ");

            foreach (string file in Directory.GetFiles(pack_directory))
            {
                if (Path.GetExtension(file) == App.ARCHIVE_EXTENSION)
                {
                    //Load json
                    var pack_json = json_io.TryReadPackZip(file);
                    if (pack_json == null) { continue; }

                    App.Console.PrintLn(1, $"-Found pack: {pack_json.JSONModPack.ModPackName} at: {file}");
                    //Add to collection
                    var pack = new PackInfo(pack_json, file);
                    pack.PropertyChanged += Pack_PropertyChanged;
                    Packs.Add(pack);
                }
            }
        }

        private void InitializeInstaller()
        {

            _installer.SetGameDir(App.TgtGamePath);
            _installer.SetStagingDirectory(App.InstallerStagingDir);

            if (_installer.ActivePack != null)
            {
                //_installer.VerifyPack();

                bool persistent_exists = false;
                foreach (PackInfo p in Packs)
                {
                    if (p.PackHash == _installer.ActivePack.PackHash)
                    {
                        persistent_exists = true;
                        _installer.SwapPackObject(p);
                        break;
                    }
                }

                _installer.VerifyPack();
                _installer.SetDesiredToInstalled();

                if (!persistent_exists)
                {
                    App.Console.PrintLn("Warning! The installed modpack has no source files");
                    Packs.Add(_installer.ActivePack);
                }

                this.SelectedPack = _installer.ActivePack;
            }
        }

        private void Installpack()
        {
            if (!ClearStaging()) { return; }
            if (SelectedPack == null) { return; }
            if (!CreateStaging()) { return; }

            _installer.InstallPack(SelectedPack);

        }

        private void RetargetBepinexInfo()
        {
            object? context = null;
            if (SelectedPack != null) context = SelectedPack.Bepinex;
            Comp_BepinexInfo.DataContext = context;

        }

        private bool ClearStaging()
        {
            if (Directory.Exists(App.InstallerStagingDir))
            {
                App.Console.PrintLn("Removing Staging ... ");
                try
                {
                    Directory.Delete(App.InstallerStagingDir, true);
                    App.Console.Print("OK");
                    return true;
                }
                catch (Exception ex)
                {
                    App.Console.Print("FAIL");
                    App.Console.PrintLn(1, ex.ToString());
                    return false;
                }
            }

            return true;
        }

        private bool CreateStaging()
        {
            if (Directory.Exists(App.InstallerStagingDir)) { return false; }

            App.Console.PrintLn("Creating Staging ... ");
            if (SelectedPack == null) { App.Console.Print("FAIL: No Pack Selected"); return false; }
            
            if (SelectedPack.Path == null) { 
                App.Console.Print("FAIL: Selected Pack has No Path");
                SelectedPack.SetError(true, "Pack has no Source File!");
                return false; 
            }

            if (!File.Exists(SelectedPack.Path)) { 
                App.Console.Print($"FAIL: No pack at path {SelectedPack.Path}");
                SelectedPack.SetError(true, "Pack Source does not Exist!");
                return false; 
            }

            try
            {
                ZipFile.ExtractToDirectory(SelectedPack.Path, App.InstallerStagingDir);
                App.Console.Print("OK");
                return true;
            }
            catch (Exception ex)
            {
                App.Console.Print("FAIL: Exception");
                App.Console.PrintLn(1, ex.Message);
                SelectedPack.SetError(true, "Exception!");
                return true;
            }
        }

        private void DetermineButtonState()
        {
            if (SelectedPack == null) { _ButtonState = InstallerButtonState.NoneSelected; return; }
            if (_installer.ActivePack == null) { _ButtonState = InstallerButtonState.Install; return; }
            if (_installer.ActivePack != _SelectedPack) { _ButtonState = InstallerButtonState.SwitchPack; return; }
            if (_installer.IsUpdateRequired) { _ButtonState = InstallerButtonState.UpdatePack; return; }
            _ButtonState = InstallerButtonState.Launch;
        }

        private void SetButtonState()
        {
            DetermineButtonState();
            Button_Update.IsEnabled = _ButtonState > InstallerButtonState.NoneSelected;

            switch (_ButtonState)
            {
                case InstallerButtonState.Install:
                    Button_Update.Content = "Install";
                    return;
                case InstallerButtonState.UpdatePack:
                    Button_Update.Content = "Update";
                    return;
                case InstallerButtonState.SwitchPack:
                    Button_Update.Content = "Change Pack";
                    return;
                case InstallerButtonState.Launch:
                    Button_Update.Content = "Launch";
                    return;
                case InstallerButtonState.NoneSelected:
                    Button_Update.Content = "No Pack Selected";
                    return;
            }

            Button_Update.Content = "ERROR";
        }

        private void CombiBox_SelectPack_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetButtonState();
            RetargetBepinexInfo();
        }

        private void Pack_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            SetButtonState();
        }

        private void Button_Update_Click(object sender, RoutedEventArgs e)
        {
            switch (_ButtonState)
            {
                case InstallerButtonState.Install:
                    Installpack();
                    break;

                case InstallerButtonState.UpdatePack:
                    Installpack();
                    break;

                case InstallerButtonState.SwitchPack:
                    Installpack();
                    break;

                case InstallerButtonState.Launch:
                    App.LaunchGame();
                    break;
            }

            SetButtonState();
        }


        // #### Event Handlers ####

        private void PackDropTgt_CopyRequested(object sender, RoutedEventArgs e)
        {
            var pack_path = PackDropTgt.Requested_File;

            App.Console.CR();
            App.Console.PrintLn($"Reading pack from dropped zip ...");

            if (pack_path == null) { App.Console.Print($"FAIL: No Src"); return; }

            AddModpackText.Content = "ERROR";
            ConfimAddPackUI.Visibility = Visibility.Visible;
            AddModpackButton.IsEnabled = false;

            //Try and read pack info
            PackDropTgt.TryRead();
            if (!PackDropTgt.TryRead())
            {
                App.Console.Print($"FAIL: Can't read pack");
                AddModpackText.Content = "Can't Read Pack!";
                PackDropTgt.Reset();
                return;
            }

            //Check if the pack exists already
            foreach(var existing_pack in this.Packs)
            {
                if (existing_pack.PackHash == PackDropTgt.Requested_Pack.PackHash)
                {
                    if (!existing_pack.HasSource) continue;
                    App.Console.Print($"OK: Pack already exists");
                    AddModpackText.Content = "Pack Already Exists!";
                    PackDropTgt.Reset();
                    return;
                }
            }

            AddModpackButton.IsEnabled = true;
            AddModpackText.Content = PackDropTgt.Requested_Pack.Name;
            App.Console.Print($"OK");
        }

        private void ButtonAddModpack_Click(object sender, RoutedEventArgs e)
        {
            AddModpackFromDrop();
            PackDropTgt.Reset();
            ConfimAddPackUI.Visibility = Visibility.Hidden;
        }

        private void AddModpackFromDrop()
        {
            if (PackDropTgt.Requested_File == null) return;
            if (PackDropTgt.Requested_Pack == null) return;

            App.Console.PrintLn($"Adding dropped pack ... ");

            if (!Directory.Exists(App.PackPath))
            {
                App.Console.Print($"FAIL: no pack directory");
                return;
            }

            if (!File.Exists(PackDropTgt.Requested_File))
            {
                App.Console.Print($"FAIL: src does not exist!");
                return;
            }

            var dest = Path.Combine(App.PackPath, PackDropTgt.Requested_Pack.Name + App.ARCHIVE_EXTENSION);

            if (File.Exists(dest))
            {
                App.Console.Print($"FAIL: Cannot overwrite file {dest}");
                return;
            }

            try
            {
                File.Copy(PackDropTgt.Requested_File, dest);
                this.Packs.Add(PackDropTgt.Requested_Pack);
                App.Console.Print($"OK");
            }
            catch (Exception ex)
            {
                App.Console.Print($"FAIL: Unspecified");
                App.Console.PrintLn(1, ex.Message);
                return;
            }

            //If required, swap in to active pack
            if(PackDropTgt.Requested_Pack.PackHash == _installer.ActivePack.PackHash) 
            {
                _installer.SwapPackObject(PackDropTgt.Requested_Pack);
            }

            this._SelectedPack = PackDropTgt.Requested_Pack;
        }

        private void PackDropTgt_MouseLeave(object sender, MouseEventArgs e)
        {
            if (PackDropTgt.Requested_File != null)
            {
                PackDropTgt.Reset();
                ConfimAddPackUI.Visibility = Visibility.Hidden;
            }
        }
    }


    public enum InstallerButtonState
    {
        Error = 0,
        NoneSelected = 1,
        Install = 2,
        UpdatePack = 3,
        SwitchPack = 4,
        Launch = 5,
    }

}
