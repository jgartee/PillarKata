namespace PillarKata.ViewModels
{
    public class CurrentNewspaperItemChangedMessage
    {
        public NewspaperItemViewModel ItemViewModel { get; private set; }

        public CurrentNewspaperItemChangedMessage(NewspaperItemViewModel itemViewModel)
        {
            ItemViewModel = itemViewModel;
        }
    }
}