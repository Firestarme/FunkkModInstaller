using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace FunkkModInstaller.Utilities
{
    /// <summary>
    /// Interaction logic for Console.xaml
    /// </summary>
    public partial class SimpleConsoleViewer : UserControl
    {

        private SimpleConsole? _console;
        public SimpleConsole? Console
        {
            get { return _console; }
            set
            {
                if(_console != value && _console != null)
                {
                    BindingOperations.ClearBinding(this.TB, TextBox.TextProperty);
                }

                if(value != null)
                {
                    this.TB.SetBinding(TextBox.TextProperty, value.GetBinding());
                }

                _console = value;
            }
        }

        public SimpleConsoleViewer()
        {
            InitializeComponent();
        }

        public void OnConsoleWrite(object sender, string data)
        {
            this.TB.Text += data;   
        }

    }

    public class SimpleConsole : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public int Indent = 4;

        private String _data;
        public String Data => _data;
        
        public Binding GetBinding()
        {
            return new Binding(nameof(Data)) { Source = this, Mode = BindingMode.OneWay };
        }

        public void Print(string str)
        {
            _data += str;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Data)));
        }

        public void Print(int lv, string str)
        {
            Print(new string(' ', lv * 2) + str);
        }

        public void PrintLn(string str)
        {
            Print("\n" + str);
        }

        public void PrintLn(int lv, string str)
        {
            PrintLn(new string(' ',lv*Indent) + str);
        }

        public void CR()
        {
            Print("\n");
        }

    }
}
