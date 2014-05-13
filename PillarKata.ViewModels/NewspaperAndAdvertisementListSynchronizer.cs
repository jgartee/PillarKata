using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

namespace PillarKata.ViewModels
{
    public class NewspaperAndAdvertisementListSynchronizer : ViewModelBase
    {
        private AdvertisementCollectionReadyMessage _adMessage;
        private NewspaperCollectionReadyMessage _paperMessage;

        public NewspaperAndAdvertisementListSynchronizer()
        {
            Messenger.Default.Register<AdvertisementCollectionReadyMessage>(this, AdvertisementCollectionReadyMessageHandler);
            Messenger.Default.Register<NewspaperCollectionReadyMessage>(this, NewspaperCollectionReadyMessageHandler);
        }

        private void NewspaperCollectionReadyMessageHandler(NewspaperCollectionReadyMessage obj)
        {
            _paperMessage = obj;
            SynchronizeLists();
        }

        private void AdvertisementCollectionReadyMessageHandler(AdvertisementCollectionReadyMessage obj)
        {
            _adMessage = obj;
            SynchronizeLists();
        }

        private void SynchronizeLists()
        {
            if (_adMessage == null || _paperMessage == null)
                return;
            var paperList = _paperMessage.NewspaperList.ToList();
            var adList = _adMessage.AdvertisementList.ToList();

            foreach (var paper in paperList)
            {
                var tempAdList = new List<AdvertisementItemViewModel> ();

                foreach (var adModel in paper.Model.Advertisements)
                {
                    try
                    {
                        if (!paper.Advertisements.Any(a => a.UKey == adModel.UKey))
                            tempAdList.Add(adList.First(a=>a.UKey == adModel.UKey));
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                    }
                }

                tempAdList.ForEach(paper.Advertisements.Add);
            }

            foreach (var ad in adList)
            {
                var tempPaperList = new List<NewspaperItemViewModel>();
                foreach(var paperModel in ad.Model.Newspapers)
                {
                    try
                    {
                        if (!ad.Newspapers.Any(n => n.UKey == paperModel.UKey))
                            tempPaperList.Add(paperList.First(p => p.UKey == paperModel.UKey));
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;                        
                    }
                }
                tempPaperList.ForEach(ad.Newspapers.Add);   
            }

        }
    }
}
