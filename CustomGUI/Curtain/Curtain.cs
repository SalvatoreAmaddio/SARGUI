using SARModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SARGUI.CustomGUI
{
    /// <summary>
    /// How to instantiate this object in the XAML:
    /// <code>
    /// &lt;Grid x:Name="MainGrid">
    /// &lt;Grid.RowDefinitions>
    /// &lt;RowDefinition Height = "40"/>
    /// &lt;RowDefinition Height="*"/>
    /// &lt;/Grid.RowDefinitions>
    /// &lt;Menu x:Name="menu" Margin="20.1,0,0,0">
    /// ...
    /// &lt;/Menu>
    /// &lt;wpf:Curtain Height="{Binding ElementName=menu, Path=ActualHeight}" Row1Height="yourValue" SoftwareInfo="{Binding RelativeSource={RelativeSource AncestorType=Window}, Path=SoftwareInfoObject}"/>
    /// &lt;TabControl Grid.Row="1">
    /// ...
    /// &lt;/TabControl>
    /// &lt;/Grid>
    /// </code>
    /// <include file='../SARModel/Docs.xml' path='docs/author'/>
    /// </summary>
    public class Curtain : Grid
    {
        private bool DropDownIsOpen { get; set; } = false;
        private double ExpandTo { get; set; } = 200;
        private readonly string show;
        private readonly string hide;
        private const double initWidth = 20;
        private const double initHeight = 33;
        private readonly RowDefinition row1 = new();
        private readonly CurtainContent curtainContent = new();
        private readonly Thickness DefaultPadding = new(0);
        private readonly Thickness AnimatedPadding = new(0,0,10,0);
        private static SolidColorBrush DefaultBackground { get => new(View.GetColor("#FFF0F0F0")); }
        private readonly Button switcherButton = new()
        {
            BorderThickness = new(0),
            HorizontalAlignment = HorizontalAlignment.Right, 
            ToolTip="Info",
            Background = DefaultBackground
        };

        #region Row1Height
        public static readonly DependencyProperty Row1HeightProperty
        = View.Binder.Register<GridLength, Curtain>(nameof(Row1Height), true, new(33), null, true, true, true);
        
        public GridLength Row1Height 
        {
            get => (GridLength)GetValue(Row1HeightProperty);
            set=>SetValue(Row1HeightProperty,value);
        }
        #endregion

        #region SoftwareInfo
        public static readonly DependencyProperty SoftwareInfoProperty
        = View.Binder.Register<SoftwareInfo, Curtain>(nameof(SoftwareInfo), true, null, (d,e)=> ((Curtain)d).SetInfo(), true, true, true);

        public SoftwareInfo SoftwareInfo 
        {
            get => (SoftwareInfo) GetValue(SoftwareInfoProperty);
            set => SetValue(SoftwareInfoProperty, this);
        }
        #endregion

        public Curtain() 
        {
            show = View.GetResource<string>("First");
            hide = View.GetResource<string>("Last");
            switcherButton.Content = hide;
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
            Background = Brushes.White;
            View.Binder.BindUp(this, nameof(Row1Height), row1, RowDefinition.HeightProperty);
            RowDefinitions.Add(row1);
            RowDefinitions.Add(new RowDefinition { Height = new(1,GridUnitType.Star) });
            switcherButton.Click += ToggleButtonClicked;
            Children.Add(switcherButton);
            Children.Add(curtainContent);
            SetZIndex(this,1);
            Width = initWidth;
            Height = initHeight;
            SetRow(this,0);
            SetRowSpan(this,2);
            SetRow(curtainContent,1);
            curtainContent.Visibility = Visibility.Hidden;
            InputManager.Current.PreProcessInput += (sender, e) =>
            {
                if (e.StagingItem.Input is MouseButtonEventArgs args) 
                {
                    if (!IsMouseOver && DropDownIsOpen)
                        ToggleButtonClicked(sender, new());
                }
            };
        }

        private void SetInfo() 
        {
            curtainContent.SoftwareName = SoftwareInfo.SoftwareName;
            curtainContent.ClientName = SoftwareInfo.ClientName;
            curtainContent.Year = SoftwareInfo.Year;
            curtainContent.Version = SoftwareInfo.Version;
        }

        /// <summary>
        /// This method loop through all its ancestors until it finds the Window.
        /// <para>Once found</para>
        /// <include file='../SARModel/Docs.xml' path='docs/author'/>
        /// </summary>
        /// <returns>The actual Window height that will be used by the Curtain to strech all the way down.</returns>
        /// <exception cref="Exception">Failed to find Window Parent</exception>
        private double ParentWindowMeasurements()
        {
            bool found = false;
            FrameworkElement baseObject=this;

            while (!found)
            {
                var parent = baseObject.Parent;
                bool IsWidow = parent is Window;

                if (IsWidow)
                {
                    Border border = (Border)Children[1];
                    return
                        ((FrameworkElement)parent).ActualHeight
                        - RowDefinitions[0].ActualHeight
                        - border.Margin.Top - border.Margin.Bottom
                        - border.BorderThickness.Top - border.BorderThickness.Bottom;
                }

                baseObject = (FrameworkElement)parent;
            }

            throw new Exception("Failed to find Window Parent");
        }
        private DoubleAnimation CreateAnimation()
        {
            DoubleAnimation animation = new()
            {
                Duration = new(new TimeSpan(0, 0, 0, 0, 250)),
                EasingFunction = new ExponentialEase()
            };
            animation.Completed += (object? sender, EventArgs e) =>
            {
                switch(DropDownIsOpen) 
                {
                    case true:
                        switcherButton.Content = show;
                        switcherButton.Background = Brushes.Transparent;
                        switcherButton.Padding = AnimatedPadding;
                    break;
                    case false:
                        switcherButton.Content = hide;
                        switcherButton.Background = DefaultBackground;
                        switcherButton.Padding = DefaultPadding;
                        break;
                }
            };

            return animation;
        }

        private void ToggleButtonClicked(object sender, RoutedEventArgs e) {
            DoubleAnimation heightAnimation = CreateAnimation();
            DoubleAnimation widthAnimation = CreateAnimation();

            switch (DropDownIsOpen) 
            {
                case true:
                    widthAnimation.To = initWidth;
                    heightAnimation.To = Row1Height.Value;
                    curtainContent.Visibility = Visibility.Hidden;
                break;
                case false:
                    curtainContent.Visibility = Visibility.Visible;
                    widthAnimation.To = ExpandTo;
                    heightAnimation.To = ParentWindowMeasurements();
                break;
            }

             DropDownIsOpen = !DropDownIsOpen;
             BeginAnimation(WidthProperty, widthAnimation);
             BeginAnimation(HeightProperty, heightAnimation);
        }    
    }
}