using System;
using System.Collections.Generic;

namespace Models
{
    public interface INewspaperAdRepository : IRepository<Newspaper,Guid>
    {
        void DeleteAdvertisement(Advertisement entity);
        Advertisement GetAdvertisement(Guid id);
        List<Advertisement> GetAllAdvertisements();
        List<Newspaper> GetAllNewspapers();
    }
}