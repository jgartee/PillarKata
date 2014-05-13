namespace PillarKata.ViewModels
{
    public class SaveNewspaperItemMessage
    {
        public NewspaperItemViewModel ItemViewModel { get; private set; }

        public SaveNewspaperItemMessage(NewspaperItemViewModel itemViewModel)
        {
            ItemViewModel = itemViewModel;
        }
    }
}