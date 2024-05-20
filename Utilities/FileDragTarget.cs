using FunkkModInstaller.Installer;
using FunkkModInstaller.JSON;
using System;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FunkkModInstaller.Utilities
{
    public abstract class FileDragTarget : Panel
    {

        public static readonly DependencyProperty OverlayProperty = DependencyProperty.Register(nameof(Overlay), typeof(object), typeof(FileDragTarget));
        public object Overlay
        {
            get => GetValue(OverlayProperty);
            set => SetValue(OverlayProperty, value);
        }

        private UIElement? OverlayElement => Overlay as UIElement;

        protected override int VisualChildrenCount
        {
            get
            {
                int count = this.Children.Count;
                if (OverlayElement != null) { count++; }
                return count;
            }
        }

        public FileDragTarget() : base()
        {
            //SetContentVisibility(Visibility.Hidden);
            this.IsHitTestVisible = true;
            this.AllowDrop = true;

        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {

            foreach (UIElement element in this.Children)
            {
                element.Arrange(new Rect(arrangeBounds));
            }

            if (this.OverlayElement != null)
            {
                this.OverlayElement.Arrange(new Rect(arrangeBounds));
            }

            return arrangeBounds;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Size max = new Size(0, 0);

            foreach (UIElement element in this.Children)
            {
                element.Measure(constraint);
                if (element.DesiredSize.Height > max.Height) max.Height = element.DesiredSize.Height;
                if (element.DesiredSize.Width > max.Width) max.Width = element.DesiredSize.Width;
            }

            return max;
        }

        // Provide a required override for the GetVisualChild method.
        protected override Visual GetVisualChild(int index)
        {
            if (OverlayElement == null)
            {
                return this.Children[index];
            }
            else
            {
                if (index == this.Children.Count) return OverlayElement;
                return this.Children[index];
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == FileDragTarget.OverlayProperty)
            {
                var OldElement = e.OldValue as UIElement;
                if (OldElement != null)
                {

                    this.AddVisualChild(OldElement);
                    this.AddLogicalChild(OldElement);
                }

                var NewElement = e.NewValue as UIElement;
                if (NewElement != null)
                {

                    this.AddVisualChild(NewElement);
                    this.AddLogicalChild(NewElement);
                }

                SetOverlayVisibility(Visibility.Hidden);
                this.InvalidateVisual();
            }

            base.OnPropertyChanged(e);
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }

        protected void SetOverlayVisibility(Visibility vis)
        {
            var overlay = this.OverlayElement;
            if (overlay == null) return;
            overlay.Visibility = vis;
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                if (CheckFiles((string[])e.Data.GetData(DataFormats.FileDrop)))
                {
                    SetOverlayVisibility(Visibility.Visible);
                }
                else
                {
                    SetOverlayVisibility(Visibility.Hidden);
                }
            }
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            SetOverlayVisibility(Visibility.Hidden);
        }

        protected override void OnDrop(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                HandleFileDrop(files);
            }

            SetOverlayVisibility(Visibility.Hidden);
        }

        protected abstract void HandleFileDrop(string[] files);
        protected abstract bool CheckFiles(string[] files);
    }


    public class CopyFileTarget : FileDragTarget
    {
        public String? Dest_Path;
        public String? Desired_Extension;

        public bool AllowZipDrop = true;

        public CopyFileTarget() : base() { }
        public event RoutedEventHandler? FilesCopied;

        protected override bool CheckFiles(string[] files)
        {
            if (Desired_Extension == null)
            {
                return files.Length > 0;
            }
            else
            {
                if (files.Length == 0) return false;

                foreach (string file in files)
                {
                    if (Path.GetExtension(file) != Desired_Extension) { return false; }
                }

                return true;
            }
        }

        protected override void HandleFileDrop(string[] files)
        {
            if (Dest_Path == null) return;
            if (!Directory.Exists(Dest_Path)) return;

            int files_copied = 0;

            if (files.Length == 1 && Path.GetExtension(files[0]) == App.ARCHIVE_EXTENSION && AllowZipDrop)
            {
                if (ZipDrop(files[0])) FilesCopied?.Invoke(this, new RoutedEventArgs());
                return;
            }


            foreach (string file in files)
            {
                var name = Path.GetFileName(file);
                var dest = Path.Combine(Dest_Path, name);

                if (Desired_Extension != null)
                {
                    if (Path.GetExtension(file) != Desired_Extension) continue;
                }
                if (File.Exists(dest)) continue;

                try
                {
                    File.Copy(file, dest);
                    files_copied++;
                }
                catch { }
            }

            if (files_copied > 0) { FilesCopied?.Invoke(this, new RoutedEventArgs()); }
        }


        private bool ZipDrop(string file)
        {
            if (Dest_Path == null) return false;

            try
            {
                var dest = Path.Combine(Dest_Path, Path.GetFileNameWithoutExtension(file));
                ZipFile.ExtractToDirectory(file, Dest_Path);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }


    public class CopyModpackTarget : FileDragTarget
    {
        public String? Desired_Extension;

        public String? Requested_File;
        public PackInfo? Requested_Pack;

        public CopyModpackTarget() : base() { }
        public event RoutedEventHandler? CopyRequested;

        protected override bool CheckFiles(string[] files)
        {
            if (files.Length > 1) { return false; }
            if (Desired_Extension == null) { return true; }
            return Path.GetExtension(files[0]) == Desired_Extension;
        }

        protected override void OnDrop(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var sel_file = files[0];

                if (sel_file != null)
                {
                    if (Path.GetExtension(sel_file) == Desired_Extension)
                    {
                        Requested_File = sel_file;
                        CopyRequested?.Invoke(this, new RoutedEventArgs());
                        return;
                    }
                }
            }

            SetOverlayVisibility(Visibility.Hidden);
        }

        protected override void HandleFileDrop(string[] files) { }

        public void Reset()
        {
            Requested_File = null;
            Requested_Pack = null;
            SetOverlayVisibility(Visibility.Hidden);
        }

        public bool TryRead()
        {
            if (Requested_File == null) return false;

            var json_io = new JSONReader();
            var pack = json_io.TryReadPackZip(Requested_File);
            if (pack == null) return false;

            this.Requested_Pack = new PackInfo(pack, Requested_File);
            return true;
        }

    }
}
