using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Models;

namespace PillarKata.ViewModels
{
    public class NewspaperItemViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        #region Instance fields

        private readonly INewspaperAdRepository _adRepository;
        private ObservableCollection<AdvertisementItemViewModel> _advertisements;
        private Newspaper _model;

        #endregion

        #region Constructors

        public NewspaperItemViewModel(INewspaperAdRepository adRepository)
        {
            _adRepository = adRepository;
            _advertisements = new ObservableCollection<AdvertisementItemViewModel>();
            _advertisements.CollectionChanged += Advertisements_CollectionChanged;
        }

        #endregion

        #region Properties

        public ObservableCollection<AdvertisementItemViewModel> Advertisements
        {
            get { return _advertisements; }
            private set { _advertisements = value; }
        }

        public DbModificationState DbStatus
        {
            get { return Model.DbStatus; }
            set { Model.DbStatus = value; }
        }

        public Newspaper Model
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

        public Guid UKey
        {
            get { return _model.UKey; }
        }

        internal bool isEventAddingNewspaperToAdvertisement { get; set; }
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

        private void Advertisements_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var newItems = e.NewItems;
            var oldItems = e.OldItems;
            var action = e.Action;
            var update_advertisements = true;
            DbStatus = DbModificationState.Modified;

            switch (action)
            {
                case NotifyCollectionChangedAction.Add:

                    var distinctItems = _advertisements.ToList().Distinct().ToList();
                    newItems.Cast<AdvertisementItemViewModel>().ToList().ForEach(n => n.DbStatus = DbModificationState.Modified);

                    if (_advertisements.Count > distinctItems.Count)
                    {
                        var deletedItems = _advertisements.ToList();
                        distinctItems.ForEach(model => deletedItems.Remove(model));

                        foreach (var vm in distinctItems)
                        {
                            if (_advertisements.Count > 1)
                            {
                                isEventRemovingAdvertisementFromNewspaper = true;
                                _advertisements.Remove(vm);
                                isEventRemovingAdvertisementFromNewspaper = true;
                            }
                        }

                        update_advertisements = false;
                    }

                    if (update_advertisements || _advertisements.Count == 1)
                    {
                        foreach (AdvertisementItemViewModel ad in newItems)
                        {
                            if (!ad.isEventAddingAdvertisementToNewspaper)
                            {
                                isEventAddingNewspaperToAdvertisement = true;
                                ad.Newspapers.Add(this);
                                isEventAddingNewspaperToAdvertisement = false;
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (AdvertisementItemViewModel ad in oldItems)
                    {
                        if(ad.DbStatus == DbModificationState.Unchanged)
                            ad.DbStatus = DbModificationState.Modified;

                        if (!ad.isEventRemovingAdvertisementFromNewspaper)
                        {
                            isEventRemovingAdvertisementFromNewspaper = true;
                            ad.Newspapers.Remove(this);
                            isEventRemovingAdvertisementFromNewspaper = false;
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    break;

                case NotifyCollectionChangedAction.Reset:
                    break;

                default:
                    throw new InvalidAdvertisementCollectionChangedAction(
                        new InvalidAdvertisementCollectionChangedActionArgs(_advertisements, e.Action));
            }

            SynchroniseModelAndViewModelCollections();
        }

        private void SynchroniseModelAndViewModelCollections()
        {
            List<AdvertisementItemViewModel> list = new List<AdvertisementItemViewModel>();
            foreach (AdvertisementItemViewModel advertisement in Advertisements)
                list.Add(advertisement);
            list.ForEach(a =>
                                            {
                                                a.Model.Newspapers.Clear();
                                                a.Newspapers.ToList().ForEach(n => a.Model.Newspapers.Add(n.Model));
                                            });
        }

        private void _model_ErrorsChanged(object sender, DataErrorsChangedEventArgs e)
        {
            if (ErrorsChanged != null)
                ErrorsChanged(sender, e);
        }

        #endregion
    }

    public class InvalidAdvertisementCollectionChangedActionArgs
    {
        #region Instance fields

        private readonly NotifyCollectionChangedAction _action;
        private readonly ObservableCollection<AdvertisementItemViewModel> _advertisements;

        #endregion

        #region Constructors

        public InvalidAdvertisementCollectionChangedActionArgs(ObservableCollection<AdvertisementItemViewModel> advertisements,
                                                               NotifyCollectionChangedAction action)
        {
            _advertisements = advertisements;
            _action = action;
        }

        #endregion
    }

    public class InvalidAdvertisementCollectionChangedAction : Exception
    {
        #region Instance fields

        private readonly InvalidAdvertisementCollectionChangedActionArgs _invalidAdvertisementCollectionChangedActionArgs;

        #endregion

        #region Constructors

        public InvalidAdvertisementCollectionChangedAction(
            InvalidAdvertisementCollectionChangedActionArgs invalidAdvertisementCollectionChangedActionArgs)
        {
            _invalidAdvertisementCollectionChangedActionArgs = invalidAdvertisementCollectionChangedActionArgs;
        }

        #endregion

        #region Properties

        public InvalidAdvertisementCollectionChangedActionArgs InvalidAdvertisementCollectionChangedActionArgs
        {
            get { return _invalidAdvertisementCollectionChangedActionArgs; }
        }

        #endregion
    }

    public class MissingModelException : Exception
    {
    }
}