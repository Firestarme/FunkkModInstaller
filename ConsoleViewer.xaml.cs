using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FunkkModInstaller
{
    /// <summary>
    /// Interaction logic for Console.xaml
    /// </summary>
    public partial class ConsoleViewer : UserControl
    {

        private Console? _console;
        public Console? Console
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

        public ConsoleViewer()
        {
            InitializeComponent();
        }

        public void OnConsoleWrite(object sender, string data)
        {
            this.TB.Text += data;   
        }

    }

    public class Console : INotifyPropertyChanged
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
