﻿using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Provider;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace UWPCommunity.Controls
{
    public sealed partial class GridViewCardItem : UserControl, IInvokeProvider
    {
        public GridViewCardItem()
        {
            UseBackdropBlur = SettingsManager.GetUseBlurEffects();
            SettingsManager.UseBlurEffectsChanged += SettingsManager_UseBlurEffectsChanged;
            this.InitializeComponent();
            //DataContextChanged += (sender, args) =>
            //{
            //    if (args.NewValue != null) Bindings.Update();
            //};
        }

        private void SettingsManager_UseBlurEffectsChanged(bool value)
        {
            UseBackdropBlur = value;
        }

        #region Access Options
        public bool IsEditable
        {
            get => (bool)GetValue(IsEditableProperty);
            set
            {
                SetValue(IsEditableProperty, value);
                EditButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                EditMenuButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register(nameof(IsEditable), typeof(bool), typeof(GridViewCardItem), null);

        public bool IsDeletable
        {
            get => (bool)GetValue(IsDeletableProperty);
            set
            {
                SetValue(IsDeletableProperty, value);
                DeleteButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                DeleteMenuButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public static readonly DependencyProperty IsDeletableProperty =
            DependencyProperty.Register(nameof(IsDeletable), typeof(bool), typeof(GridViewCardItem), null);
        #endregion

        #region Content
        public string TitleText
        {
            get => (string)GetValue(TitleTextProperty);
            set => SetValue(TitleTextProperty, value);
        }
        public static readonly DependencyProperty TitleTextProperty = DependencyProperty.Register(
            nameof(TitleText), typeof(string), typeof(GridViewCardItem), new PropertyMetadata(string.Empty));

        public Visibility TitleTextVisibility
        {
            get => (Visibility)GetValue(TitleTextVisibilityProperty);
            set => SetValue(TitleTextVisibilityProperty, value);
        }
        public static readonly DependencyProperty TitleTextVisibilityProperty = DependencyProperty.Register(
            nameof(TitleTextVisibility), typeof(Visibility), typeof(GridViewCardItem), new PropertyMetadata(Visibility.Visible));

        public string BodyText
        {
            get => (string)GetValue(BodyTextProperty);
            set => SetValue(BodyTextProperty, value);
        }
        public static readonly DependencyProperty BodyTextProperty = DependencyProperty.Register(
            nameof(BodyText), typeof(string), typeof(GridViewCardItem), new PropertyMetadata(string.Empty));

        public Visibility BodyTextVisibility
        {
            get => (Visibility)GetValue(BodyTextVisibilityProperty);
            set => SetValue(BodyTextVisibilityProperty, value);
        }
        public static readonly DependencyProperty BodyTextVisibilityProperty = DependencyProperty.Register(
            nameof(BodyTextVisibility), typeof(Visibility), typeof(GridViewCardItem), new PropertyMetadata(Visibility.Visible));

        public ImageSource ImageSource
        {
            get => (ImageSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            nameof(ImageSource), typeof(ImageSource), typeof(GridViewCardItem), null);

        public object BadgeContent
        {
            get => (object)GetValue(BadgeContentProperty);
            set => SetValue(BadgeContentProperty, value);
        }
        public static readonly DependencyProperty BadgeContentProperty = DependencyProperty.Register(
            nameof(BadgeContent), typeof(object), typeof(GridViewCardItem), null);

        public bool UseBackdropBlur
        {
            get => (bool)GetValue(UseBackdropBlurProperty);
            set => SetValue(UseBackdropBlurProperty, value);
        }
        public static readonly DependencyProperty UseBackdropBlurProperty = DependencyProperty.Register(
            nameof(UseBackdropBlur), typeof(bool), typeof(GridViewCardItem), new PropertyMetadata(true));
        #endregion

        #region Events
        public delegate void EditRequestedHandler(object p);
        public event EditRequestedHandler EditRequested;
        private void EditButton_Click(object sender, RoutedEventArgs args)
        {
            EditRequested?.Invoke(DataContext);            
        }

        public delegate void DeleteRequestedHandler(object p);
        public event DeleteRequestedHandler DeleteRequested;
        private void DeleteButton_Click(object sender, RoutedEventArgs args)
        {
            DeleteRequested?.Invoke(DataContext);            
        }

        public delegate void ViewRequestedHandler(object p);
        public event ViewRequestedHandler ViewRequested;
        private void ViewButton_Click(object sender, RoutedEventArgs args)
        {
            ViewRequested?.Invoke(DataContext);            
        }

        public void Invoke()
        {
            ViewRequested?.Invoke(DataContext);            
        }
        #endregion
    }
}
