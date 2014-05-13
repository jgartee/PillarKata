namespace PillarKata.ViewModels
{
    public class RemovingAdvertisementItemMessage
    {
        #region Constructors

        public RemovingAdvertisementItemMessage(AdvertisementItemViewModel itemViewModel)
        {
            ItemViewModel = itemViewModel;
        }

        #endregion

        #region Properties

        public AdvertisementItemViewModel ItemViewModel { get; private set; }

        #endregion
    }
}