using System;
using System.Linq;
using FluentAssertions;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Models;
using NSubstitute;
using Xunit;

namespace PillarKata.ViewModels.Tests.IntegrationTests
{
    public class NewspaperDetailIntegrationTests : ViewModelBase
    {
        #region Class Members

        [Fact]
        public void AddAdvertisement_WithValidAd_AddsCurrentNewspaperItemToNewspapersCollectionInTheAddedAd()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var detailViewModel = new NewspaperDetailViewModel(repository);
            var collectionViewModel = new NewspaperCollectionViewModel(repository);
            var paperModel1 = new Newspaper() {Name = "New paper 1"};
            var paperItemViewModel1 = new NewspaperItemViewModel(repository) {Model = paperModel1};
            var adModel = new Advertisement() {Name = "Ad 1 Name", Text = "Ad 1 text."};
            var adItemViewModel = new AdvertisementItemViewModel(repository) {Model = adModel};
            collectionViewModel.Newspapers.Add(paperItemViewModel1);
            collectionViewModel.CurrentItem.Should().Be(paperItemViewModel1, "The current item is the item that was added.");
            detailViewModel.ItemViewModel.Should().Be(collectionViewModel.CurrentItem,
                                                      "The detail view model references the collection's current item.");
            paperItemViewModel1.Advertisements.Count.Should().Be(0, "No newspapers are referenced in the advertisement.");
            adItemViewModel.Newspapers.Count.Should().Be(0, "There are no papers in this ad's collection.");

            //	Act
            paperItemViewModel1.Advertisements.Add(adItemViewModel);

            //	Assert
            paperItemViewModel1.Advertisements.Count.Should().Be(1, "One item was added");
            adItemViewModel.Newspapers.Count.Should().Be(1, "One newspaper reference was added to this advertisement.");
            adItemViewModel.Newspapers.ToList().First().Should().Be(collectionViewModel.CurrentItem,
                                                                    "The paper added to the advertisment's newspaper collection was the Newspaper collection's CurrentItem");
        }

        [Fact]
        public void AddItemCommand_WhenInvokedAgainstEmptyNewspapersCollection_ReturnsItemViewModelAsCurrentItemInCollection()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var detailViewModel = new NewspaperDetailViewModel(repository);
            var collectionViewModel = new NewspaperCollectionViewModel(repository);

            //	Act
            detailViewModel.AddItemCommand.Execute(null);

            //	Assert
            detailViewModel.ItemViewModel.Should().Be(collectionViewModel.CurrentItem,
                                                      "The detail view model should be the CurrentItem in the collection.");
        }

        [Fact]
        public void NewspaperDetailItem_OnCreate_SendsNewspaperDetailViewModelReadyMessage()
        {
            //	Arrange
            var messageSent = false;
            var repository = Substitute.For<INewspaperRepository>();
            Messenger.Default.Register<NewspaperDetailViewModelReady>(this, (newspaperReady) => { messageSent = true; });

            //	Act
            var detailViewModel = new NewspaperDetailViewModel(repository);

            //	Assert
            messageSent.Should().Be(true, "The message was sent.");
        }

        [Fact]
        public void NewspaperDetailItem_WhenNewspaperCurrentItemChangedMessageReceived_ResetsItemViewModelToValueSentInMessage()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var detailViewModel = new NewspaperDetailViewModel(repository);
            var collectionViewModel = new NewspaperCollectionViewModel(repository);
            var paperModel1 = new Newspaper() {Name = "New paper 1"};
            var paperModel2 = new Newspaper() {Name = "New paper 2"};
            var paperItemViewModel1 = new NewspaperItemViewModel(repository) {Model = paperModel1};
            var paperItemViewModel2 = new NewspaperItemViewModel(repository) {Model = paperModel2};
            collectionViewModel.Newspapers.Add(paperItemViewModel1);
            collectionViewModel.Newspapers.Add(paperItemViewModel2);
            collectionViewModel.CurrentItem.Should().Be(paperItemViewModel2);

            //	Act
            collectionViewModel.CurrentItem = paperItemViewModel1;

            //	Assert
            detailViewModel.ItemViewModel.Should().Be(paperItemViewModel1);
        }

        [Fact]
        public void RemoveAdvertisement_WithExistingAdvInAdsCollection_RemovesCurrentNewspaperReferenceFromAdNewspapersCollection()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var detailViewModel = new NewspaperDetailViewModel(repository);
            var collectionViewModel = new NewspaperCollectionViewModel(repository);
            var paperModel1 = new Newspaper() {Name = "New paper 1"};
            var paperItemViewModel1 = new NewspaperItemViewModel(repository) {Model = paperModel1};
            var adModel = new Advertisement() {Name = "Ad 1 Name", Text = "Ad 1 text."};
            var adItemViewModel = new AdvertisementItemViewModel(repository) {Model = adModel};

            collectionViewModel.Newspapers.Add(paperItemViewModel1);
            collectionViewModel.CurrentItem.Should().Be(paperItemViewModel1, "The current item is the item that was added.");
            detailViewModel.ItemViewModel.Should().Be(collectionViewModel.CurrentItem,
                                                      "The detail view model references the collection's current item.");
            paperItemViewModel1.Advertisements.Count.Should().Be(0, "No newspapers are referenced in the advertisement.");
            adItemViewModel.Newspapers.Count.Should().Be(0, "There are no papers in this ad's collection.");

            paperItemViewModel1.Advertisements.Add(adItemViewModel);
            paperItemViewModel1.Advertisements.Count.Should().Be(1, "One item was added");
            adItemViewModel.Newspapers.Count.Should().Be(1, "One newspaper reference was added to this advertisement.");
            adItemViewModel.Newspapers.ToList().First().Should().Be(collectionViewModel.CurrentItem,
                                                                    "The paper added to the advertisment's newspaper collection was the Newspaper collection's CurrentItem");

            //	Act
            paperItemViewModel1.Advertisements.Remove(adItemViewModel);

            //	Assert
            paperItemViewModel1.Advertisements.Count.Should().Be(0, "No items remain in the advertisement list");
            adItemViewModel.Newspapers.Count.Should().Be(0, "No items remain in the newspaper list");
        }

        [Fact]
        public void SaveCommand_WhenInvoked_UpdatesNewspaperNameInCollectionCurrentItem()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var detailViewModel = new NewspaperDetailViewModel(repository);
            var collectionViewModel = new NewspaperCollectionViewModel(repository);
            detailViewModel.AddItemCommand.Execute(null);
            var originalName = detailViewModel.Name;
            var newName = "Changed Newspaper Name";
            detailViewModel.Name = newName;
            Newspaper model = null;
            collectionViewModel.CurrentItem.Name.Should().Be(originalName, "Name does not change until Save command.");
            detailViewModel.ItemViewModel.Name.Should().Be(originalName, "Name does not change until Save command.");
            repository.WhenForAnyArgs(r => r.Save(detailViewModel.ItemViewModel.Model)).Do(
                (newspaper) => { model = (Newspaper) newspaper.Args()[0]; });
            //	Act
            detailViewModel.SaveItemCommand.Execute(new SaveNewspaperItemMessage(detailViewModel.ItemViewModel));

            //	Assert
            detailViewModel.Name.Should().Be(detailViewModel.ItemViewModel.Name, "ItemViewModel should have been updated");
            detailViewModel.ItemViewModel.Should().Be(collectionViewModel.CurrentItem,
                                                      "CurrentItem should be the same as detail ItemViewModel");
            repository.Received().Save(detailViewModel.ItemViewModel.Model);
        }

        #endregion
    }
}