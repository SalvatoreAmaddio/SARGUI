using SARGUI.CustomGUI;
using SARModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SARGUI
{
    /// <summary>
    /// Usefull tips to add in the constructor:
    /// <para>
    /// If you have SubControllers decleare a read-only property as follow:
    /// </para>
    /// <code>
    /// public AnotherController AnotherController { get; } = new();
    /// </code>
    /// Then add in the constructor the following:
    /// <code>
    /// SubControllers.Add(AnotherController);
    /// </code>
    /// If you have external RecordSources decleare a read-only property as follow:
    /// <code>
    ///  public RecordSource&lt;Type> ExternRecordSource { get; }
    /// </code>
    /// Then add in the constructor the following:
    /// <code>
    /// ExternRecordSource = new((IEnumerable&lt;Type>)DatabaseManager.GetDatabaseTable&lt;Type>().DataSource);
    /// DatabaseManager.AddChild&lt;Type>(ExternRecordSource);
    /// </code>
    /// <include file='../SARModel/Docs.xml' path='docs/author'/>
    /// </summary>
    public abstract class AbstractDataController<M> : AbstractNotifier, IAbstractController where M : AbstractTableModel<M>, new()
    {
        #region BackProps
        private object? _ui;
        private IAbstractModel? _selectedRecord;
        private string _search = string.Empty;
        private bool _isLoading = false;
        #endregion

        #region DatabaseAndSources
        public virtual IDB DB { get; } = DatabaseManager.GetDatabaseTable<M>();
        public IRecordSource MainSource => DB.DataSource;
        public IRecordSource ChildSource { get; set; } 
        public RecordSource<M> RecordSource 
        { 
            get=> (RecordSource<M>)ChildSource;
            set => ChildSource = value;
        }
        #endregion

        #region Properties
        public bool IsDirty { get => ChildSource.Any(s => s.IsDirty);}
        public IAbstractModel? SelectedRecord { get => (M?)_selectedRecord; set => Set(ref value, ref _selectedRecord); }
        public M? CurrentRecord { get=>(M?)_selectedRecord; }
        public List<IAbstractController> SubControllers { get; } = new();
        public Type ModelType => typeof(M);
        public string Search { get => _search; set => Set(ref value, ref _search); }
        public bool IsLoading { get => _isLoading; set => Set(ref value, ref _isLoading); }

        protected Task<Excel>? callingExcelTask;
        protected Task<object?[,]>? organiseExcelDataTask;
        #endregion

        #region Commands
        public ICommand OpenCMD { get; }
        public ICommand OpenNewCMD { get; }
        public ICommand SaveCMD { get; }
        public ICommand ClearCMD { get; }
        public ICommand DeleteCMD { get; }
        #endregion

        public AbstractDataController()
        {
            ChildSource = new RecordSource<M>((IEnumerable<M>)MainSource);
            MainSource.AddChild(ChildSource);
            RecordSource.OnRecordMoved += OnRecordMoved;
            RecordSource.OnRecordMoving += OnRecordMoving;
            ChildSource.GoFirst();
            SelectedRecord = ChildSource.FirstOrDefault();
            SaveCMD = new Commando(Save);
            DeleteCMD = new Commando(Delete);
            OpenCMD = new Commando(OpenRecord);
            OpenNewCMD = new Commando(OpenNewRecord);
            ClearCMD = new Commando(ClearRecord);
            AfterUpdate += OnAfterUpdate;
        }

        /// <summary>It runs a series of commands after a property gets updated.<br/>
        /// For Example:
        /// <para>override this method, then add some code inside like the following:</para>
        /// <include file="../SARModel/Docs.xml" path="docs/simpleSearchExample"/>
        /// If you are using an AbstractRecordsOrganizer:
        /// <include file="../SARModel/Docs.xml" path="docs/recordOrganizerSearch"/>
        /// <include file='../SARModel/Docs.xml' path='docs/recordsOrganizer'/><br/>
        /// <include file='../SARModel/Docs.xml' path='docs/author'/>
        /// </summary>
        protected virtual void OnAfterUpdate(object? sender, AbstractPropChangedEventArgs e) { }

        #region Events
        /// <summary>
        /// This method is a bridge between the controller and the RecordSource.
        /// <para>
        /// Once the form has moved to a new record
        /// it ensures that the SelectedRecord points at the current record in the RecordSource
        /// </para>
        /// <include file='../SARModel/Docs.xml' path='docs/author'/>
        /// </summary>
        public virtual void OnRecordMoved(object? sender, RecordMovedEvtArgs e) => SelectedRecord = (M?)e.Record;

        /// <summary>It runs a series of check before moving to another record.
        /// <include file='../SARModel/Docs.xml' path='docs/author'/>
        /// </summary>
        private void OnRecordMoving(object? sender, RecordMovingEvtArgs e)
        {
            if(!CheckSubForms()) 
            {
                e.Cancel = true;
                return;
            }
            if (!IsDirty) return;
            e.Cancel = e.Record != null && !e.Record.CanSave();
            if (e.Cancel) 
            {
                View.MandatoryFieldsDialog();
                return;
            }
            switch (SaveActionConfirmed())
            {
                case true:
                     Save(e.Record);
                break;
                case false:
                        if (e.IsNew()) ChildSource.GoPrevious();
                        else e?.Record?.Undo();
                break;
            }
        }
        #endregion

        #region CRUD
        public virtual bool Save(IAbstractModel? record)
        {
            if (record==null || !record.IsDirty) return false;
            if (!record.CanSave()) return View.MandatoryFieldsDialog();

            SelectedRecord = null;
            DB.OpenConnection();
            if (record.IsNewRecord)
                DB.Insert(record);
            else
                DB.Update(record);            
            DB.CloseConnection();
            SelectedRecord = (M?)record;
            return true;
        }
        public virtual bool Delete(IAbstractModel? record)
        {
            if (record == null) return false;
            if (!DeleteActionConfirmed()) return false;
            SelectedRecord = null;
            DB.OpenConnection();
            DB.Delete(record);
            DB.CloseConnection();
            //SelectedRecord = (M?)DB.DataSource.CurrentRecord;
            return true;
        }
        public virtual void ClearRecord(IAbstractModel? record) => record?.Clear();
        public void UndoChanges()
        {
            Parallel.ForEach(ChildSource.Where(s => s.IsDirty),
                (record) =>
                {
                    record.Undo();
                });
        }
        public void CommitChanges()
        {
            Parallel.ForEach(ChildSource.Where(s => s.IsDirty),
            (record) =>
            {
                Save(record);
            });
        }
        #endregion

        #region OpenCloseForms
        public virtual void OpenRecord(IAbstractModel? record)=>throw new NotImplementedException();
        public virtual void OpenNewRecord(IAbstractModel? record)=>OpenRecord(new M());
        public virtual bool OnFormClosing(CancelEventArgs e)
        {
            if (!CheckSubForms()) return false;
            if (!IsDirty) return true;
            if (SelectedRecord != null)
            {
                e.Cancel = !SelectedRecord.CanSave();
                if (e.Cancel) return View.MandatoryFieldsDialog();
            }

            bool save = SaveActionConfirmed();
            if (save) CommitChanges();
            else UndoChanges();
            return true;
        }
        #endregion

        #region Dialogs
        public bool DeleteActionConfirmed()
        {
            DeleteDialog deleteMesssageDialog = new();
            deleteMesssageDialog.ShowDialog();
            return deleteMesssageDialog.Response.Equals(DialogResponse.YES);
        }
        public bool SaveActionConfirmed()
        {
            YesNoDialog yesNoDialog = new();
            yesNoDialog.ShowDialog();
            return yesNoDialog.Response.Equals(DialogResponse.YES);
        }
        #endregion

        #region Movements
        public virtual void OnAppearingGoTo(IAbstractModel? record) 
        {
            if (record == null || record.IsNewRecord)
            {
                ChildSource.GoNewRecord();
                return;
            }

            GoTo(record);
        }
        public void GoTo(IAbstractModel record) => SelectedRecord = ChildSource.FirstOrDefault(s => s.Equals(record));
        public void GoFirst() => SelectedRecord = ChildSource.FirstOrDefault();
        public void AllowNewRecord(bool value) => ChildSource.AllowNewRecord = value;
        #endregion

        #region UI
        public bool UIIsWindow()=>_ui is Window;
        public bool UIIsPage()=> _ui is Page;
        public void SetUI(object obj) => _ui = obj;
        public T GetUI<T>() => _ui==null ? throw new Exception("No UI has been set") : (T)_ui;
        #endregion

        #region checkRecordsIntegrity
        private bool CheckSubForms() 
        {
            bool outcome = true;

            Application.Current.Dispatcher.Invoke((Action)delegate {
                Parallel.ForEach(SubControllers.Where(s => s.IsDirty), (controller, stop) =>
                {
                    outcome = controller.RecordIntegrityCheck();
                    if (!outcome) stop.Break();
                });
            });

            return outcome;
        }
        public bool RecordIntegrityCheck()
        {
            bool outcome = true;
            Parallel.ForEach(ChildSource.Where(s => s.IsDirty), (record, stop) =>
            {
                outcome = record.CanSave();
                if (!outcome) stop.Break();
                bool save = SaveActionConfirmed();
                if (save) CommitChanges();
                else UndoChanges();
            });
            return outcome;
        }
        #endregion
        public override void Set<T>(ref T value, ref T _backprop, [CallerMemberName] string propName = "") where T : default
        {
            PropChangedEventArgs<T> prop = new(ref value, ref _backprop, propName);

            InvokeBeforeUpdate(prop);

            if (prop.Cancel) return;
            _backprop = value;
            InvokeAfterUpdate(prop);
            NotifyView(propName);

            if (propName.Equals(nameof(SelectedRecord)))
            {
                NotifyView(nameof(CurrentRecord));
                ChildSource.SetCurrentRecord((IAbstractModel?)value);
            }
        }
        public override string ToString() => $"DataController<{ModelType.Name}>";
        public static bool SearchFilter(object? record, string? criteria)
        {
            if (criteria is null || record is null) return false;
            bool? val = record?.ToString()?.ToLower().Contains(criteria);
            return val != null && val.Value;
        }

        public virtual void RunOffice(OfficeApplication officeApp) => throw new NotImplementedException();

        public static Task<Excel> CallExcel(OfficeFileMode mode, string path)
        {
            if (Sys.FileExists(path) && mode.Equals(OfficeFileMode.WRITE))
            {
                MessageBoxResult result = MessageBox.Show("This file already exists, do you want to replace it?", "Confirm Action", MessageBoxButton.YesNo);
                CancellationTokenSource tokenSource = new();
                CancellationToken token = tokenSource.Token;
                tokenSource.Cancel();
                if (result.Equals(MessageBoxResult.No)) return Task.FromCanceled<Excel>(token);
                Sys.DeleteFile(path);               
            }
            return Task.FromResult<Excel>(new(mode, path));
        }

        public virtual Task<object?[,]> OrganiseExcelData() => throw new NotImplementedException();

        public async Task<bool> WriteExcel(OfficeFileMode mode, string path, Action<Excel>? additionalStyle)
        {
            organiseExcelDataTask = OrganiseExcelData();
            callingExcelTask = CallExcel(mode, path);
            object?[,] data;
            Excel excel;

            try
            {
                excel = await callingExcelTask;
                data = await organiseExcelDataTask;
            }
            catch
            {
                return false;
            }

            excel.Range.WriteTable(data);
            additionalStyle?.Invoke(excel);
            excel.SaveAndClose();
            return false;
        }

        public object?[,] GenerateDataTable(params string[] columnValues) 
        {
            object?[,] data = new object[ChildSource.RecordCount + 1, columnValues.Length];

            for(int i = 0; i < columnValues.Length; i++) 
                data[0, i] = columnValues[i];
            
            return data;
        }
    }

    public class Commando : ICommand
    {
        readonly Func<IAbstractModel?, bool>? _execute;
        readonly Action<IAbstractModel?>? _executeAction;
        public Commando(Func<IAbstractModel?,bool> execute) =>_execute = execute;
        public Commando(Action<IAbstractModel?> execute) =>_executeAction = execute;
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => true;
        public async void Execute(object? parameter) 
        {
            var t = InternetConnection.IsAvailableTask();

            IAbstractModel? model=null;

            if (parameter is Button button)
                model = (IAbstractModel)button.DataContext;

            if (parameter is IAbstractModel)
                model = (IAbstractModel?)parameter;
            
            if (parameter is IAbstractController controller)
                model = controller.SelectedRecord;

            await t;
            if (!t.Result)
            {
                return;
            }

            if (_execute != null) 
            {
                _execute(model);
                return;
            }

            _executeAction?.Invoke(model);
        } 
    }
}