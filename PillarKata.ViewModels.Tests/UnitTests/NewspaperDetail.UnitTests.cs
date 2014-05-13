using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Models;
using NSubstitute;
using Xunit;

namespace PillarKata.ViewModels.Tests.UnitTests
{
    public class NewspaperDetailUnitTests : ViewModelBase
    {
        #region Class Members

        [Fact]
        public void AddAdvertisement_WithValidAdvertisement_AddsAdvertisementToAdvertisementsCollection()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var paperModel = new Newspaper();
            var adModel = new Advertisement();
            var adItemViewModel = new AdvertisementItemViewModel(repository) {Model = adModel};
            var adDetailViewModel = new AdvertisementDetailViewModel(repository) {ItemViewModel = adItemViewModel};
            var detailViewModel = new NewspaperDetailViewModel(repository)
                                  {ItemViewModel = new NewspaperItemViewModel(repository) {Model = paperModel}};

            //	Act
            detailViewModel.Advertisements.Add(adItemViewModel);
            //	Assert
            detailViewModel.Advertisements.Count.Should().Be(1, "There should be one item in the collection");
        }

        [Fact]
        public void AddAdvertisement_WithValidAdvertisement_AddsNewspaperToAdvertisementNewspaperssCollection()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var paperModel = new Newspaper();
            var adModel = new Advertisement();
            var adItemViewModel = new AdvertisementItemViewModel(repository) {Model = adModel};
            var adDetailViewModel = new AdvertisementDetailViewModel(repository) {ItemViewModel = adItemViewModel};
            var detailViewModel = new NewspaperDetailViewModel(repository)
                                  {ItemViewModel = new NewspaperItemViewModel(repository) {Model = paperModel}};

            //	Act
            detailViewModel.Advertisements.Add(adItemViewModel);
            //	Assert
            detailViewModel.Advertisements.ToList().First().Newspapers.Contains(detailViewModel.ItemViewModel);
        }

        [Fact]
        public void AddItemCommand_WhenInvoked_SendsMessageOfTypeAddingItemMessage()
        {
            var msgSent = false;
            var repository = Substitute.For<INewspaperRepository>();
            Messenger.Default.Register<AddingNewspaperItemMessage>(this, (message) => { msgSent = true; });
            //	Act
            var viewModel = new NewspaperDetailViewModel(repository);
            viewModel.AddItemCommand.Execute(null);

            //	Assert
            msgSent.Should().Be(true, "The message was sent.");
        }

        [Fact]
        public void CancelCommand_WhenInvoked_ResetsNameToCollectionCurrentValue()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var detailViewModel = new NewspaperDetailViewModel(repository);
            var collectionViewModel = new NewspaperCollectionViewModel(repository);
            detailViewModel.AddItemCommand.Execute(null);
            collectionViewModel.CurrentItem.Should().Be(detailViewModel.ItemViewModel,
                                                        "The detail model points to the Collection CurrentItem");
            var oldName = collectionViewModel.CurrentItem.Name;
            var newName = "Changed Newspaper Name";
            detailViewModel.Name = newName;
            collectionViewModel.CurrentItem.Name.Should().Be(oldName, "Name in the collection should not change");
            detailViewModel.Name.Should().Be(newName, "Alteration is reflected in the detail view model");

            //	Act
            detailViewModel.CancelItemCommand.Execute(null);

            //	Assert
            detailViewModel.Name.Should().Be(oldName, "The Newspaper name should revert back");
            collectionViewModel.CurrentItem.Should().Be(detailViewModel.ItemViewModel,
                                                        "The entire instance should have reverted back to the original value");
        }

        [Fact]
        public void Name_WhenNotNullOrEmpty_SetsAllowAddToTrue()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var model = new Newspaper();
            var itemViewModel = new NewspaperItemViewModel(repository) {Model = model};

            //	Act
            var viewModel = new NewspaperDetailViewModel(repository) {ItemViewModel = itemViewModel};

            //	Assert
            viewModel.AllowSave.Should().Be(true, "We have a valid name to reference.");
        }

        [Fact]
        public void Name_WhenNullOrEmpty_SetsAllowSaveToFalse()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var model = new Newspaper();
            var itemViewModel = new NewspaperItemViewModel(repository) {Model = model};
            var viewModel = new NewspaperDetailViewModel(repository) {ItemViewModel = itemViewModel};
            viewModel.AllowSave.Should().Be(true, "We have a valid name to reference.");

            //	Act
            viewModel.Name = "";

            //	Assert
            viewModel.AllowSave.Should().Be(false, "Cannot save an newspaper without a name.");
        }

        [Fact]
        public void NewspaperDetailViewModel_WhenCreated_SendsDetailViewModelReadyMessage()
        {
            //	Arrange
            var msgSent = false;
            //NewspaperDetailViewModelReady msg = null;
            var repository = Substitute.For<INewspaperRepository>();
            Messenger.Default.Register<NewspaperDetailViewModelReady>(this, (message) => { msgSent = true; });
            //	Act
            var viewModel = new NewspaperDetailViewModel(repository);

            //	Assert
            msgSent.Should().Be(true, "The message was sent.");
        }

        [Fact]
        public void
            RemoveAdvertisement_WithExistingAdvertisementInAdvertisementsCollection_RemovesAdvertisementFromAdvertisementsCollection
            ()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var paperModel = new Newspaper();
            var adModel1 = new Advertisement();
            var adItemViewModel1 = new AdvertisementItemViewModel(repository) {Model = adModel1};
            var adModel2 = new Advertisement();
            var adItemViewModel2 = new AdvertisementItemViewModel(repository) {Model = adModel2};
            var adDetailViewModel = new AdvertisementDetailViewModel(repository) {ItemViewModel = adItemViewModel1};
            var detailViewModel = new NewspaperDetailViewModel(repository)
                                  {ItemViewModel = new NewspaperItemViewModel(repository) {Model = paperModel}};
            detailViewModel.Advertisements.Add(adItemViewModel1);
            detailViewModel.Advertisements.Add(adItemViewModel2);
            detailViewModel.Advertisements.Should().BeEquivalentTo(new List<AdvertisementItemViewModel>
                                                                   {adItemViewModel1, adItemViewModel2});
            //	Act
            detailViewModel.Advertisements.Remove(adItemViewModel1);

            //	Assert
            detailViewModel.Advertisements.Should().BeEquivalentTo(new List<AdvertisementItemViewModel> {adItemViewModel2});
        }

        [Fact]
        public void
            RemoveAdvertisement_WithExistingAdvertisementInAdvertisementsCollection_RemovesNewspaperFromAdvertisementRemoved()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var paperModel = new Newspaper();
            var adModel1 = new Advertisement();
            var adItemViewModel1 = new AdvertisementItemViewModel(repository) {Model = adModel1};
            var adModel2 = new Advertisement();
            var adItemViewModel2 = new AdvertisementItemViewModel(repository) {Model = adModel2};
            var adDetailViewModel = new AdvertisementDetailViewModel(repository) {ItemViewModel = adItemViewModel1};
            var detailViewModel = new NewspaperDetailViewModel(repository)
                                  {ItemViewModel = new NewspaperItemViewModel(repository) {Model = paperModel}};
            detailViewModel.Advertisements.Add(adItemViewModel1);
            detailViewModel.Advertisements.Add(adItemViewModel2);
            detailViewModel.Advertisements.Should().BeEquivalentTo(new List<AdvertisementItemViewModel>
                                                                   {adItemViewModel1, adItemViewModel2});
            detailViewModel.Advertisements.ToList().All(a => a.Newspapers.Contains(detailViewModel.ItemViewModel));
            //	Act
            detailViewModel.Advertisements.Remove(adItemViewModel1);

            //	Assert
            detailViewModel.Advertisements.Should().BeEquivalentTo(new List<AdvertisementItemViewModel> {adItemViewModel2});
        }

        [Fact]
        public void SaveCommand_WhenInvoked_UpdatesCollectionCurrentItemWithNewValue()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var detailViewModel = new NewspaperDetailViewModel(repository);
            var collectionViewModel = new NewspaperCollectionViewModel(repository);
            detailViewModel.AddItemCommand.Execute(null);
            collectionViewModel.CurrentItem.Should().Be(detailViewModel.ItemViewModel,
                                                        "The detail model points to the Collection CurrentItem");
            var oldName = collectionViewModel.CurrentItem.Name;
            var newName = "Changed Newspaper Name";
            detailViewModel.Name = newName;

            //	Act
            detailViewModel.SaveItemCommand.Execute(null);

            //	Assert
            collectionViewModel.CurrentItem.Should().Be(detailViewModel.ItemViewModel,
                                                        "The current item in the collection matches the one in the detail view model.");
            repository.Received().Save(detailViewModel.ItemViewModel.Model);
        }

        [Fact]
        public void ViewModel_WithNullItemViewModel_SetsAllowSaveToFalse()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();

            //	Act
            var viewModel = new NewspaperDetailViewModel(repository);

            //	Assert
            viewModel.AllowSave.Should().Be(false, "If not item view model set, do not allow a Save to be called.");
        }

        #endregion
    }
}