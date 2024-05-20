﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FunkkModInstaller.JSON;
using FunkkModInstaller;
using System.Collections.ObjectModel;
using FunkkModInstaller.Creator.TreeItems;
using System.Runtime.CompilerServices;
using System.Diagnostics.Contracts;
using System.ComponentModel;

namespace FunkkModInstaller
{
    public class PackInfo : INotifyPropertyChanged
    {

        public String Name
        {
            get => JSONObj.ModPackName != null ? JSONObj.ModPackName : "Not Set";
        }

        public string? _Path;
        public bool IsInstalled = false;
        public string? Path => _Path;
        public bool HasSource => _Path != null;

        private bool _IsPackActive = false;
        public bool IsPackActive
        {
            get => _IsPackActive;
            set
            {
                _IsPackActive = value;
                if (this.Bepinex != null) { this.Bepinex.IsPackActive = value; }
                foreach (ModInfo mod in Mods) { mod.IsPackActive = value; }
            }
        }

        private bool _IsUpdateRequired;
        public bool IsUpdateRequired
        {
            get { return _IsUpdateRequired; }
            set { _IsUpdateRequired = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUpdateRequired))); }
        }

        private readonly JSONModPack _JSONObj;
        public JSONModPack JSONObj => _JSONObj;

        public readonly Hash16 PackHash;

        private readonly ObservableCollection<ModInfo> _Mods = new ObservableCollection<ModInfo>();
        public ObservableCollection<ModInfo> Mods => _Mods;

        private ModInfoBepinex? _Bepinex;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ModInfoBepinex? Bepinex => _Bepinex;


        private PackInfo(JSONReader.JSONDoc JSONObj)
        {
            _JSONObj = JSONObj.JSONModPack;
            PackHash = JSONObj.Hash;

            //Add Bepinex
            if (_JSONObj.Bepinex != null)
            {
                _Bepinex = new ModInfoBepinex(_JSONObj.Bepinex);
                _Bepinex.PropertyChanged += Mod_PropertyChanged;
            }

            //populate Mods List
            if (_JSONObj.Mods != null)
            {
                foreach (var m in _JSONObj.Mods) 
                { 
                    var mod  = new ModInfo(m);
                    mod.PropertyChanged += Mod_PropertyChanged;
                    Mods.Add(mod);
                }
            }

            SetDefaultInstallState();
        }

        private void Mod_PropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(ModInfo.IsUpdateRequired)) return;
            foreach (ModInfo mod in this.Mods)
            {
                if (mod.IsUpdateRequired) { IsUpdateRequired = true; return; }
                IsUpdateRequired = false;
            }
        }

        public PackInfo(JSONReader.JSONDoc JSONObj, string path) : this(JSONObj)
        {
            this._Path = path;
        }

        public static PackInfo CreateWithNoSource(JSONReader.JSONDoc JSONObj)
        {
            return new PackInfo(JSONObj);
        }

        public void SetError(bool isError, string? Message)
        {
            foreach (ModInfo mod in this.Mods)
            {
                mod.IsErrored = isError;
                mod.Message = Message;
            }
        }

        public void SetDefaultInstallState()
        {
            foreach (ModInfo mod in this.Mods)
            {
                mod.IsInstallDesired = mod.IsInstalledByDefault;
            }
        }

        public Dictionary<string, ModInfo> GetModDict()
        {
            return Mods.ToDictionary((ModInfo m) => { return m.ModID; });
        }

    }
}
