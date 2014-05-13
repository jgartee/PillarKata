namespace PillarKata.ViewModels
{
    public class CurrentAdvertisementItemChangedMessage
    {
        #region Constructors

        public CurrentAdvertisementItemChangedMessage(AdvertisementItemViewModel itemViewModel)
        {
            ItemViewModel = itemViewModel;
        }

        #endregion

        #region Properties

        public AdvertisementItemViewModel ItemViewModel { get; private set; }

        #endregion
    }
}