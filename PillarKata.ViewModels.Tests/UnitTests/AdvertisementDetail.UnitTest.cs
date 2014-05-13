using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using GalaSoft.MvvmLight.Messaging;
using Models;
using NSubstitute;
using Xunit;

namespace PillarKata.ViewModels.Tests.UnitTests
{
    public class AdvertisementDetailUnitTests
    {
        #region Class Members

        [Fact]
        public void AddItemCommand_WhenInvoked_SendsMessageOfTypeAddingItemMessage()
        {
            var msgSent = false;
            var repository = Substitute.For<INewspaperRepository>();
            Messenger.Default.Register<AddingAdvertisementItemMessage>(this, (message) => { msgSent = true; });
            //	Act
            var viewModel = new AdvertisementDetailViewModel(repository);
            viewModel.AddItemCommand.Execute(null);

            //	Assert
            msgSent.Should().Be(true, "The message was sent.");
        }

        [Fact]
        public void AddNewspaper_WithValidNewspaper_AddsAdvertisementToNewspaperAdvertisementssCollection()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var advertisementModel = new Advertisement();
            var paperModel = new Newspaper();
            var newspaperItemViewModel = new NewspaperItemViewModel(repository) {Model = paperModel};
            var adDetailViewModel = new NewspaperDetailViewModel(repository) {ItemViewModel = newspaperItemViewModel};
            var detailViewModel = new AdvertisementDetailViewModel(repository)
                                  {ItemViewModel = new AdvertisementItemViewModel(repository) {Model = advertisementModel}};

            //	Act
            detailViewModel.Newspapers.Add(newspaperItemViewModel);
            //	Assert
            detailViewModel.Newspapers.ToList().First().Advertisements.Contains(detailViewModel.ItemViewModel);
        }

        [Fact]
        public void AddNewspaper_WithValidNewspaper_AddsNewspaperToNewspapersCollection()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var advertisementModel = new Advertisement();
            var paperModel = new Newspaper();
            var newspaperItemViewModel = new NewspaperItemViewModel(repository) {Model = paperModel};
            var adDetailViewModel = new NewspaperDetailViewModel(repository) {ItemViewModel = newspaperItemViewModel};
            var detailViewModel = new AdvertisementDetailViewModel(repository)
                                  {ItemViewModel = new AdvertisementItemViewModel(repository) {Model = advertisementModel}};

            //	Act
            detailViewModel.Newspapers.Add(newspaperItemViewModel);
            //	Assert
            detailViewModel.Newspapers.Count.Should().Be(1, "There should be one item in the collection");
        }

        [Fact]
        public void AdvertisementDetailViewModel_WhenCreated_SendsDetailViewModelReadyMessage()
        {
            //	Arrange
            var msgSent = false;
            // AdvertisementDetailViewModelReady msg = null;
            var repository = Substitute.For<INewspaperRepository>();
            Messenger.Default.Register<AdvertisementDetailViewModelReady>(this, (message) => { msgSent = true; });

            //	Act
            var viewModel = new AdvertisementDetailViewModel(repository);

            //	Assert
            msgSent.Should().Be(true, "The message was sent.");
        }

        [Fact]
        public void CancelCommand_WhenInvoked_ResetsNameToCollectionCurrentValue()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            repository.GetAllAdvertisements().Returns(new List<Advertisement>());
            var detailViewModel = new AdvertisementDetailViewModel(repository);
            var collectionViewModel = new AdvertisementCollectionViewModel(repository);
            detailViewModel.AddItemCommand.Execute(null);
            collectionViewModel.CurrentItem.Should().Be(detailViewModel.ItemViewModel,
                                                        "The detail model points to the Collection CurrentItem");
            var oldName = collectionViewModel.CurrentItem.Name;
            var newName = "Changed Advertisement Name";
            detailViewModel.Name = newName;
            collectionViewModel.CurrentItem.Name.Should().Be(oldName, "Name in the collection should not change");
            detailViewModel.Name.Should().Be(newName, "Alteration is reflected in the detail view model");

            //	Act
            detailViewModel.CancelItemCommand.Execute(null);

            //	Assert
            detailViewModel.Name.Should().Be(oldName, "The Advertisement name should revert back");
            collectionViewModel.CurrentItem.Should().Be(detailViewModel.ItemViewModel,
                                                        "The entire instance should have reverted back to the original value");
        }

        [Fact]
        public void Name_WhenNotNullOrEmpty_SetsAllowAddToTrue()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var model = new Advertisement();
            var itemViewModel = new AdvertisementItemViewModel(repository) {Model = model};

            //	Act
            var viewModel = new AdvertisementDetailViewModel(repository) {ItemViewModel = itemViewModel};

            //	Assert
            viewModel.AllowSave.Should().Be(true, "We have a valid name to reference.");
        }

        [Fact]
        public void Name_WhenNullOrEmpty_SetsAllowSaveToFalse()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var model = new Advertisement();
            var itemViewModel = new AdvertisementItemViewModel(repository) {Model = model};
            var viewModel = new AdvertisementDetailViewModel(repository) {ItemViewModel = itemViewModel};
            viewModel.AllowSave.Should().Be(true, "We have a valid name to reference.");

            //	Act
            viewModel.Name = "";

            //	Assert
            viewModel.AllowSave.Should().Be(false, "Cannot save an Advertisement without a name.");
        }

        [Fact]
        public void RemoveNewspaper_WithExistingNewspaperInNewspapersCollection_RemovesAdvertisementFromNewspaperRemoved()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var advertisementModel = new Advertisement();
            var paperModel1 = new Newspaper();
            var newspaperItemViewModel1 = new NewspaperItemViewModel(repository) {Model = paperModel1};
            var paperModel2 = new Newspaper();
            var newspaperItemViewModel2 = new NewspaperItemViewModel(repository) {Model = paperModel2};
            var adDetailViewModel = new NewspaperDetailViewModel(repository) {ItemViewModel = newspaperItemViewModel1};
            var detailViewModel = new AdvertisementDetailViewModel(repository)
                                  {ItemViewModel = new AdvertisementItemViewModel(repository) {Model = advertisementModel}};
            detailViewModel.Newspapers.Add(newspaperItemViewModel1);
            detailViewModel.Newspapers.Add(newspaperItemViewModel2);
            detailViewModel.Newspapers.Should().BeEquivalentTo(new List<NewspaperItemViewModel>
                                                               {newspaperItemViewModel1, newspaperItemViewModel2});
            detailViewModel.Newspapers.ToList().All(a => a.Advertisements.Contains(detailViewModel.ItemViewModel));
            //	Act
            detailViewModel.Newspapers.Remove(newspaperItemViewModel1);

            //	Assert
            detailViewModel.Newspapers.Should().BeEquivalentTo(new List<NewspaperItemViewModel> {newspaperItemViewModel2});
        }

        [Fact]
        public void RemoveNewspaper_WithExistingNewspaperInNewspapersCollection_RemovesNewspaperFromNewspapersCollection()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var advertisementModel = new Advertisement();
            var paperModel1 = new Newspaper();
            var newspaperItemViewModel1 = new NewspaperItemViewModel(repository) {Model = paperModel1};
            var paperModel2 = new Newspaper();
            var newspaperItemViewModel2 = new NewspaperItemViewModel(repository) {Model = paperModel2};
            var adDetailViewModel = new NewspaperDetailViewModel(repository) {ItemViewModel = newspaperItemViewModel1};
            var detailViewModel = new AdvertisementDetailViewModel(repository)
                                  {ItemViewModel = new AdvertisementItemViewModel(repository) {Model = advertisementModel}};
            detailViewModel.Newspapers.Add(newspaperItemViewModel1);
            detailViewModel.Newspapers.Add(newspaperItemViewModel2);
            detailViewModel.Newspapers.Should().BeEquivalentTo(new List<NewspaperItemViewModel>
                                                               {newspaperItemViewModel1, newspaperItemViewModel2});
            //	Act
            detailViewModel.Newspapers.Remove(newspaperItemViewModel1);

            //	Assert
            detailViewModel.Newspapers.Should().BeEquivalentTo(new List<NewspaperItemViewModel> {newspaperItemViewModel2});
        }

        [Fact]
        public void SaveCommand_WhenInvoked_UpdatesCollectionCurrentItemWithNewValue()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            repository.GetAllAdvertisements().Returns(new List<Advertisement>());
            var detailViewModel = new AdvertisementDetailViewModel(repository);
            var collectionViewModel = new AdvertisementCollectionViewModel(repository);
            detailViewModel.AddItemCommand.Execute(null);
            collectionViewModel.CurrentItem.Should().Be(detailViewModel.ItemViewModel,
                                                        "The detail model points to the Collection CurrentItem");
            var oldName = collectionViewModel.CurrentItem.Name;
            var newName = "Changed Advertisement Name";
            detailViewModel.Name = newName;
            var paperCount = detailViewModel.Newspapers.Count;

            //	Act
            detailViewModel.SaveItemCommand.Execute(null);

            //	Assert
            collectionViewModel.CurrentItem.Should().Be(detailViewModel.ItemViewModel,
                                                        "The current item in the collection matches the one in the detail view model.");
            detailViewModel.Newspapers.ToList().ForEach(n => repository.Received().Save(n.Model));
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