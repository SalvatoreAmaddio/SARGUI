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

namespace SARGUI {
    public static class View 
    {
        #region ImageStorageManager 
        public class ImageStorageManager : AbstractNotifier
        {
            #region BackProps
            Visibility _placeholderVisibility = Visibility.Visible;
            Visibility _buttonVisibility = Visibility.Visible;
            ImageSource? _imgSrc = null;
            string FolderName { get; set; } = string.Empty;
            readonly string appPath;
            string imgOriginalPath =string.Empty;
            string destinationImagePath=string.Empty;
            readonly RemovedImages RemovedImages;
            #region Funcs
            Func<bool> CurrentRecordIsNull;
            Func<string?> CurrentRecordImgPath;
            #endregion

            #region Actions
            Action<bool,string?> CurrentRecordBehaviour;
            Action<object?> CurrentRecordImgPathSetter;
            #endregion
            #endregion

            public Visibility PlaceholderVisibility { get => _placeholderVisibility; set => Set(ref value, ref _placeholderVisibility); }
            public Visibility ButtonVisibility { get => _buttonVisibility; set => Set(ref value, ref _buttonVisibility); }
            public ImageSource? ImgSrc { get => _imgSrc; set => Set(ref value, ref _imgSrc); }    
            
            #region Commands
            public ICommand PickUpImageCMD { get; }
            public ICommand RemoveImgCMD { get; }
            public ICommand ViewBannerCMD { get; }
            #endregion

            public ImageStorageManager(string folderPath, Action<bool,string?> currentRecordBehaviour, Func<string?> currentRecordImgPath, Func<bool> currentRecordIsNull, Action<object?> currentRecordImgPathSetter)
            {
                FolderName = folderPath;
                RemovedImages = new(FolderName);
                appPath = Path.GetDirectoryName(Sys.AppLocation) + $@"\{FolderName}\";
                
                CurrentRecordBehaviour = currentRecordBehaviour;
                CurrentRecordImgPath = currentRecordImgPath;
                CurrentRecordIsNull = currentRecordIsNull;
                CurrentRecordImgPathSetter = currentRecordImgPathSetter;
                
                PickUpImageCMD = new Command(PickUpImage);
                RemoveImgCMD = new Command(RemoveImg);
                ViewBannerCMD = new Command(ViewBanner);
                AfterUpdate += ImageStorageManager_AfterUpdate;
            }
            
            public void ResetActionsAndFunctions(Action<bool, string?> currentRecordBehaviour, Func<string?> currentRecordImgPath, Func<bool> currentRecordIsNull, Action<object?> currentRecordImgPathSetter)
            {
                CurrentRecordBehaviour = currentRecordBehaviour;
                CurrentRecordImgPath = currentRecordImgPath;
                CurrentRecordIsNull = currentRecordIsNull;
                CurrentRecordImgPathSetter = currentRecordImgPathSetter;
            }

            private void ImageStorageManager_AfterUpdate(object? sender, AbstractPropChangedEventArgs e)
            {
                if (e.PropIs(nameof(ImgSrc)))
                {
                    if (e.GetNewValue() == null && (!CurrentRecordIsNull.Invoke()))
                    {
                        CurrentRecordImgPathSetter.Invoke(null);
                    }
                    PlaceholderVisibility = (ImgSrc == null) ? Visibility.Visible : Visibility.Collapsed;
                    return;
                }

                if (e.PropIs(nameof(PlaceholderVisibility)))
                {
                    Visibility value = (Visibility)e.GetNewValue();
                    ButtonVisibility = (value.Equals(Visibility.Visible)) ? Visibility.Hidden : Visibility.Visible;
                    return;
                }

            }

            BitmapImage? GenderateBitMapImg(string? newImgPath) => SARGUI.View.CreateImageSource(newImgPath);

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
                    Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
                                             "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                                             "Portable Network Graphic (*.png)|*.png"
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

            public void Somethign()
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
            MessageBox.Show($"{e?.Exception?.Message}\n{e?.Exception?.StackTrace}", "Startup Error");
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

    }

}
