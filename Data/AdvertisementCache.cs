using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Models;

namespace Data
{
    public class AdvertisementCache : IDictionary<Guid, Advertisement>
    {
        #region Instance fields

        private readonly Dictionary<Guid, Advertisement> _adDictionary;

        #endregion

        #region Constructors

        public AdvertisementCache()
        {
            _adDictionary = new Dictionary<Guid, Advertisement>();
        }

        #endregion

        #region IDictionary<Guid,Advertisement> Properties and Members

        public void Add(KeyValuePair<Guid, Advertisement> item)
        {
            _adDictionary.Add(item.Key, item.Value);
        }

        public void Add(Guid key, Advertisement value)
        {
            _adDictionary.Add(key, value);
        }

        public void Clear()
        {
            _adDictionary.Clear();
        }

        public bool Contains(KeyValuePair<Guid, Advertisement> item)
        {
            return _adDictionary.Contains(item);
        }

        public bool ContainsKey(Guid key)
        {
            return _adDictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<Guid, Advertisement>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count { get; private set; }

        public IEnumerator<KeyValuePair<Guid, Advertisement>> GetEnumerator()
        {
            return _adDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool IsReadOnly { get; private set; }

        public Advertisement this[Guid key]
        {
            get { return _adDictionary[key]; }
            set { _adDictionary[key] = value; }
        }

        public ICollection<Guid> Keys { get; private set; }

        public bool Remove(KeyValuePair<Guid, Advertisement> item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(Guid key)
        {
            return _adDictionary.Remove(key);
        }

        public bool TryGetValue(Guid key, out Advertisement value)
        {
            var returnValue = _adDictionary.TryGetValue(key, out value);
            return returnValue;
        }

        public ICollection<Advertisement> Values
        {
            get { return _adDictionary.Values; }
            private set { }
        }

        #endregion

        #region Class Members


        #endregion
    }
}