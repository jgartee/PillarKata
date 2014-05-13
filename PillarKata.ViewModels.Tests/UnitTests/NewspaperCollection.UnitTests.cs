using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using FluentAssertions;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Models;
using NSubstitute;
using Xunit;

namespace PillarKata.ViewModels.Tests.UnitTests
{
    public class NewspaperCollectionUnitTests : ViewModelBase
    {
        // ReSharper disable InconsistentNaming

        #region Class Members

        [Fact]
        public void AddingItemMessageHandler_WhenInvoked_AddsNewItemToNewspapersCollection()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var message = new AddingNewspaperItemMessage();
            var collectionViewModel = new NewspaperCollectionViewModel(repository);
            collectionViewModel.Newspapers.Count.Should().Be(0, "There are no items in the collection initially.");
            //	Act
            Messenger.Default.Send(message);

            //	Assert
            collectionViewModel.Newspapers.Count.Should().Be(1, "A new item should have been added to the collection.");
            collectionViewModel.Newspapers.First().Name.Should().Be(Newspaper.MSG_NEW_NEWSPAPER_NAME,
                                                                    "The new item should be a new Newspaper.");
        }

        [Fact]
        public void AddingItemMessageHandler_WhenInvoked_SendsCurrentItemChangedMessage()
        {
            //  Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var message = new AddingNewspaperItemMessage();
            var collectionViewModel = new NewspaperCollectionViewModel(repository);
            var changedMessageReceived = false;
            NewspaperItemViewModel receivedItemViewModel = null;

            collectionViewModel.Newspapers.Count.Should().Be(0, "There are no items in the collection initially.");
            Messenger.Default.Register<CurrentNewspaperItemChangedMessage>(this, msg =>
                                                                                 {
                                                                                     changedMessageReceived = true;
                                                                                     receivedItemViewModel = msg.ItemViewModel;
                                                                                 });
            //	Act
            Messenger.Default.Send(message);

            //	Assert
            changedMessageReceived.Should().Be(true, "The message was sent from the NewspaperCollectionsViewModel.");
            receivedItemViewModel.Should().Be(collectionViewModel.Newspapers.First(),
                                              "The item received in the message matches the CurrentItem in the collection.");
        }

        [Fact]
        public void CurrentItem_AfterDeletingLastItemOnNewspapersCollection_SetToLastItemInNewspapersCollection()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var model1 = new Newspaper {Name = "Paper 1"};
            var model2 = new Newspaper {Name = "Paper 2"};
            var model3 = new Newspaper {Name = "Paper 3"};
            var paperItemVm1 = new NewspaperItemViewModel(repository) {Model = model1};
            var paperItemVm2 = new NewspaperItemViewModel(repository) {Model = model2};
            var paperItemVm3 = new NewspaperItemViewModel(repository) {Model = model3};
            var collectionViewModel = new NewspaperCollectionViewModel(repository);
            collectionViewModel.Newspapers.Add(paperItemVm1);
            collectionViewModel.Newspapers.Add(paperItemVm2);
            collectionViewModel.Newspapers.Add(paperItemVm3);
            collectionViewModel.CurrentItem.Should().Be(paperItemVm3, "CurrentItem should be the last item added.");

            //	Act
            collectionViewModel.Newspapers.Remove(paperItemVm3);

            //	Assert
            collectionViewModel.CurrentItem.Should().Be(paperItemVm2,
                                                        "Removing the last entry makes the current last entry the CurrentItem");
        }

        [Fact]
        public void CurrentItem_AfterDeletingNotLastExistingItem_IsTheFollowingItem()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var model1 = new Newspaper {Name = "Paper 1"};
            var model2 = new Newspaper {Name = "Paper 2"};
            var model3 = new Newspaper {Name = "Paper 3"};
            var model4 = new Newspaper {Name = "Paper 4"};
            var paperItemVm1 = new NewspaperItemViewModel(repository) {Model = model1};
            var paperItemVm2 = new NewspaperItemViewModel(repository) {Model = model2};
            var paperItemVm3 = new NewspaperItemViewModel(repository) {Model = model3};
            var paperItemVm4 = new NewspaperItemViewModel(repository) {Model = model4};
            var collectionViewModel = new NewspaperCollectionViewModel(repository);
            collectionViewModel.Newspapers.Add(paperItemVm1);
            collectionViewModel.Newspapers.Add(paperItemVm2);
            collectionViewModel.Newspapers.Add(paperItemVm3);
            collectionViewModel.Newspapers.Add(paperItemVm4);
            collectionViewModel.CurrentItem.Should().Be(paperItemVm4, "CurrentItem should be the last item added.");

            //	Act
            collectionViewModel.Newspapers.Remove(paperItemVm2);

            //	Assert
            collectionViewModel.CurrentItem.Should().Be(paperItemVm3,
                                                        "CurrentItem should be the first item following the one removed");
        }

        [Fact]
        public void CurrentItem_AfterNewItemAdded_IsNewItem()
        {
            //	Arrange

            var repository = Substitute.For<INewspaperRepository>();
            var model = new Newspaper();
            var itemViewModel = new NewspaperItemViewModel(repository) {Model = model};
            var collectionViewModel = new NewspaperCollectionViewModel(repository);

            //	Act
            collectionViewModel.Newspapers.Add(itemViewModel);

            //	Assert
            collectionViewModel.CurrentItem.Should().Be(itemViewModel, "After adding a new item, it becomes the CurrentItem.");
        }

        [Fact]
        public void CurrentItem_WhenItemDeletedThatIsNotInTheCollection_DoesNothing()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var model1 = new Newspaper {Name = "Paper 1"};
            var model2 = new Newspaper {Name = "Paper 2"};
            var model3 = new Newspaper {Name = "Paper 3"};
            var paperItemVm1 = new NewspaperItemViewModel(repository) {Model = model1};
            var paperItemVm2 = new NewspaperItemViewModel(repository) {Model = model2};
            var paperItemVm3 = new NewspaperItemViewModel(repository) {Model = model3};
            var collectionViewModel = new NewspaperCollectionViewModel(repository);
            collectionViewModel.Newspapers.Add(paperItemVm1);
            collectionViewModel.Newspapers.Add(paperItemVm2);
            collectionViewModel.CurrentItem.Should().Be(paperItemVm2, "CurrentItem should be the last item added.");
            var count = collectionViewModel.Newspapers.Count;
            var itemsInCollection = collectionViewModel.Newspapers.ToList();

            //	Act
            collectionViewModel.Newspapers.Remove(paperItemVm3);

            //	Assert
            collectionViewModel.Newspapers.Count.Should().Be(count, "The number of papers in the collection should not change");
            collectionViewModel.Newspapers.ShouldBeEquivalentTo(itemsInCollection, "The collection remains unchanged");
        }

        [Fact]
        public void DetailViewModelReadyMessageHandler_WhenReceived_SendsCurrentItemAsCurrentItemChangedMessage()
        {
            //  Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var model1 = new Newspaper {Name = "Paper 1"};
            var model2 = new Newspaper {Name = "Paper 2"};
            var model3 = new Newspaper {Name = "Paper 3"};
            var paperItemVm1 = new NewspaperItemViewModel(repository) {Model = model1};
            var paperItemVm2 = new NewspaperItemViewModel(repository) {Model = model2};
            var paperItemVm3 = new NewspaperItemViewModel(repository) {Model = model3};
            var collectionViewModel = new NewspaperCollectionViewModel(repository);
            collectionViewModel.Newspapers.Add(paperItemVm1);
            collectionViewModel.Newspapers.Add(paperItemVm2);
            collectionViewModel.Newspapers.Add(paperItemVm3);
            collectionViewModel.CurrentItem.Should().Be(paperItemVm3, "CurrentItem should be the last item added.");
            NewspaperItemViewModel itemFromMessage = paperItemVm2;
            Messenger.Default.Register<CurrentNewspaperItemChangedMessage>(this,
                                                                           (message) => itemFromMessage = message.ItemViewModel);

            //  Act
            Messenger.Default.Send(new NewspaperDetailViewModelReady());

            //  Assert

            itemFromMessage.Model.Should().Be(model3,
                                              "When the NewspaperDetailViewModelIsReady the collection's current item is sent");
        }

        [Fact]
        public void Newspapers_AddNewItem_InsertsItemInTheCollection()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var model1 = new Newspaper {Name = "Paper 1"};
            var paperItemVm1 = new NewspaperItemViewModel(repository) {Model = model1};
            var collectionViewModel = new NewspaperCollectionViewModel(repository);

            //	Act
            collectionViewModel.Newspapers.Add(paperItemVm1);

            //	Assert
            collectionViewModel.Newspapers.Count.Should().Be(1, "There is one item in the collection");
            collectionViewModel.Newspapers.First().Should().Be(paperItemVm1,
                                                               "The item added was the same as the paper we created.");
        }

        [Fact]
        public void Newspapers_DeleteExistingItem_RemovesItemFromCollection()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var model1 = new Newspaper {Name = "Paper 1"};
            var model2 = new Newspaper {Name = "Paper 2"};
            var model3 = new Newspaper {Name = "Paper 3"};
            var paperItemVm1 = new NewspaperItemViewModel(repository) {Model = model1};
            var paperItemVm2 = new NewspaperItemViewModel(repository) {Model = model2};
            var paperItemVm3 = new NewspaperItemViewModel(repository) {Model = model3};
            var collectionViewModel = new NewspaperCollectionViewModel(repository);
            collectionViewModel.Newspapers.Add(paperItemVm1);
            collectionViewModel.Newspapers.Add(paperItemVm2);
            collectionViewModel.Newspapers.Add(paperItemVm3);
            var itemsInCollection = new List<NewspaperItemViewModel>() {paperItemVm1, paperItemVm3};

            //	Act
            collectionViewModel.Newspapers.Remove(paperItemVm2);

            //	Assert
            collectionViewModel.Newspapers.ShouldBeEquivalentTo(itemsInCollection,
                                                                "The items in the object match the items in the list.");
        }

        [Fact]
        public void RemovingItemMessageHandler_WhenInvoked_CallsRepositoryDeleteWithDeletedItem()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();

            var model1 = new Newspaper {Name = "Paper 1"};
            var model2 = new Newspaper {Name = "Paper 2"};
            var model3 = new Newspaper {Name = "Paper 3"};

            var paperItemVm1 = new NewspaperItemViewModel(repository) {Model = model1};
            var paperItemVm2 = new NewspaperItemViewModel(repository) {Model = model2};
            var paperItemVm3 = new NewspaperItemViewModel(repository) {Model = model3};

            var collectionViewModel = new NewspaperCollectionViewModel(repository);
            collectionViewModel.Newspapers.Add(paperItemVm1);
            collectionViewModel.Newspapers.Add(paperItemVm2);
            collectionViewModel.Newspapers.Add(paperItemVm3);

            collectionViewModel.CurrentItem.Should().Be(paperItemVm3, "CurrentItem should be the last item added.");
            collectionViewModel.Newspapers.Count.Should().Be(3, "There are now three items in the list.");

            var message = new RemovingNewspaperItemMessage(paperItemVm2);

            //	Act
            Messenger.Default.Send<RemovingNewspaperItemMessage>(message);

            //	Assert
            repository.Received().Save(model2);
        }

        [Fact]
        public void RemovingItemMessageHandler_WhenInvoked_RemovesItemFromNewspapersCollection()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();

            var model1 = new Newspaper {Name = "Paper 1"};
            var model2 = new Newspaper {Name = "Paper 2"};
            var model3 = new Newspaper {Name = "Paper 3"};

            var paperItemVm1 = new NewspaperItemViewModel(repository) {Model = model1};
            var paperItemVm2 = new NewspaperItemViewModel(repository) {Model = model2};
            var paperItemVm3 = new NewspaperItemViewModel(repository) {Model = model3};

            var collectionViewModel = new NewspaperCollectionViewModel(repository);
            collectionViewModel.Newspapers.Add(paperItemVm1);
            collectionViewModel.Newspapers.Add(paperItemVm2);
            collectionViewModel.Newspapers.Add(paperItemVm3);

            collectionViewModel.CurrentItem.Should().Be(paperItemVm3, "CurrentItem should be the last item added.");
            collectionViewModel.Newspapers.Count.Should().Be(3, "There are now three items in the list.");

            var message = new RemovingNewspaperItemMessage(paperItemVm2);

            //	Act
            Messenger.Default.Send<RemovingNewspaperItemMessage>(message);

            //	Assert
            collectionViewModel.Newspapers.Count.Should().Be(2, "An item should have been removed from the collection.");
            collectionViewModel.Newspapers.ShouldBeEquivalentTo(new List<NewspaperItemViewModel>() {paperItemVm1, paperItemVm3},
                                                                "Item paperItemVm2 was removed, leaving paperItemVm1 and paperItemVm3 in the list.");
        }

        [Fact]
        public void RemovingItemMessageHandler_WhenInvoked_SendsCurrentItemChangedMessage()
        {
            //	Arrange
            var itemChangedMessageReceived = false;
            NewspaperItemViewModel changedItemViewModel = null;
            var repository = Substitute.For<INewspaperRepository>();

            var model1 = new Newspaper {Name = "Paper 1"};
            var model2 = new Newspaper {Name = "Paper 2"};
            var model3 = new Newspaper {Name = "Paper 3"};

            var paperItemVm1 = new NewspaperItemViewModel(repository) {Model = model1};
            var paperItemVm2 = new NewspaperItemViewModel(repository) {Model = model2};
            var paperItemVm3 = new NewspaperItemViewModel(repository) {Model = model3};

            var collectionViewModel = new NewspaperCollectionViewModel(repository);
            collectionViewModel.Newspapers.Add(paperItemVm1);
            collectionViewModel.Newspapers.Add(paperItemVm2);
            collectionViewModel.Newspapers.Add(paperItemVm3);

            collectionViewModel.CurrentItem.Should().Be(paperItemVm3, "CurrentItem should be the last item added.");
            collectionViewModel.Newspapers.Count.Should().Be(3, "There are now three items in the list.");

            var message = new RemovingNewspaperItemMessage(paperItemVm2);
            Messenger.Default.Register<CurrentNewspaperItemChangedMessage>(this, (msg) =>
                                                                                 {
                                                                                     itemChangedMessageReceived = true;
                                                                                     changedItemViewModel = msg.ItemViewModel;
                                                                                 });

            //	Act
            Messenger.Default.Send<RemovingNewspaperItemMessage>(message);

            //	Assert
            itemChangedMessageReceived.Should().Be(true, "The message was received from the collection view model.");
            changedItemViewModel.Should().Be(collectionViewModel.CurrentItem,
                                             "The item sent is the current item from the collection view model.");
        }

        [Fact]
        public void SaveCommand_WhenPopulatedAndNewItemAdded_IsExecuted()
        {
            //	Arrange
            var saveCommandCalled = false;
            var repository = Substitute.For<INewspaperRepository>();
            var model1 = new Newspaper {Name = "Paper 1"};
            var paperItemVm1 = new NewspaperItemViewModel(repository) {Model = model1};
            var collectionViewModel = new NewspaperCollectionViewModel(repository);
            collectionViewModel.SaveCommand = new RelayCommand(() => { saveCommandCalled = true; });

            //	Act
            collectionViewModel.Newspapers.Add(paperItemVm1);

            //	Assert
            repository.Received().Save(model1);
            //saveCommandCalled.Should().Be(true, "Adding an item to the list causes the contents of SaveCommand to be executed");
        }

        [Fact]
        public void SelectedItemChangedHandler_WhenCollectionChanged_IsCalled()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var collectionChangedCalled = false;
            var model1 = new Newspaper {Name = "Paper 1"};
            var paperItemVm1 = new NewspaperItemViewModel(repository) {Model = model1};
            var collectionViewModel = new NewspaperCollectionViewModel(repository);
            collectionViewModel.SelectedItemChangedCommand =
                new RelayCommand<RoutedPropertyChangedEventArgs<object>>((args) => { collectionChangedCalled = true; });
            collectionViewModel.Newspapers.Add(paperItemVm1);

            //	Act
            collectionViewModel.SelectedItemChangedCommand.Execute(null);
            //	Assert
            collectionChangedCalled.Should().Be(true, "The event was called");
        }

        [Fact]
        public void SelectedItemChangedHandler_WhenInvoked_SendsMessageNewspaperChanged()
        {
            //	Arrange
            var messageReceived = false;
            NewspaperItemViewModel vm = null;
            var repository = GetNewspaperRepository();
            var model1 = new Newspaper {Name = "Paper 1"};
            var model2 = new Newspaper {Name = "Paper 2"};
            var paperItemVm1 = new NewspaperItemViewModel(repository) {Model = model1};
            var paperItemVm2 = new NewspaperItemViewModel(repository) {Model = model2};
            var collectionViewModel = new NewspaperCollectionViewModel(repository);
            Messenger.Default.Register<CurrentNewspaperItemChangedMessage>(this, (msg) =>
                                                                                 {
                                                                                     messageReceived = true;
                                                                                     vm = msg.ItemViewModel;
                                                                                 });

            collectionViewModel.Newspapers.Add(paperItemVm1);
            collectionViewModel.Newspapers.Add(paperItemVm2);
            var args = new RoutedPropertyChangedEventArgs<object>(null, paperItemVm2);

            //	Act
            collectionViewModel.SelectedItemChangedCommand.Execute(args);

            //	Assert
            messageReceived.Should().Be(true, "The CurrentItemChangedMessage message was sent.");
            collectionViewModel.CurrentItem.Should().Be(paperItemVm2, "The current item is the last one added.");
            vm.Model.Should().Be(paperItemVm2.Model, "The added model was sent in the message.");
        }

        [Fact]
        public void SelectedItemChangedHandler_WhenInvoked_SetsCurrentItemToNewSelection()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var model1 = new Newspaper {Name = "Paper 1"};
            var paperItemVm1 = new NewspaperItemViewModel(repository) {Model = model1};
            var collectionViewModel = new NewspaperCollectionViewModel(repository);
            collectionViewModel.CurrentItem.Should().Be(null, "There is no current item in an empty list.");

            //	Act
            collectionViewModel.Newspapers.Add(paperItemVm1);

            //	Assert
            collectionViewModel.CurrentItem.Should().Be(paperItemVm1, "The current item is now");
        }

        #endregion

        #region Utility Routines

        private static INewspaperRepository GetNewspaperRepository()
        {
            var repository = Substitute.For<INewspaperRepository>();
            repository.GetAllAdvertisements().Returns(new List<Advertisement>());
            return repository;
        }

        #endregion Utility Routines
    }
}