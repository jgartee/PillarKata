using System;

namespace PillarKata.ViewModels
{
    public class RemovingNewspaperItemMessage
    {
        public NewspaperItemViewModel ItemViewModel { get; set; }

        public RemovingNewspaperItemMessage(NewspaperItemViewModel itemViewModel)
        {
            ItemViewModel = itemViewModel;
        }
    }
}