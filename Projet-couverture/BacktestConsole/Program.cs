using PricingLibrary.Computations;
using PricingLibrary.DataClasses;
using PricingLibrary.MarketDataFeed;
using static System.Net.Mime.MediaTypeNames;
using PricingLibrary.TimeHandler;
using MathNet.Numerics.Distributions;
using CsvHelper;
using System.Globalization;
using AutofinancingSystematicPortfolio;
using PricingLibrary.RebalancingOracleDescriptions;



public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            /* Getting instructions from the command line*/
            string pathTestParams = "C:\\Users\\localuser\\Documents\\projet-couverture\\Projet-couverture\\TestParameters\\share_5_strike_10.json"; //args[0];
            string mktData = "C:\\Users\\localuser\\Documents\\projet-couverture\\Projet-couverture\\MarketData\\InputCSVFile\\data_share_5_3.csv"; // args[1];   
            string pfVals = "C:\\Users\\localuser\\Documents\\projet-couverture\\Projet-couverture\\MarketData\\OutputCSVFile\\realValues.csv"; // args[2];    
            string tfVals = "C:\\Users\\localuser\\Documents\\projet-couverture\\Projet-couverture\\MarketData\\OutputCSVFile\\theoricalValues.csv"; // args[3];    

            /* Getting the test parameters in the .json and the market data .csv */
            JsonParser donneesJson = new JsonParser(pathTestParams);
            TestParameters testParams = donneesJson.GiveParameter();
            IRebalancingOracleDescription rebalancingOracleDescription = testParams.RebalancingOracleDescription;
            IRebalancingOracle rebalancingOracle;
            if (rebalancingOracleDescription.Type == RebalancingOracleType.Regular)
            {
                RegularOracleDescription regular = (RegularOracleDescription)rebalancingOracleDescription;
                rebalancingOracle = new RegularRebalancingOracle(regular.Period);
            }

            else
            {
                WeeklyOracleDescription week = (WeeklyOracleDescription)rebalancingOracleDescription;
                rebalancingOracle = new WeeklyRebalancingOracle(week.RebalancingDay);
            }
            var test = new ListMarketData();
            test.ReadCSVFile(mktData);
            List<ShareValue> datas = test.Listdata;
            List<DataFeed> listDatafeed = test.GetDataFeed();


            /* SYSTEMATIC STRATEGY */

            // Initialization
            List<PortfolioPrice> portfolioRealValues = new List<PortfolioPrice>();
            List<PortfolioPrice> PortfolioTheoreticalValues = new List<PortfolioPrice>();
            Pricer pricer = new Pricer(testParams);
            DateTime maturity = testParams.BasketOption.Maturity;
            DateTime startDate = listDatafeed[0].Date;
            double timeToMaturity = MathDateConverter.ConvertToMathDistance(startDate, maturity);
            double[] spot = new double[listDatafeed[0].PriceList.Count];
            for (int i = 1; i <= listDatafeed[0].PriceList.Count; i++)
                spot[i - 1] = listDatafeed[0].PriceList["share_" + i];
            double P0 = pricer.Price(timeToMaturity, spot).Price;
            PortfolioTheoreticalValues.Add(new PortfolioPrice { DateOfPrice = listDatafeed[0].Date, Price = P0 });
            Dictionary<String, double> deltas = new Dictionary<string, double>();
            double[] tabDelta = pricer.Price(timeToMaturity, spot).Deltas;
            for (int i = 1; i <= tabDelta.Length; i++)
                deltas["share_" + i] = tabDelta[i - 1];

            // instanciation of the autofinancing portfolio
            AutofinancingPortfolio book = new AutofinancingPortfolio(P0, listDatafeed[0].PriceList, deltas, startDate);
            portfolioRealValues.Add(new PortfolioPrice { DateOfPrice = listDatafeed[0].Date, Price = book.PfValueBeforeRebalancing(book.Composition, listDatafeed[0].PriceList, listDatafeed[0].Date) });

            // Systematic strategy
            for (int i = 1; i < listDatafeed.Count; i++)
            {
                if (rebalancingOracle.RebalancingTime(listDatafeed[i].Date))
                {
                    for (int k = 1; k <= listDatafeed[i].PriceList.Count; k++)
                        spot[k - 1] = listDatafeed[i].PriceList["share_" + k];
                    PortfolioTheoreticalValues.Add(new PortfolioPrice { DateOfPrice = listDatafeed[i].Date, Price = pricer.Price(MathDateConverter.ConvertToMathDistance(listDatafeed[i].Date, maturity), spot).Price });
                    deltas = book.getDeltas(pricer, maturity, listDatafeed[i]);
                    portfolioRealValues.Add(new PortfolioPrice { DateOfPrice = listDatafeed[i].Date, Price = book.PfValueBeforeRebalancing(book.Composition, listDatafeed[i].PriceList, listDatafeed[i].Date) });
                    book.Rebalancing(deltas, listDatafeed[i].PriceList, listDatafeed[i].Date);
                }
            }

            // Writting in the output .csv files
            using (var writer = new StreamWriter(pfVals))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(portfolioRealValues);
            }
            using (var writer = new StreamWriter(tfVals))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(PortfolioTheoreticalValues);
            }
            Console.WriteLine("Backtest terminé, vous pouvez voir les résultats dans MarketDate/OutputCSVFile");
        }
        catch(Exception e)
        {
            throw new Exception("Les inputs sont mauvaises");
        }

    }
}


