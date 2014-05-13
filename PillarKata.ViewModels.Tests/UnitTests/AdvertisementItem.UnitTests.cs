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
    public class AdvertisementItem
    {
        #region Class Members

        [Fact]
        public void Name_AccessedWhenModelNotPresent_ThrowsMissingModelException()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperAdRepository>();
            var vm = new AdvertisementItemViewModel(repository);
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

            var vm = GetValidAdvertisementItemViewModel();
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
            errorMessage.Should().Be(Advertisement.MSG_EMPTY_NAME_ERROR);
        }

        [Fact]
        public void Name_SetToInvalidValue_SetsHasErrorsToTrue()
        {
            //	Arrange

            var vm = GetValidAdvertisementItemViewModel();

            //	Act
            vm.Name = null;

            //	Assert
            vm.HasErrors.Should().Be(true, "Null is invalid for Advertisement name.");
        }

        [Fact]
        public void Name_SetWhenModelNotPresent_ThrowsMissingModelException()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperAdRepository>();
            var vm = new AdvertisementItemViewModel(repository);
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
            var vm = GetValidAdvertisementItemViewModel();
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
            var vm = GetValidAdvertisementItemViewModel();
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
            var vm = GetValidAdvertisementItemViewModel();
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

            var vm = GetValidAdvertisementItemViewModel();
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
            errorMessage.Should().Be(Advertisement.MSG_EMPTY_NAME_ERROR, "The error message reflects an empty value set attempt.");

            //	Act
            vm.Name = "New Advertisement Name";

            //	Assert
            vm.HasErrors.Should().Be(false, "No errors should be recorded for the non-empty Name property.");
        }

        [Fact]
        public void Text_AccessedWhenModelNotPresent_ThrowsMissingModelException()
        {
            //	Arrange
            var repository = Substitute.For<INewspaperAdRepository>();
            var vm = new AdvertisementItemViewModel(repository);
            vm.Model.Should().BeNull("Because vm.Model not set during object instantiation.");

            //	Act
            var action = new Action(() => { var name = vm.Text; });

            //	Assert
            action.ShouldThrow<MissingModelException>("Cannot access any properties if Model not present.");
            var ex = Assert.Throws<MissingModelException>(() => vm.Text);
        }

        [Fact]
        public void Text_SetToInvalidValue_SetsHasErrorsToTrue()
        {
            //	Arrange

            var vm = GetValidAdvertisementItemViewModel();

            //	Act
            vm.Text = null;

            //	Assert
            vm.HasErrors.Should().Be(true, "Null is invalid for Advertisement body text.");
        }

        [Fact]
        public void Text_SetWithSameValueAsInModel_DoesNotInvokePropertyChanged()
        {
            //	Arrange
            var vm = GetValidAdvertisementItemViewModel();
            var currentText = new String(vm.Text.ToCharArray());
            var propetyChangedEvent = new PropertyChangedEventAssert(vm);
            propetyChangedEvent.ExpectNothing("No events should be pending.");

            //	Act
            vm.Text = currentText;

            //	Assert
            propetyChangedEvent.ExpectNothing(
                "No PropertyChanged event should be invoked because the model and assigned values are equal.");
        }

        [Fact]
        public void Text_WhenSettingNewValidValue_InvokesPropertyChangedOnNameProperty()
        {
            //	Arrange
            var vm = GetValidAdvertisementItemViewModel();
            var newText = new String(vm.Text.ToCharArray()) + "_Changed";
            var propertyChangedEvent = new PropertyChangedEventAssert(vm);
            propertyChangedEvent.ExpectNothing("No events should be pending.");

            //	Act
            vm.Text = newText;

            //	Assert
            propertyChangedEvent.Expect("Text");
        }

        [Fact]
        public void Text_WhenSettingNewValidValue_SetsNewValueInModel()
        {
            //	Arrange
            var vm = GetValidAdvertisementItemViewModel();
            var newText = new String(vm.Text.ToCharArray()) + "_Changed";
            var propertyChangedEvent = new PropertyChangedEventAssert(vm);
            propertyChangedEvent.ExpectNothing("No events should be pending.");

            //	Act
            vm.Text = newText;

            //	Assert
            vm.Model.Text.Should().Be(newText, "New value has been assigned to the property.");
        }

        [Fact]
        public void Text_WithInvalidValueChangedToValidValue_CallsErrorsChangedWithHasErrorsSetToFalse()
        {
            //	Arrange

            var vm = GetValidAdvertisementItemViewModel();
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
            vm.Text = null;
            vm.HasErrors.Should().Be(true, "A null Text value forces an error.");
            errorProperty.Should().Be("Text", "The property Text in error is Text.");
            errorMessage.Should().Be(Advertisement.MSG_EMPTY_TEXT_ERROR, "The error message reflects an empty value set attempt.");

            //	Act
            vm.Text = "New Advertisement Text Body";

            //	Assert
            vm.HasErrors.Should().Be(false, "No errors should be recorded for the non-empty Text property.");
        }

        [Fact]
        public void Newspapers_WhenNewItemAdded_CallsCollectionChangedEvent()
        {
            //	Arrange
            var paperVm = GetValidNewspaperItemViewModel();
            var adVm = GetValidAdvertisementItemViewModel();
            IList addedItems = new ArrayList();

            var collectionChangedEvent = new NotifyCollectionChangedEventHandler((sender, arg) => { addedItems = arg.NewItems; });
            adVm.Newspapers.CollectionChanged += collectionChangedEvent;

            //	Act
            adVm.Newspapers.Add(paperVm);

            //	Assert
            addedItems.Count.Should().Be(1, "One item was added");
            paperVm.Should().Be(addedItems[0]);
        }

        [Fact]
        public void Newspapers_WhenNewspaperCreated_ContainsEmptyList()
        {
            //	Arrange
            var vm = GetValidAdvertisementItemViewModel();

            //	Act
            var paperCollection = vm.Newspapers;

            //	Assert
            paperCollection.Count.Should().Be(0, "No Newspapers exist in new Advertisement object.");
        }

        [Fact]
        public void Newspapers_WhenOneAdded_AddsReferenceToTheNewspapersCollection()
        {
            //	Arrange

            var paperVm = GetValidNewspaperItemViewModel();
            var advertisementVm = GetValidAdvertisementItemViewModel();

            //	Act
            advertisementVm.Newspapers.Add(paperVm);

            //	Assert
            advertisementVm.Newspapers.Count.Should().Be(1, "The item is added");
            advertisementVm.Newspapers.First().Should().Be(paperVm,
                                                       "The object added is a reference to the local newspaper.");
        }

        [Fact]
        public void Newspapers_WhenAddedToAdvertisement_AddsAdvertisementToNewspaperAdvertisementsCollection()
        {
            //	Arrange
            var adVm = GetValidAdvertisementItemViewModel();
            var paperVm = GetValidNewspaperItemViewModel();

            //	Act

            adVm.Newspapers.Add(paperVm);
            //	Assert
            var paperAdVm = paperVm.Advertisements.FirstOrDefault(a => a.UKey == adVm.UKey);
            paperAdVm.Should().Be(adVm,
                                      "When an newspaper is added to a Advertisement, the Advertisement is added to that Newspaper also.");
        }

        [Fact]
        public void Newspapers_WhenRemovedFromAdvertisement_RemoveNewspapersFromAdvertisementsCollection()
        {
            //	Arrange
            var adVm = GetValidAdvertisementItemViewModel();
            var paperVm = GetValidNewspaperItemViewModel();
            var paper2 = new Newspaper { Name = "Second Newspaper Name"};
            var paperVm2 = GetValidNewspaperItemViewModel(paper2);
            adVm.Newspapers.Add(paperVm);
            adVm.Newspapers.Add(paperVm2);
            adVm.Newspapers.Count.Should().Be(2, "We added two items.");
            paperVm.Advertisements.ToList().First().Should().Be(adVm, "The paper reflects the advertisement it was added to.");
            paperVm2.Advertisements.ToList().First().Should().Be(adVm, "Second paper also references the advertisement it was added to");

            //	Act
            adVm.Newspapers.Remove(paperVm);

            //	Assert
            adVm.Newspapers.Count.Should().Be(1, "One item remains.");
            adVm.Newspapers.ShouldAllBeEquivalentTo(new List<NewspaperItemViewModel> { paperVm2 });
        }


        [Fact]
        public void Newspapers_WhenRemovedFromAdvertisement_RemovesAdvertismentFromNewspapersAdvertismentCollection()
        {
            //	Arrange
            var adVm = GetValidAdvertisementItemViewModel();
            var paperVm = GetValidNewspaperItemViewModel();
            var paper2 = new Newspaper { Name = "Second Newspaper Name" };
            var paperVm2 = GetValidNewspaperItemViewModel(paper2);
            adVm.Newspapers.Add(paperVm);
            adVm.Newspapers.Add(paperVm2);
            adVm.Newspapers.Count.Should().Be(2, "We added two items.");
            paperVm.Advertisements.ToList().First().Should().Be(adVm, "The paper reflects the advertisement it was added to.");
            paperVm2.Advertisements.ToList().First().Should().Be(adVm, "Second paper also references the advertisement it was added to");

            //	Act
            adVm.Newspapers.Remove(paperVm);

 
            //	Assert
            paperVm2.Advertisements.Count.Should().Be(1, "One item remains.");
            paperVm2.Advertisements.ShouldAllBeEquivalentTo(new List<AdvertisementItemViewModel> { adVm });
        }


        [Fact]
        public void Newspaper_WhenRemovedFromAdvertisement_RemovesAdvertisementFromNewspaperAdvertisementsCollection()
        {
            //	Arrange
            var adVm = GetValidAdvertisementItemViewModel();
            var paperVm = GetValidNewspaperItemViewModel();
            adVm.Newspapers.Add(paperVm);
            var paperAdVm = paperVm.Advertisements.FirstOrDefault(a => a.UKey == adVm.UKey);
            paperAdVm.Should().Be(adVm,
                                      "When an newspaper is added to an Advertisement, the Advertisement is added to that Newspaper also.");
            paperVm.Advertisements.ToList().First().Should().Be(adVm, "The ad was added to the Newspaper Advertisements collection.");

            //  Act
            adVm.Newspapers.Remove(paperVm);

            //  Assert
            paperVm.Advertisements.Count.Should().Be(0, "The advertisement item was removed.");
        }


        [Fact]
        public void Newspaperss_WhenDuplicateNewspaperAdded_DoesNotReferenceAdvertisementInNewspaperAdvertisementsCollection()
        {
            //	Arrange
            var adVm = GetValidAdvertisementItemViewModel();
            var paperVm = GetValidNewspaperItemViewModel();
            adVm.Newspapers.Add(paperVm);

            //	Act
            adVm.Newspapers.Add(paperVm);

            //	Assert
            paperVm.Advertisements.Count.Should().Be(1, "An additional advertisement was not added");
            paperVm.Advertisements.ShouldAllBeEquivalentTo(new List<AdvertisementItemViewModel> { adVm });

        }

        [Fact]
        public void Newspapers_WhenDuplicateNewspaperAdded_DoesNotAddDuplicateToAdvertisementNewspapersCollection()
        {
            //	Arrange
            var adVm = GetValidAdvertisementItemViewModel();
            var paperVm = GetValidNewspaperItemViewModel();
            adVm.Newspapers.Add(paperVm);

            //	Act
            adVm.Newspapers.Add(paperVm);

            //	Assert
            adVm.Newspapers.Count.Should().Be(1, "Duplicate Newspapers cannot be added.");
        }

        #endregion

        #region Utility Setup Routines

        private AdvertisementItemViewModel GetValidAdvertisementItemViewModel(Advertisement advertisement = null,
                                                                              INewspaperAdRepository adRepository = null)
        {
            var model = advertisement ?? new Advertisement() {Name = "<New Advertisement>", Text="<New Advertisement body text."};
            var repo = adRepository ?? Substitute.For<INewspaperAdRepository>();
            return new AdvertisementItemViewModel(repo) {Model = model};
        }

        private NewspaperItemViewModel GetValidNewspaperItemViewModel(Newspaper paper = null,
                                                                      INewspaperAdRepository adRepository = null)
        {
            var model = paper ?? new Newspaper() { Name = "<New Newspaper>" };
            var repo = adRepository ?? Substitute.For<INewspaperAdRepository>();
            return new NewspaperItemViewModel(repo) { Model = model };
        }

        #endregion
    }
}