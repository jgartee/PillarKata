using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Models
{
    public class Newspaper : EntityBase
    {
        #region Constants and Fields

        public static readonly string MSG_EMPTY_NAME_ERROR = "Newspaper name cannot be empty.";
        public static readonly string MSG_NEW_NEWSPAPER_NAME = "<New Newspaper>";

        #endregion

        #region Instance fields

        private readonly List<Advertisement> _advertisements = new List<Advertisement>();
        private string _name = "";
        private Guid _uKey;

        #endregion

        #region Constructors

        public Newspaper()
        {
            Name = MSG_NEW_NEWSPAPER_NAME;
            UKey = Guid.NewGuid();
            DbStatus = DbModificationState.Added;
            DbStatusChanged += Advertisement_DbStatusChanged;
        }

        public Newspaper(Guid uKey, string name)
        {
            UKey = uKey;
            _name = name;
            DbStatusChanged += Advertisement_DbStatusChanged;
            DbStatus = DbModificationState.Unchanged;
        }

        #endregion

        #region Properties

        public List<Advertisement> Advertisements
        {
            get { return _advertisements; }
            set
            {
                _advertisements.Clear();

                if (value == null)
                    return;

                foreach (Advertisement ad in value)
                    _advertisements.Add(ad);
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (!string.IsNullOrEmpty(_name) && _name == value)
                    return;

                OnPropertyChanging(() => Name);
                _name = value ?? "";

                if (string.IsNullOrEmpty(_name))
                    SetError(() => Name, MSG_EMPTY_NAME_ERROR);
                else
                    ClearError(() => Name);
                DbStatus = DbModificationState.Modified;
                OnPropertyChanged(() => Name);
            }
        }

        [DataMember]
        public Guid UKey
        {
            get { return _uKey; }
            set
            {
                if (_uKey == value)
                    return;

                OnPropertyChanging(() => UKey);
                _uKey = value;
                OnPropertyChanged(() => UKey);
            }
        }

        #endregion

        #region Class Members

        public void AddAdvertisement(Advertisement advertisement)
        {
            if (advertisement == null)
                return;

            if (!_advertisements.Contains(advertisement))
            {
                if(!advertisement.Newspapers.Contains(this))
                    advertisement.Newspapers.Add(this);

                _advertisements.Add(advertisement);
            }
        }

        public void AddAdvertisements(List<Advertisement> advertisements)
        {
            if (advertisements == null)
                return;

            advertisements.ForEach(AddAdvertisement);
        }

        public void RemoveAdvertisement(Advertisement advertisement)
        {
            if (advertisement == null)
                return;

            if (advertisement.Newspapers.Contains(this))
                advertisement.Newspapers.Remove(this);

            _advertisements.Remove(advertisement);
        }

        public void RemoveAdvertisements(List<Advertisement> advertisements)
        {
            if (advertisements == null)
                return;

            advertisements.ForEach(RemoveAdvertisement);
        }

        private void Advertisement_DbStatusChanged(object sender, EventArgs e)
        {
            foreach (var ad in Advertisements)
            {
                if (ad.DbStatus == DbModificationState.Unchanged)
                    ad.DbStatus = DbModificationState.Modified;
            }
        }

        #endregion
    }
}