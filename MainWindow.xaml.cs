using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FunkkModInstaller.Creator;
using FunkkModInstaller.Utilities;
using FunkkModInstaller.Properties;

namespace FunkkModInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Settings AppSettings = Settings.Default;
        private TabItem? EditorTab;


        public MainWindow()
        {
            InitializeComponent();
            this.CViewer.Console = App.Console;
            this.SettingsTab.Content = Settings.Control;
            this.Closed += OnWindowClose;

            AppSettings.PropertyChanged += OnSettingsChanged;
            SetEditorEnabled(AppSettings.EnableEditor);
        }

        private void OnSettingsChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(AppSettings.EnableEditor))
            {
                if (Tabs == null) return;
                SetEditorEnabled(AppSettings.EnableEditor);
            }
        }

        private void SetEditorEnabled(bool? enabled)
        {
            if (enabled == true)
            {
                if (EditorTab == null)
                {
                    EditorTab = new TabItem()
                    {
                        Header = "Pack Editor",
                        Content = new PackCreator()
                    };
                }

                Tabs.Items.Add(EditorTab);
            }
            else
            {
                if (EditorTab == null) return;
                Tabs.Items.Remove(EditorTab);
            }
        }

        public void OnWindowClose(object? sender, EventArgs args)
        {
            Application.Current.Shutdown();
        }

        private void EnableEditor_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            WinExplorer.Open(App.PackPath);
        }

        private void ButtonWindowClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonWindowMinimise_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void DragBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
                this.DragMove();
        }


        //private MessageBoxResult ShowUninstallPreviousPrompt()
        //{
        //    // Initializes the variables to pass to the MessageBox.Show method.
        //    string message = "A Previous installaton must be removed before the installer can proceed";
        //    string caption = "Remove Previous Installation?";
        //    MessageBoxButton buttons = MessageBoxButton.YesNo;
        //    MessageBoxResult result;

        //    // Displays the MessageBox.
        //    return MessageBox.Show(message, caption, buttons);
        //}

        //private void DisableInstaller()
        //{
        //    App.Console.PrintLn("#### Installer Disabled ####");
        //    this.InstallTab.IsEnabled = false;
        //    Tabs.SelectedItem = ConsoleTab;
        //}

    }
}
