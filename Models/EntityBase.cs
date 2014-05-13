using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Models
{
    [Flags]
    public enum DbModificationState
    {
        Unchanged = 1,
        Added = 2,
        Modified = 4,
        Deleted = 8
    };

    public abstract class EntityBase : INotifyPropertyChanging, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        #region Instance fields
        
        protected event EventHandler<EventArgs> DbStatusChanged;
        private readonly Dictionary<string, List<string>> _errorDictionary = new Dictionary<string, List<string>>();

        private DbModificationState _dbStatus;

        #endregion

        #region Constructors

        protected EntityBase()
        {
            DbStatus = DbModificationState.Unchanged;
        }

        #endregion

        #region Properties

        public DbModificationState DbStatus
        {
            get { return _dbStatus; }
            set
            {
                if (_dbStatus == value)
                    return;

                _dbStatus = value;
                OnPropertyChanged(() => DbStatus);
            }
        }

        public bool IsAdded
        {
            get { return DbStatus == DbModificationState.Added; }
        }

        public bool IsChanged
        {
            get { return DbStatus == DbModificationState.Modified; }
        }

        public bool IsDeleted
        {
            get { return DbStatus == DbModificationState.Deleted; }
        }

        public bool IsEntityValid
        {
            get { return !HasErrors; }
        }

        public bool IsUnchanged
        {
            get { return DbStatus == DbModificationState.Unchanged; }
        }

        #endregion

        #region INotifyDataErrorInfo Properties and Members

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            var errorReturn = new List<string>();

            if (_errorDictionary.ContainsKey(propertyName))
                errorReturn = _errorDictionary[propertyName];

            return errorReturn;
        }

        public bool HasErrors
        {
            get { return _errorDictionary.Values.Any(); }
        }

        #endregion

        #region INotifyPropertyChanged Properties and Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region INotifyPropertyChanging Properties and Members

        public event PropertyChangingEventHandler PropertyChanging;

        #endregion

        #region Class Members

        protected void ClearError<T>(Expression<Func<T>> propertyExpression)
        {
            List<string> errorCollection;

            _errorDictionary.TryGetValue(ExtractPropertyName(propertyExpression), out errorCollection);

            if (errorCollection != null && errorCollection.Any())
                _errorDictionary.Remove(ExtractPropertyName(propertyExpression));
            NotifyErrorsChanged(propertyExpression);
        }

        protected void NotifyErrorsChanged<T>(Expression<Func<T>> propertyExpression)
        {
            if (ErrorsChanged != null)
                ErrorsChanged(this, new DataErrorsChangedEventArgs(ExtractPropertyName(propertyExpression)));
        }

        protected void OnPropertyChanged<T>(Expression<Func<T>> expression)
        {
            var memberExpression = (MemberExpression) expression.Body;
            string propertyName = memberExpression.Member.Name;

            if (memberExpression.Member.DeclaringType != null &&
                (propertyName != "DbStatus" && memberExpression.Member.DeclaringType.Name != "EntityBase"))
            {
                DbStatus = DbStatus == DbModificationState.Unchanged ? DbModificationState.Modified : DbStatus;
            }

            if(DbStatusChanged != null)
                DbStatusChanged(this, new EventArgs());

            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChanging<T>(Expression<Func<T>> expression)
        {
            var memberExpression = (MemberExpression) expression.Body;
            string propertyName = memberExpression.Member.Name;

            if (PropertyChanging != null)
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
        }

        protected void SetError<T>(Expression<Func<T>> propertyExpression, string message)
        {
            List<string> errorCollection;

            _errorDictionary.TryGetValue(ExtractPropertyName(propertyExpression), out errorCollection);

            if (errorCollection != null && errorCollection.Any())
            {
                if (!errorCollection.Contains(message))
                    errorCollection.Add(message);
            }
            else
            {
                errorCollection = new List<string> {message};
                _errorDictionary.Add(ExtractPropertyName(propertyExpression), errorCollection);
            }

            NotifyErrorsChanged(propertyExpression);
        }

        private static string ExtractPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("not member access expression.");

            var property = memberExpression.Member as PropertyInfo;
            if (property == null)
                throw new ArgumentException("expression not property.");

            var getMethod = property.GetGetMethod(true);
            if (getMethod == null)
                throw new ArgumentException("static expression.");

            return memberExpression.Member.Name;
        }

        #endregion
    }
}