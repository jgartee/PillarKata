using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight;
using Models;

namespace PillarKata.ViewModels
{
    public class AdvertisementItemViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        #region Instance fields

        private readonly INewspaperAdRepository _adRepository;
        private Advertisement _model;
        private ObservableCollection<NewspaperItemViewModel> _newspapers;

        #endregion

        #region Constructors

        public AdvertisementItemViewModel(INewspaperAdRepository adRepository)
        {
            _adRepository = adRepository;
            _newspapers = new ObservableCollection<NewspaperItemViewModel>();
            _newspapers.CollectionChanged += Newspapers_CollectionChanged;
        }

        #endregion

        #region Properties

        public DbModificationState DbStatus
        {
            get { return Model.DbStatus; }
            set { Model.DbStatus = value; }
        }

        public Advertisement Model
        {
            get { return _model; }
            set
            {
                if (_model == value)
                    return;

                _model = value;

                if (_model != null)
                    _model.ErrorsChanged += _model_ErrorsChanged;
            }
        }

        public string Name
        {
            get
            {
                if (_model == null)
                    throw new MissingModelException();

                return _model.Name;
            }
            set
            {
                if (_model == null)
                    throw new MissingModelException();

                if (_model.Name == value)
                    return;

                var oldValue = _model.Name;
                _model.Name = value;
                RaisePropertyChanged(() => Name, oldValue, _model.Name, true);
            }
        }

        public ObservableCollection<NewspaperItemViewModel> Newspapers
        {
            get { return _newspapers; }
        }

        public string Text
        {
            get
            {
                if (_model == null)
                    throw new MissingModelException();

                return _model.Text;
            }
            set
            {
                if (_model == null)
                    throw new MissingModelException();

                if (_model.Text == value)
                    return;

                var oldValue = _model.Text;
                _model.Text = value;
                RaisePropertyChanged(() => Text, oldValue, _model.Text, true);
            }
        }

        public Guid UKey
        {
            get { return _model.UKey; }
        }

        internal bool isEventAddingAdvertisementToNewspaper { get; set; }
        internal bool isEventRemovingAdvertisementFromNewspaper { get; set; }

        #endregion

        #region INotifyDataErrorInfo Properties and Members

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            return _model.GetErrors(propertyName);
        }

        public bool HasErrors
        {
            get { return _model.HasErrors; }
        }

        #endregion

        #region Class Members

        private void Newspapers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var newItems = e.NewItems;
            var oldItems = e.OldItems;
            var action = e.Action;
            var update_newspapers = true;

            switch (action)
            {
                case NotifyCollectionChangedAction.Add:

                    var distinctItems = _newspapers.ToList().Distinct().ToList();
                    newItems.Cast<NewspaperItemViewModel>().ToList().ForEach(n => n.DbStatus = DbModificationState.Modified);

                    if (_newspapers.Count > distinctItems.Count)
                    {
                        var deletedItems = _newspapers.ToList();
                        distinctItems.ForEach(model => deletedItems.Remove(model));

                        foreach (var vm in distinctItems)
                        {
                            if (_newspapers.Count > 1)
                            {
                                isEventRemovingAdvertisementFromNewspaper = true;
                                _newspapers.Remove(vm);
                                isEventRemovingAdvertisementFromNewspaper = false;
                            }
                        }

                        update_newspapers = false;
                    }

                    if (update_newspapers || _newspapers.Count == 1)
                    {
                        foreach (NewspaperItemViewModel paper in newItems)
                        {
                            if (!paper.isEventAddingNewspaperToAdvertisement)
                            {
                                isEventAddingAdvertisementToNewspaper = true;
                                paper.Advertisements.Add(this);
                                isEventAddingAdvertisementToNewspaper = true;
                            }
                        }
                    }

                    break;

                case NotifyCollectionChangedAction.Move:
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (NewspaperItemViewModel paper in oldItems)
                    {
                        paper.DbStatus = DbModificationState.Modified;

                        if (!paper.isEventRemovingAdvertisementFromNewspaper)
                        {
                            isEventRemovingAdvertisementFromNewspaper = true;
                            paper.Advertisements.Remove(this);
                            isEventRemovingAdvertisementFromNewspaper = false;
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    break;

                case NotifyCollectionChangedAction.Reset:
                    break;

                default:
                    throw new InvalidNewspaperCollectionChangedAction(new InvalidNewspaperCollectionChangedActionArgs(
                                                                          _newspapers, e.Action));
            }

            SynchroniseModelAndViewModelCollections();
        }

        private void SynchroniseModelAndViewModelCollections()
        {
            var list = new List<NewspaperItemViewModel>();
            foreach (NewspaperItemViewModel newspaper in Newspapers)
                list.Add(newspaper);
            list.ForEach(n =>
                         {
                             n.Model.Advertisements.Clear();
                             n.Advertisements.ToList().ForEach(a => n.Model.Advertisements.Add(a.Model));
                         });
        }

        private void _model_ErrorsChanged(object sender, DataErrorsChangedEventArgs e)
        {
            if (ErrorsChanged != null)
                ErrorsChanged(sender, e);
        }

        #endregion
    }

    public class InvalidNewspaperCollectionChangedActionArgs
    {
        #region Instance fields

        private readonly ObservableCollection<NewspaperItemViewModel> _Newspapers;
        private readonly NotifyCollectionChangedAction _action;

        #endregion

        #region Constructors

        public InvalidNewspaperCollectionChangedActionArgs(ObservableCollection<NewspaperItemViewModel> Newspapers,
                                                           NotifyCollectionChangedAction action)
        {
            _Newspapers = Newspapers;
            _action = action;
        }

        #endregion
    }

    public class InvalidNewspaperCollectionChangedAction : Exception
    {
        #region Instance fields

        private readonly InvalidNewspaperCollectionChangedActionArgs _invalidNewspaperCollectionChangedActionArgs;

        #endregion

        #region Constructors

        public InvalidNewspaperCollectionChangedAction(
            InvalidNewspaperCollectionChangedActionArgs invalidNewspaperCollectionChangedActionArgs)
        {
            _invalidNewspaperCollectionChangedActionArgs = invalidNewspaperCollectionChangedActionArgs;
        }

        #endregion

        #region Properties

        public InvalidNewspaperCollectionChangedActionArgs InvalidNewspaperCollectionChangedActionArgs
        {
            get { return _invalidNewspaperCollectionChangedActionArgs; }
        }

        #endregion
    }
}