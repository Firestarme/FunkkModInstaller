using FunkkMI_Common.JSON;
using System;
using System.ComponentModel;

namespace FunkkModInstaller.Installer
{
    public class ModInfo : INotifyPropertyChanged
    {

        private ModStatus _ModsStatus;
        public ModStatus ModStatus => _ModsStatus;

        //Status icon properties
        private bool _isInstalled;
        private bool _isErrored;
        private bool _isInstallDesired;
        private bool _IsPackActive;

        public bool IsInstalled
        {
            get => _isInstalled;
            set
            {
                _isInstalled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsInstalled)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUpdateRequired)));
            }
        }
        public bool IsErrored
        {
            get => _isErrored;
            set
            {
                _isErrored = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsErrored)));
            }
        }
        public bool IsInstallDesired
        {
            get
            {
                if (IsRequired) return true;
                return _isInstallDesired;
            }
            set
            {
                _isInstallDesired = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsInstallDesired)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUpdateRequired)));
            }
        }
        public bool IsPackActive
        {
            get => _IsPackActive;
            set
            {
                _IsPackActive = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPackActive)));
            }
        }

        //Other properties
        private string _Message;

        public string? Message
        {
            get => _Message;
            set
            {
                _Message = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
            }
        }

        public bool IsInstalledByDefault
        {
            get { return JSONObj.InstallByDefault.HasValue ? JSONObj.InstallByDefault.Value : false; }
        }

        public bool IsRequired
        {
            get { return JSONObj.IsRequired.HasValue ? JSONObj.IsRequired.Value : false; }
        }

        public bool IsOptional => !IsRequired;

        public string ModID
        {
            get { return JSONObj.GUID != null ? JSONObj.GUID : "??"; }
        }

        public bool IsUpdateRequired { get => IsInstalled != IsInstallDesired; }

        private JSONMod _JSONObj;

        public event PropertyChangedEventHandler? PropertyChanged;

        public JSONMod JSONObj => _JSONObj;

        public string ModName => _JSONObj.Name != null ? _JSONObj.Name : "Not Set";
        public string Description => _JSONObj.Description != null ? _JSONObj.Description : "Not Set";
        public string Version => _JSONObj.Version != null ? _JSONObj.Version : "Not Set";

        public ModInfo(JSONMod JSONObj)
        {
            _JSONObj = JSONObj;
        }
    }

    public class ModInfoBepinex : ModInfo
    {

        //public override bool IsInstallDesired
        //{ 
        //    get => true; 
        //    set => base.IsInstallDesired = true; 
        //}

        //public override bool IsRequired 
        //{
        //    get => true; 
        //    set => base.IsRequired = true; 
        //}

        public ModInfoBepinex(JSONMod JSONObj) : base(JSONObj)
        {
            IsInstallDesired = true;
            this.JSONObj.IsRequired = true;
        }
    }



    [Flags]
    public enum ModStatus
    {
        Installed = 0b_0000_0001,
        ChangeRequested = 0b_0000_0010,
        InstallRequested = 0b_0000_0010,
        UninstallRequested = 0b_0000_0011,
        PackActive = 0b_0000_0100,
    }
}
