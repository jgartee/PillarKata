using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Models;

namespace PillarKata.ViewModels
{
    public class AdvertisementCollectionViewModel : ViewModelBase
    {
        #region Instance fields

        private readonly INewspaperAdRepository _adRepository;
        private AdvertisementItemViewModel _currentItem;

        #endregion

        #region Constructors

        public AdvertisementCollectionViewModel(INewspaperAdRepository adRepository)
        {
            _adRepository = adRepository as INewspaperAdRepository;
            Advertisements = new ObservableCollection<AdvertisementItemViewModel>();

            var ads = _adRepository.GetAllAdvertisements().OrderBy(a => a.Name).ToList();
            ads.ForEach(a => Advertisements.Add(new AdvertisementItemViewModel(_adRepository) {Model = a}));

            Advertisements.CollectionChanged += AdvertisementsCollectionChanged;

            DeleteCommand = new RelayCommand<RoutedEventArgs>(DeleteCommandHandler);
            SaveCommand = new RelayCommand<AdvertisementItemViewModel>(SaveCommandHandler);

            Messenger.Default.Register<AddingAdvertisementItemMessage>(this, AddingItemMessageHandler);
            Messenger.Default.Register<RemovingAdvertisementItemMessage>(this, RemovingAdvertisementItemMessageHandler);
            Messenger.Default.Register<SaveAdvertisementItemMessage>(this, SaveItemMessageHandler);
            Messenger.Default.Register<AdvertisementDetailViewModelReady>(this, DetailViewModelReadyHandler);
            SelectedItemChangedCommand =
                new RelayCommand<RoutedPropertyChangedEventArgs<object>>(SelectedItemChangedCommandHandler);
            Messenger.Default.Send(new AdvertisementCollectionReadyMessage() {AdvertisementList = Advertisements});
        }

        private void TreeViewItemSelectedCommandHandler(object obj)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Properties

        public ObservableCollection<AdvertisementItemViewModel> Advertisements { get; private set; }

        public AdvertisementItemViewModel CurrentItem
        {
            get { return _currentItem; }
            set
            {
                _currentItem = value;
                RaisePropertyChanged(() => CurrentItem);
                Messenger.Default.Send(new CurrentAdvertisementItemChangedMessage(_currentItem));
            }
        }

        public ICommand DeleteCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand SelectedItemChangedCommand { get; set; }

        #endregion

        #region Class Members

        private void AddingItemMessageHandler(AddingAdvertisementItemMessage obj)
        {
            var model = new Advertisement();
            var vm = new AdvertisementItemViewModel(_adRepository) {Model = model};
            Advertisements.Add(vm);
            var sortedAds = Advertisements.OrderBy(a => a.Name).ToList();
            Advertisements.Clear();
            sortedAds.ForEach(Advertisements.Add);

            Messenger.Default.Send(new CurrentAdvertisementItemChangedMessage(vm));
        }

        private void AdvertisementsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var newItems = new List<AdvertisementItemViewModel>();
            var oldItems = e.OldItems as List<AdvertisementItemViewModel>;

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems != null)
                {
                    foreach (AdvertisementItemViewModel vm in e.NewItems)
                    {
                        CurrentItem = vm;
                        vm.Newspapers.ToList().ForEach(n => _adRepository.Save(n.Model));
                    }
                }

                foreach (var entity in newItems)
                {
                    foreach (var paper in entity.Newspapers)
                    {
                        if (paper.DbStatus == DbModificationState.Unchanged)
                            paper.DbStatus = DbModificationState.Modified;
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems != null)
                {
                    foreach (AdvertisementItemViewModel deletedAd in e.OldItems)
                    {
                        CurrentItem = Advertisements.Count > 0
                                          ? Advertisements[
                                              e.OldStartingIndex < Advertisements.Count
                                                  ? e.OldStartingIndex : Advertisements.Count - 1] : null;
                        var unlinkedPapers = deletedAd.Newspapers.ToList();

                        unlinkedPapers.ForEach(p =>
                                               {
                                                   if (!deletedAd.isEventRemovingAdvertisementFromNewspaper)
                                                   {
                                                       deletedAd.isEventRemovingAdvertisementFromNewspaper = true;
                                                       p.Advertisements.Remove(deletedAd);
                                                       deletedAd.isEventRemovingAdvertisementFromNewspaper = false;
                                                   }
                                               });
                    }
                }
            }
        }

        static public TreeViewItem GetParentItem(TreeViewItem item)
        {
            for (var i = VisualTreeHelper.GetParent(item); i != null; i = VisualTreeHelper.GetParent(i))
                if (i is TreeViewItem)
                    return (TreeViewItem)i;

            return null;
        }
        //        private void DeleteCommandHandler(TreeView treeView)
        private void DeleteCommandHandler(RoutedEventArgs args)
        {
            if((args as KeyEventArgs).Key != Key.Delete)
                return;

            TreeViewItem item = args.OriginalSource as TreeViewItem;
            var parent = GetParentItem(item);
            var tv = args.Source as TreeView;
            var deletedNewspaperItem = tv.SelectedItem as NewspaperItemViewModel;
            var deletedAd = tv.SelectedItem as AdvertisementItemViewModel;

            if (deletedNewspaperItem != null)
            {
                var ad = parent.Header as AdvertisementItemViewModel;
                ad.Newspapers.Remove(deletedNewspaperItem);
                deletedNewspaperItem.Advertisements.Remove(ad);
            }
            else if(deletedAd != null)
            {
                deletedAd.DbStatus = DbModificationState.Deleted;
                Advertisements.Remove(deletedAd);
                deletedAd.Newspapers.ToList().ForEach(n =>
                                                      {
                                                          n.DbStatus = DbModificationState.Modified;
                                                      });
                SaveCommandHandler(deletedAd);
            }
        }

        private void DetailViewModelReadyHandler(AdvertisementDetailViewModelReady obj)
        {
            Messenger.Default.Send(new CurrentAdvertisementItemChangedMessage(CurrentItem));
        }

        private void RemovingAdvertisementItemMessageHandler(RemovingAdvertisementItemMessage message)
        {
            Advertisements.Remove(message.ItemViewModel);
            Messenger.Default.Send<CurrentAdvertisementItemChangedMessage>(new CurrentAdvertisementItemChangedMessage(CurrentItem));
        }

        private bool inSave;
        private void SaveCommandHandler(AdvertisementItemViewModel viewModel)
        {
            if (inSave)
                return;

            inSave = true;
            viewModel.Newspapers.ToList().ForEach(n => _adRepository.Save(n.Model));
            var sortedList = Advertisements.OrderBy(a=>a.Name).ToList();
            Advertisements.Clear();
            sortedList.ForEach(a=>Advertisements.Add(a));
            inSave = false;
        }

        private void SaveItemMessageHandler(SaveAdvertisementItemMessage msg)
        {
            SaveCommand.Execute(msg.ItemViewModel);
        }

        private void SelectedItemChangedCommandHandler(RoutedPropertyChangedEventArgs<object> args)
        {
            var adItem = args.NewValue as AdvertisementItemViewModel;
            var paperItem = args.NewValue as NewspaperItemViewModel;

            if (adItem != null)
            {
                CurrentItem = (AdvertisementItemViewModel) (args.NewValue) ??
                              new AdvertisementItemViewModel(_adRepository) {Model = new Advertisement()};
                Messenger.Default.Send(new CurrentAdvertisementItemChangedMessage(CurrentItem));
            }
        }

        #endregion
    }
}