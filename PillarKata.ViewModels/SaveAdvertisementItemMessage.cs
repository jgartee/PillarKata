namespace PillarKata.ViewModels
{
    public class SaveAdvertisementItemMessage
    {
        #region Constructors

        public SaveAdvertisementItemMessage(AdvertisementItemViewModel itemViewModel)
        {
            ItemViewModel = itemViewModel;
        }

        #endregion

        #region Properties

        public AdvertisementItemViewModel ItemViewModel { get; private set; }

        #endregion
    }
}