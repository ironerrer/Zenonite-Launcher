using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Zenonite_Launcher.Epic_Shit;
using Zenonite_Launcher.JsonParser;
using Zenonite_Launcher.Properties;
using Zenonite_Launcher.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace Zenonite_Launcher
{
    public partial class MainWindow : Window
    {
        private string token;
        private string accountId;
        private string username;
        private Process KILLMEPLS;

        //private Process clientlauncherProcess;

        public MainWindow()
        {
            InitializeComponent();
            FindPath();
            FNPath.Text = Settings.Default.Path;
        }

        private string DatFile
        {
            get
            {
                foreach (string str in MainWindow.drives)
                {
                    if (File.Exists(str + ":\\ProgramData\\Epic\\UnrealEngineLauncher\\LauncherInstalled.dat"))
                    {
                        return str + ":\\ProgramData\\Epic\\UnrealEngineLauncher\\LauncherInstalled.dat";
                    }
                }
                return "a";
            }
        }

        private void FindPath()
        {
            if (this.DatFile != "a")
            {
                string text = File.ReadAllText(this.DatFile);
                JToken jtoken = JsonConvert.DeserializeObject<JToken>(text);
                if (jtoken == null)
                {
                    return;
                }
                JArray jarray = Extensions.Value<JArray>(jtoken["InstallationList"]);
                if (jarray == null)
                {
                    return;
                }
                using (IEnumerator<JToken> enumerator = jarray.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        JToken jtoken2 = enumerator.Current;
                        if (string.Equals(Extensions.Value<string>(jtoken2["AppName"]), "Fortnite"))
                        {
                            string path = Extensions.Value<string>(jtoken2["InstallLocation"]) ?? "";
                            Settings.Default.Path = path;
                            Settings.Default.Save();
                        }
                    }
                    return;
                }
            }
            MessageBox.Show("ERROR: Could Not Find Your Fortnite Path Please Set The Path Manually!");
        }

        private static readonly string[] drives = new string[]
        {
            "A",
            "B",
            "C",
            "D",
            "E",
            "F",
            "G",
            "H",
            "I",
            "J",
            "K",
            "L",
            "M",
            "N",
            "O",
            "P",
            "Q",
            "R",
            "S",
            "T",
            "U",
            "V",
            "W",
            "X",
            "Y",
            "Z"
        };

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var switchAuth = Auth.GetDevicecodetoken();
            var rawToken = Auth.GetDevicecode(switchAuth);
            Console.WriteLine(rawToken);
            if (switchAuth.Contains("expired"))
            {
                return;
            }

            var jsondata1 = JsonConvert.DeserializeObject<Token>(rawToken);

            DisplayName.Visibility = Visibility.Visible;
            dfvfdvd.Visibility = Visibility.Visible;
            LoginButton.Visibility = Visibility.Hidden;
            LaunchButton.Visibility = Visibility.Visible;
            username = jsondata1.DisplayName;
            DisplayName.Content = jsondata1.DisplayName;
            token = jsondata1.access_token;
            accountId = jsondata1.account_id;
            Console.WriteLine(jsondata1.account_id);
        }

        private void LaunchButton_Click_1(object sender, RoutedEventArgs e)
        {
            var exchange = Auth.GetExchange(token);
            // Sigh...
            var clientPath = Path.Combine(Settings.Default.Path, $"FortniteGame\\Binaries\\Win64\\FortniteClient-Win64-Shipping.exe");
            var launcherpath = Path.Combine(Settings.Default.Path, $"FortniteGame\\Binaries\\Win64\\FortniteLauncher.exe");
            var eacPath = Path.Combine(Settings.Default.Path, $"FortniteGame\\Binaries\\Win64\\FortniteClient-Win64-Shipping_EAC.exe");
            if (!File.Exists(clientPath))
            {
                var text =
                    $"\"{clientPath}\" was not found, please make sure it exists." + "\n\n" +
                    "Did you set the Install Location correctly?" + "\n\n" +
                    "TIP: The Install Location must be set to a folder that contains 2 folders named \"Engine\" and \"FortniteGame\".";

                MessageBox.Show(text);
                return;
            }

            var nativePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ZenoniteNative.dll");
            var consolePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ZenoniteConsole.dll");
            if (!File.Exists(nativePath))
            {
                var text = "Your Native Is Not In The Same Path As Launcher.";
                MessageBox.Show(text);
                return;
            }

            var arguments = $"-AUTH_LOGIN=unused -AUTH_PASSWORD={exchange} -AUTH_TYPE=exchangecode -epicapp=Fortnite -epicenv=Prod -epiclocale=en-us -epicportal -noeac -fromfl=be -fltoken=f7b9gah4h5380d10f721dd6a -skippatchcheck";

            var eacProcess = new Process();
            eacProcess.StartInfo.FileName = eacPath;
            eacProcess.StartInfo.Arguments = arguments;
            eacProcess.Start();
            foreach (ProcessThread processThread in eacProcess.Threads)
            {
                Win32.SuspendThread(Win32.OpenThread(2, false, processThread.Id));
            }
            var LauncherProcess = new Process();
            LauncherProcess.StartInfo.FileName = launcherpath;
            LauncherProcess.StartInfo.Arguments = arguments;
            LauncherProcess.Start();
            foreach (ProcessThread processThread1 in LauncherProcess.Threads)
            {
                Win32.SuspendThread(Win32.OpenThread(2, false, processThread1.Id));
            }

            KILLMEPLS = new Process
            {
                StartInfo = new ProcessStartInfo(clientPath, arguments)
                {
                    UseShellExecute = false,

                    RedirectStandardOutput = false,

                    CreateNoWindow = false
                }
            };
            {
                Process clientlauncherProcess = new Process();
                if (!File.Exists(eacPath))
                {
                    var nijn =
                        $"\"{eacPath}\" was not found, please make sure it exists." + "\n\n" +
                        "Did you set the Install Location correctly?" + "\n\n" +
                        "TIP: The Install Location must be set to a folder that contains 2 folders named \"Engine\" and \"FortniteGame\".";

                    MessageBox.Show(nijn);
                    return;
                }
                KILLMEPLS.Start();
                Thread.Sleep(6000);
                var client = new WebClient();
                string temp = Path.GetTempPath();
                try
                {
                    File.Delete($@"{temp}/Injector.exe");
                }
                catch
                {
                    MessageBox.Show("ERROR");
                }
                try
                {
                    client.DownloadFile("https://cdn.discordapp.com/attachments/786579039086968868/789708837523423252/Injector.exe", $@"{temp}/Injector.exe");
                }
                catch
                {
                    MessageBox.Show("There Was a error");
                    return;
                }
                Process Injector = new Process();
                Injector.StartInfo.Arguments = $"\"{KILLMEPLS.Id}\" \"{nativePath}\"";
                Injector.StartInfo.CreateNoWindow = true;
                Injector.StartInfo.RedirectStandardInput = true;
                Injector.StartInfo.UseShellExecute = false;
                Injector.StartInfo.FileName = $@"{temp}/Injector.exe";
                Injector.Start();
                Thread.Sleep(200);
                while (true)
                {
                    if (KILLMEPLS.HasExited)
                    {
                        LoginButton.Visibility = Visibility.Visible;
                        LaunchButton.Visibility = Visibility.Hidden;
                        dfvfdvd.Visibility = Visibility.Hidden;
                        DisplayName.Visibility = Visibility.Hidden;
                        Environment.Exit(0);
                        eacProcess.Kill();
                        LauncherProcess.Kill();
                    }
                    else
                    {
                    }
                    Thread.Sleep(200);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WinForms.FolderBrowserDialog folderBrowser = new WinForms.FolderBrowserDialog();
            folderBrowser.ShowNewFolderButton = false;
            WinForms.DialogResult result = folderBrowser.ShowDialog();

            if (result == WinForms.DialogResult.OK)
            {
                String FNSPATH = folderBrowser.SelectedPath;
                FNPath.Text = FNSPATH;
            }
            Settings.Default.Path = FNPath.Text;
            Settings.Default.Save();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Process.Start("https://discord.gg/bPVKyPZFdj");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Process.Start("https://youtube.com/xoelf");
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Process.Start("https://twitter.com/xoElfy");
        }

        private void FNPath_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
