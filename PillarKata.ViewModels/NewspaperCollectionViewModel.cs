using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Models;

namespace PillarKata.ViewModels
{
    public class NewspaperCollectionViewModel : ViewModelBase
    {
        #region Instance fields

        private readonly INewspaperAdRepository _adRepository;
        private NewspaperItemViewModel _currentItem;
        private bool inSave;

        #endregion

        #region Constructors

        public NewspaperCollectionViewModel(INewspaperAdRepository adRepository)
        {
            _adRepository = adRepository;
            Newspapers = new ObservableCollection<NewspaperItemViewModel>();

            var newspapers = _adRepository.Find(n => n != null).ToList().OrderBy(paper => paper.Name).ToList();
            newspapers.ForEach(n => Newspapers.Add(new NewspaperItemViewModel(_adRepository) {Model = n}));

            Newspapers.CollectionChanged += NewspapersCollectionChanged;
            SaveCommand = new RelayCommand<NewspaperItemViewModel>(SaveCommandHandler);
            DeleteCommand = new RelayCommand<RoutedEventArgs>(DeleteCommandHandler);
            DropCommand = new RelayCommand<DragEventArgs>(DropAdHandler);
            Messenger.Default.Register<AddingNewspaperItemMessage>(this, AddingNewspaperItemMessageHandler);
            Messenger.Default.Register<RemovingNewspaperItemMessage>(this, RemovingItemMessageHandler);
            Messenger.Default.Register<NewspaperDetailViewModelReady>(this, DetailViewModelReadyHandler);
            Messenger.Default.Register<SaveNewspaperItemMessage>(this, SaveItemMessageHandler);
            SelectedItemChangedCommand =
                new RelayCommand<RoutedPropertyChangedEventArgs<object>>(SelectedItemChangedCommandHandler);
            Messenger.Default.Send(new NewspaperCollectionReadyMessage {NewspaperList = Newspapers});
        }

        #endregion

        #region Properties

        public NewspaperItemViewModel CurrentItem
        {
            get { return _currentItem; }
            set
            {
                _currentItem = value;
                RaisePropertyChanged(() => CurrentItem);
                Messenger.Default.Send(new CurrentNewspaperItemChangedMessage(_currentItem));
            }
        }

        public ICommand DeleteCommand { get; set; }
        public ICommand DropCommand { get; private set; }

        public ObservableCollection<NewspaperItemViewModel> Newspapers { get; private set; }
        public ICommand SaveCommand { get; set; }
        public ICommand SelectedItemChangedCommand { get; set; }

        #endregion

        #region Class Members

        private void AddingNewspaperItemMessageHandler(AddingNewspaperItemMessage message)
        {
            var model = new Newspaper();
            var vm = new NewspaperItemViewModel(_adRepository) {Model = model};
            Newspapers.Add(vm);
            var sortedNewspapers = Newspapers.OrderBy(n => n.Name).ToList();
            Newspapers.Clear();
            sortedNewspapers.ForEach(Newspapers.Add);
            Messenger.Default.Send(new CurrentNewspaperItemChangedMessage(vm));
        }

        private void DeleteCommandHandler(RoutedEventArgs args)
        {
            if ((args as KeyEventArgs).Key != Key.Delete)
                return;

            var item = args.OriginalSource as TreeViewItem;
            var parent = AdvertisementCollectionViewModel.GetParentItem(item);
            var tv = args.Source as TreeView;
            var deletedNewspaperItem = tv.SelectedItem as NewspaperItemViewModel;
            var deletedAd = tv.SelectedItem as AdvertisementItemViewModel;

            if (deletedNewspaperItem != null)
            {
                deletedNewspaperItem.DbStatus = DbModificationState.Deleted;
                Newspapers.Remove(deletedNewspaperItem);
                deletedNewspaperItem.Advertisements.ToList().ForEach(a => { a.DbStatus = DbModificationState.Modified; });

                SaveCommandHandler(deletedNewspaperItem);
            }
            else if (deletedAd != null)
            {
                var paper = parent.Header as NewspaperItemViewModel;
                paper.Advertisements.Remove(deletedAd);
                deletedAd.Newspapers.Remove(paper);
            }
        }

        private void DetailViewModelReadyHandler(NewspaperDetailViewModelReady obj)
        {
            Messenger.Default.Send(new CurrentNewspaperItemChangedMessage(CurrentItem));
        }

        private void DropAdHandler(DragEventArgs obj)
        {
            return;
        }

        private void NewspapersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var newItems = new List<NewspaperItemViewModel>();

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems != null)
                {
                    foreach (NewspaperItemViewModel vm in e.NewItems)
                    {
                        CurrentItem = vm;
                        _adRepository.Save(vm.Model);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems != null)
                {
                    foreach (NewspaperItemViewModel deletedPaper in e.OldItems)
                    {
                        CurrentItem = Newspapers.Count > 0
                                          ? Newspapers[
                                              e.OldStartingIndex < Newspapers.Count ? e.OldStartingIndex : Newspapers.Count - 1]
                                          : null;

                        deletedPaper.DbStatus = DbModificationState.Deleted;
                        var unlinkedAds = deletedPaper.Advertisements.ToList();

                        unlinkedAds.ForEach(a => a.Newspapers.Remove(deletedPaper));
                        _adRepository.Save(deletedPaper.Model);
                    }
                }
            }
        }

        private void RemovingItemMessageHandler(RemovingNewspaperItemMessage message)
        {
            Newspapers.Remove(message.ItemViewModel);
            Messenger.Default.Send<CurrentNewspaperItemChangedMessage>(new CurrentNewspaperItemChangedMessage(CurrentItem));
        }

        private void SaveCommandHandler(NewspaperItemViewModel viewModel)
        {
            if (inSave)
                return;

            inSave = true;
            _adRepository.Save(viewModel.Model);
            var sortedList = Newspapers.OrderBy(n => n.Name).ToList();
            Newspapers.Clear();
            sortedList.ForEach(n => Newspapers.Add(n));
            inSave = false;
        }

        private void SaveItemMessageHandler(SaveNewspaperItemMessage msg)
        {
            SaveCommand.Execute(msg.ItemViewModel);
        }

        private void SelectedItemChangedCommandHandler(RoutedPropertyChangedEventArgs<object> obj)
        {
            var paperItem = obj.NewValue as NewspaperItemViewModel;
            var adItem = obj.NewValue as AdvertisementItemViewModel;

            if (paperItem != null)
            {
                CurrentItem = (NewspaperItemViewModel) (obj.NewValue) ??
                              new NewspaperItemViewModel(_adRepository) {Model = new Newspaper()};
                Messenger.Default.Send(new CurrentNewspaperItemChangedMessage(CurrentItem));
            }
        }

        #endregion
    }
}