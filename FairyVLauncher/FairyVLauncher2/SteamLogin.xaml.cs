using System;
using System.IO;
using System.Windows;
using SteamKit2;
using System.Security.Cryptography;
using System.Windows.Input;
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace FairyVLauncher2
{
    /// <summary>
    /// Interaction logic for SteamLogin.xaml
    /// </summary>
    public partial class SteamLogin : Window
    {
        static string steamUsername, steamPassword;
        static SteamClient steamClient;
        static CallbackManager manager;
        static SteamUser steamUser;
        static bool isRunning = false;
        static public string authCode, twoFactorAuth;
        public static decimal steamID64;
        static string lblText;

        public SteamLogin()
        {
            Console.WriteLine(Properties.Settings.Default.SteamLogin);
            Console.WriteLine(Properties.Settings.Default.SteamId64);
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.SteamLogin) && Properties.Settings.Default.SteamId64 != 0)
            {
                MainWindow mainWindow = new MainWindow
                {
                    Steam64 = Properties.Settings.Default.SteamId64,
                    steamUsername = Properties.Settings.Default.SteamLogin
                };
                mainWindow.Show();
            }
            else
            {
                InitializeComponent();
                txtSteamUsername.Focus();
                InitData();
            }
        }

        private void BtnCloseApp_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnSteamLogin_Click(object sender, RoutedEventArgs e)
        {
            steamUsername = txtSteamUsername.Text;
            steamPassword = pwdSteamPassword.Password;
            if(string.IsNullOrWhiteSpace(steamUsername))
            {
                txtShowMessage.Text = "Veuillez remplir le user";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(steamPassword))
                {
                    txtShowMessage.Text = "Veuillez remplir le mot de passe";
                }
            }
            if(!string.IsNullOrWhiteSpace(steamUsername) && !string.IsNullOrWhiteSpace(steamPassword))
            {
                SteamLoginFunction();
                txtShowMessage.Text = lblText;
                SaveData();
            }
        }
        static private void SteamLoginFunction()
        {
            // create our steamclient instance
            steamClient = new SteamClient();
            // create the callback manager which will route callbacks to function calls
            manager = new CallbackManager(steamClient);

            // get the steamuser handler, which is used for logging on after successfully connecting
            steamUser = steamClient.GetHandler<SteamUser>();

            // register a few callbacks we're interested in
            // these are registered upon creation to a callback manager, which will then route the callbacks
            // to the functions specified
            manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

            manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);
            // this callback is triggered when the steam servers wish for the client to store the sentry file
            manager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth);
            isRunning = true;
            //Console.WriteLine("Connecting to Steam...");
            // initiate the connection
            steamClient.Connect();
            while (isRunning)
            {
                // in order for the callbacks to get routed, they need to be handled by the manager
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }

        }
        static void OnConnected(SteamClient.ConnectedCallback callback)
        {
            //Console.WriteLine("Connected to Steam! Logging in '{0}'...", steamUsername);
            byte[] sentryHash = null;
            if (File.Exists("sentry.bin"))
            {
                // if we have a saved sentry file, read and sha-1 hash it
                byte[] sentryFile = File.ReadAllBytes("sentry.bin");
                sentryHash = CryptoHelper.SHAHash(sentryFile);
            }

            steamUser.LogOn(new SteamUser.LogOnDetails
            {
                Username = steamUsername,
                Password = steamPassword,
                // in this sample, we pass in an additional authcode
                // this value will be null (which is the default) for our first logon attempt
                AuthCode = authCode,

                // if the account is using 2-factor auth, we'll provide the two factor code instead
                // this will also be null on our first logon attempt
                TwoFactorCode = twoFactorAuth,

                // our subsequent logons use the hash of the sentry file as proof of ownership of the file
                // this will also be null for our first (no authcode) and second (authcode only) logon attempts
                SentryFileHash = sentryHash,
            });
        }

        static void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            //Console.WriteLine("Disconnected from Steam");

            isRunning = false;
        }

        static void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            bool isSteamGuard = callback.Result == EResult.AccountLogonDenied;
            bool is2FA = callback.Result == EResult.AccountLoginDeniedNeedTwoFactor;

            if (isSteamGuard || is2FA)
            {
                //Console.WriteLine("This account is SteamGuard protected!");

                if (is2FA)
                {
                    //Console.WriteLine("Please enter your 2 factor auth code from your authenticator app: ");
                    string message = "Please enter the auth code sent to the email at: " + callback.EmailDomain;
                    SteamGuardPassword steamGuardScreen = new SteamGuardPassword
                    {
                        Message = message,
                        KindOfCode = "twoFactorAuth"
                    };
                    steamGuardScreen.ShowDialog();
                    //Console.WriteLine("twoFactorAuth :");
                    //Console.WriteLine(twoFactorAuth);
                    SteamLoginFunction();
                }
                else
                {
                    //Console.WriteLine("Please enter the auth code sent to the email at {0}: ", callback.EmailDomain);
                    string message = "Please enter the auth code sent to the email at: " + callback.EmailDomain;
                    SteamGuardPassword steamGuardScreen = new SteamGuardPassword
                    {
                        Message = message,
                        KindOfCode = "authCode"
                    };

                    steamGuardScreen.ShowDialog();
                    //Console.WriteLine("authCode :");
                    //Console.WriteLine(authCode);
                    SteamLoginFunction();
                }

                return;
            }

            if (callback.Result != EResult.OK)
            {
                //Console.WriteLine("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult);
                lblText = "Unable to logon to Steam: \n"+callback.Result+" / "+ callback.ExtendedResult;

                isRunning = false;
                return;
            }

            //Console.WriteLine("Successfully logged on!");
            lblText = "Connection réussite!";

            // at this point, we'd be able to perform actions on Steam
            //Console.WriteLine("steam ID:");
            steamID64 = Convert.ToDecimal(steamUser.SteamID);
            MainWindow mainWindow = new MainWindow
            {
                Steam64 = steamID64,
                steamUsername = steamUsername
            };
            mainWindow.Show();
            
            // for this sample we'll just log off
            steamUser.LogOff();
            
        }

        private void PwdSteamPassword_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnSteamLogin_Click(sender,e);
            }
        }

        private void TxtSteamUsername_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnSteamLogin_Click(sender, e);
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        static void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            //Console.WriteLine("Logged off of Steam: {0}", callback.Result);
        }
        static void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
        {
            //Console.WriteLine("Updating sentryfile...");

            // write out our sentry file
            // ideally we'd want to write to the filename specified in the callback
            // but then this sample would require more code to find the correct sentry file to read during logon
            // for the sake of simplicity, we'll just use "sentry.bin"

            int fileSize;
            byte[] sentryHash;
            using (var fs = File.Open("sentry.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fs.Seek(callback.Offset, SeekOrigin.Begin);
                fs.Write(callback.Data, 0, callback.BytesToWrite);
                fileSize = (int)fs.Length;

                fs.Seek(0, SeekOrigin.Begin);
                using (var sha = SHA1.Create())
                {
                    sentryHash = sha.ComputeHash(fs);
                }
            }

            // inform the steam servers that we're accepting this sentry file
            steamUser.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
            {
                JobID = callback.JobID,

                FileName = callback.FileName,

                BytesWritten = callback.BytesToWrite,
                FileSize = fileSize,
                Offset = callback.Offset,

                Result = EResult.OK,
                LastError = 0,

                OneTimePassword = callback.OneTimePassword,

                SentryFileHash = sentryHash,
            });

            //Console.WriteLine("Done!");
            
        }

        private void InitData()
        {
            if(Properties.Settings.Default.Login != string.Empty)
            {
                if(Properties.Settings.Default.Remember == "Yes")
                {
                    txtSteamUsername.Text = Properties.Settings.Default.Login;
                    pwdSteamPassword.Password = Properties.Settings.Default.Password;
                    ckbxRememberMe.IsChecked = true;
                }
                else
                {
                    txtSteamUsername.Text = Properties.Settings.Default.Login;

                }
            }
        }

        private void SaveData()
        {
            if (ckbxRememberMe.IsChecked == true)
            {
                Properties.Settings.Default.Login = txtSteamUsername.Text;
                Properties.Settings.Default.Password = pwdSteamPassword.Password;
                Properties.Settings.Default.Remember = "Yes";
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.Login = txtSteamUsername.Text;
                Properties.Settings.Default.Password = "";
                Properties.Settings.Default.Remember = "No";
                Properties.Settings.Default.Save();
            }
        }
    }
}
