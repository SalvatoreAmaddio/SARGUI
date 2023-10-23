using SARGUI;
using SARGUI.Converters;
using SARModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace SARGUI.CustomGUI
{
    public class LoadingMask : Window
    {
        #region BackProps
        Image DefaultBorderImage { get; set; } = new();
        Grid DefaultMainGrid { get; set; } = new();
        ProgressBar ProgressBar { get; set; } = new();
        Border ImageBorder { get; set; } = new();
        RowDefinition DefaultRow1 { get; set; } = new();
        RowDefinition DefaultRow2 { get; set; } = new();
        RowDefinition DefaultRow3 { get; set; } = new();
        #endregion

        #region RequiresInternetConnectionProperty
        public static readonly DependencyProperty RequiresInternetConnectionProperty
        = View.Binder.Register<bool, LoadingMask>(nameof(RequiresInternetConnection), true, true, null, true, true);

        public bool RequiresInternetConnection
        {
            get => (bool)GetValue(RequiresInternetConnectionProperty);
            set => SetValue(RequiresInternetConnectionProperty, value);
        }
        #endregion

        #region MainWindowProperty
        public static readonly DependencyProperty MainWindowProperty
        = View.Binder.Register<Window, LoadingMask>(nameof(MainWindow), true, null, null, true, true);

        public Window MainWindow
        {
            get => (Window)GetValue(MainWindowProperty);
            set => SetValue(MainWindowProperty, value);
        }
        #endregion

        #region ImageBorderPaddingProperty
        public static readonly DependencyProperty ImageBorderMarginProperty
        = View.Binder.Register<Thickness, LoadingMask>(nameof(ImageBorderMargin), true, new(0, 0, 0, 5), null, true, true);

        public Thickness ImageBorderMargin
        {
            get => (Thickness)GetValue(ImageBorderMarginProperty);
            set => SetValue(ImageBorderMarginProperty, value);
        }
        #endregion

        #region ImageBorderPaddingProperty
        public static readonly DependencyProperty ImageBorderPaddingProperty
        = View.Binder.Register<Thickness, LoadingMask>(nameof(ImageBorderPadding), true, new(5), null, true, true);

        public Thickness ImageBorderPadding
        {
            get => (Thickness)GetValue(ImageBorderPaddingProperty);
            set => SetValue(ImageBorderPaddingProperty, value);
        }
        #endregion

        #region ImageBorderThicknessProperty
        public static readonly DependencyProperty ImageBorderThicknessProperty
        = View.Binder.Register<Thickness, LoadingMask>(nameof(ImageBorderThickness), true, new(1), null, true, true);

        public Thickness ImageBorderThickness
        {
            get => (Thickness)GetValue(ImageBorderThicknessProperty);
            set => SetValue(ImageBorderThicknessProperty, value);
        }
        #endregion

        #region MainGridMarginProperty
        public static readonly DependencyProperty MainGridMarginProperty
        = View.Binder.Register<Thickness, LoadingMask>(nameof(MainGridMargin), true, new(5), null, true, true);

        public Thickness MainGridMargin
        {
            get => (Thickness)GetValue(MainGridMarginProperty);
            set => SetValue(MainGridMarginProperty, value);
        }
        #endregion

        #region ImageBorderCornerRadiusProperty
        public static readonly DependencyProperty ImageBorderCornerRadiusProperty
        = View.Binder.Register<CornerRadius, LoadingMask>(nameof(ImageBorderCornerRadius), true, new(10), null, true, true);

        public CornerRadius ImageBorderCornerRadius
        {
            get => (CornerRadius)GetValue(ImageBorderCornerRadiusProperty);
            set => SetValue(ImageBorderCornerRadiusProperty, value);
        }
        #endregion

        #region ImageBorderBrushProperty
        public static readonly DependencyProperty ImageBorderBrushProperty
        = View.Binder.Register<Brush, LoadingMask>(nameof(ImageBorderBrush), true, Brushes.Black, null, true, true);

        public Brush ImageBorderBrush
        {
            get => (Brush)GetValue(ImageBorderBrushProperty);
            set => SetValue(ImageBorderBrushProperty, value);
        }
        #endregion

        #region ImageBorderBackgroundProperty
        public static readonly DependencyProperty ImageBorderBackgroundProperty
        = View.Binder.Register<Brush, LoadingMask>(nameof(ImageBorderBackground), true, Brushes.White, null, true, true);

        public Brush ImageBorderBackground
        {
            get => (Brush)GetValue(ImageBorderBackgroundProperty);
            set => SetValue(ImageBorderBackgroundProperty, value);
        }
        #endregion

        #region ImageSourceLogoProperty
        public static readonly DependencyProperty ImageLogoSourceProperty
        = View.Binder.Register<ImageSource, LoadingMask>(nameof(ImageSourceLogo), true, null, null, true, true);
        public ImageSource ImageSourceLogo
        {
            get => (ImageSource)GetValue(ImageLogoSourceProperty);
            set => SetValue(ImageLogoSourceProperty, value);
        }
        #endregion

        #region Row1HeightProperty
        public static readonly DependencyProperty Row1HeightProperty
        = View.Binder.Register<GridLength, LoadingMask>(nameof(Row1Height), true, new(1, GridUnitType.Star), null, true, true, true);

        public GridLength Row1Height
        {
            get => (GridLength)GetValue(Row1HeightProperty);
            set => SetValue(Row1HeightProperty, value);
        }
        #endregion

        #region Row2HeightProperty
        public static readonly DependencyProperty Row2HeightProperty
        = View.Binder.Register<GridLength, LoadingMask>(nameof(Row2Height), true, new(50), null, true, true, true);

        public GridLength Row2Height
        {
            get => (GridLength)GetValue(Row2HeightProperty);
            set => SetValue(Row2HeightProperty, value);
        }
        #endregion

        #region Row3HeightProperty
        public static readonly DependencyProperty Row3HeightProperty
        = View.Binder.Register<GridLength, LoadingMask>(nameof(Row3Height), true, new(20), null, true, true, true);

        public GridLength Row3Height
        {
            get => (GridLength)GetValue(Row3HeightProperty);
            set => SetValue(Row3HeightProperty, value);
        }
        #endregion

        #region UseInternetProperty
        public static readonly DependencyProperty UseInternetProperty = View.Binder.Register<bool, LoadingMask>(nameof(UseInternet), true, true, (d, e) => InternetConnection.UseIternet = (bool)e.NewValue, true, true, true);
        public bool UseInternet
        {
            get => (bool)GetValue(UseInternetProperty);
            set => SetValue(UseInternetProperty, value);
        }
        #endregion

        readonly Task SettingPrinterTask;
        readonly Task SettingCultureTask;
        readonly Task KeepTryingToConnectTask;

        public LoadingMask() 
        {
            InternetConnection.Availability =
            (value) => Application.Current?.Dispatcher?.Invoke(new (() => InternetConnection.Call(value)));
                        
            InternetConnection.WhileChecking =
            (value) => Application.Current?.Dispatcher?.Invoke(new (() => InternetConnection.Call(value)));

            KeepTryingToConnectTask = InternetConnection.TaskTryUntilConnect();
            SettingCultureTask = View.CultureManager.SetCulture();
            SettingPrinterTask = PrinterManager.SetPrinter();

            Title = "Welcome";   
            Height = 450;
            Width = 450;

            ResizeMode=ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            DefaultMainGrid.RowDefinitions.Add(DefaultRow1);
            DefaultMainGrid.RowDefinitions.Add(DefaultRow2);
            DefaultMainGrid.RowDefinitions.Add(DefaultRow3);

            SetProgressBar();
            CreateImageBorder();
            AddNoIternetLayer();
            AddChild(DefaultMainGrid);
            Loaded += OnLoading;

            View.Binder.BindUp(this, nameof(ImageSourceLogo),DefaultBorderImage,Image.SourceProperty);
            View.Binder.BindUp(this, nameof(Row1Height), DefaultRow1, RowDefinition.HeightProperty);
            View.Binder.BindUp(this, nameof(Row2Height), DefaultRow2, RowDefinition.HeightProperty);
            View.Binder.BindUp(this, nameof(Row3Height), DefaultRow3, RowDefinition.HeightProperty);

            View.Binder.BindUp(this, nameof(Row2Height), ProgressBar, ProgressBar.IsIndeterminateProperty, BindingMode.TwoWay, new GridLengthToBooleanConverter());

            View.Binder.BindUp(this, nameof(MainGridMargin), DefaultMainGrid, Grid.MarginProperty);
            
            View.Binder.BindUp(this, nameof(ImageBorderBackground), ImageBorder, Border.BackgroundProperty);
            View.Binder.BindUp(this, nameof(ImageBorderBrush), ImageBorder, Border.BorderBrushProperty);
            View.Binder.BindUp(this, nameof(ImageBorderCornerRadius), ImageBorder, Border.CornerRadiusProperty);
            View.Binder.BindUp(this, nameof(ImageBorderThickness), ImageBorder, Border.BorderThicknessProperty);
            View.Binder.BindUp(this, nameof(ImageBorderPadding), ImageBorder, Border.PaddingProperty);
            View.Binder.BindUp(this, nameof(ImageBorderMargin), ImageBorder, Border.MarginProperty);

        }

        void SetProgressBar()
        {
            ProgressBar.IsIndeterminate = true;
            DefaultMainGrid.Children.Add(ProgressBar);
            Grid.SetRow(ProgressBar, 2);
        }

        void AddNoIternetLayer()
        {
            NoWifiStackPanel stack = new() { HorizontalAlignment=HorizontalAlignment.Center};

            Border NoInternetBorder = new()
            {
                Background = ImageBorderBackground,
                BorderBrush = ImageBorderBrush,
                CornerRadius = ImageBorderCornerRadius,
                BorderThickness = ImageBorderThickness,
                Padding = new Thickness(3),
                Margin = ImageBorderMargin,
                Child = stack,
            };
            
            DefaultMainGrid.Children.Add(NoInternetBorder);
            Grid.SetRow(NoInternetBorder, 1);
        }

        void CreateImageBorder()
        {
            ImageBorder = new()
            {
                Background = ImageBorderBackground,
                BorderBrush = ImageBorderBrush,
                CornerRadius = ImageBorderCornerRadius,
                BorderThickness = ImageBorderThickness,
                Padding = ImageBorderPadding,
                Margin = ImageBorderMargin,
                Child = DefaultBorderImage,
            };
            DefaultMainGrid.Children.Add(ImageBorder);
            Grid.SetRow(ImageBorder, 0);
        }

        private async void OnLoading(object sender, RoutedEventArgs e)
        {
            await Task.WhenAll(
                (UseInternet) ?
                new List<Task>() { SettingCultureTask, KeepTryingToConnectTask, SettingPrinterTask }
                : new List<Task>() { SettingCultureTask, SettingPrinterTask }
                );

            if (UseInternet) _ = InternetConnection.CheckingInternetConnection();
            Row2Height = new(0);

            await DatabaseManager.FecthDatabaseTablesData();
            await Task.Delay(1000);
            Visibility = Visibility.Hidden;
            MainWindow.Show();
            Close();
        }
    }
}
