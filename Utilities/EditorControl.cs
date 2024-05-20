using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FunkkModInstaller.Utilities
{

    public class EditorControl : UserControl
    {

        private readonly List<IEditableProperty> Items = new List<IEditableProperty>();
        private readonly Grid grid;

        public EditorControl()
        {
            grid = new Grid();
            this.Content = grid;
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1,GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1,GridUnitType.Star) });

        }

        public void RegisterProperty(EditableProperty property)
        {
            this.Items.Add(property);

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) });

            grid.Children.Add(property.Control);
            Grid.SetRow(property.Control, grid.RowDefinitions.Count-1);
            Grid.SetColumn(property.Control, 1);

            Label label = new Label { Content = property.Name };
            grid.Children.Add(label);
            Grid.SetRow(label, grid.RowDefinitions.Count-1);
        }

        public void SetItem(object item)
        {
            foreach (IEditableProperty property in this.Items) 
            { 
                property.Bind(item);
            }
        }

    }

    public interface IEditableProperty
    {
        public void Bind(Object obj);
        public FrameworkElement Control { get; }
    }

    public abstract class EditableProperty : IEditableProperty
    {

        public readonly String Path;
        public readonly String Name;

        public readonly FrameworkElement _Control;
        public FrameworkElement Control => _Control;

        protected EditableProperty(String name, String path)
        {
            this._Control = this.CreateControl();
            this.Path = path;
            this.Name = name;
        }

        protected Binding GetBinding(Object obj)
        {
            Binding binding = new Binding(this.Path);
            binding.Source = obj;
            return binding;
        }

        public abstract FrameworkElement CreateControl();
        public abstract void Bind(Object obj);
    
    }

    public class EditablePropertyString : EditableProperty
    {
        public EditablePropertyString(string name, string path) : base(name, path) { }

        public override void Bind(Object obj)
        {
            this.Control.SetBinding(TextBox.TextProperty,this.GetBinding(obj));
        }

        public override FrameworkElement CreateControl()
        {
           return new TextBox();
        }
    }

    public class EditablePropertyReadonlyString : EditableProperty
    {
        public EditablePropertyReadonlyString(string name, string path) : base(name, path) { }

        public override void Bind(Object obj)
        {
            var binding = GetBinding(obj);
            binding.Mode = BindingMode.OneWay;
            this.Control.SetBinding(TextBox.TextProperty, binding);
        }

        public override FrameworkElement CreateControl()
        {
            return new TextBox() { IsEnabled = false };
        }
    }

    public class EditablePropertyBool : EditableProperty
    {
        public EditablePropertyBool(string name, string path) : base(name, path) { }

        public override void Bind(Object obj)
        {
            this.Control.SetBinding(CheckBox.IsCheckedProperty, this.GetBinding(obj));
        }

        public override FrameworkElement CreateControl()
        {
            return new CheckBox();
        }
    }

    public class EditablePropertyBrowseString : EditableProperty
    {

        public EditablePropertyBrowseString(string name, string path,bool isFolderPicker = false) : base(name, path) 
        { this.IsFolderPicker = isFolderPicker; }

        protected TextBox InputBox;
        protected Button BrowseButton;

        public bool IsFolderPicker = false;

        private object? DialogObject;

        public override void Bind(Object obj)
        {
            this.InputBox.SetBinding(TextBox.TextProperty, this.GetBinding(obj));
        }

        public override FrameworkElement CreateControl()
        {

            BrowseButton = new Button();
            InputBox = new TextBox();

            var dock = new DockPanel();
            dock.LastChildFill = true;
            
            dock.Children.Add(BrowseButton);
            DockPanel.SetDock(BrowseButton, Dock.Right);
            BrowseButton.Content = "Browse";
            BrowseButton.Click += ButtonBrowse_Click;
            
            dock.Children.Add(InputBox);

            return dock;
        }

        private void ButtonBrowse_Click(object? sender, RoutedEventArgs e) 
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = IsFolderPicker;
            dialog.Multiselect = false;
            dialog.EnsureValidNames = true;
            var result = dialog.ShowDialog();

            if (result != CommonFileDialogResult.Ok) return;

            if (IsFolderPicker)
            {
                if (!Directory.Exists(dialog.FileName)) return;
            }
            else
            {
                if (!File.Exists(dialog.FileName)) return;
            }

            InputBox.Text = dialog.FileName;
            InputBox.CaptureMouse();
        }
    }


    public class EditablePropertyBrowseStringRoot : IEditableProperty
    {

        public readonly String DataPath;
        public readonly String Name;
        public readonly String RootPath;

        private TextBox RootTB;
        private TextBox DataTB;
        private Button BrowseButton;

        public readonly FrameworkElement _Control;
        public FrameworkElement Control => _Control;

        public EditablePropertyBrowseStringRoot(String name, String root_path, String data_path)
        {
            this._Control = this.CreateControl();
            this.DataPath = data_path;
            this.Name = name;
            this.RootPath = root_path;

            this.RootTB = new TextBox();
            this.DataTB = new TextBox();
            this.BrowseButton = new Button();
            _Control = this.CreateControl();
        }

        public void Bind(Object obj)
        {
            var rootBind = new Binding(this.RootPath);
            rootBind.Source = obj;
            this.RootTB.SetBinding(TextBox.TextProperty, rootBind);

            var dataBind = new Binding(this.DataPath);
            dataBind.Source = obj;
            this.DataTB.SetBinding(TextBox.TextProperty, dataBind);
        }

        public FrameworkElement CreateControl()
        {

            var dock = new DockPanel();
            dock.LastChildFill = true;

            RootTB.IsEnabled = false;
            DockPanel.SetDock(RootTB, Dock.Left);
            RootTB.Width = 100;
            dock.Children.Add(RootTB);

            DockPanel.SetDock(BrowseButton, Dock.Right);
            BrowseButton.Content = "Browse";
            dock.Children.Add(BrowseButton);

            dock.Children.Add(DataTB);

            return dock;

        }


    }

}
