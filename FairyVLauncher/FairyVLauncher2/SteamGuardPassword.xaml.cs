using System.Windows;
using System.Windows.Input;

namespace FairyVLauncher2
{
    /// <summary>
    /// Interaction logic for SteamGuardPassword.xaml
    /// </summary>
    public partial class SteamGuardPassword : Window
    {
        public string Message { get; set; }
        public string KindOfCode { get; set; }
        public SteamGuardPassword()
        {
            InitializeComponent();
            txtSteamGuardCode.Focus();
        }

        private void BtnSteamGuardOK_Click(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(txtSteamGuardCode.Text))
            {
                string steamGuardCode = txtSteamGuardCode.Text.ToUpper();
                if(KindOfCode == "authCode")
                {
                    SteamLogin.authCode = steamGuardCode;
                    Close();
                }
                else
                {
                    SteamLogin.twoFactorAuth = steamGuardCode;
                    Close();
                }
            }
            else
            {
                lblShowMessge.Content = "Veuillez écrire le Steam Guard";
                txtSteamGuardCode.Focus();
            }
        }

        private void TxtSteamGuardCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnSteamGuardOK_Click(sender, e);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtShowSteamGuardComment.Text = Message;
        }
    }
}
