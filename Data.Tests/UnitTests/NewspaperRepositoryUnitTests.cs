using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Models;
using NSubstitute;
using Xunit;
using Xunit.Extensions;

namespace Data.Tests.Unit_Tests
{
    public class NewspaperRepositoryUnitTests
    {
        // ReSharper disable InconsistentNaming

        #region Class Members

        [Fact]
        public void NewspaperRepository_OnCreate_CallsNewspaperCacheRestore()
        {
            //	Arrange
            var cache = Substitute.For<NewspaperCache>();
            var serializer = Substitute.For<ISerializer<NewspaperCache>>();

            //	Act
            var repository = new NewspaperAdRepository(cache,serializer);

            //	Assert
            serializer.Received().RestoreCache(cache);
        }

        [Theory]
        [PropertyData("FiveNewNewspapersInList")]
        public void Delete_FromPapersAdded_ReducesTheCountOfItemsByOne(List<Newspaper> papers)
        {
            //	Arrange
            var repo = GetBasicNewspaperRepository();
            papers.ForEach(repo.Save);
            var paperToDelete = papers.Skip(2).First();

            //	Act
            var allPapers = repo.Find(n => n != null).ToList();
            var allPapersCount = allPapers.Count();

            repo.Delete(paperToDelete);
            var papersAfterDelete = repo.Find(n => n != null).ToList();

            //	Assert
            papersAfterDelete.Count()
                    .Should()
                    .Be(allPapersCount - 1, "There should be one less paper after the delete.");
        }

        [Theory]
        [PropertyData("FiveNewNewspapersInList")]
        public void Find_AgainstNameSubstringOdd_ReturnsCorrectNumberOfResults(List<Newspaper> papers)
        {
            //	Arrange

            var repo = GetBasicNewspaperRepository();
            papers.ForEach(repo.Save);

            //	Act

            var results = repo.Find(n => n.Name.Contains("odd"));

            //	Assert
            results.Count().Should().Be(3, "There are three names with 'odd' in the list");
        }

        [Theory]
        [PropertyData("FiveNewNewspapersInList")]
        public void Find_AgainstNameSubstringPaper_ReturnsCorrectNumberOfResults(List<Newspaper> papers)
        {
            //	Arrange
            var papersCount = papers.Count;
            var repo = GetBasicNewspaperRepository();
            papers.ForEach(repo.Save);

            //	Act

            var results = repo.Find(n => n.Name.Contains("paper"));

            //	Assert
            results.Count().Should().Be(papersCount, "There are " + papersCount + " names with 'paper' in the list");
        }

        [Theory]
        [PropertyData("SingleNewNewspaperInList")]
        [PropertyData("TwoNewNewspapersInList")]
        [PropertyData("ThreeNewNewspapersInList")]
        [PropertyData("FiveNewNewspapersInList")]
        public void Get_WhenSearchingByKey_ReturnsExactMatch(List<Newspaper> papers)
        {
            //	Arrange
            var repo = GetBasicNewspaperRepository();
            papers.ForEach(repo.Save);
            var lastPaper = papers.Last();

            //	Act
            var resultPaper = repo.Get(lastPaper.UKey);

            //	Assert
            resultPaper.UKey.Should().Be(lastPaper.UKey, "Find the last on on the list every time.");
        }

        [Fact]
        public void OnCreate_Repository_ReturnsEmptyCollectionOnFind()
        {
            //	Arrange
            var repo = GetBasicNewspaperRepository();

            //	Act
            var result = repo.Find(newspaper => newspaper != null);

            //	Assert

            result.Count().Should().Be(0, "No results returned from search");
        }

        [Theory]
        [PropertyData("SingleNewNewspaperInList")]
        [PropertyData("TwoNewNewspapersInList")]
        [PropertyData("ThreeNewNewspapersInList")]
        [PropertyData("FiveNewNewspapersInList")]
        public void Save_NewNewspaperObjects_StoresObjectsInRepository(List<Newspaper> papers)
        {
            //	Arrange

            var repo = GetBasicNewspaperRepository();

            papers.ForEach(repo.Save);

            //	Act

            var result = repo.Find(n => n != null).ToList();

            //	Assert
            result.ShouldBeEquivalentTo(papers);
        }

        [Fact]
        public void Save_WithDbStatusModified_CallsSerializer()
        {
            //	Arrange

            //	Act

            //	Assert
        }

        [Theory]
        [PropertyData("FiveNewNewspapersInList")]
        public void Save_WithDbStatusOfDeleted_RemovesItemFromTheRepository(List<Newspaper> papers)
        {
            //  Arrange
            var repo = GetBasicNewspaperRepository();
            papers.ForEach(repo.Save);
            var paperToSetDeleted = papers.Skip(1).First();
            paperToSetDeleted.DbStatus = DbModificationState.Deleted;
            var paperAddedToRepoCount = repo.Find(n => n != null).Count();

            //  Act
            repo.Save(paperToSetDeleted);
            var papersAfterSavingDeletedItem = repo.Find(n => n != null).ToList();

            //  Assert
            papersAfterSavingDeletedItem.Count().Should().Be(paperAddedToRepoCount - 1,
                    "Should be one less paper in the repository");
        }

        #endregion

        #region Static test data

        #region Constants and Fields

        // eliminate race condition...how?
        //        private static Newspaper paper1 = new Newspaper {Name = "New paper 1 odd"};
        //        private static Newspaper paper2 = new Newspaper {Name = "New paper 2 even"};
        //        private static Newspaper paper3 = new Newspaper {Name = "New paper 3 odd"};
        //        private static Newspaper paper4 = new Newspaper {Name = "New paper 4 even"};
        //        private static Newspaper paper5 = new Newspaper {Name = "New paper 5 odd"};
        //
        //        private static List<Newspaper> fivePaperList = new List<Newspaper> { paper1, paper2, paper3, paper4, paper5 };
        //        private static List<Newspaper> fourPaperList = new List<Newspaper> { paper1, paper2, paper3, paper4 };
        //        private static List<Newspaper> threePaperList = new List<Newspaper> { paper1, paper2, paper3 };
        //        private static List<Newspaper> twoPaperList = new List<Newspaper> { paper1, paper2 };
        //        private static List<Newspaper> onePaperList = new List<Newspaper> { paper1 };
        //

        #endregion

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

        #endregion Static test data

        #region Utility Routines

        private NewspaperAdRepository GetBasicNewspaperRepository()
        {
            var cache = new NewspaperCache();
            var serializer = Substitute.For<ISerializer<NewspaperCache>>();
            var repo = new NewspaperAdRepository(cache, serializer);
            return repo;
        }

        #endregion Utility Routines

        // ReSharper restore InconsistentNaming
    }
}