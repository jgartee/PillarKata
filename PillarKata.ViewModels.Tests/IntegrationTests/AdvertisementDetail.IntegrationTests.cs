using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Models;
using NSubstitute;
using Xunit;

namespace PillarKata.ViewModels.Tests.IntegrationTests
{
    public class AdvertisementDetailIntegrationTests
    {
        #region Class Members

        [Fact]
        public void AddNewspaper_WithValidNewspaper_AddsCurrentAdModelReferenceToAdsCollectionInTheAddedNewspaper()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var detailViewModel = new NewspaperDetailViewModel(repository);
            var paperCollectionViewModel = new NewspaperCollectionViewModel(repository);
            var paperModel1 = new Newspaper() {Name = "New paper 1"};
            var paperItemViewModel1 = new NewspaperItemViewModel(repository) {Model = paperModel1};
            var adModel = new Advertisement() {Name = "Ad 1 Name", Text = "Ad 1 text."};
            var adItemViewModel = new AdvertisementItemViewModel(repository) {Model = adModel};
            paperCollectionViewModel.Newspapers.Add(paperItemViewModel1);
            paperCollectionViewModel.CurrentItem.Should().Be(paperItemViewModel1, "The current item is the item that was added.");
            detailViewModel.ItemViewModel.Should().Be(paperCollectionViewModel.CurrentItem,
                                                      "The detail view model references the collection's current item.");
            paperItemViewModel1.Advertisements.Count.Should().Be(0, "No newspapers are referenced in the advertisement.");
            adItemViewModel.Newspapers.Count.Should().Be(0, "There are no papers in this ad's collection.");

            //	Act
            adItemViewModel.Newspapers.Add(paperItemViewModel1);

            //	Assert
            paperItemViewModel1.Advertisements.Count.Should().Be(1, "One item was added");
            adItemViewModel.Newspapers.Count.Should().Be(1, "One newspaper reference was added to this advertisement.");
            adItemViewModel.Newspapers.ToList().First().Should().Be(paperCollectionViewModel.CurrentItem,
                                                                    "The paper added to the advertisment's newspaper collection was the Newspaper collection's CurrentItem");
        }

        [Fact]
        public void RemoveNewspaper_WithExistingPaperInPapersCollection_RemovesCurrentAdReferenceFromPaperAdsCollection()
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
            adItemViewModel.Newspapers.Remove(paperItemViewModel1);

            //	Assert
            adItemViewModel.Newspapers.Count.Should().Be(0, "No items remain in the newspaper list");
            paperItemViewModel1.Advertisements.Count.Should().Be(0, "No items remain in the advertisement list");
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