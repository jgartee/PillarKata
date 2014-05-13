using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Models;

namespace Data
{
    public class NewspaperSerializer : ISerializer<NewspaperCache>
    {
        private const string PaperFileName = "NewspaperData.txt";

        public void RestoreCache(NewspaperCache cache)
        {
            try
            {
                if (File.Exists(PaperFileName))
                {
                    using (var fs = File.Open(PaperFileName, FileMode.Open))
                    {
                        using (var sr = new StreamReader(fs))
                        {
                            var sb = new StringBuilder(sr.ReadToEnd());

                            var newspaperObjects = sb.ToString().Split('\x5');

                            foreach (string paperObject in newspaperObjects)
                            {
                                var adList = new List<Advertisement>();
                                if (string.IsNullOrEmpty(paperObject) || paperObject == Environment.NewLine)
                                    continue;

                                var classParts = paperObject.Split('\x2');
                                var paperFields = classParts[0].Split('\x1');
                                var ads = classParts[1].Split('\x4');

                                adList.AddRange(from adClass in ads
                                                where !string.IsNullOrEmpty(adClass)
                                                select adClass.Split('\x3')
                                                into adFields
                                                select
                                                    new Advertisement
                                                    {UKey = new Guid(adFields[0]), Name = adFields[1], Text = adFields[2]});
                                adList.ForEach(a => a.DbStatus = DbModificationState.Unchanged);

                                var classFields = classParts[0].Split('\x1');
                                var newsPaper = new Newspaper {UKey = new Guid(classFields[0]), Name = classFields[1]};
                                newsPaper.AddAdvertisements(adList);
                                newsPaper.DbStatus = DbModificationState.Unchanged;
                                cache.Add(newsPaper.UKey, newsPaper);
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                
            }
        }

        public void SaveCache(NewspaperCache cache)
        {
            var newsPapers = cache.Values.ToList();
            var sb = new StringBuilder("");

            using(var fs = File.Open(PaperFileName, FileMode.Create))
            {
                using(var sw = new StreamWriter(fs))
                {
                    foreach(var paper in newsPapers)
                    {
                        sb.Append(paper.UKey + "\x1");
                        sb.Append(paper.Name + "\x2");

                        foreach(var ad in paper.Advertisements)
                        {
                            sb.Append(ad.UKey + "\x3");
                            sb.Append(ad.Name + "\x3");
                            sb.Append(ad.Text + "\x3");
                            sb.Append("\x4");
                        }

                        sb.Append("\x5");
                    }

                    sw.WriteLine(sb.ToString());
                }
            }

            cache.Values.ToList().ForEach(n =>
                               {
                                   n.Advertisements.ForEach(a => a.DbStatus = DbModificationState.Unchanged);
                                   n.DbStatus = DbModificationState.Unchanged;
                               });
        }
    }
}