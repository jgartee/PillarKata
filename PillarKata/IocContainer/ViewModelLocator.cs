using System;
using Autofac;
using PillarKata.ViewModels;

namespace PillarKata.IocContainer
{
    public class ViewModelLocator
    {
        #region Properties

        private NewspaperAndAdvertisementListSynchronizer syncObject = new NewspaperAndAdvertisementListSynchronizer();

        public AdvertisementCollectionViewModel AdvertisementCollection
        {
            get { return IocAutofac.Container.Resolve<AdvertisementCollectionViewModel>(); }
        }

        public AdvertisementDetailViewModel AdvertisementDetail
        {
            get
            {
                AdvertisementDetailViewModel model = null;
                try
                {
                    model = IocAutofac.Container.Resolve<AdvertisementDetailViewModel>();
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                }

                return IocAutofac.Container.Resolve<AdvertisementDetailViewModel>();
            }
        }

        public AdvertisementItemViewModel AdvertisementItem
        {
            get { return IocAutofac.Container.Resolve<AdvertisementItemViewModel>(); }
        }

        public MainWindowViewModel Main
        {
            get { return IocAutofac.Container.Resolve<MainWindowViewModel>(); }
        }

        public NewspaperCollectionViewModel NewspaperCollection
        {
            get
            {
                NewspaperCollectionViewModel vm;
                try
                {
                    vm = IocAutofac.Container.Resolve<NewspaperCollectionViewModel>();
                }
                catch (Exception e)
                {
                    vm = null;
                }
                return vm;
            }
        }

        public NewspaperDetailViewModel NewspaperDetail
        {
            get { return IocAutofac.Container.Resolve<NewspaperDetailViewModel>(); }
        }

        public NewspaperItemViewModel NewspaperItem
        {
            get { return IocAutofac.Container.Resolve<NewspaperItemViewModel>(); }
        }

        #endregion
    }
}