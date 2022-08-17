using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;

using Newtonsoft.Json;

using Genshin__.lib;
using Genshin__.lib.json;

using DGP.Genshin.FPSUnlocking;

namespace Genshin__
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool MESSAGEBUBBLE = true;
        private int FPSLIMITED = -1;
        private bool CUSTOMIZESTARTUPRESOLUTION = false;
        private bool CUSTOMIZELAUNCHERIMAGE = false;

        // 程序配置
        private MainConfig mainConfig = new MainConfig();
        private ProgramJson programJson = new ProgramJson();
        private ServerJson serverJson = new ServerJson();

        // 原神帧率解锁组件
        Unlocker? unlocker = null;

        // 文件选择组件
        OpenFileDialog openFileDialog = new OpenFileDialog();

        /// <summary>
        /// 初始化控件
        /// </summary>
        public MainWindow()
        {
            // 窗口初始化
            InitializeComponent();
            // 设置标题当前版本
            ProgramVersion.Content = System.Windows.Application.ResourceAssembly.GetName().Version.ToString();
            // 初始化应用程序参数
            InitializeProgramData();
        }


        private bool ComponentFileCheck()
        {

            return true;
        }

        /// <summary>
        /// 检测当前版本号是否大于等于目标版本号
        /// </summary>
        /// <param name="srcVersion">源版本号</param>
        /// <param name="tarVersion">目的版本号</param>
        /// <returns></returns>
        private bool VersionCheck(string srcVersion, string tarVersion)
        {
            string[] src = srcVersion.Split('.');
            string[] tar = tarVersion.Split('.');
            if (src.Length != tar.Length)
                return false;
            for (int i = 0; i < src.Length; i++)
            {
                if (int.Parse(src[i]) > int.Parse(tar[i]))
                    return true;
                if (int.Parse(src[i]) < int.Parse(tar[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 程序参数初始化
        /// </summary>
        private void InitializeProgramData()
        {
            if (File.Exists(Environment.CurrentDirectory + "\\config.json"))
            {
                mainConfig = JsonConvert.DeserializeObject<MainConfig>(File.ReadAllText(Environment.CurrentDirectory + "\\config.json"));
            }
            else
            {
                mainConfig.gameInfoDisplay = true;
                mainConfig.popupAlert = false;
                mainConfig.fps = new FpsSettingInfo();
                mainConfig.fps.unlock = false;
                mainConfig.fps.fpsLimit = 144;
                mainConfig.startResolution = new StartResolutionSettingInfo();
                mainConfig.startResolution.enable = false;
                mainConfig.startResolution.fullScreen = true;
                mainConfig.startResolution.popupWindow = false;
                mainConfig.startResolution.width = 1280;
                mainConfig.startResolution.height = 720;
                mainConfig.launchImage = new LauncherBackgroudImage();
                mainConfig.launchImage.enable = false;
                mainConfig.launchImage.path = null;
                File.WriteAllText(Environment.CurrentDirectory + "\\config.json", JsonConvert.SerializeObject(mainConfig));
            }

            try
            {
                // 左上参数展示初始化
                if (!mainConfig.gameInfoDisplay)
                {
                    GameInfoDisplaySwitch.IsChecked = false;
                    GameInfos.Visibility = Visibility.Hidden;
                }
                // 弹出提示初始化
                if (mainConfig.popupAlert)
                {
                    BubbleMessageSwitch.IsChecked = true;
                    MESSAGEBUBBLE = false;
                }
                // FPS解锁初始化
                FpsLimitNum.Text = mainConfig.fps.fpsLimit.ToString();
                if (mainConfig.fps.unlock)
                {
                    FpsLimitSwitch.IsChecked = true;
                    FPSLIMITED = mainConfig.fps.fpsLimit;
                }
                // 启动分辨率初始化
                HorizontalResolution.Text = mainConfig.startResolution.width.ToString();
                VerticalResolution.Text = mainConfig.startResolution.height.ToString();
                if (mainConfig.startResolution.fullScreen)
                    GameStartDisplayMode.SelectedIndex = 0;
                else if (mainConfig.startResolution.popupWindow)
                    GameStartDisplayMode.SelectedIndex = 2;
                else
                    GameStartDisplayMode.SelectedIndex = 1;
                if (mainConfig.startResolution.enable)
                {
                    GameResolutionSwitch.IsChecked = true;
                    CUSTOMIZESTARTUPRESOLUTION = true;
                }
                // 启动器图片初始化
                if (mainConfig.launchImage.enable)
                {
                    CustomizeLauncherImage.IsChecked = true;
                    CUSTOMIZELAUNCHERIMAGE = true;
                }
                if (CUSTOMIZELAUNCHERIMAGE && File.Exists(mainConfig.launchImage.path))
                    if (mainConfig.launchImage.path.EndsWith("jpg") || mainConfig.launchImage.path.EndsWith("png"))
                        BackgroundImage.ImageSource = new BitmapImage(new Uri(mainConfig.launchImage.path, UriKind.Absolute));
            }
            catch (Exception ex)
            {
                MainAlertText(ex.Message);
            }

            if (File.Exists(Environment.CurrentDirectory + "\\ProgramConfig.json"))
            {
                // 解析已有游戏配置记录文件
                string jsonStr = File.ReadAllText(Environment.CurrentDirectory + "\\ProgramConfig.json");
                programJson = JsonConvert.DeserializeObject<ProgramJson>(jsonStr);

                if (!Directory.Exists(programJson.game) || !Directory.Exists(programJson.launcher))
                    try
                    {
                        if (ProgramConfigUpdate(programJson.launcher) != 0)
                            throw new Exception("配置更新失败");
                    }
                    catch (Exception ex)
                    {
                        MainAlertText(ex.Message);
                    }

                try
                {
                    if (ProgramConfigUpdate(programJson.launcher, false) != 0)
                        throw new Exception("配置获取失败");
                }
                catch (Exception ex)
                {
                    MainAlertText(ex.Message);
                }
            }
            else
            {
                // 尝试新建游戏配置记录文件
                try
                {
                    if (ProgramConfigUpdate(RegReader.getGenShinInstallPath()) != 0)
                        throw new Exception("配置获取失败");
                }
                catch (Exception ex)
                {
                    MainAlertText(ex.Message);
                }
            }

            if (File.Exists(Environment.CurrentDirectory + "\\ServerJson.json"))
            {
                string jsonStr = File.ReadAllText(Environment.CurrentDirectory + "\\ServerJson.json");
                serverJson = JsonConvert.DeserializeObject<ServerJson>(jsonStr);
                ServerList.ItemsSource = serverJson.gameSettings;
                MainAlertText("成功读取本地服务器设置配置文件");
            }
            else
            {
                RequestForServerConfig();
            }
        }

        /// <summary>
        /// 关闭程序自动保存设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveProgramSettings(object sender, System.ComponentModel.CancelEventArgs e)
        {
            File.WriteAllText(Environment.CurrentDirectory + "\\config.json", JsonConvert.SerializeObject(mainConfig));
        }

        /// <summary>
        /// 从Github服务器获取服务器设置按键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateServerConfigInfo(object sender, RoutedEventArgs e)
        {
            RequestForServerConfig();
        }

        /// <summary>
        /// 从Github更新配置
        /// </summary>
        /// <param name="writeJson"></param>
        private void RequestForServerConfig(bool writeJson = true)
        {
            // 尝试从Github获取服务器设置信息
            try
            {
                string jsonStr = RequestWebApi("https://aurora211.github.io/GenShinServerSwitcher/ServerJson.json");
                serverJson = JsonConvert.DeserializeObject<ServerJson>(jsonStr);
                if (writeJson)
                    File.WriteAllText(Environment.CurrentDirectory + "\\ServerJson.json", jsonStr);
                ServerList.ItemsSource = serverJson.gameSettings;
                MainAlertText("成功从Github获取服务器参数配置");
            }
            catch (Exception ex)
            {
                MainAlertText(ex.Message);
            }
        }

        /// <summary>
        /// 调用API接口需要的方法
        /// </summary>
        /// <param name="url">API的URL</param>
        /// <param name="method">调用方法</param>
        /// <param name="body">POST参数</param>
        /// <returns></returns>
        private string RequestWebApi(string url, string method = "GET", string body = null)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = method;
            httpWebRequest.Accept = "*/*";
            httpWebRequest.ContentType = "application/json";

            if (method == "POST")
            {
                byte[] buffer = Encoding.UTF8.GetBytes(body);
                httpWebRequest.ContentLength = buffer.Length;
                httpWebRequest.GetRequestStream().Write(buffer, 0, buffer.Length);
            }

            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
            return streamReader.ReadToEnd();
        }

        /// <summary>
        /// 手动刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ManualUpdateLocalGameConfig(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ProgramConfigUpdate(programJson.launcher, false) != 0)
                    throw new Exception("配置更新失败");
                MainAlertText("重新获取本地游戏配置成功");
            }
            catch (Exception ex)
            {
                MainAlertText(ex.Message);
            }
        }

        /// <summary>
        /// 启动原神
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void StartGame(object sender, RoutedEventArgs e)
        {
            // 新建原神进程对象
            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = programJson.game + "\\YuanShen.exe",
                    Verb = "runas",
                    UseShellExecute = true,
                    WorkingDirectory = programJson.game
                }
            };
            
            // 判断自定义分辨率模式
            if (CUSTOMIZESTARTUPRESOLUTION)
            {
                process.StartInfo.ArgumentList.Add("-screen-fullscreen");
                if (mainConfig.startResolution.fullScreen)
                    process.StartInfo.ArgumentList.Add("1");
                else
                {
                    process.StartInfo.ArgumentList.Add("0");
                    if (mainConfig.startResolution.popupWindow)
                        process.StartInfo.ArgumentList.Add("-popupwindow");
                }
                process.StartInfo.ArgumentList.Add("-screen-height");
                process.StartInfo.ArgumentList.Add(mainConfig.startResolution.height.ToString());
                process.StartInfo.ArgumentList.Add("-screen-width");
                process.StartInfo.ArgumentList.Add(mainConfig.startResolution.width.ToString());
            }
            // 检测是否在超FPS状态
            if (FPSLIMITED >= 30)
            {
                // 解锁超FPS
                unlocker = new(process, FPSLIMITED);
            }
            // 启动原神进程
            process.Start();
            MainAlertText("游戏启动中");
            // 解锁模式下阻塞，等待原神进程
            if (FPSLIMITED >= 30)
            {
                // 本窗口最小化
                this.WindowState = WindowState.Minimized;
                var result = await unlocker.UnlockAsync();
                MainAlertText(result.ToString());
            }
        }

        /// <summary>
        /// 获取指定进程ID
        /// </summary>
        /// <param name="processName">进程名称</param>
        /// <returns></returns>
        private IList<int> ProcessIdDetect(string processName)
        {
            Process[] processes = Process.GetProcesses();
            IList<int> ids = new List<int>();
            foreach (Process process in processes)
            {
                if (process.ProcessName == processName)
                {
                    ids.Add(process.Id);
                }
            }
            return ids;
        }

        /// <summary>
        /// 还原最初启动Genshin++的游戏服务器参数设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetGameServerConfig(object sender, RoutedEventArgs e)
        {
            try
            {
                ProgramJson json = JsonConvert.DeserializeObject<ProgramJson>(File.ReadAllText(Environment.CurrentDirectory + "\\ProgramConfig.json"));

                IniParser iniParser = new IniParser(programJson.game + "\\config.ini");
                if (!iniParser.SetKeyValue("General", "channel", json.gameInfo.channel))
                    MainAlertText("Channal Recover Failed");
                if (!iniParser.SetKeyValue("General", "sub_channel", json.gameInfo.sub_channel))
                    MainAlertText("SubChannal Recover Failed");
                if (!iniParser.SetKeyValue("General", "cps", json.gameInfo.cps))
                    MainAlertText("CPS Recover Failed");
                iniParser.SaveAsIniFile(programJson.game + "\\config.ini");
                ProgramConfigUpdate(programJson.launcher, false);
                MainAlertText("已恢复最初启动参数");
            }
            catch (Exception ex)
            {
                MainAlertText(ex.Message);
            }
        }

        /// <summary>
        /// 更新程序配置文件，刷新本地游戏相关信息
        /// </summary>
        /// <param name="writeJson">是否写入JSON配置文件</param>
        /// <returns></returns>
        private int ProgramConfigUpdate(string launcherPath, bool writeJson = true)
        {
            programJson = new ProgramJson();
            string path = launcherPath;
            if (!Directory.Exists(path))
            {
                return 1;
            }
            LauncherPath.Content = path;
            programJson.launcher = path;

            IniParser iniParser = new IniParser(path + "\\config.ini");
            path = iniParser.GetKeyValue("launcher", "game_install_path");

            if (!Directory.Exists(path))
            {
                return 2;
            }
            GamePath.Content = path;
            programJson.game = path;
            programJson.launcherInfo = new LauncherInfo();
            programJson.launcherInfo.cps = iniParser.GetKeyValue("launcher", "cps");
            programJson.launcherInfo.sub_channel = iniParser.GetKeyValue("launcher", "sub_channel");
            programJson.launcherInfo.channel = iniParser.GetKeyValue("launcher", "channel");
            programJson.launcherInfo.game_install_path = path;
            programJson.launcherInfo.game_dynamic_bg_name = iniParser.GetKeyValue("launcher", "game_dynamic_bg_name");
            programJson.launcherInfo.game_start_name = iniParser.GetKeyValue("launcher", "game_start_name");

            iniParser = new IniParser(path + "\\config.ini");
            programJson.gameInfo = new GameInfo();
            programJson.gameInfo.cps = iniParser.GetKeyValue("General", "cps");
            programJson.gameInfo.sub_channel = iniParser.GetKeyValue("General", "sub_channel");
            programJson.gameInfo.channel = iniParser.GetKeyValue("General", "channel");
            programJson.gameInfo.game_version = iniParser.GetKeyValue("General", "game_version");
            programJson.gameInfo.plugin_sdk_version = iniParser.GetKeyValue("General", "plugin_sdk_version");
            programJson.gameInfo.plugin_5_version = iniParser.GetKeyValue("General", "plugin_5_version");

            GameVersion.Content = programJson.gameInfo.game_version;
            GameCPS.Content = programJson.gameInfo.cps;
            GameChannel.Content = programJson.gameInfo.channel;
            GameSubChannel.Content = programJson.gameInfo.sub_channel;

            programJson.updateTime = DateTime.Now.ToString();

            if (CUSTOMIZELAUNCHERIMAGE == false)
                BackgroundImage.ImageSource = new BitmapImage(new Uri(programJson.launcher + "\\bg\\" + programJson.launcherInfo.game_dynamic_bg_name, UriKind.Absolute));

            if (writeJson)
            {
                string jsonStr = JsonConvert.SerializeObject(programJson);
                File.WriteAllText(Environment.CurrentDirectory + "\\ProgramConfig.json", jsonStr);
            }
            return 0;
        }

        /// <summary>
        /// 阻塞式弹出错误气泡
        /// </summary>
        /// <param name="text"></param>
        private void MainAlertText(string text, bool fadeOut = true)
        {
            if (MESSAGEBUBBLE)
            {
                WarningBubble.Visibility = Visibility.Hidden;
                WarningText.Content = text;
                WarningBubble.Visibility = Visibility.Visible;
                if (fadeOut)
                {
                    DoubleAnimation doubleAnimation = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(3)));
                    WarningBubble.BeginAnimation(UIElement.OpacityProperty, doubleAnimation);
                }
            }
            else
            {
                System.Windows.MessageBox.Show(text);
            }
        }

        /// <summary>
        /// 复制显示的游戏配置到剪切包
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyDisplayConfigToClipBoard(object sender, RoutedEventArgs e)
        {
            string configStr = string.Empty;
            configStr += "LauncherInstallPath: " + LauncherPath.Content + "\n";
            configStr += "GameInstallPath: " + GamePath.Content + "\n";
            configStr += "GameVersion: " + GameVersion.Content + "\n";
            configStr += "CPS: " + GameCPS.Content + "\n";
            configStr += "Channel: " + GameChannel.Content + "\n";
            configStr += "SubChannel: " + GameSubChannel.Content + "\n";

            try
            {
                System.Windows.Clipboard.SetText(configStr);
                MainAlertText("成功复制到剪切板");
            }
            catch (Exception ex)
            {
                MainAlertText(ex.Message);
            }
        }

        /// <summary>
        /// 窗口移动绑定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        /// <summary>
        /// 程序关闭按键绑定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgramShutdown(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 程序最小化绑定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgramMinimize(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// 跳转至作者bilibili主页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void JumpToAuthorBili(object sender, RoutedEventArgs e)
        {
            Process process = new Process()
            {
                StartInfo =
                {
                    FileName = "https://space.bilibili.com/57896387",
                    UseShellExecute = true,
                }
            };
            process.Start();
        }

        /// <summary>
        /// 跳转至项目Github页面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void JumpToAuthorGithub(object sender, RoutedEventArgs e)
        {
            Process process = new Process()
            {
                StartInfo =
                {
                    FileName = "https://github.com/Aurora211/GenShinServerSwitcher",
                    UseShellExecute = true,
                }
            };
            process.Start();
        }

        /// <summary>
        /// 隐藏右侧按键提示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnHoverBubble(object sender, RoutedEventArgs e)
        {
            HelpBubble.Visibility = Visibility.Hidden;
            HelpBubble_Bili.Visibility = Visibility.Hidden;
            HelpBubble_Github.Visibility = Visibility.Hidden;
            HelpBubble_Rollback.Visibility = Visibility.Hidden;
            HelpBubble_UpdateServer.Visibility = Visibility.Hidden;
            HelpBubble_Settings.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// 展示右侧按键提示-哔哩哔哩按键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HoverBili(object sender, RoutedEventArgs e)
        {
            HelpBubble_Bili.Visibility = Visibility.Visible;
            HelpBubble.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 展示右侧按键提示-Github
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HoverGithub(object sender, RoutedEventArgs e)
        {
            HelpBubble_Github.Visibility = Visibility.Visible;
            HelpBubble.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 展示右侧按键提示-回滚参数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HoverRollback(object sender, RoutedEventArgs e)
        {
            HelpBubble_Rollback.Visibility = Visibility.Visible;
            HelpBubble.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 展示右侧按键提示-更新服务器参数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HoverUpdateServer(object sender, RoutedEventArgs e)
        {
            HelpBubble_UpdateServer.Visibility = Visibility.Visible;
            HelpBubble.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 展示右侧按键提示-设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HoverSettings(object sender, RoutedEventArgs e)
        {
            HelpBubble_Settings.Visibility = Visibility.Visible;
            HelpBubble.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 展示与隐藏设置窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisplaySettings(object sender, RoutedEventArgs e)
        {
            if (SettingsWindow.Visibility == Visibility.Visible)
            {
                SettingsWindow.Visibility = Visibility.Hidden;
            }
            else
            {
                SettingsWindow.Opacity = 0;
                SettingsWindow.Visibility = Visibility.Visible;
                DoubleAnimation doubleAnimation = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(300)));
                SettingsWindow.BeginAnimation(UIElement.OpacityProperty, doubleAnimation);
            }
        }

        /// <summary>
        /// 解锁按键响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnableUnlockFps(object sender, RoutedEventArgs e)
        {
            if (FpsLimitSwitch.IsChecked == true)
                try
                {
                    FpsLimitWarning.Visibility = Visibility.Visible;
                    mainConfig.fps.fpsLimit = int.Parse(FpsLimitNum.Text);
                    mainConfig.fps.unlock = true;
                    FPSLIMITED = mainConfig.fps.fpsLimit;
                }
                catch (Exception ex)
                {
                    MainAlertText(ex.Message);
                }
            else
            {
                mainConfig.fps.unlock = false;
                FPSLIMITED = -1;
            }
        }

        /// <summary>
        /// 最大帧率修改响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeFpsLimit(object sender, TextChangedEventArgs e)
        {
            if (FpsLimitSwitch.IsChecked == true)
                try
                {
                    mainConfig.fps.fpsLimit = int.Parse(FpsLimitNum.Text);
                    FPSLIMITED = mainConfig.fps.fpsLimit;
                }
                catch (Exception ex)
                {
                    MainAlertText(ex.Message);
                }
            else
                try
                {
                    mainConfig.fps.fpsLimit = int.Parse(FpsLimitNum.Text);
                }
                catch (Exception ex)
                {
                    MainAlertText(ex.Message);
                }
        }

        /// <summary>
        /// 自定义分辨率启用响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnableCustomizeResolution(object sender, RoutedEventArgs e)
        {
            if (GameResolutionSwitch.IsChecked == true)
            {
                mainConfig.startResolution.enable = true;
                CUSTOMIZESTARTUPRESOLUTION = true;
            }
            else
            {
                mainConfig.startResolution.enable = false;
                CUSTOMIZESTARTUPRESOLUTION = false;
            }
        }

        /// <summary>
        /// 自定义启动模式修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameStartWindowMode(object sender, SelectionChangedEventArgs e)
        {
            switch (GameStartDisplayMode.SelectedIndex)
            {
                case 1:
                    mainConfig.startResolution.fullScreen = false;
                    mainConfig.startResolution.popupWindow = false;
                    break;
                case 2:
                    mainConfig.startResolution.fullScreen = false;
                    mainConfig.startResolution.popupWindow = true;
                    break;
                default:
                    mainConfig.startResolution.fullScreen = true;
                    mainConfig.startResolution.popupWindow = false;
                    break;
            }
        }

        /// <summary>
        /// 自定义窗口高度修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameStartWindowHeight(object sender, TextChangedEventArgs e)
        {
            try
            {
                mainConfig.startResolution.height = int.Parse(VerticalResolution.Text);
            }
            catch(Exception ex)
            {
                MainAlertText(ex.Message);
            }
        }

        /// <summary>
        /// 自定义窗口宽度修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameStartWindowWidth(object sender, TextChangedEventArgs e)
        {
            try
            {
                mainConfig.startResolution.width = int.Parse(HorizontalResolution.Text);
            }
            catch(Exception ex)
            {
                MainAlertText(ex.Message);
            }
        }

        /// <summary>
        /// 弹窗提示模式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PopoutAlertMode(object sender, RoutedEventArgs e)
        {
            if (BubbleMessageSwitch.IsChecked == true)
            {
                mainConfig.popupAlert = true;
                MESSAGEBUBBLE = false;
            }
            else
            {
                mainConfig.popupAlert = false;
                MESSAGEBUBBLE = true;
            }
        }

        /// <summary>
        /// 左上角游戏信息显示模式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HideGameInfoBubbleMode(object sender, RoutedEventArgs e)
        {
            if (GameInfoDisplaySwitch.IsChecked == true)
            {
                mainConfig.gameInfoDisplay = true;
                GameInfos.Visibility = Visibility.Visible;
            }
            else
            {
                mainConfig.gameInfoDisplay = false;
                GameInfos.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// 启动自定义背景图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnableCustomizeImage(object sender, RoutedEventArgs e)
        {
            if (CustomizeLauncherImage.IsChecked == true)
            {
                mainConfig.launchImage.enable = true;
                CUSTOMIZELAUNCHERIMAGE = true;

                if (File.Exists(mainConfig.launchImage.path))
                    if (mainConfig.launchImage.path.EndsWith("jpg") || mainConfig.launchImage.path.EndsWith("png"))
                        BackgroundImage.ImageSource = new BitmapImage(new Uri(mainConfig.launchImage.path, UriKind.Absolute));
            }
            else
            {
                mainConfig.launchImage.enable = false;
                CUSTOMIZELAUNCHERIMAGE = false;
            }
        }

        /// <summary>
        /// 选择背景图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectBackgroundImage(object sender, RoutedEventArgs e)
        {
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            openFileDialog.Filter = "(*.jpg; *.png)|*.jpg; *.png";
            openFileDialog.Title = "选择图片";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = false;
            openFileDialog.CheckFileExists = true;
            if (openFileDialog.ShowDialog() == true)
            {
                mainConfig.launchImage.path = openFileDialog.FileName;
                if (CUSTOMIZELAUNCHERIMAGE && File.Exists(mainConfig.launchImage.path))
                    if (mainConfig.launchImage.path.EndsWith("jpg") || mainConfig.launchImage.path.EndsWith("png"))
                        BackgroundImage.ImageSource = new BitmapImage(new Uri(mainConfig.launchImage.path, UriKind.Absolute));
            }
        }

        /// <summary>
        /// 选择游戏启动器路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectLauncherPath(object sender, RoutedEventArgs e)
        {
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            openFileDialog.Filter = "(*.exe)|*.exe";
            openFileDialog.Title = "选择原神启动器";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = false;
            openFileDialog.CheckFileExists = true;
            if (openFileDialog.ShowDialog() == true)
            {
                string path = openFileDialog.FileName.Replace(openFileDialog.SafeFileName, string.Empty);
                if (Directory.Exists(path))
                {
                    programJson.launcher = path;
                    try
                    {
                        if (ProgramConfigUpdate(programJson.launcher) == 0)
                            MainAlertText("游戏信息获取成功");
                        else
                            MainAlertText("游戏信息获取失败");
                    }
                    catch (Exception ex)
                    {
                        MainAlertText(ex.Message);
                    }
                    return;
                }
                MainAlertText("路径错误");
            }
        }

        /// <summary>
        /// 关闭FPS解锁警告
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HideFpsLimitWarning(object sender, RoutedEventArgs e)
        {
            FpsLimitWarning.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// 服务器切换响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerSwitch(object sender, SelectionChangedEventArgs e)
        {
            // 获取服务器设置信息
            GameSetting setting = (GameSetting)ServerList.SelectedItem;
            if (!VersionCheck(setting.info.game_version, programJson.gameInfo.game_version))
                MainAlertText("服务器分发版本低于游戏版本");

            // ini服务器配置文件修改
            try
            {
                if (!IniConfigExchange(setting.info))
                    MainAlertText("配置更改失败");
            }
            catch (Exception ex)
            {
                MainAlertText(ex.Message);
            }

            // 文件替换逻辑调用
            try
            {
                if (!FileExchange(setting.info.fileRequirements))
                    MainAlertText("关键文件替换失败");
                else
                    MainAlertText("关键文件替换成功");
            }
            catch (Exception ex)
            {
                MainAlertText(ex.Message);
            }

            // 刷新配置显示
            if (ProgramConfigUpdate(programJson.launcher, false) != 0)
                MainAlertText("配置显示刷新失败");
        }

        private bool IniConfigExchange(GameSettingInfo gameSettingInfo)
        {
            bool status = true;
            IniParser iniParser = new IniParser(programJson.game + "\\config.ini");
            if (!iniParser.SetKeyValue("General", "channel", gameSettingInfo.channel))
                status = false;
            if (!iniParser.SetKeyValue("General", "sub_channel", gameSettingInfo.sub_channel))
                status = false;
            if (!iniParser.SetKeyValue("General", "cps", gameSettingInfo.cps))
                status = false;
            if (status)
                iniParser.SaveAsIniFile(programJson.game + "\\config.ini");
            return status;
        }

        /// <summary>
        /// 游戏换服关键文件修改
        /// </summary>
        /// <param name="fileRequirements"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private bool FileExchange(FileRequirement[] fileRequirements)
        {
            if (fileRequirements.Length == 0 || fileRequirements == null)
                return true;
            if (Directory.Exists(Environment.CurrentDirectory + "\\assets") == false)
                Directory.CreateDirectory(Environment.CurrentDirectory + "\\assets");
            foreach (FileRequirement fileRequirement in fileRequirements)
            {
                
                switch (fileRequirement.method)
                {
                    case "add":
                        if (!File.Exists(Environment.CurrentDirectory + "\\assets\\" + fileRequirement.fileName))
                        {
                            MessageBox.Show(
                                "关键文件缺失！\n" +
                                "请下载后放置于本启动器的 assets 文件夹下。\n" +
                                "下载地址: https://aurora211.github.io/GenShinServerSwitcher/packages/" + fileRequirement.fileName,
                                "文件缺失");
                            return false;
                        }
                        if (fileRequirement.fileMd5 != GetFileMd5(Environment.CurrentDirectory + "\\assets\\" + fileRequirement.fileName))
                            throw new Exception("目标文件 " + fileRequirement.fileName + " 可能被修改");
                        if (File.Exists(fileRequirement.filePath.Replace("%GAMEPATH%", programJson.game)))
                            if (MessageBox.Show("是否覆盖已存在的同名文件\n" + fileRequirement.filePath, "覆盖", MessageBoxButton.YesNo) == MessageBoxResult.No)
                                return true;
                        File.Copy(Environment.CurrentDirectory + "\\assets\\" + fileRequirement.fileName, fileRequirement.filePath.Replace("%GAMEPATH%", programJson.game), true);
                        break;
                    case "remove":
                        if (!File.Exists(fileRequirement.filePath.Replace("%GAMEPATH%", programJson.game)))
                            return true;
                        if (fileRequirement.fileMd5 != GetFileMd5(fileRequirement.filePath.Replace("%GAMEPATH%", programJson.game)))
                            if (MessageBox.Show("待删除文件可能经过更新是否继续删除\n" + fileRequirement.filePath, "删除", MessageBoxButton.YesNo) == MessageBoxResult.No)
                                return true;
                        File.Delete(fileRequirement.filePath.Replace("%GAMEPATH%", programJson.game));
                        break;
                    default:
                        throw new Exception("出现未知操作 " + fileRequirement.method);
                }
            }
            return true;
        }

        /// <summary>
        /// 计算文件MD5哈希值
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        private string GetFileMd5(string path)
        {
            string md5Str = string.Empty;
            try
            {
                using (FileStream fileStream = File.OpenRead(path))
                {
                    MD5 md5 = MD5.Create();
                    byte[] md5Bytes = md5.ComputeHash(fileStream);
                    md5Str = BitConverter.ToString(md5Bytes).Replace("-","").ToUpper();
                }
            }
            catch (Exception ex)
            {
                MainAlertText(ex.Message);
            }
            return md5Str;
        }
    }
}
