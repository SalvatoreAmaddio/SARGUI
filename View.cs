using Microsoft.Win32;
using MvvmHelpers.Commands;
using SARGUI.CustomGUI;
using SARModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Frame = System.Windows.Controls.Frame;

namespace SARGUI {
    public static class View 
    {
        public static IView GetTabView(this TabControl tab) 
        {
            TabItem tabItem = (TabItem)tab.SelectedItem;
            Frame frame = (Frame)tabItem.Content;
            return (IView)frame.Content;
        }

        #region ImageStorageManager 
        public class ImageStorageManager : AbstractNotifier
        {
            #region Vars
            private readonly string appPath;
            private string imgOriginalPath = string.Empty;
            private string destinationImagePath = string.Empty;
            private readonly RemovedImages RemovedImages;
            #endregion

            #region BackProps
            private Visibility _placeholderVisibility = Visibility.Visible;
            private Visibility _buttonVisibility = Visibility.Visible;
            private ImageSource? _imgSrc;
            #endregion

            #region Funcs
            private Func<bool> CurrentRecordIsNull;
            private Func<string?> CurrentRecordImgPath;
            #endregion

            #region Actions
            private Action<bool,string?> CurrentRecordBehaviour;
            private Action<object?> CurrentRecordImgPathSetter;
            #endregion

            #region Props
            private string FolderName { get; set; } = string.Empty;
            public Visibility PlaceholderVisibility { get => _placeholderVisibility; set => Set(ref value, ref _placeholderVisibility); }
            public Visibility ButtonVisibility { get => _buttonVisibility; set => Set(ref value, ref _buttonVisibility); }
            public ImageSource? ImgSrc { get => _imgSrc; set => Set(ref value, ref _imgSrc); }
            #endregion

            #region Commands
            public ICommand PickUpImageCMD { get; }
            public ICommand RemoveImgCMD { get; }
            public ICommand ViewBannerCMD { get; }
            #endregion

            /// <summary>
            /// The folder containing the images will be next to .exe
            /// </summary>
            /// <param name="folderName">Folder to save images in</param>
            /// <param name="currentRecordBehaviour"></param>
            /// <param name="currentRecordImgPath"></param>
            /// <param name="currentRecordIsNull"></param>
            /// <param name="currentRecordImgPathSetter"></param>
            public ImageStorageManager(string folderName, Action<bool,string?> currentRecordBehaviour, Func<string?> currentRecordImgPath, Func<bool> currentRecordIsNull, Action<object?> currentRecordImgPathSetter)
            {
                FolderName = folderName;
                RemovedImages = new(FolderName);
                appPath = Path.GetDirectoryName(Sys.AppLocation) + $@"\{FolderName}\";

                CurrentRecordBehaviour = currentRecordBehaviour;
                CurrentRecordImgPath = currentRecordImgPath;
                CurrentRecordIsNull = currentRecordIsNull;
                CurrentRecordImgPathSetter = currentRecordImgPathSetter;

                PickUpImageCMD = new Command(PickUpImage);
                RemoveImgCMD = new Command(RemoveImg);
                ViewBannerCMD = new Command(ViewBanner);
                AfterUpdate += OnAfterUpdate;
            }
            
            public void ResetActionsAndFunctions(Action<bool, string?> currentRecordBehaviour, Func<string?> currentRecordImgPath, Func<bool> currentRecordIsNull, Action<object?> currentRecordImgPathSetter)
            {
                CurrentRecordBehaviour = currentRecordBehaviour;
                CurrentRecordImgPath = currentRecordImgPath;
                CurrentRecordIsNull = currentRecordIsNull;
                CurrentRecordImgPathSetter = currentRecordImgPathSetter;
            }

            private void OnAfterUpdate(object? sender, AbstractPropChangedEventArgs e)
            {
                if (e.PropIs(nameof(ImgSrc)))
                {
                    if (e.GetNewValue() == null && (!CurrentRecordIsNull.Invoke()))
                        CurrentRecordImgPathSetter.Invoke(null);
                    
                    PlaceholderVisibility = (ImgSrc == null) ? Visibility.Visible : Visibility.Collapsed;
                    return;
                }

                if (e.PropIs(nameof(PlaceholderVisibility)))
                {
                    Visibility? value = (Visibility?)e.GetNewValue();
                    ButtonVisibility = value.Equals(Visibility.Visible) ? Visibility.Hidden : Visibility.Visible;
                    return;
                }
            }

            private static BitmapImage? GenderateBitMapImg(string? newImgPath) => CreateImageSource(newImgPath);

            public void SetImageSource(string? newImgPath, bool IsDirty = true)
            {
                ImgSrc = GenderateBitMapImg(newImgPath);
                CurrentRecordBehaviour.Invoke(IsDirty,newImgPath);
            }

            void RemoveImg()
            {
                string? currentRecordImgPath = CurrentRecordImgPath.Invoke();
                if (CurrentRecordIsNull.Invoke()
                    || ImgSrc == null
                    || string.IsNullOrEmpty(currentRecordImgPath)
                    ) return;
                
                RemovedImages.AddImage(currentRecordImgPath);
                ImgSrc = null;
                PlaceholderVisibility = Visibility.Visible;
            }

            void PickUpImage()
            {
                if (CurrentRecordIsNull.Invoke()) return;
                OpenFileDialog openFileDialog = new()
                {
                    Title = "Choose a picture",
                    Filter = "All supported graphics|*.jpg;*.jpeg;*.png,*.webp|" +
                                             "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                                             "Portable Network Graphic (*.png)|*.png|" +
                                             "Google-designed image format (*.webp)|*.webp"
                };

                if (openFileDialog.ShowDialog() == false) return;

                imgOriginalPath = openFileDialog.FileName;
                var fileNameToSave = openFileDialog.SafeFileName;
                destinationImagePath = Path.Combine(appPath + fileNameToSave);

                if (File.Exists(destinationImagePath))
                {
                    PlaceholderVisibility = Visibility.Hidden;
                    SetImageSource(destinationImagePath);
                    return;
                }

                RemovedImages.AddImage(CurrentRecordImgPath.Invoke());
                PlaceholderVisibility = Visibility.Hidden;
                SetImageSource(imgOriginalPath);
            }

            void StoreImg()
            {
                if (string.IsNullOrEmpty(imgOriginalPath) || string.IsNullOrEmpty(destinationImagePath)) return;
                if (!Directory.Exists(appPath)) Directory.CreateDirectory(appPath);
                if (File.Exists(destinationImagePath)) return;

                try
                {
                    File.Copy(imgOriginalPath, destinationImagePath);
                    imgOriginalPath = string.Empty;
                    destinationImagePath = string.Empty;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Storing Image: An Error Occured");
                    return;
                }
            }

            void ViewBanner()
            {
                string? currentRecordImgPath = CurrentRecordImgPath.Invoke();
                if (CurrentRecordIsNull.Invoke()
                    || ImgSrc == null
                    || string.IsNullOrEmpty(currentRecordImgPath)
                    ) return;

                Process Process = new();
                ProcessStartInfo StartInfo = new()
                {
                    FileName = currentRecordImgPath,
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                Process.StartInfo = StartInfo;
                Process.Start();
            }
            void DeleteImg()
            {
                if (CurrentRecordIsNull() || RemovedImages.Count == 0) return;
                try
                {
                    foreach (var path in RemovedImages)
                    {
                        if (!string.IsNullOrEmpty(path))
                            File.Delete(path);
                    }
                    RemovedImages.Clear();
                }
                catch
                {
                    MessageBox.Show("I've failed to remove the picture from the storare.", "An error occured");
                }
            }

            public void SetImageOnPlaceholder()
            {
                PlaceholderVisibility = (ImgSrc == null) ? Visibility.Visible : Visibility.Collapsed;
                SetImageSource(CurrentRecordImgPath.Invoke(), false);
            }

            public void OnRecordDeleted(string? imgpath)
            {
                if (string.IsNullOrEmpty(imgpath)) return;
                try
                {
                    RemovedImages.AddImage(imgpath);
                    foreach (var path in RemovedImages)
                    {
                        if (!string.IsNullOrEmpty(path))
                            File.Delete(path);
                    }
                    RemovedImages.Clear();
                }
                catch
                {
                    MessageBox.Show("I've failed to remove the picture from the storare.", "An error occured");
                }
            }

            public async Task UpdateStorage()
            {
                Task t1 = new(() =>
                {
                    DeleteImg();
                });
                t1.Start();
                Task t2 = new(() =>
                {
                    StoreImg();
                });
                t2.Start();

                Task t3 = new(() =>
                {
                    // var bets = DatabaseManager.GetDatabaseTable<Bet>().RecordSource;

                    // Parallel.ForEach(bets, (bet) =>
                    // {
                    //     bet.RefreshPromo();
                    // });
                });
                t3.Start();
                await Task.WhenAll(t1, t2, t3);
            }


        }

        public class RemovedImages : List<string>
        {
            readonly string appPath = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory) + @"\Banner\";
            string FolderName { get; set; } =string.Empty;
            public RemovedImages(string folderName)
            {
                FolderName = folderName;
                appPath = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory) + $@"\{FolderName}\";
            }

            public RemovedImages() { }

            public void AddImage(string? path)
            {
                if (path == null) return;
                string folderPath = Path.Combine(appPath + path.Split("\\").LastOrDefault());
                bool result = File.Exists(folderPath);

                if (result)
                    if (!this.Any(s => s.Equals(folderPath))) Add(folderPath);
            }
        }

        #endregion

        /// <summary>
        /// Implement the 'DispatcherUnhandledException' event as follow:
        /// <code>
        /// private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)=>
        /// SARGUI.View.ReportStartupExceptions(e);
        /// </code>
        /// <include file='../SARModel/Docs.xml' path='docs/author'/>
        /// </summary>
        /// <param name="e"></param>
        public static void ReportStartupExceptions(DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"{e?.Exception?.Message}\n{e?.Exception?.StackTrace}\nINNER: {e?.Exception?.InnerException?.Message}", "Startup Error");
            e.Handled = true;
            Environment.Exit(0);
        }

        public static BitmapImage? CreateImageSource(string? path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            try
            {
//                uristring = path;
                BitmapImage image = new();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(path);
                image.EndInit();
                image.Freeze();
                return image;
            }
            catch
            {
                return null;
            }
        }
        public static Color GetColor(string hex) => (Color)ColorConverter.ConvertFromString(hex);
        public static bool MandatoryFieldsDialog() 
        {
            ErrorDialog errorDialog = new("Please fill all the mandatory fields.\nThese are marked with a red (!)", "Something is missing");
            errorDialog.ShowDialog();
            return false;
        } 
        public static T GetResource<T>(string key) =>(T)Application.Current.TryFindResource(key); 
        public static class CultureManager
        {
            public static Country? DefaultCountry { get; set; }
            public static async Task SetCulture()
            {
                JSONManager.FileName = "Setting";
                var task1 = JSONManager.RecreateObjectFormJSONAsync<Country>();
                var task2 = WorldMap.GetDataAsync();
                await Task.WhenAll(task1, task2);

                try
                {
                    DefaultCountry = task1.Result ?? WorldMap.Countries.First(s => s.EnglishName.Contains("United State"));
                }
                catch
                {
                    return;
                }

                var culture = new CultureInfo(DefaultCountry.ID);
                FrameworkElement.LanguageProperty.OverrideMetadata(
                                                  typeof(FrameworkElement),
                                                   new FrameworkPropertyMetadata(

                XmlLanguage.GetLanguage(culture.IetfLanguageTag)));
            }
        }

        /// <summary>
        /// Binder static class provides methods to easily perform binding operations.
        /// </summary>
        public static class Binder
        {
            /// <summary>
            /// Short hand method to generate a Binding object.
            ///<para>
            ///Usually used in a multibinding context.
            /// </para>
            /// </summary>
            /// <param name="sender">The object sending the input</param>
            /// <param name="senderProperty">The sender's property to bind from</param>
            /// <param name="mode"></param>
            /// <returns>Binding</returns>
            public static Binding QuickBindUp(object sender, string senderProperty, BindingMode mode = BindingMode.TwoWay) =>
            new QuickBinder(sender, senderProperty, mode);

            /// <summary>
            /// A short hand method to declare Multi Binding
            /// </summary>
            /// <param name="receiver">The object receiving the input</param>
            /// <param name="receiverProperty">The receiver's property to bind to</param>
            /// <param name="Converter"></param>
            /// <param name="Bindings">An array of bindings are the senders to bind from. Use QuickBindUp</param>
            public static void MultiBindUp(FrameworkElement receiver, DependencyProperty receiverProperty, IMultiValueConverter Converter, params Binding[] Bindings)
            {
                MultiBinding multiBinding = new()
                {
                    Converter = Converter
                };

                foreach (Binding bind in Bindings)
                    multiBinding.Bindings.Add(bind);

                receiver.SetBinding(receiverProperty, multiBinding);
            }

            /// <summary>
            /// A shorthand method to bind a object's property to another
            /// </summary>
            /// <param name="sender">The object sending the input</param>
            /// <param name="senderProperty">The object's property to bind from</param>
            /// <param name="receiver">The object receiving the input</param>
            /// <param name="receiverProperty">The object's property to bind to</param>
            /// <param name="mode"></param>
            /// <param name="converter"></param>
            public static void BindUp(object sender, string senderProperty, DependencyObject receiver, DependencyProperty receiverProperty, BindingMode mode = BindingMode.TwoWay, IValueConverter? converter = null,object? targetNullValue=null) =>
            BindingOperations.SetBinding(receiver, receiverProperty, new QuickBinder(sender, senderProperty, mode, converter, targetNullValue));

            /// <summary>
            ///This method registers an array dependency property.
            /// <example>
            ///<para>For Example:</para>
            ///<code>
            ///private static readonly DependencyPropertyKey namePropertyKey =
            ///Sys.Binder.RegisterKey&lt;<typeparamref name = "T" />, <typeparamref name = "O" />>(...);
            /// 
            ///public static readonly DependencyProperty NameProperty = namePropertyKey.DependencyProperty;
            ///
            ///public Type PropertyNameGetAccessor => (Type)GetValue(NameProperty);
            ///
            ///Finally in class' constructor:
            ///SetValue(namePropertyKey, new Type());
            /// </code>
            /// </example>
            /// </summary>
            /// <remarks>
            /// <c>Author: Salvatore Amaddio R.</c>
            /// </remarks>
            /// <returns>
            /// DependencyPropertyKey
            /// </returns>
            public static DependencyPropertyKey RegisterKey<T,O>(string propName, bool twoway, T? defaultValue, PropertyChangedCallback? callback = null)
            {
                return DependencyProperty.RegisterReadOnly(
                        propName,
                        typeof(T),
                        typeof(O),
                        new FrameworkPropertyMetadata()
                                     {
                                      AffectsArrange = true,
                                      AffectsMeasure = true,
                                      DefaultValue = defaultValue,
                                      AffectsRender = true,
                                      AffectsParentArrange = true,
                                      AffectsParentMeasure = true,
                                      BindsTwoWayByDefault = twoway,
                                      PropertyChangedCallback = callback
                                      }
                );
            }

            /// <summary>
            ///This method registers a dependency property
            /// <example>
            ///<para>For Example:</para>
            /// <code>
            ///public static readonly DependencyProperty nameProperty = Sys.Binder.Register&lt;<typeparamref name = "T" />, <typeparamref name = "O" />>(...);
            /// 
            ///public Type PropertyName
            ///{
            ///     get => (Type)GetValue(nameProperty);
            ///     set => SetValue(nameProperty, value);
            ///}
            /// </code>
            /// </example>
            /// </summary>
            /// <remarks>
            /// Author: Salvatore Amaddio R.
            /// </remarks>
            /// <returns>
            /// DependencyProperty
            /// </returns>
            public static DependencyProperty Register<T, O>(string propName, bool twoway, T? defaultValue, PropertyChangedCallback? callback = null, bool affectsRender = false, bool affectsMeasure = false, bool affectsArrange = false)
            {
                return DependencyProperty.Register(
                propName,
                typeof(T),
                typeof(O),
                new FrameworkPropertyMetadata()
                {
                    BindsTwoWayByDefault = twoway,
                    DefaultValue = defaultValue,
                    PropertyChangedCallback = callback,
                    AffectsRender = affectsRender,
                    AffectsMeasure = affectsMeasure,
                    AffectsArrange = affectsArrange,
                }
            );
            }

            /// <summary>
            /// A short way to decleare a Binding
            /// </summary>
            internal class QuickBinder : Binding
            {
                public QuickBinder(object sender, string senderProperty, BindingMode mode = BindingMode.TwoWay, IValueConverter? converter = null, object? targetNullValue=null) : base(senderProperty)
                {
                    Source = sender;
                    Mode = mode;
                    if (converter!=null) Converter = converter;
                    if (targetNullValue != null) TargetNullValue = targetNullValue;
                }
            }
        }

        #region Exstentions
        public static IAbstractController GetController(this Window window) =>
        (IAbstractController)window.DataContext;

        public static IAbstractController GetController(this Page page) =>
        (IAbstractController)page.DataContext;

        public static void CloseAndOpen(this Window oldWindow, Window newWindow, bool dialog = false)
        {
            oldWindow.Hide();
            if (!dialog)
                newWindow.Show();
            else
                newWindow.ShowDialog();

            oldWindow.Close();
        }
        #endregion

        public static class Help 
        {
            public static void CSPROYFile()
            {
                //<PropertyGroup>
                //<OutputType>WinExe</OutputType>
                //<TargetFramework>net7.0-windows</TargetFramework>
                //<Nullable>enable</Nullable>
                //<UseWPF>true</UseWPF>
                //<ApplicationIcon>AppIcon.ico</ApplicationIcon>
                //<Platforms>AnyCPU; x64; x86</Platforms>
                //<SelfContained>true</SelfContained>
                //<RuntimeIdentifier>win-x64</RuntimeIdentifier>
                //<PublishSingleFile>true</PublishSingleFile>
                //<Description>Demo</Description>
                //<IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
                //<EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
                //<ApplicationManifest>app.manifest</ApplicationManifest>
                //<SignAssembly>True</SignAssembly>
                //<AssemblyOriginatorKeyFile>sgKey.snk</AssemblyOriginatorKeyFile>
                //<DelaySign>True</DelaySign>
                //<Authors>Salvatore Amaddio R.</Authors>
                //<Copyright>Salvatore Amaddio R.</Copyright>
                //<Version>1.0.0.0</Version>
                // </PropertyGroup>
            }

            /// <summary>
            /// Set the database's Build Action to 'Embedded resource' and keep CopyToOutput empty.
            /// <include file='../SARModel/Docs.xml' path='docs/author'/>
            /// </summary>        
            public static void Suggestion()
            {
                // public App() {
                //     DatabaseManager.Load();
                //     try {
                //         DatabaseManager.AddDatabaseTable(
                //             new SQLiteTable&lt;Model1>(),
                //             new SQLiteTable&lt;Model2>(),
                //             new SQLiteTable&lt;Model3>(),
                //             ...
                //             new SQLiteTable&lt;ModelN>()
                //         );
                //     }
                //     catch (Exception ex ) {}
                // }
            }

            /// <summary>
            /// <include file='../SARModel/Docs.xml' path='docs/author'/>
            /// </summary>
            public static void AppXAMLSuggestion()
            {
                //<ResourceDictionary >
                //<ResourceDictionary.MergedDictionaries>
                //<ResourceDictionary Source = "pack://application:,,,/SARGUI;component/SARResources.xaml" />
                //</ResourceDictionary.MergedDictionaries>
                //<view:MainWindow x:Key = "MainWindow"/>
                //
                //Other stuff if necessary.
                //</ResourceDictionary>
                //</Application.Resources>
            }
        }
    }

    public interface IView 
    {
        IAbstractController Controller { get; }
    }
}
