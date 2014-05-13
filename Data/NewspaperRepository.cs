using System;
using System.Collections.Generic;
using System.Linq;
using Models;

namespace Data
{
    public class NewspaperRepository : INewspaperRepository
    {
        #region Instance fields

        private readonly AdvertisementCache _advertisementCache;
        private readonly NewspaperCache _newspaperCache;
        private readonly ISerializer<NewspaperCache> _serializer;

        #endregion

        #region Constructors

        public NewspaperRepository(NewspaperCache newspaperCache, ISerializer<NewspaperCache> serializer)
        {
            _serializer = serializer;
            _newspaperCache = newspaperCache;
            _advertisementCache = new AdvertisementCache();
            _serializer.RestoreCache(_newspaperCache);
            BuildAdvertisementCache();
        }

        #endregion

        #region INewspaperRepository Properties and Members

        public void Delete(object entity)
        {
            var paper = entity as Newspaper;
            var ad = entity as Advertisement;

            if (paper == null)
                DeleteAdvertisement(ad);
            else
                DeleteNewspaper(paper);
        }

        public void DeleteAdvertisement(Advertisement entity)
        {
            if (_advertisementCache.ContainsKey(entity.UKey))
            {
                _advertisementCache.Remove(entity.UKey);

                foreach (var paper in entity.Newspapers)
                    _newspaperCache[paper.UKey].Advertisements.Remove(entity);
            }

            _serializer.SaveCache(_newspaperCache);
        }

        public IEnumerable<Newspaper> Find(Func<Newspaper, bool> predicate)
        {
            var papers = (_newspaperCache.Values ?? new List<Newspaper>()).ToList();

            if (predicate != null)
                papers = papers.Where<Newspaper>(predicate).ToList();

            return papers;
        }

        public Newspaper Get(Guid id)
        {
            return _newspaperCache[id];
        }

        public Advertisement GetAdvertisement(Guid id)
        {
            Advertisement foundAd;
            var isPresent = _advertisementCache.TryGetValue(id, out foundAd);
            return foundAd;
        }

        public List<Advertisement> GetAllAdvertisements()
        {
            return _advertisementCache.Values.ToList();
        }

        public List<Newspaper> GetAllNewspapers()
        {
            return _newspaperCache.Values.OrderBy(n => n.Name).ToList();
        }

        public event EventHandler<RepositoryItemAddedEventArgs> ItemAdded;

        public event EventHandler<RepositoryItemChangedEventArgs> ItemChanged;

        public event EventHandler<RepositoryItemRemovedEventArgs> ItemRemoved;

        public void Save(Newspaper entity)
        {
//            if (entity.DbStatus == DbModificationState.Unchanged)
//                return;

            if (entity.DbStatus != DbModificationState.Deleted)
            {
                if (_newspaperCache.ContainsKey(entity.UKey))
                {
                    _newspaperCache[entity.UKey] = entity;
                    UpdateAdCacheFromPaper(entity);
                    var deletedAds = entity.Advertisements.ToList().Where(a => a.DbStatus == DbModificationState.Deleted).ToList();
                    deletedAds.ForEach(a=>entity.Advertisements.Remove(a));
                    OnItemChanged(entity);
                }
                else
                {
                    _newspaperCache.Add(entity.UKey, entity);
                    UpdateAdCacheFromPaper(entity);
                    OnItemAdded(entity);
                }
            }
            else
            {
                if (_newspaperCache.ContainsKey(entity.UKey))
                {
                    DeleteNewspaper(entity);
                    UpdateAdCacheFromPaper(entity);
                    OnItemRemoved(entity);
                }
            }

            _serializer.SaveCache(_newspaperCache);
        }

        #endregion

        #region Class Members

        private void BuildAdvertisementCache()
        {
            _advertisementCache.Clear();

            foreach (var paper in _newspaperCache.Values)
            {
                foreach (var ad in paper.Advertisements)
                {
                    Advertisement cacheAd;

                    if (!_advertisementCache.TryGetValue(ad.UKey, out cacheAd))
                        _advertisementCache.Add(ad.UKey, ad);
                    else
                    {
                        if (!cacheAd.Newspapers.Contains(paper))
                        {
                            if(!_advertisementCache.ContainsKey(ad.UKey))
                                paper.Advertisements.Add(ad);
                        }
                    }
                }
            }

            UpdateNewspaperCacheFromAdvertisements();
        }

        private void DeleteNewspaper(Newspaper entity)
        {
            if (_newspaperCache.ContainsKey(entity.UKey))
            {
                _newspaperCache.Remove(entity.UKey);

                foreach (var ad in entity.Advertisements)
                    _advertisementCache[ad.UKey].Newspapers.Remove(entity);
            }

            _serializer.SaveCache(_newspaperCache);
        }

        private void OnItemAdded(Newspaper paper)
        {
            if (ItemAdded != null)
                ItemAdded(this, new RepositoryItemAddedEventArgs {Entity = paper});
        }

        private void OnItemChanged(Newspaper paper)
        {
            if (ItemChanged != null)
                ItemChanged(this, new RepositoryItemChangedEventArgs {Entity = paper});
        }

        private void OnItemRemoved(Newspaper paper)
        {
            if (ItemRemoved != null)
                ItemRemoved(this, new RepositoryItemRemovedEventArgs {Entity = paper});
        }

        //jlg bad logic
        private void UpdateAdCacheFromPaper(Newspaper entity)
        {
            List<Advertisement> deletedAds = new List<Advertisement>();

            foreach (var ad in entity.Advertisements)
            {
                switch (entity.DbStatus)
                {
                    case DbModificationState.Unchanged:
                    case DbModificationState.Added:
                    case DbModificationState.Modified:

                        if(!_advertisementCache.ContainsKey(ad.UKey))
                            _advertisementCache.Add(ad.UKey,ad);

                        deletedAds.AddRange(entity.Advertisements.Where(a=>a.DbStatus==DbModificationState.Deleted));
                        break;

                    case DbModificationState.Deleted:

                        foreach (var cacheAd in _advertisementCache.Values.ToList())
                        {
                            cacheAd.Newspapers.Remove(entity);
                        }
                        break;
                }
            }

            deletedAds.ForEach(a=>_advertisementCache.Remove(a.UKey));
        }

        private void UpdateNewspaperCacheFromAdvertisements()
        {
            foreach (var ad in _advertisementCache.Values)
            {
                foreach (var paper in _newspaperCache.Values)
                {
                    foreach (var paperAd in paper.Advertisements)
                    {
                        if (ad.UKey == paper.UKey)
                        {
                            paper.Advertisements[paper.Advertisements.IndexOf(paperAd)] = ad;
                            if (!ad.Newspapers.Contains(paper))
                                ad.Newspapers.Add(paper);
                        }
                    }
                }
            }
        }

        #endregion
    }
}