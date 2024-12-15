using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using Windows.Storage;

using OsuServerLoader.Services;

namespace OsuServerLoader.Pages
{
    public sealed partial class WelcomePage : Page
    {
        private bool allInitializated = false;

        DataService dataService = new DataService();
        ConfigService configService = new ConfigService();
        Services.UiSettings uiConfig;

        public WelcomePage()
        {
            this.InitializeComponent();
            uiConfig = configService.Load();

            if (uiConfig.osuPath == "")
            {
                SettingsCardOsu.Description = "Not initializated!";
            }
            else { SettingsCardOsu.Description = uiConfig.osuPath; }
            TextBoxLabel.Text = uiConfig.currentLabel;
            TextBoxDevflag.Text = uiConfig.currentDevflag;
            TextBoxNickname.Text = uiConfig.currentNickname;
            PasswordBoxAccount.Password = uiConfig.currentPassword;

            allInitializated = true;
        }

        private async void ButtonSelectOsuHandler(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new();
            folderPicker.FileTypeFilter.Add("*");

            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.m_window);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();

            if (folder != null)
            {
                SettingsCardOsu.Description = folder.Path;
                uiConfig.osuPath = folder.Path;
                configService.Save(uiConfig);
            }
        }

        private void TextBoxLabelHandler(object sender, RoutedEventArgs e)
        {
            if (allInitializated)
            {
                uiConfig.currentLabel = TextBoxLabel.Text;
                configService.Save(uiConfig);
            }
        }

        private void TextBoxDevflagHandler(object sender, RoutedEventArgs e)
        {
            if (allInitializated)
            {
                uiConfig.currentDevflag = TextBoxDevflag.Text;
                configService.Save(uiConfig);
            }
        }

        private void TextBoxNicknameHandler(object sender, RoutedEventArgs e)
        {
            if (allInitializated)
            {
                uiConfig.currentNickname = TextBoxNickname.Text;
                configService.Save(uiConfig);
            }
        }

        private void PasswordBoxAccountHandler(object sender, RoutedEventArgs e)
        {
            if (allInitializated)
            {
                uiConfig.currentPassword = PasswordBoxAccount.Password;
                configService.Save(uiConfig);
            }
        }

        private async void ButtonFinishHandler(object sender, RoutedEventArgs e)
        {
            if (uiConfig.currentNickname == "" || uiConfig.currentPassword == "" || uiConfig.currentLabel == "" || uiConfig.currentDevflag == "" || uiConfig.osuPath == "")
            {
                ContentDialog dialog = new ContentDialog();
                dialog.XamlRoot = this.XamlRoot;
                dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
                dialog.Title = "Some values are empty!";
                dialog.PrimaryButtonText = "OK";
                dialog.DefaultButton = ContentDialogButton.Primary;
                dialog.Content = "Please, input some data in the text boxes or select folders.";
                var dialogResultButton = await dialog.ShowAsync();
                return;
            }
            else
            {
                Services.Server account = new Server();
                account.label = uiConfig.currentLabel;
                account.devflag = uiConfig.currentDevflag;
                account.nickname = uiConfig.currentNickname;
                account.password = uiConfig.currentPassword;
                dataService.AddRow(account);

                this.Frame.Navigate(typeof(HomePage));
                MainWindow.CurrentInstance.NaviagateTo(0);

                uiConfig.showWelcomePage = false;
                uiConfig.currentLabel = "";
                uiConfig.currentDevflag = "";
                uiConfig.currentNickname = "";
                uiConfig.currentPassword = "";
                configService.Save(uiConfig);
            }
        }
    }
}

