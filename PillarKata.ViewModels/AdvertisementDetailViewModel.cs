using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Models;

namespace PillarKata.ViewModels
{
    public class AdvertisementDetailViewModel : ViewModelBase
    {
        #region Instance fields

        private readonly INewspaperRepository _repository;
        private AdvertisementItemViewModel _itemViewModel;
        private string _name;
        private string _text = "";

        #endregion

        #region Constructors

        private NewspaperItemViewModel _currentNewspaper;

        public AdvertisementDetailViewModel(INewspaperRepository repository)
        {
            _repository = repository;
            Newspapers = new ObservableCollection<NewspaperItemViewModel>();
            Messenger.Default.Register<CurrentAdvertisementItemChangedMessage>(this, (message) =>
                                                                                     {
                                                                                         
                                                                                         if (message.ItemViewModel != null)
                                                                                         {
                                                                                             ItemViewModel = message.ItemViewModel;
                                                                                             Name = ItemViewModel.Name;
                                                                                             Text = ItemViewModel.Text;
                                                                                         }
                                                                                         else
                                                                                         {
                                                                                             Name = Text = "";
                                                                                         }

                                                                                         RaisePropertyChanged(() => AllowSave);
                                                                                     });
            Messenger.Default.Register<CurrentNewspaperItemChangedMessage>(this, (message) =>
                                                                                 {
                                                                                     _currentNewspaper = message.ItemViewModel;
                                                                                 });
            AddItemCommand = new RelayCommand<RoutedEventArgs>(AddItemCommandHandler);
            CancelItemCommand = new RelayCommand(CancelItemCommandHandler);
            SaveItemCommand = new RelayCommand(SaveItemCommandHandler);
            DeleteItemCommand = new RelayCommand(DeleteItemCommandHandler);
            LinkToCurrentPaper = new RelayCommand(LinkToCurrentPaperHandler);
            Messenger.Default.Send(new AdvertisementDetailViewModelReady());
        }

        private void LinkToCurrentPaperHandler()
        {
//            if(!_currentNewspaper.Advertisements.Contains(ItemViewModel))
                _currentNewspaper.Advertisements.Add(ItemViewModel);
            Messenger.Default.Send(new SaveAdvertisementItemMessage(ItemViewModel));
        }

        #endregion

        #region Properties

        public ICommand AddItemCommand { get; private set; }
        public DbModificationState DbStatus
        {
            get { return ItemViewModel.DbStatus; }
            set { ItemViewModel.DbStatus = value; }
        }
        public bool AllowSave
        {
            get { return ItemViewModel != null && !string.IsNullOrEmpty(_name) && !string.IsNullOrEmpty(_text); }
        }

        public ICommand CancelItemCommand { get; private set; }
        public ICommand DeleteItemCommand { get; private set; }

        public AdvertisementItemViewModel ItemViewModel
        {
            get { return _itemViewModel; }
            set
            {
                _itemViewModel = value;

                if(_itemViewModel != null)
                {
                    Name = ItemViewModel.Name;
                    Text = ItemViewModel.Text;
                    Newspapers.Clear();
                    ItemViewModel.Newspapers.ToList().ForEach(a => Newspapers.Add(a));
                    ItemViewModel.Newspapers.ToList().ForEach((p) =>
                                                              {
                                                                  if (!p.Advertisements.Contains(ItemViewModel))
                                                                      p.Advertisements.Add(ItemViewModel);
                                                              });
                    
                }
                RaisePropertyChanged(()=>AllowSave);
                RaisePropertyChanged(() => ItemViewModel);
                RaisePropertyChanged(() => Newspapers);
            }
        }

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

        public ObservableCollection<NewspaperItemViewModel> Newspapers { get; set; }
        public ICommand SaveItemCommand { get; private set; }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                RaisePropertyChanged(() => Text);
                RaisePropertyChanged(() => AllowSave);
            }
        }

        public ICommand LinkToCurrentPaper { get; private set; }

        #endregion

        #region Class Members

        private void AddItemCommandHandler(RoutedEventArgs routedEventArgs)
        {
            Messenger.Default.Send(new AddingAdvertisementItemMessage());
        }

        private void CancelItemCommandHandler()
        {
            _name = ItemViewModel.Name;
            _text = ItemViewModel.Text;
        }

        private void DeleteItemCommandHandler()
        {
            Messenger.Default.Send(new RemovingAdvertisementItemMessage(ItemViewModel));
        }

        private void SaveItemCommandHandler()
        {
            ItemViewModel.Name = _name;
            ItemViewModel.Text = _text;
            Messenger.Default.Send(new SaveAdvertisementItemMessage(ItemViewModel));
        }

        #endregion
    }
}