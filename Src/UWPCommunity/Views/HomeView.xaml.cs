﻿using System;
using System.Diagnostics;
using UWPCommLib.Api.UWPComm.Models;
using UWPCommunity.Views.Dialogs;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPCommunity.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomeView : Page
    {
        public HomeView()
        {
            this.InitializeComponent();
            Loaded += HomeView_Loaded;

            ShowLatestAppMessage();
        }

        private async void HomeView_Loaded(object sender, RoutedEventArgs e)
        {
            // Get the card information from the website frontend
            var response = await new System.Net.Http.HttpClient().GetAsync("https://raw.githubusercontent.com/UWPCommunity/uwpcommunity.github.io/master/assets/views/home.json");
            string json = await response.Content.ReadAsStringAsync();
            var card = Newtonsoft.Json.JsonConvert.DeserializeObject<CardInfoResponse>(json).Main;
            CardSubtitle.Text = card.Subtitle;
            CardDetails.Text = String.Join(" ", card.Details);

            SettingsManager.ApplyLiveTile(SettingsManager.GetShowLiveTile());
        }

        private async void DiscordButton_Click(object sender, RoutedEventArgs e)
        {
            await NavigationManager.OpenDiscordInvite("eBHZSKG");
        }

        private void Launch2020Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationManager.Navigate(typeof(LaunchView));
        }

        private async void GitHubButton_Click(object sender, RoutedEventArgs e)
        {
            await NavigationManager.OpenInBrowser("https://github.com/UWPCommunity/");
        }

        private async void Launch2019Button_Click(object sender, RoutedEventArgs e)
        {
            await NavigationManager.OpenInBrowser("https://medium.com/@Arlodottxt/launch-2019-7efd37cc0877");
        }

        private async void ShowLatestAppMessage()
        {
            int level = SettingsManager.AppMessageSettings.GetImportanceLevel();
            if (level == 0)
                return;

            // Load most recent app message
            UWPCommLib.Api.Yoshi.Models.AppMessage message = default;

            try
            {
                message = (await Common.YoshiApi.GetAppMessages("UWPCommunity", 0))[0];
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ex] HomeView - YoshiApi.GetAppMessages Exception: " + ex.Message);
            }

            if (message != null)
            {
                if (message.Id != SettingsManager.AppMessageSettings.GetLastAppMessageId()
                    && message.Importance <= level)
                {
                    await new AppMessageDialog(message).ShowAsync();
                    SettingsManager.AppMessageSettings.SetLastAppMessageId(message.Id);
                }
            }
        }
    }
}
