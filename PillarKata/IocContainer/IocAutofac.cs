using System;
using System.Collections.Generic;
using Autofac;
using Data;
using Models;
using PillarKata.ViewModels;

namespace PillarKata.IocContainer
{
    public class IocAutofac
    {
        #region Properties

        public static IContainer Container { get; set; }

        #endregion

        #region Class Members

        public static void RegisterComponents()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<MainWindow>().AsSelf();
            builder.RegisterType<NewspaperSerializer>().As<ISerializer<NewspaperCache>>().SingleInstance();
            builder.RegisterType<NewspaperCache>().AsSelf().SingleInstance();
            builder.RegisterType<AdvertisementCache>().AsSelf().SingleInstance();
//            builder.RegisterType<NewspaperRepository>().As<IRepository<Newspaper, Guid>>().SingleInstance();
            builder.RegisterType<NewspaperAdRepository>().As<INewspaperAdRepository>().SingleInstance();
            builder.RegisterType<MainWindowViewModel>().AsSelf();
            builder.RegisterType<NewspaperItemViewModel>().AsSelf();
            builder.RegisterType<NewspaperDetailViewModel>().AsSelf();
            builder.RegisterType<NewspaperCollectionViewModel>().AsSelf();
            builder.RegisterType<AdvertisementItemViewModel>().AsSelf();
            builder.RegisterType<AdvertisementDetailViewModel>().AsSelf();
            builder.RegisterType<AdvertisementCollectionViewModel>().AsSelf();
            Container = builder.Build();
        }

        #endregion
    }
}