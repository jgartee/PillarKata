using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Models;
using NSubstitute;
using Xunit.Extensions;

namespace Data.Tests.Integration_Tests
{
    public class DataIntegrationTests
    {
        //TODO jlg - Add tests for AdvertisementCache, etc.

        // ReSharper disable InconsistentNaming
        [Theory]
        [PropertyData("FiveNewNewspapersInList")]
        public void Serializer_SaveAndRestoreCache_ResultsMatchSavedItems(List<Newspaper> papers)
        {
            //	Arrange
            var directoryFileNames = Directory.GetFiles(Directory.GetCurrentDirectory()).ToList();
            var fileName = directoryFileNames.FirstOrDefault(f => f.EndsWith("NewspaperData.txt"));
            File.Delete(fileName ?? "NewspaperData.txt");
            var serializer = new NewspaperSerializer();
            var cache = new NewspaperCache();
            var repository = new NewspaperAdRepository(cache, serializer);

            papers.ForEach(repository.Save);

            //	Act
            directoryFileNames = Directory.GetFiles(Directory.GetCurrentDirectory()).ToList();
            fileName = directoryFileNames.FirstOrDefault(f => f.EndsWith("NewspaperData.txt"));

            //	Assert
            fileName.Should().NotBeNull();

            //  There are multiple tests because we are using a PropertyData construct and we must deal with each file 
            //  independently.  XUnit runs tests concurrently, and this test must run in isolation.  I could have 
            //  named the file based on the papers.Count, but I didn't.  In real life, I would have.

            //  Arrange for restore cache from disk
            var cacheCount = cache.Values.Count;
            serializer = new NewspaperSerializer();
            cache = new NewspaperCache();
            cache.Values.ToList().Should().BeEmpty("The cache should be empty");

            //  Act
            repository = new NewspaperAdRepository(cache, serializer);

            //  Assert
            cache.Values.Count.Should().Be(cacheCount, cacheCount + " items should be in the cache");

            //  Arrange for Delete function
            var paper = cache.Values.First();

            //  Act
            repository.Delete(paper);

            //  Assert
            cache.Values.Count.Should().Be(cacheCount - 1, "One item should have been removed.");

            //  Arrange for save on deleted status function
            paper = cache.Values.First();
            paper.DbStatus = DbModificationState.Deleted;
            
            //  Act
            repository.Save(paper);

            //  Assert
            cache.Values.Count.Should().Be(cacheCount - 2, "One more item should have been removed.");

//            //  Arrange for save on nothing saved function
//            paper = cache.Values.First();
//            paper.DbStatus.Should().Be(DbModificationState.Unchanged, "Nothing has been modified since cache restore");
//
//            //  act
//            repository.Save(paper);
//
//            //  Assert
//            paper.DbStatus.Should().Be(DbModificationState.Unchanged, "Still no change in state.");

            //  Arrange for retrieval of an individual Advertisement from the cache
            paper = cache.Values.First();
            var advertisement = paper.Advertisements[1];

            //  Act
            var repoAdvertisement = repository.GetAdvertisement(advertisement.UKey);

            //  Assert
            advertisement.UKey.Should().Be(repoAdvertisement.UKey, "We should retrieve the same object from the cache");

            //  Arrange showing that GetAllAdvertisements returns a distinct list (no duplicates)
            var remainingAdvertisements = repository.GetAllAdvertisements();
            var dupCheckAdOriginalCount = cache.Values.Sum(n => n.Advertisements.Count);
            var adToTryToDuplicate = remainingAdvertisements.First(a=>a.Newspapers.Count > 0);
            var papersRelatedToAd = adToTryToDuplicate.Newspapers;
            var differentPaperToAddAdTo = cache.Values.First(n => papersRelatedToAd.Any(p => p.UKey != n.UKey));
            differentPaperToAddAdTo.AddAdvertisement(adToTryToDuplicate);

            var retrievedDuplicateAd = repository.GetAdvertisement(adToTryToDuplicate.UKey);
            var firstRelatedNewspaper = repository.Get(retrievedDuplicateAd.Newspapers[0].UKey);
            var secondRelatedNewspaper = repository.Get(retrievedDuplicateAd.Newspapers[1].UKey);

            //  Act
            var remainingNonDuplicateAds = repository.GetAllAdvertisements().Where(a=>a.Newspapers.Count > 0).ToList();

            //  Assert that at least one ad occurs twice in cache but not on returned list
            remainingNonDuplicateAds.Count.Should().Be(dupCheckAdOriginalCount,
                                                       "The count of distinct ads should be the same as when we started");
            retrievedDuplicateAd.Newspapers.Count.Should().Be(2, "This ad should be related to two different Newspapers");

            firstRelatedNewspaper.Advertisements.Contains(retrievedDuplicateAd).Should().Be(true, "The first paper contains the desired advertisement");
            secondRelatedNewspaper.Advertisements.Contains(retrievedDuplicateAd).Should().Be(true, "The second paper contains the desired advertisement");
            cache.Values.Sum(n => n.Advertisements.Count).Should().Be(remainingNonDuplicateAds.Count + 1,
                                                  "One ad should be on two different papers and not in the advertisments retrieved from the repository.");
        }

        #region Properties

        public static IEnumerable<object[]> FiveNewNewspapersInList
        {
            get
            {
                var paper1 = new Newspaper {Name = "New paper 1 odd"};
                var paper2 = new Newspaper {Name = "New paper 2 even"};
                var paper3 = new Newspaper {Name = "New paper 3 odd"};
                var paper4 = new Newspaper {Name = "New paper 4 even"};
                var paper5 = new Newspaper {Name = "New paper 5 odd"};

                paper1.AddAdvertisements(new List<Advertisement>
                                         {
                                                 new Advertisement {Name = "Paper 1 Ad 1", Text = "Paper 1 Text 1"},
                                                 new Advertisement {Name = "Paper 1 Ad 2", Text = "Paper 1 Text 2"},
                                                 new Advertisement {Name = "Paper 1 Ad 3", Text = "Paper 1 Text 3"},
                                                 new Advertisement {Name = "Paper 1 Ad 4", Text = "Paper 1 Text 4"},
                                                 new Advertisement {Name = "Paper 1 Ad 5", Text = "Paper 1 Text 5"},
                                         });

                paper3.AddAdvertisements(new List<Advertisement>
                                         {
                                                 new Advertisement {Name = "Paper 3 Ad 1", Text = "Paper 3 Text 1"},
                                                 new Advertisement {Name = "Paper 3 Ad 2", Text = "Paper 3 Text 2"},
                                                 new Advertisement {Name = "Paper 3 Ad 3", Text = "Paper 3 Text 3"},
                                                 new Advertisement {Name = "Paper 3 Ad 4", Text = "Paper 3 Text 4"},
                                                 new Advertisement {Name = "Paper 3 Ad 5", Text = "Paper 3 Text 5"},
                                         });

                paper5.AddAdvertisements(new List<Advertisement>
                                         {
                                                 new Advertisement {Name = "Paper 5 Ad 1", Text = "Paper 5 Text 1"},
                                                 new Advertisement {Name = "Paper 5 Ad 2", Text = "Paper 5 Text 2"},
                                                 new Advertisement {Name = "Paper 5 Ad 3", Text = "Paper 5 Text 3"},
                                                 new Advertisement {Name = "Paper 5 Ad 4", Text = "Paper 5 Text 4"},
                                                 new Advertisement {Name = "Paper 5 Ad 5", Text = "Paper 5 Text 5"},
                                         });

                return new[] {new[] {(object) (new List<Newspaper> {paper1, paper2, paper3, paper4, paper5})}};
            }
        }

        public static IEnumerable<object[]> SingleNewNewspaperInList
        {
            get
            {
                var paper1 = new Newspaper {Name = "New paper 1 odd"};
                return new[] {new[] {(object) (new List<Newspaper> {paper1})}};
            }
        }

        public static IEnumerable<object[]> ThreeNewNewspapersInList
        {
            get
            {
                var paper1 = new Newspaper {Name = "New paper 1 odd"};
                var paper2 = new Newspaper {Name = "New paper 2 even"};
                var paper3 = new Newspaper {Name = "New paper 3 odd"};
                return new[] {new[] {(object) (new List<Newspaper> {paper1, paper2, paper3})}};
            }
        }

        public static IEnumerable<object[]> TwoNewNewspapersInList
        {
            get
            {
                var paper1 = new Newspaper {Name = "New paper 1 odd"};
                var paper2 = new Newspaper {Name = "New paper 2 even"};

                return new[] {new[] {(object) (new List<Newspaper> {paper1, paper2})}};
            }
        }

        #endregion

        // ReSharper restore InconsistentNaming
    }
}