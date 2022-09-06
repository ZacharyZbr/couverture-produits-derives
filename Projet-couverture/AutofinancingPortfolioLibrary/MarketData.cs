using CsvHelper;
using PricingLibrary.MarketDataFeed;
using System.Globalization;



namespace AutofinancingSystematicPortfolio
{
    /// <summary>
    /// Classe permettant de manipuler les prix des actions
    /// </summary>
    public class ListMarketData
    {
        /// <summary>
        /// Liste des données sur les sous-jacents de l'option
        /// </summary>
        public List<ShareValue> Listdata {get; private set;}

        /// <summary>
        /// Constructeur de la liste recepérant les données
        /// </summary>
        public ListMarketData()
        {
            Listdata = new List<ShareValue>();
        }

        /// <summary>
        /// Parse un fichier CSV contenant une colonne Id, DateOfPrice, et Value.
        /// </summary>
        /// <param name="mktData">Chemin du fichier à parser</param>
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

        /// <summary>
        /// Permet de transformer la liste des données obtenues en liste de DataFeed
        /// </summary>
        /// <returns></returns>
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

}
