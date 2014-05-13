using System;
using System.Collections.Generic;

namespace Models
{
    public class Advertisement : EntityBase
    {
        #region Constants and Fields

        public const string MSG_EMPTY_NAME_ERROR = "Advertisement name cannot be empty.";
        public const string MSG_EMPTY_TEXT_ERROR = "Advertisement body text cannot be empty";
        public const string MSG_NEW_ADVERTISEMENT_NAME = "<New Advertisement>";
        public const string MSG_NEW_ADVERTISEMENT_TEXT = "<New Advertisement Body Text";

        #endregion

        #region Instance fields

        private readonly List<Newspaper> _newspapers = new List<Newspaper>();
        private string _name;
        private string _text;
        private Guid _uKey;

        #endregion

        #region Constructors

        public Advertisement()
        {
            UKey = Guid.NewGuid();
            DbStatus = DbModificationState.Added;
            Name = MSG_NEW_ADVERTISEMENT_NAME;
            Text = MSG_NEW_ADVERTISEMENT_TEXT;
            DbStatusChanged += Advertisement_DbStatusChanged;
        }

        public Advertisement(Guid uKey, string name, string text)
        {
            UKey = uKey;
            _name = name;
            _text = text;
            DbStatusChanged += Advertisement_DbStatusChanged;
            DbStatus = DbModificationState.Unchanged;
        }

        #endregion

        #region Properties

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value)
                    return;

                OnPropertyChanging(() => Name);
                _name = value ?? "";

                if (string.IsNullOrEmpty(_name))
                    SetError(() => Name, MSG_EMPTY_NAME_ERROR);
                else
                    ClearError(() => Name);

                OnPropertyChanged(() => Name);
                NotifyErrorsChanged(() => Name);
            }
        }

        public List<Newspaper> Newspapers
        {
            get { return _newspapers; }
            set
            {
                _newspapers.Clear();

                if (value == null)
                    return;

                foreach (Newspaper paper in value)
                    AddNewspaper(paper);
            }
        }

        public string Text
        {
            get { return _text; }
            set
            {
                if (_text == value)
                    return;

                OnPropertyChanging(() => Text);
                _text = value;

                if (string.IsNullOrEmpty(_text))
                    SetError(() => Text, MSG_EMPTY_TEXT_ERROR);
                else
                    ClearError(() => Text);

                OnPropertyChanged(() => Text);
            }
        }

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

        public void AddNewspaper(Newspaper paper)
        {
            if (paper == null)
                return;

            if (!_newspapers.Contains(paper))
            {
                if (!paper.Advertisements.Contains(this))
                    paper.Advertisements.Add(this);

                _newspapers.Add(paper);
            }
        }

        public void AddNewspapers(List<Newspaper> newspapers)
        {
            if (newspapers == null)
                return;

            newspapers.ForEach(AddNewspaper);
        }

        private void Advertisement_DbStatusChanged(object sender, EventArgs e)
        {
            foreach (var paper in Newspapers)
            {
                if (paper.DbStatus == DbModificationState.Unchanged)
                    paper.DbStatus = DbModificationState.Modified;
            }
        }

        #endregion
    }
}