using FunkkModInstaller.Utilities;
using System.ComponentModel;

namespace FunkkModInstaller.Properties
{


    // This class allows you to handle specific events on the settings class:
    //  The SettingChanging event is raised before a setting's value is changed.
    //  The PropertyChanged event is raised after a setting's value is changed.
    //  The SettingsLoaded event is raised after the setting values are loaded.
    //  The SettingsSaving event is raised before the setting values are saved.
    internal sealed partial class Settings : INotifyPropertyChanged {


        private static readonly EditorControl _Control = new EditorControl();
        public static EditorControl Control => _Control;
        static Settings()
        {
            //TODO Use reflection here?
            _Control.RegisterProperty(new EditablePropertyBrowseString("Game Install Directory", nameof(GameInstallPath),isFolderPicker: true));
            _Control.RegisterProperty(new EditablePropertyBrowseString("Steam .exe Path", nameof(SteamExecPath), isFolderPicker: false));
            _Control.RegisterProperty(new EditablePropertyBool("Lauch Game Through Steam", nameof(LaunchWithSteam)));
            _Control.RegisterProperty(new EditablePropertyBool("Enable Pack Creator", nameof(EnableEditor)));

            _Control.SetItem(defaultInstance);
        }

        public Settings() {
            // To add event handlers for saving and changing settings, uncomment the lines below:
            //this.SettingChanging += this.SettingChangingEventHandler;
            //this.SettingsSaving += this.SettingsSavingEventHandler;

            this.PropertyChanged += PropertyChangedEventHandr;
        }

        private void PropertyChangedEventHandr(object? sender, PropertyChangedEventArgs e)
        {
            this.Save();
        }

        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
            // Add code to handle the SettingChangingEvent event here.
        }
        
        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
            // Add code to handle the SettingsSaving event here.
        }
    }
}
