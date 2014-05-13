using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using GalaSoft.MvvmLight.Messaging;
using Models;
using NSubstitute;
using NSubstitute.Core;
using Xunit;

namespace PillarKata.ViewModels.Tests.IntegrationTests
{
    public class NewspaperCollectionIntegrationTests
    {
        // ReSharper disable InconsistentNaming
        #region Class Members

        [Fact]
        public void AddingItemMessageHandler_WhenInvoked_CallsRepositorySaveWithNewItem()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperAdRepository>();
            var message = new AddingNewspaperItemMessage();
            var collectionViewModel = new NewspaperCollectionViewModel(repository);
            collectionViewModel.Newspapers.Count.Should().Be(0, "There are no items in the collection initially.");

            //	Act
            Messenger.Default.Send(message);

            //	Assert
            repository.Received().Save(collectionViewModel.Newspapers.First().Model);
        }

        [Fact]
        public void AddingItemMessage_WhenReceived_AddsNewNewspaperToCollection()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperAdRepository>();
            var detailViewModel = new NewspaperDetailViewModel(repository);
            var collectionViewModel = new NewspaperCollectionViewModel(repository);
            var countBeforeAdd = collectionViewModel.Newspapers.Count;
            countBeforeAdd.Should().Be(0, "The collection should be empty");

            //	Act
            detailViewModel.AddItemCommand.Execute(null);
            var countAfterAdd = collectionViewModel.Newspapers.Count;

            //	Assert
            countAfterAdd.Should().Be(1, "One item should have been added.");
        }

        [Fact]
        public void DeleteCommand_WhenItemDeletedFromTheCollection_CallsRepositoryDelete()
        {
            var repository = Substitute.For<INewspaperAdRepository>();
            repository.GetAllAdvertisements().Returns(new List<Advertisement>());
            bool deleteCalled;
            Newspaper deletedModel;

            var model1 = new Newspaper {Name = "Paper 1"};
            var model2 = new Newspaper {Name = "Paper 2"};
            var paperItemVm1 = new NewspaperItemViewModel(repository) {Model = model1};
            var paperItemVm2 = new NewspaperItemViewModel(repository) {Model = model2};
            var collectionViewModel = new NewspaperCollectionViewModel(repository);

            repository.When(x => x.Delete(model1)).Do(x => RepositoryUpdateFields(x, out deleteCalled, out deletedModel));

            collectionViewModel.Newspapers.Add(paperItemVm1);
            collectionViewModel.Newspapers.Add(paperItemVm2);

            //	Act
            collectionViewModel.Newspapers.Remove(paperItemVm1);
            paperItemVm1.DbStatus.Should().Be(DbModificationState.Deleted, "State of object should be deleted.");

            //	Assert
            repository.Received().Save(model1);
        }

        [Fact]
        public void SaveCommand_WhenInvoked_CallsRepositorySaveOnCurrentItem()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperAdRepository>();
            var saveCalled = false;
            Newspaper savedModel;

            var model1 = new Newspaper {Name = "Paper 1"};
            var paperItemVm1 = new NewspaperItemViewModel(repository) {Model = model1};
            var collectionViewModel = new NewspaperCollectionViewModel(repository);

            repository.When(x => x.Save(model1)).Do(x => RepositoryUpdateFields(x, out saveCalled, out savedModel));
            collectionViewModel.Newspapers.Add(paperItemVm1);
            var currentItem = collectionViewModel.CurrentItem;

            //	Act
            collectionViewModel.SaveCommand.Execute(paperItemVm1);

            //	Assert
            repository.Received().Save(model1);
            currentItem.Model.Should().Be(model1, "The Save was called with the added model.");
        }

        [Fact]
        public void SaveCommand_WhenItemAdded_CallsRepositorySaveOnCurrentItem()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperAdRepository>();
            var model1 = new Newspaper {Name = "Paper 1"};
            var model2 = new Newspaper {Name = "Paper 2"};
            var model3 = new Newspaper {Name = "Paper 3"};
            var model4 = new Newspaper {Name = "Paper 4"};
            var paperItemVm1 = new NewspaperItemViewModel(repository) {Model = model1};
            var paperItemVm2 = new NewspaperItemViewModel(repository) {Model = model2};
            var paperItemVm3 = new NewspaperItemViewModel(repository) {Model = model3};
            var paperItemVm4 = new NewspaperItemViewModel(repository) {Model = model4};
            var collectionViewModel = new NewspaperCollectionViewModel(repository);

            //	Act
            collectionViewModel.Newspapers.Add(paperItemVm1);
            collectionViewModel.Newspapers.Add(paperItemVm2);
            collectionViewModel.Newspapers.Add(paperItemVm3);

            //	Assert
            repository.Received().Save(model1);
            repository.Received().Save(model2);
            repository.Received().Save(model3);
            repository.DidNotReceive().Save(paperItemVm4.Model);
        }

        [Fact]
        public void SavingItemMessageHandler_WhenInvoked_CallsRepositorySaveOnCurrentItem()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperAdRepository>();
            var model1 = new Newspaper {Name = "Paper 1"};
            var paperItemVm1 = new NewspaperItemViewModel(repository) {Model = model1};
            var collectionViewModel = new NewspaperCollectionViewModel(repository);
            collectionViewModel.CurrentItem.Should().Be(null, "There is no current item in an empty list.");

            //	Act
            collectionViewModel.Newspapers.Add(paperItemVm1);

            //	Assert
            repository.Received().Save(model1);
        }

        #endregion

        #region Utility Routines

        private void RepositoryUpdateFields(CallInfo callInfo, out bool save1Called, out Newspaper savedModel)
        {
            save1Called = true;
            savedModel = (Newspaper) (callInfo.Args()[0]);
        }

        #endregion Utility Routines
    }
}