using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Models;

namespace Data
{
    public class NewspaperCache : IDictionary<Guid, Newspaper>
    {
        #region Instance fields

        private readonly Dictionary<Guid, Newspaper> _paperDictionary;

        #endregion

        #region Constructors

        public NewspaperCache()
        {
            _paperDictionary = new Dictionary<Guid, Newspaper>();
        }

        #endregion

        #region IDictionary<Guid,Newspaper> Properties and Members

        public void Add(KeyValuePair<Guid, Newspaper> item)
        {
            _paperDictionary.Add(item.Key, item.Value);
        }

        public void Add(Guid key, Newspaper value)
        {
            _paperDictionary.Add(key, value);
        }

        public void Clear()
        {
            _paperDictionary.Clear();
        }

        public bool Contains(KeyValuePair<Guid, Newspaper> item)
        {
            return _paperDictionary.Contains(item);
        }

        public bool ContainsKey(Guid key)
        {
            return _paperDictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<Guid, Newspaper>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count { get; private set; }

        public IEnumerator<KeyValuePair<Guid, Newspaper>> GetEnumerator()
        {
            return _paperDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool IsReadOnly { get; private set; }

        public Newspaper this[Guid key]
        {
            get { return _paperDictionary[key]; }
            set { _paperDictionary[key] = value; }
        }

        public ICollection<Guid> Keys { get; private set; }

        public bool Remove(KeyValuePair<Guid, Newspaper> item)
        {
            return _paperDictionary.Remove(item.Key);
        }

        public bool Remove(Guid key)
        {
            return _paperDictionary.Remove(key);
        }

        public bool TryGetValue(Guid key, out Newspaper value)
        {
            return _paperDictionary.TryGetValue(key, out value);
        }

        public ICollection<Newspaper> Values
        {
            get { return _paperDictionary.Values;}
            private set { }
        }

        #endregion
    }
}