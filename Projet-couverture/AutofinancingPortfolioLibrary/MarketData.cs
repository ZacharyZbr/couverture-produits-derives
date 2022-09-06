using CsvHelper;
using PricingLibrary.MarketDataFeed;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;


namespace AutofinancingSystematicPortfolio
{
    public class ListMarketData
    {
        private List<ShareValue> Listdata = new List<ShareValue>();
        public List<ShareValue> GetListSharedValue()
        {
            return Listdata;
        }

        public void ReadCSVFile(string mktData)
        {
            using (var reader = new StreamReader(mktData))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<ShareValue>();
                foreach (ShareValue element in records)
                {
                    Listdata.Add(element);
                }
            }
        }

        public List<DataFeed> GetDataFeed()
        {
            List<DataFeed> listDataFeed = new List<DataFeed>();
            var currentdate = DateTime.Now;
            DataFeed datafeed = new DataFeed(DateTime.Now, new Dictionary<string, double>());
            foreach (ShareValue element in Listdata)
            {
                if (currentdate != element.DateOfPrice)
                {
                    listDataFeed.Add(datafeed);
                    currentdate = element.DateOfPrice;
                    Dictionary<string, double> dictValue = new Dictionary<string, double>();
                    datafeed = new DataFeed(currentdate, dictValue);
                    datafeed.PriceList.Add(element.Id, element.Value);
                }
                else
                {
                    datafeed.PriceList.Add(element.Id, element.Value);
                }
            }
            listDataFeed.RemoveAt(0);
            return listDataFeed;
        }

    }

    public class ReplicatingPortfolio
    {
        public string Id { get; set; }
        public DateTime DateOfPrice { get; set; }
        public double Composition { get; set; }
        public double Price { get; set; }

    }

}
