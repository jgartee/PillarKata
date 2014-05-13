using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using FluentAssertions;
using Granite.Testing;
using Models;
using NSubstitute;
using Xunit;

namespace PillarKata.ViewModels.Tests.UnitTests
{
    public class NewspaperItemUnitTests
    {
        #region Class Members

        [Fact]
        public void Advertisement_WhenAddedToNewspaper_AddsNewspaperToAdvertisementNewspapersCollection()
        {
            //	Arrange
            var paperVm = GetValidNewspaperItemViewModel();
            var adVm = GetValidAdvertisementItemViewModel();

            //	Act

            paperVm.Advertisements.Add(adVm);
            //	Assert
            var adNewspaperVm = adVm.Newspapers.FirstOrDefault(n => n.UKey == paperVm.UKey);
            adNewspaperVm.Should().Be(paperVm,
                                      "When an advertisement is added to a Newspaper, the Newspaper is added to that Advertisement also.");
        }

        [Fact]
        public void Advertisement_WhenRemovedFromNewspaper_RemoveAdvertismentsFromNewspaperAdvertisementsCollection()
        {
            //	Arrange
            var paperVm = GetValidNewspaperItemViewModel();
            var adVm = GetValidAdvertisementItemViewModel();
            var ad2 = new Advertisement {Name = "Second Advertisement Name", Text = "Second Advertisement body text."};
            var adVm2 = GetValidAdvertisementItemViewModel(ad2);
            paperVm.Advertisements.Add(adVm);
            paperVm.Advertisements.Add(adVm2);
            paperVm.Advertisements.Count.Should().Be(2, "We added two items.");
            adVm.Newspapers.ToList().First().Should().Be(paperVm, "The ad reflects the paper it was added to.");
            adVm2.Newspapers.ToList().First().Should().Be(paperVm, "Second ad also references the paper it was added to");

            //	Act
            paperVm.Advertisements.Remove(adVm);

            //	Assert
            paperVm.Advertisements.Count.Should().Be(1, "One item remains.");
            paperVm.Advertisements.ShouldAllBeEquivalentTo(new List<AdvertisementItemViewModel> {adVm2});
        }

        [Fact]
        public void Advertisement_WhenRemovedFromNewspaper_RemovesNewspaperFromAdvertisementNewspapersCollection()
        {
            //	Arrange
            var paperVm = GetValidNewspaperItemViewModel();
            var adVm = GetValidAdvertisementItemViewModel();
            paperVm.Advertisements.Add(adVm);
            var adNewspaperVm = adVm.Newspapers.FirstOrDefault(n => n.UKey == paperVm.UKey);
            adNewspaperVm.Should().Be(paperVm,
                                      "When an advertisement is added to a Newspaper, the Newspaper is added to that Advertisement also.");
            adVm.Newspapers.ToList().First().Should().Be(paperVm, "The paper was added to the Advertisment Newspapers collection.");

            //  Act
            paperVm.Advertisements.Remove(adVm);

            //  Assert
            adVm.Newspapers.Count.Should().Be(0, "The newspaper item was removed.");
        }

        [Fact]
        public void Advertisement_WhenRemovedFromNewspaper_RemovesNewspaperFromAdvertismentsNewspaperCollection()
        {
            //	Arrange
            var paperVm = GetValidNewspaperItemViewModel();
            var adVm = GetValidAdvertisementItemViewModel();
            var ad2 = new Advertisement {Name = "Second Advertisement Name", Text = "Second Advertisement body text."};
            var adVm2 = GetValidAdvertisementItemViewModel(ad2);
            paperVm.Advertisements.Add(adVm);
            paperVm.Advertisements.Add(adVm2);
            paperVm.Advertisements.Count.Should().Be(2, "We added two items.");
            adVm.Newspapers.ToList().First().Should().Be(paperVm, "The ad reflects the paper it was added to.");
            adVm2.Newspapers.ToList().First().Should().Be(paperVm, "Second ad also references the paper it was added to");

            //	Act
            paperVm.Advertisements.Remove(adVm);

            //	Assert
            paperVm.Advertisements.Count.Should().Be(1, "One item remains.");
            paperVm.Advertisements.ShouldAllBeEquivalentTo(new List<AdvertisementItemViewModel> {adVm2});
        }

        [Fact]
        public void Advertisements_WhenDuplicateAdvertisementAdded_DoesNotReferenceNewspaperInAdvertisementNewspapersCollection()
        {
            //	Arrange
            var paperVm = GetValidNewspaperItemViewModel();
            var adVm = GetValidAdvertisementItemViewModel();
            paperVm.Advertisements.Add(adVm);

            //	Act
            paperVm.Advertisements.Add(adVm);

            //	Assert
            adVm.Newspapers.Count.Should().Be(1, "An additional newspaper was not added");
            adVm.Newspapers.ShouldAllBeEquivalentTo(new List<NewspaperItemViewModel> {paperVm});
        }

        [Fact]
        public void Advertisements_WhenNewItemAdded_CallsCollectionChangedEvent()
        {
            //	Arrange
            var paperVm = GetValidNewspaperItemViewModel();
            var adVm = GetValidAdvertisementItemViewModel();
            IList addedItems = new ArrayList();

            var collectionChangedEvent = new NotifyCollectionChangedEventHandler((sender, arg) => { addedItems = arg.NewItems; });
            paperVm.Advertisements.CollectionChanged += collectionChangedEvent;

            //	Act
            paperVm.Advertisements.Add(adVm);

            //	Assert
            addedItems.Count.Should().Be(1, "One item was added");
            adVm.Should().Be(addedItems[0]);
        }

        [Fact]
        public void Advertisements_WhenNewspaperCreated_ContainsEmptyList()
        {
            //	Arrange
            var vm = GetValidNewspaperItemViewModel();

            //	Act
            var adCollection = vm.Advertisements;

            //	Assert
            adCollection.Count.Should().Be(0, "No Advertisements exist in new Newspaper object.");
        }

        [Fact]
        public void Advertisements_WhenOneAdded_AddsReferenceToTheAdvertisementsCollection()
        {
            //	Arrange

            var paperVm = GetValidNewspaperItemViewModel();
            var advertisementVm = GetValidAdvertisementItemViewModel();

            //	Act
            paperVm.Advertisements.Add(advertisementVm);

            //	Assert
            paperVm.Advertisements.Count.Should().Be(1, "The item is added");
            paperVm.Advertisements.First().Should().Be(advertisementVm,
                                                       "The object added is a reference to the local advertisement.");
        }

        [Fact]
        public void Advertisments_WhenDuplicateAdvertisementAdded_DoesNotAddDuplicatesToNewspaperAdvertisementsCollection()
        {
            //	Arrange
            var paperVm = GetValidNewspaperItemViewModel();
            var adVm = GetValidAdvertisementItemViewModel();
            paperVm.Advertisements.Add(adVm);

            //	Act
            paperVm.Advertisements.Add(adVm);

            //	Assert
            paperVm.Advertisements.Count.Should().Be(1, "Duplicate Advertisements cannot be added.");
        }

        [Fact]
        public void Name_AccessedWhenModelNotPresent_ThrowsMissingModelException()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var vm = new NewspaperItemViewModel(repository);
            vm.Model.Should().BeNull("Because vm.Model not set during object instantiation.");

            //	Act
            var action = new Action(() => { var name = vm.Name; });

            //	Assert
            action.ShouldThrow<MissingModelException>("Cannot access any properties if Model not present.");
            var ex = Assert.Throws<MissingModelException>(() => vm.Name);
        }

        [Fact]
        public void Name_SetToInvalidValue_CallsErrorsChangedEventWithNullNameErrorMessage()
        {
            //	Arrange

            var vm = GetValidNewspaperItemViewModel();
            var errorProperty = "";
            var errorMessage = "";

            var errorsChangedAction = new EventHandler<DataErrorsChangedEventArgs>((sender, arg) =>
                                                                                   {
                                                                                       errorProperty = arg.PropertyName;
                                                                                       errorMessage =
                                                                                           (vm.GetErrors(arg.PropertyName)
                                                                                              .As<List<string>>())[0];
                                                                                   });
            vm.ErrorsChanged += errorsChangedAction;

            //	Act
            vm.Name = null;

            //	Assert
            vm.HasErrors.Should().Be(true);
            errorProperty.Should().Be("Name");
            errorMessage.Should().Be(Newspaper.MSG_EMPTY_NAME_ERROR.ToString());
        }

        [Fact]
        public void Name_SetToInvalidValue_SetsHasErrorsToTrue()
        {
            //	Arrange

            var vm = GetValidNewspaperItemViewModel();

            //	Act
            vm.Name = null;

            //	Assert
            vm.HasErrors.Should().Be(true, "Null is invalid for Newspaper name.");
        }

        [Fact]
        public void Name_SetWhenModelNotPresent_ThrowsMissingModelException()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperRepository>();
            var vm = new NewspaperItemViewModel(repository);
            vm.Model.Should().BeNull("Because vm.Model not set during object instantiation.");

            //	Act
            var action = new Action(() => { vm.Name = "Name"; });

            //	Assert
            action.ShouldThrow<MissingModelException>("Cannot set any properties if Model not present.");
        }

        [Fact]
        public void Name_SetWithSameValueAsInModel_DoesNotInvokePropertyChanged()
        {
            //	Arrange
            var vm = GetValidNewspaperItemViewModel();
            var currentName = new String(vm.Name.ToCharArray());
            var propetyChangedEvent = new PropertyChangedEventAssert(vm);
            propetyChangedEvent.ExpectNothing("No events should be pending.");

            //	Act
            vm.Name = currentName;

            //	Assert
            propetyChangedEvent.ExpectNothing(
                "No PropertyChanged event should be invoked because the model and assigned values are equal.");
        }

        [Fact]
        public void Name_WhenSettingNewValidValue_InvokesPropertyChangedOnNameProperty()
        {
            //	Arrange
            var vm = GetValidNewspaperItemViewModel();
            var newName = new String(vm.Name.ToCharArray()) + "_Changed";
            var propertyChangedEvent = new PropertyChangedEventAssert(vm);
            propertyChangedEvent.ExpectNothing("No events should be pending.");

            //	Act
            vm.Name = newName;

            //	Assert
            propertyChangedEvent.Expect("Name");
        }

        [Fact]
        public void Name_WhenSettingNewValidValue_SetsNewValueInModel()
        {
            //	Arrange
            var vm = GetValidNewspaperItemViewModel();
            var newName = new String(vm.Name.ToCharArray()) + "_Changed";
            var propertyChangedEvent = new PropertyChangedEventAssert(vm);
            propertyChangedEvent.ExpectNothing("No events should be pending.");

            //	Act
            vm.Name = newName;

            //	Assert
            vm.Model.Name.Should().Be(newName, "New value has been assigned to the property.");
        }

        [Fact]
        public void Name_WithInvalidValueChangedToValidValue_CallsErrorsChangedWithHasErrorsSetToFalse()
        {
            //	Arrange

            var vm = GetValidNewspaperItemViewModel();
            var errorProperty = "";
            var errorMessage = "";

            var errorsChangedAction = new EventHandler<DataErrorsChangedEventArgs>((sender, arg) =>
                                                                                   {
                                                                                       errorProperty = arg.PropertyName;
                                                                                       var errorList =
                                                                                           vm.GetErrors(arg.PropertyName)
                                                                                             .As<List<string>>();
                                                                                       errorMessage = errorList.Count > 0
                                                                                                          ? errorList[0] : "";
                                                                                   });

            vm.ErrorsChanged += errorsChangedAction;
            vm.Name = null;
            vm.HasErrors.Should().Be(true, "A null Name value forces an error.");
            errorProperty.Should().Be("Name", "The property name in error is Name.");
            errorMessage.Should().Be(Newspaper.MSG_EMPTY_NAME_ERROR, "The error message reflects an empty value set attempt.");

            //	Act
            vm.Name = "New Newspaper Name";

            //	Assert
            vm.HasErrors.Should().Be(false, "No errors should be recorded for the non-empty Name property.");
        }

        #endregion

        #region Utility Setup Routines

        private AdvertisementItemViewModel GetValidAdvertisementItemViewModel(Advertisement ad = null,
                                                                              INewspaperRepository repository = null)
        {
            var model = ad ?? new Advertisement {Name = "<New Advertisement>", Text = "<New Advertisemnet body text."};
            var repo = repository ?? Substitute.For<INewspaperRepository>();
            var vm = new AdvertisementItemViewModel(repo) {Model = model};
            return vm;
        }

        private NewspaperItemViewModel GetValidNewspaperItemViewModel(Newspaper paper = null,
                                                                      INewspaperRepository repository = null)
        {
            var model = paper ?? new Newspaper() {Name = "<New Newspaper>"};
            var repo = repository ?? Substitute.For<INewspaperRepository>();
            return new NewspaperItemViewModel(repo) {Model = model};
        }

        #endregion Utility Setup Routines
    }
}