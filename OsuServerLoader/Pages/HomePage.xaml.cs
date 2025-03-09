using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using OsuServerLoader;
using OsuServerLoader.Services;
using OsuServerLoader.Tools;

namespace OsuServerLoader.Pages
{
    public sealed partial class HomePage : Page
    {
        private bool allInitializated = false;

        ConfigService configService = new ConfigService();
        DataService dataService = new DataService();
        FileEdit fileEditTool = new FileEdit();

        Services.UiSettings uiConfig;
        List<Server> servers = new List<Server>();


        public HomePage()
        {
            this.InitializeComponent();
            uiConfig = configService.Load();
            servers = dataService.LoadServers();

            foreach (Server server in servers)
            {
                Classes.ComboBoxItem comboBoxItem = new Classes.ComboBoxItem();
                comboBoxItem.Text = server.label;
                ComboBoxServers.Items.Add(comboBoxItem);
            }

            if (servers.Count > 0)
            {
                ButtonPlay.IsEnabled = true;
                ButtonEdit.IsEnabled = true;
                ButtonDelete.IsEnabled = true;
                ComboBoxServers.IsEnabled = true;
                ComboBoxServers.SelectedIndex = uiConfig.serverIndex;

                uiConfig.selectedLabel = ComboBoxServers.Items[uiConfig.serverIndex].ToString();
                configService.Save(uiConfig);
            }

            ToggleSwitchPatcher.IsOn = uiConfig.usePatcher;
            ToggleSwitchAccount.IsOn = uiConfig.useAccount;

            allInitializated = true;
        }

        private void ComboBoxServersHandler(object sender, SelectionChangedEventArgs e)
        {
            if (allInitializated)
            {
                uiConfig.serverIndex = ComboBoxServers.SelectedIndex;
                uiConfig.selectedLabel = ComboBoxServers.Items[uiConfig.serverIndex].ToString();
                configService.Save(uiConfig);
            }
        }

        private void ToggleSwitchPatcherHandler(object sender, RoutedEventArgs e)
        {
            if (ToggleSwitchPatcher.IsOn == true)
            {
                uiConfig.usePatcher = true;
            }
            else
            {
                uiConfig.usePatcher = false;
            }
            configService.Save(uiConfig);
        }

        private void ToggleSwitchAccountHandler(object sender, RoutedEventArgs e)
        {
            if (ToggleSwitchAccount.IsOn == true)
            {
                uiConfig.useAccount = true;
            }
            else
            {
                uiConfig.useAccount = false;
            }
            configService.Save(uiConfig);
        }

        private async void ButtonAddHandler(object sender, RoutedEventArgs e)
        {
            uiConfig.serverIndex = 0;
            configService.Save(uiConfig);
            MainWindow.CurrentInstance.HeaderChange("Add server");
            this.Frame.Navigate(typeof(AddPage));
        }

        private async void ButtonEditHandler(object sender, RoutedEventArgs e)
        {
            uiConfig.serverIndex = 0;

            Server server = dataService.GetServer(uiConfig.selectedLabel);
            uiConfig.currentLabel = server.label;
            uiConfig.currentDevflag = server.devflag;
            uiConfig.currentNickname = server.nickname;
            uiConfig.currentPassword = server.password;

            configService.Save(uiConfig);

            MainWindow.CurrentInstance.HeaderChange("Edit server");
            this.Frame.Navigate(typeof(EditPage));
        }

        private async void ButtonDeleteHandler(object sender, RoutedEventArgs e)
        {
            dataService.DeleteRow(uiConfig.selectedLabel);

            uiConfig.serverIndex = 0;
            configService.Save(uiConfig);

            this.Frame.Navigate(typeof(HomePage));
        }

        private async void ButtonPlayHandler(object sender, RoutedEventArgs e)
        {
            ButtonPlay.Content = "Starting osu!...";
            ButtonPlay.IsEnabled = false;

            Server server = dataService.GetServer(uiConfig.selectedLabel);
            string osuFolderPath = uiConfig.osuPath;
            string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string username = Environment.UserName;

            if (uiConfig.useAccount)
            {
                fileEditTool.ChangeAccountForOsu((osuFolderPath + "\\osu!." + username + ".cfg"), server.nickname, server.password, server.devflag);
            }          

            await Task.Delay(1000);

            var osuProcessHandler = new Process();
            osuProcessHandler.StartInfo.FileName = osuFolderPath + "\\osu!.exe";
            osuProcessHandler.StartInfo.Arguments = "-devserver " + server.devflag;
            osuProcessHandler.Start();

            if (ToggleSwitchPatcher.IsOn == true && osuProcessHandler.StartInfo.Arguments != "-devserver" && osuProcessHandler.StartInfo.Arguments != "-devserver ppy.sh")
            {
                string patcherpath = userFolderPath + "\\.OsuServerLoader\\Osu!Patcher";
                if(!Directory.Exists(patcherpath))
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile("http://osuokayu.moe/static/Osu!Patcher.zip", userFolderPath + "\\.OsuServerLoader\\Osu!Patcher.zip");
                        System.IO.Compression.ZipFile.ExtractToDirectory(userFolderPath + "\\.OsuServerLoader\\Osu!Patcher.zip", userFolderPath + "\\.OsuServerLoader");
                        File.Delete(userFolderPath + "\\.OsuServerLoader\\Osu!Patcher.zip");
                    }
                }
                await Task.Delay(1000);
                var patcherProcessHandler = new Process();
                patcherProcessHandler.StartInfo.FileName = patcherpath + "\\osu!.patcher.exe";
                patcherProcessHandler.Start();
            }

            if (uiConfig.showExitDialog)
            {
                ContentDialog dialog = new ContentDialog();
                dialog.XamlRoot = this.XamlRoot;
                dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
                dialog.Title = "Osu Server Loader finished its work!";
                dialog.PrimaryButtonText = "OK";
                dialog.SecondaryButtonText = "Don't show this window again";
                dialog.DefaultButton = ContentDialogButton.Primary;
                dialog.Content = "You can close this window by clicking \"Ok\" button. Osu! will be running without loader. Thank you for using my loader!";
                var dialogResultButton = await dialog.ShowAsync();

                if (dialogResultButton == ContentDialogResult.Primary)
                {
                    ButtonPlay.Content = "Run osu!";
                    ButtonPlay.IsEnabled = true;
                    uiConfig.showWelcomePage = false;
                    configService.Save(uiConfig);
                    return;
                }
                else if (dialogResultButton == ContentDialogResult.Secondary)
                {
                    uiConfig.showWelcomePage = false;
                    uiConfig.showExitDialog = false;
                    configService.Save(uiConfig);
                }
            }

            uiConfig.showWelcomePage = false;
            configService.Save(uiConfig);

            await Task.Delay(100);
            Process.GetCurrentProcess().Kill();
        }
    }
}
