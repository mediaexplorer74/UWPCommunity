﻿using System;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml.Controls;
using UWPCommunity.Views;
using Windows.UI.Xaml;
using Windows.Foundation;
using System.Linq;
using System.Collections.Generic;

namespace UWPCommunity
{
    public static class NavigationManager
    {
        public static Frame PageFrame { get; set; }

        public static void NavigateToDashboard()
        {
            RequestSignIn(typeof(DashboardView));
        }

        public static void NavigateToHome()
        {
            Navigate(typeof(HomeView));
        }

        public static void NavigateToSettings()
        {
            Navigate(typeof(SettingsView));
        }
        public static void NavigateToSettings(SettingsPages page)
        {
            Navigate(typeof(SettingsView), page);
        }

        public static async void RequestSignIn(Type returnToPage)
        {
            if (!Common.IsLoggedIn)
            {
                var privacyPolicyResult = await (new Views.Dialogs.ConfirmPrivacyPolicyDialog().ShowAsync());
                if (privacyPolicyResult != ContentDialogResult.Primary)
                    return;

                PageFrame.Navigate(typeof(LoginView), returnToPage);
            }
            else
                PageFrame.Navigate(returnToPage);
        }

        public async static Task<bool> OpenInBrowser(Uri uri)
        {
            return await Launcher.LaunchUriAsync(uri);
        }
        public async static Task<bool> OpenInBrowser(string url)
        {
            // Wrap in a try-catch block in order to prevent the
            // app from crashing from invalid links.
            // (specifically from project badges)
            try
            {
                return await OpenInBrowser(new Uri(url));
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> OpenDiscordInvite(string inviteCode)
        {
            var quarrelLaunchUri = new Uri("quarrel://invite/" + inviteCode);
            var launchUri = new Uri("https://discord.gg/" + inviteCode);
            switch (await Launcher.QueryUriSupportAsync(quarrelLaunchUri, LaunchQuerySupportType.Uri)) {
                case LaunchQuerySupportStatus.Available:
                    return await Launcher.LaunchUriAsync(quarrelLaunchUri);

                default:
                    return await OpenInBrowser(launchUri);
            }
        }

        public static void Navigate(Type destinationPage)
        {
            PageFrame.Navigate(destinationPage);
        }
        public static void Navigate(Type destinationPage, object parameter)
        {
            PageFrame.Navigate(destinationPage, parameter);
        }

        public static void NavigateToEditProject(object project)
        {
            PageFrame.Navigate(typeof(Views.Subviews.EditProjectView), project);
        }

        public static void NavigateToViewProject(object project)
        {
            PageFrame.Navigate(typeof(Views.Subviews.ProjectDetailsView), project);
        }

        public static void NavigateToReconnect(System.Net.Http.HttpRequestException ex)
        {
            (Window.Current.Content as Frame).Navigate(
                typeof(Views.Subviews.NoInternetPage), ex
            );
        }

        public static void RemovePreviousFromBackStack()
        {
            PageFrame.BackStack.RemoveAt(PageFrame.BackStack.Count - 1);
        }

        public static Tuple<Type, object> ParseProtocol(Uri ptcl)
        {
            Type destination = typeof(HomeView);

            if (ptcl == null)
                return new Tuple<Type, object>(destination, null);

            string path;
            switch (ptcl.Scheme)
            {
                case "http":
                    path = ptcl.ToString().Remove(0, 23);
                    break;

                case "https":
                    path = ptcl.ToString().Remove(0, 24);
                    break;

                case "uwpcommunity":
                    path = ptcl.ToString().Remove(0, ptcl.Scheme.Length + 3);
                    break;

                default:
                    // Unrecognized protocol
                    return new Tuple<Type, object>(destination, null);
            }
            if (path.StartsWith("/"))
                path = path.Remove(0, 1);
            var queryParams = new WwwFormUrlDecoder(ptcl.Query.Replace("\r", String.Empty).Replace("\n", String.Empty))
                .ToDictionary(entry => entry.Name, entry => entry.Value);
            
            PageInfo pageInfo = MainPage.Pages.Find(p => p.Path == path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[0]);
            destination = pageInfo != null ? pageInfo.PageType : typeof(HomeView);
            return new Tuple<Type, object>(destination, queryParams);
        }
        public static Tuple<Type, object> ParseProtocol(string url)
        {
            return ParseProtocol(String.IsNullOrWhiteSpace(url) ? null : new Uri(url));
        }
    }

    public class PageInfo
    {
        public PageInfo() {}
        
        public PageInfo(string title, string subhead, IconElement icon)
        {
            Title = title;
            Subhead = subhead;
            Icon = icon;
        }

        public PageInfo(NavigationViewItem navItem)
        {
            Title = (navItem.Content == null) ? "" : navItem.Content.ToString();
            Icon = (navItem.Icon == null) ? new SymbolIcon(Symbol.Document) : navItem.Icon;
            Visibility = navItem.Visibility;

            var tooltip = ToolTipService.GetToolTip(navItem);
            Tooltip = (tooltip == null) ? "" : tooltip.ToString();
        }

        public string Title { get; set; }
        public string Subhead { get; set; }
        public IconElement Icon { get; set; }
        public Type PageType { get; set; }
        public string Path { get; set; }
        public string Tooltip { get; set; }
        public Visibility Visibility { get; set; } = Visibility.Visible;

        // Derived properties
        public NavigationViewItem NavViewItem {
            get {
                var item = new NavigationViewItem()
                {
                    Icon = Icon,
                    Content = Title,
                    Visibility = Visibility
                };
                ToolTipService.SetToolTip(item, new ToolTip() { Content = Tooltip });

                return item;
            }
        }
        public string Protocol {
            get {
                return "uwpcommunity://" + Path;
            }
        }
        public Uri IconAsset {
            get {
                return new Uri("ms-appx:///Assets/Icons/" + Path + ".png");
            }
        }
    }

    public enum SettingsPages
    {
        General,
        AppMessages,
        About,
        Debug
    }
}
