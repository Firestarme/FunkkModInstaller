using System.Windows;

namespace Valheim_ModInstaller.Creator
{
    /// <summary>
    /// Interaction logic for AddPackDialog.xaml
    /// </summary>
    public partial class AddPackDialog : Window
    {
        public bool Result_ok = false;

        public string ModpackName => TB_Name.Text;
        public string ModpackPath =>TB_Path.Text;

        private bool auto_path = true;

        public AddPackDialog()
        {
            InitializeComponent();
            TB_Name.TextChanged += OnNameChanged;
            TB_Path.GotFocus += OnPathFocus;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            this.Result_ok = true;
            this.Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Result_ok = false;
            this.Close();
        }

        private void OnNameChanged(object sender, RoutedEventArgs e)
        {
            if (auto_path)
            {
                TB_Path.Text = TB_Name.Text;
            }
        }

        private void OnPathFocus(object sender, RoutedEventArgs e)
        {
            auto_path = false;
        }


    }
}
