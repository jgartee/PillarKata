using System.Collections.Generic;

namespace Data
{
    public interface ISerializer<T>
    {
        void RestoreCache(T cache);
        void SaveCache(T cache);

    }
}