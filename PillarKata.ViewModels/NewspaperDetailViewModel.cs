using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Models;

namespace PillarKata.ViewModels
{
    public class NewspaperDetailViewModel : ViewModelBase
    {
        #region Instance fields

        private readonly IRepository<Newspaper, Guid> _repository;
        private AdvertisementItemViewModel _currentAd;
        private NewspaperItemViewModel _itemViewModel;
        private string _name;

        #endregion

        #region Constructors

        public NewspaperDetailViewModel(INewspaperRepository repository)
        {
            _repository = repository;
            Advertisements = new ObservableCollection<AdvertisementItemViewModel>();
            Messenger.Default.Send(new NewspaperDetailViewModelReady());
            Messenger.Default.Register<CurrentNewspaperItemChangedMessage>(this, (message) =>
                                                                                 {
                                                                                     if (message.ItemViewModel != null)
                                                                                     {
                                                                                         ItemViewModel = message.ItemViewModel;
                                                                                         _name = ItemViewModel.Name;
                                                                                     }
                                                                                     else
                                                                                         _name = "";

                                                                                     RaisePropertyChanged(() => AllowSave);
                                                                                 });
            Messenger.Default.Register<CurrentAdvertisementItemChangedMessage>(this,
                                                                               (message) =>
                                                                               {
                                                                                   _currentAd = message.ItemViewModel;
                                                                                   RaisePropertyChanged(() => AllowSave);
                                                                               });
            AddItemCommand = new RelayCommand(AddItemCommandHandler);
            CancelItemCommand = new RelayCommand(CancelItemCommandHandler);
            SaveItemCommand = new RelayCommand(SaveItemCommandHandler);
            DeleteItemCommand = new RelayCommand(DeleteItemCommandHandler);
            LinkToCurentAd = new RelayCommand(LinkToCurrentAdHandler);
        }

        #endregion

        #region Properties

        public ICommand AddItemCommand { get; private set; }
        public ObservableCollection<AdvertisementItemViewModel> Advertisements { get; set; }

        public bool AllowSave
        {
            get { return ItemViewModel != null && !string.IsNullOrEmpty(_name); }
        }

        public ICommand CancelItemCommand { get; private set; }
        public ICommand DeleteItemCommand { get; private set; }

        public NewspaperItemViewModel ItemViewModel
        {
            get { return _itemViewModel; }
            set
            {
                _itemViewModel = value;
                _name = ItemViewModel.Name;
                Advertisements.Clear();
                ItemViewModel.Advertisements.ToList().ForEach(a => Advertisements.Add(a));
                ItemViewModel.Advertisements.ToList().ForEach((a) =>
                                                              {
                                                                  if (!a.Newspapers.Contains(ItemViewModel))
                                                                      a.Newspapers.Add(ItemViewModel);
                                                              });
                RaisePropertyChanged(() => ItemViewModel);
                RaisePropertyChanged(() => Name);
                RaisePropertyChanged(() => Advertisements);
            }
        }

        public ICommand LinkToCurentAd { get; private set; }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged(() => Name);
                RaisePropertyChanged(() => AllowSave);
            }
        }

        public ICommand SaveItemCommand { get; private set; }

        public DbModificationState DbStatus
        {
            get { return ItemViewModel.DbStatus; }
            set { ItemViewModel.DbStatus = value; }
        }

        #endregion

        #region Class Members

        private void AddItemCommandHandler()
        {
            Messenger.Default.Send(new AddingNewspaperItemMessage());
        }

        private void CancelItemCommandHandler()
        {
            _name = ItemViewModel.Name;
        }

        private void DeleteItemCommandHandler()
        {
            Messenger.Default.Send(new RemovingNewspaperItemMessage(ItemViewModel));
        }

        private void LinkToCurrentAdHandler()
        {
//            if(!_currentAd.Newspapers.Contains(ItemViewModel))
                _currentAd.Newspapers.Add(ItemViewModel);

            Messenger.Default.Send(new SaveNewspaperItemMessage(ItemViewModel));
        }

        private void SaveItemCommandHandler()
        {
            ItemViewModel.Name = _name;
            Messenger.Default.Send(new SaveNewspaperItemMessage(ItemViewModel));
        }

        #endregion
    }
}