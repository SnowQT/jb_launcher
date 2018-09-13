using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Threading;
using System.Net;
using System.Windows.Forms;
using System.Text;
using System.Windows.Input;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;

namespace FairyVLauncher2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string GTADirectory;
        WebClient client;
        public string steamIdHex, steamUsername;
        public decimal Steam64 { get; set; }
        bool mustUpdate;
        public MainWindow()
        {
            InitializeComponent();
            wbbrsrReglement.Source = new Uri("https://docs.google.com/document/d/1Wvf9hCu44gM_YkJK9qhePuAXh0NC7OQqkchuemNkVnQ/edit");
            ShowGTALocalisation();
            CloseAllWindowsExeptMain();
            CheckUpdateIsUpdated();
            UpdateButtons();
        }
        private void UpdateButtons()
        {

            if (mustUpdate)
            {
                btnUpdateCheck.Visibility = Visibility.Visible;
                btnPlay.Visibility = Visibility.Hidden;
                btnRemoveUpdate.Visibility = Visibility.Hidden;
            }
            else
            {
                btnUpdateCheck.Visibility = Visibility.Hidden;
                btnPlay.Visibility = Visibility.Visible;
                btnRemoveUpdate.Visibility = Visibility.Visible;
            }
        }

        private void CheckUpdateIsUpdated()
        {
            if (File.Exists(Path.Combine(GTADirectory, "GTA5.exe")))
            {
                System.Net.WebRequest req = System.Net.HttpWebRequest.Create("http://91.121.73.186/dlc.rpf");
                req.Method = "HEAD";
                using (System.Net.WebResponse resp = req.GetResponse())
                {
                    if (int.TryParse(resp.Headers.Get("Content-Length"), out int ContentLength))
                    {
                        long length = new System.IO.FileInfo(GTADirectory + "\\update\\x64\\dlcpacks\\patchday15ng\\dlc.rpf").Length;
                        if (ContentLength == length)
                        {
                            mustUpdate = false;
                        }
                        else
                        {
                            mustUpdate = true;
                            Checkupdateisinstalled();
                        }
                    }
                }
            }
            else
            {
                mustUpdate = true;
            }
            
        }
        public MainWindow(Decimal steam64, string userName)
        {
            this.Steam64 = steam64;
            this.steamUsername = userName;
        }

        private void CloseAllWindowsExeptMain()
        {
            foreach (Window item in App.Current.Windows)
            {
                if (item != this)
                    item.Close();
            }
        }

        //private void Getrequest()
        //{
        //    //Console.WriteLine("Making API Call...");
        //    string URL = "http://91.121.73.186/JoinApi/Read.php";
        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
        //    request.ContentType = "application/json; charset=utf-8";
        //    request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes("username:password"));
        //    request.PreAuthenticate = true;
        //    HttpWebResponse response = request.GetResponse() as HttpWebResponse;
        //    using (Stream responseStream = response.GetResponseStream())
        //    {
        //        StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
        //        //Console.WriteLine(JsonConvert.DeserializeObject(reader.ReadToEnd()));


        //        //StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
        //        //var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        //        ////string jsonObject = reader.ReadToEnd();
        //        //var jo = JObject.Parse(reader.ReadToEnd());
        //        //string updateversion = jo["info"][0]["Version"].ToString();
        //        ////Console.WriteLine(updateversion);
        //    }
        //}

        private int CountOnlinePlayers()
        {
            string URL = "http://91.121.73.186/JoinApi/Read.php";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.ContentType = "application/json; charset=utf-8";
            request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes("username:password"));
            request.PreAuthenticate = true;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            using (Stream responseStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                var jsonObject = JObject.Parse(reader.ReadToEnd());
                if ((JArray)jsonObject["records"] != null)
                {
                    int numberOfOnlinePlayers = ((JArray)jsonObject["records"]).Count;
                    return numberOfOnlinePlayers;
                }
                else
                {
                    return 0;
                }
            }
        }

        private void Postrequest(string steamId)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://91.121.73.186/JoinApi/Create.php");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = new JavaScriptSerializer().Serialize(new
                {
                    steam = steamId
                });

                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }
        }

        private void SetGTALocalisation()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog().ToString().Equals("OK"))
            {
                Properties.Settings.Default.GtaLocation = fbd.SelectedPath;
                GTADirectory = fbd.SelectedPath;
                Properties.Settings.Default.Save();
                ShowGTALocalisation();
            }
        }
        private void ShowGTALocalisation()
        {
            GTADirectory = Properties.Settings.Default.GtaLocation;
            txtBlckShowGtaLocalisation.Text = Properties.Settings.Default.GtaLocation;
        }

        static string ConvertToHex(decimal d)
        {
            int[] bits = decimal.GetBits(d);
            if (bits[3] != 0)
            {
                throw new ArgumentException();
            }
            return string.Format("{1:x7}{2:x8}",
                (uint)bits[2], (uint)bits[1], (uint)bits[0]);
        }


        private void BtnCloseApp_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnMinimizeApp_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void GoToArticle_Click(object sender, RoutedEventArgs e)
        {
            string path = (sender as Hyperlink).Tag as string;
            Process.Start(path);
        }

        private void BtnDiscord_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://discord.gg/6NKkpdD");
        }

        private void BtnRemoveUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Path.Combine(GTADirectory, "GTA5.exe")))
            {
                if (File.Exists(GTADirectory + "/update/x64/dlcpacks/patchday15ng/" + "backup_dlc.rpf"))
                {
                    File.Delete(GTADirectory + "/update/x64/dlcpacks/patchday15ng/" + "dlc.rpf");
                    File.Move(GTADirectory + "/update/x64/dlcpacks/patchday15ng/" + "backup_dlc.rpf", GTADirectory + "/update/x64/dlcpacks/patchday15ng/" + "dlc.rpf");
                    mustUpdate = true;
                    UpdateButtons();
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Il faut mettre la bonne localisation de GTA");
            }
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Path.Combine(GTADirectory, "GTA5.exe")))
            {
                if(CountOnlinePlayers() < 32)
                {
                    CheckSteamIsOpen();
                    Postrequest(steamIdHex);
                    Process.Start("fivem://connect/91.121.73.186:30121");
                }
                else
                {
                    System.Windows.MessageBox.Show("Actuellement il y a 32 joueurs en ville.\nVeuillez réessayer plus tard.");
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Il faut mettre la bonne localisation de GTA");
                btnPlay.Visibility = Visibility.Hidden;
                btnRemoveUpdate.Visibility = Visibility.Hidden;
                btnUpdateCheck.Visibility = Visibility.Visible;
            }
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                progressBar.Minimum = 0;
                double receive = double.Parse(e.BytesReceived.ToString());
                double total = double.Parse(e.TotalBytesToReceive.ToString());
                double percentage = receive / total * 100;
                lblStatus.Content = $"Téléchargé {string.Format("{0:0.##}", percentage)}%";
                progressBar.Value = int.Parse(Math.Truncate(percentage).ToString());
                mustUpdate = false;
                UpdateButtons();
            }));
        }

        private void BtnUpdateCheck_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Path.Combine(GTADirectory, "GTA5.exe")))
            {
                CheckUpdateIsUpdated();
                if(mustUpdate)
                {
                    string url = "http://91.121.73.186/dlc.rpf";
                    if (!string.IsNullOrEmpty(url))
                    {
                        if (File.Exists(GTADirectory + "/update/x64/dlcpacks/patchday15ng/" + "dlc.rpf"))
                        {
                            File.Move(GTADirectory + "/update/x64/dlcpacks/patchday15ng/" + "dlc.rpf", GTADirectory + "/update/x64/dlcpacks/patchday15ng/" + "backup_dlc.rpf");
                            Thread thread = new Thread(() =>
                            {
                                Uri uri = new Uri(url);
                                string filename = Path.GetFileName(uri.AbsolutePath);
                                client.DownloadFileAsync(uri, GTADirectory + "/update/x64/dlcpacks/patchday15ng/" + filename);
                            });
                            thread.Start();
                        }
                        else
                        {
                            Thread thread = new Thread(() =>
                            {
                                Uri uri = new Uri(url);
                                string filename = Path.GetFileName(uri.AbsolutePath);
                                client.DownloadFileAsync(uri, GTADirectory + "/update/x64/dlcpacks/patchday15ng/" + filename);
                            });
                            thread.Start();
                        }
                    }
                }
                else
                {
                    btnUpdateCheck.Visibility = Visibility.Hidden;
                    btnPlay.Visibility = Visibility.Visible;
                    btnRemoveUpdate.Visibility = Visibility.Visible;
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Il faut mettre la bonne localisation de GTA");
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            client = new WebClient();
            client.DownloadProgressChanged += Client_DownloadProgressChanged;
            steamIdHex = "steam:" + ConvertToHex(Steam64);
            lblSteamUser.Content = steamUsername;
            lblSteamHex.Content = steamIdHex;
            if (Properties.Settings.Default.SteamId64 == 0 && string.IsNullOrWhiteSpace(Properties.Settings.Default.SteamLogin))
            {
                Properties.Settings.Default.SteamId64 = Steam64;
                Properties.Settings.Default.SteamLogin = steamUsername;
                Properties.Settings.Default.Save();
            }
        }

        private void BtnDonation_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://paypal.me/pools/c/85BSV0rxIg");
        }

        private void BtnFairyV_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://fairy-v.com/");
        }

        private void BtnTopServer_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://gta.top-serveurs.net/fairy-v");
        }

        private void BtnFaceBook_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.facebook.com/FairyV.RP/");
        }

        private void BtnTwitter_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://twitter.com/FAIRY_V_RP");
        }

        private void BntSetGTALocalisation_Click(object sender, RoutedEventArgs e)
        {
            SetGTALocalisation();
        }

        private void CheckSteamIsOpen()
        {
            Process[] processlist = Process.GetProcesses();
            bool steamprocess = false;
            bool steamwebhelperprocess = false;
            bool SteamServiceprocess = false;
            foreach (Process theprocess in processlist)
            {
                switch (theprocess.ProcessName)
                {
                    case "Steam":
                        steamprocess = true;
                        break;
                    case "steamwebhelper":
                        steamwebhelperprocess = true;
                        break;
                    case "SteamService":
                        SteamServiceprocess = true;
                        break;
                    default:
                        break;
                }
            }
            if (!(steamprocess && steamwebhelperprocess && SteamServiceprocess))
            {
                System.Windows.MessageBox.Show("Steam n'est pas démarré. Lancez le pour pouvoir jouer.");
            }
        }

        private void LblSteamHex_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Windows.Clipboard.SetText((string)lblSteamHex.Content);
        }

        private void TxtBlckShowGtaLocalisation_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Windows.Clipboard.SetText((string)txtBlckShowGtaLocalisation.Text);
        }

        private void LblSteamUser_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Windows.Clipboard.SetText((string)lblSteamUser.Content);
        }

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void BntChangeLoggedInUser_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SteamId64 = 0;
            Properties.Settings.Default.SteamLogin = "";
            SteamLogin steamLogin = new SteamLogin();
            steamLogin.Show();
            Close();
        }

        private void Checkupdateisinstalled()
        {
            if (File.Exists(GTADirectory + "/update/x64/dlcpacks/patchday15ng/" + "backup_dlc.rpf"))
            {
                if (mustUpdate)
                {
                    File.Delete(GTADirectory + "/update/x64/dlcpacks/patchday15ng/" + "dlc.rpf");
                    File.Move(GTADirectory + "/update/x64/dlcpacks/patchday15ng/" + "backup_dlc.rpf", GTADirectory + "/update/x64/dlcpacks/patchday15ng/" + "dlc.rpf");
                }
            }
        }
    }
}
