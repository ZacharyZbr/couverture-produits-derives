using PricingLibrary.Computations;
using PricingLibrary.DataClasses;
using PricingLibrary.MarketDataFeed;
using static System.Net.Mime.MediaTypeNames;
using PricingLibrary.TimeHandler;
using MathNet.Numerics.Distributions;
using CsvHelper;
using System.Globalization;
using AutofinancingSystematicPortfolio;




public class Program
{
    public static void Main(string[] args)
    {
        string pathTestParams = args[0];
        string mktData = args[1];   
        string pfVals = args[2];    
        string tfVals = args[3];    

        JsonParser donneesJson = new JsonParser(pathTestParams);
        TestParameters testParams = donneesJson.GiveParameter();



        var test = new ListMarketData();
        test.ReadCSVFile(mktData);
        List<ShareValue> datas = test.GetListSharedValue();
        List<DataFeed> listDatafeed = test.GetDataFeed();



        /* TEST de la boucle */
        List<ReplicatingPortfolio> listReplicatingPortfolios = new List<ReplicatingPortfolio>();
        Pricer pricer = new Pricer(testParams);
        DateTime maturity = testParams.BasketOption.Maturity;
        DateTime startDate = listDatafeed[0].Date;
        double timeToMaturity = MathDateConverter.ConvertToMathDistance(startDate, maturity);
        double[] spot = new double[listDatafeed[0].PriceList.Count];
        for (int i = 1; i <= listDatafeed[0].PriceList.Count; i++)
            spot[i - 1] = listDatafeed[0].PriceList["share_" + i];
        double P0 = pricer.Price(timeToMaturity, spot).Price;
        Dictionary<String, double> deltas = new Dictionary<string, double>();
        double[] tabDelta = pricer.Price(timeToMaturity, spot).Deltas;
        for (int i = 1; i <= tabDelta.Length; i++)
            deltas["share_" + i] = tabDelta[i - 1];
        AutofinancingPortfolio book = new AutofinancingPortfolio(P0, listDatafeed[0].PriceList, deltas, startDate);

        for (int i = 1; i < listDatafeed.Count; i++)
        {
            deltas = book.getDeltas(pricer, testParams, listDatafeed[i], listDatafeed[i].Date);
            book.Rebalancing(deltas, listDatafeed[i].PriceList, listDatafeed[i].Date);
            foreach (KeyValuePair<string, double> data in book.Composition)
            {
                listReplicatingPortfolios.Add(new ReplicatingPortfolio { Id = data.Key, DateOfPrice = listDatafeed[i].Date, Composition = data.Value, Price = listDatafeed[i].PriceList[data.Key] });
                Console.WriteLine("share: {0}, Valeur: {1}",
                    data.Key, data.Value);
            }
            Console.WriteLine("Quantité sans risque : " + book.RiskFreeQuantity);

        }
        foreach (ReplicatingPortfolio data in listReplicatingPortfolios)
        {
            Console.WriteLine(data.Id);
            Console.WriteLine(data.DateOfPrice);
            Console.WriteLine(data.Price);
            Console.WriteLine(data.Composition);
        }
        string filename = "C:\\Users\\localuser\\Documents\\projet-net-2.0\\test.csv";
        using (var writer = new StreamWriter(filename))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(listReplicatingPortfolios);
        }

    }
}


