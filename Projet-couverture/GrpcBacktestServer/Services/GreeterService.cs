using Grpc.Core;
using GrpcBacktestServer;
using Google.Protobuf.WellKnownTypes;
using BacktestGrpc.Protos;
using PricingLibrary.DataClasses;
using PricingLibrary.RebalancingOracleDescriptions;
using AutofinancingSystematicPortfolio;
using PricingLibrary.Computations;
using PricingLibrary.TimeHandler;
using PricingLibrary.MarketDataFeed;
using System.Xml.Linq;
using Google.Protobuf;

namespace GrpcBacktestServer.Services
{
    public class GreeterService : BacktestRunner.BacktestRunnerBase
    {
        /*private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }*/

        public override Task<BacktestOutput> RunBacktest(BacktestRequest request, ServerCallContext context)
        {
            //Console.WriteLine(request.Data.DataValues.Count);
            var price_params = request.TstParams.PriceParams;
            var basket_params = request.TstParams.BasketParams;
            var reb_params = request.TstParams.RebParams;

            var data = request.Data;

            List<DataFeed> listDatafeed = new List<DataFeed>();
            var currentdate = DateTime.Now;
            DataFeed datafeed = new DataFeed(DateTime.Now, new Dictionary<string, double>());
            for (int i = 0; i < data.DataValues.Count; i++)
            {
                Console.WriteLine(data.DataValues[i].Value);
            }
                for (int i = 0; i < request.Data.DataValues.Count; i++)
            {
                if (currentdate != request.Data.DataValues[i].Date.ToDateTime()) {
                    listDatafeed.Add(datafeed);
                    currentdate = request.Data.DataValues[i].Date.ToDateTime();
                    Dictionary<string, double> dictValue = new Dictionary<string, double>();
                    datafeed = new DataFeed(currentdate, dictValue);
                    datafeed.PriceList.Add(request.Data.DataValues[i].Id, request.Data.DataValues[i].Value);     
                }
                else
                {
                    Console.WriteLine(request.Data.DataValues[i].Date);
                    datafeed.PriceList.Add(request.Data.DataValues[i].Id, request.Data.DataValues[i].Value);
                }
            }
            listDatafeed.RemoveAt(0);
            /*ValueInfo valueInfoBoucle = new ValueInfo();
            valueInfoBoucle.Date = date;*/
        

            TestParameters testParams = new TestParameters();
            // Recup des correlations
            for (int i = 0; i < price_params.Corrs.Count; i++)
            {
                for (int j = 0; i < price_params.Corrs[j].Value.Count; j++)
                {
                    testParams.PricingParams.Correlations[i][j] = price_params.Corrs[i].Value[j];
                }
            }

            //Recup des vols
            for (int i = 0; i < price_params.Vols.Count; i++)
            {
                testParams.PricingParams.Volatilities[i] = price_params.Vols[i];
            }

            // BasketParams
            testParams.BasketOption.Strike = basket_params.Strike;
            testParams.BasketOption.Maturity = basket_params.Maturity.ToDateTime();
            for (int i = 0; i < basket_params.ShareIds.Count; i++)
            {
                testParams.BasketOption.UnderlyingShareIds[i] = basket_params.ShareIds[i];
            }
            for (int i = 0; i < basket_params.Weights.Count; i++)
            {
                testParams.BasketOption.Weights[i] = basket_params.Weights[i];
            }

            // Reb params
            IRebalancingOracle rebalancingOracle;
            if (reb_params.RebTypeCase == RebalancingParams.RebTypeOneofCase.Regular)
            {
                rebalancingOracle = new RegularRebalancingOracle(reb_params.Regular.Period);
            }
            else
            {
                rebalancingOracle = new WeeklyRebalancingOracle((DayOfWeek)(int)reb_params.Weekly.Day);//MondayDayOfWeek.Monday
            }

            BacktestOutput backtestOutput = new BacktestOutput();
            /* TEST de la boucle */
            List<PortfolioPrice> portfolioRealValues = new List<PortfolioPrice>();
            List<PortfolioPrice> PortfolioTheoreticalValues = new List<PortfolioPrice>();

            ValueInfo valueInfoTheorical = new ValueInfo();
            ValueInfo valueInfoReal = new ValueInfo();

            Pricer pricer = new Pricer(testParams);
            DateTime maturity = testParams.BasketOption.Maturity;
            DateTime startDate = listDatafeed[0].Date;
            double timeToMaturity = MathDateConverter.ConvertToMathDistance(startDate, maturity);
            double[] spot = new double[listDatafeed[0].PriceList.Count];
            for (int i = 1; i <= listDatafeed[0].PriceList.Count; i++)
                spot[i - 1] = listDatafeed[0].PriceList["share_" + i];
            double P0 = pricer.Price(timeToMaturity, spot).Price;
            PortfolioTheoreticalValues.Add(new PortfolioPrice { DateOfPrice = listDatafeed[0].Date, Price = P0 });
            valueInfoTheorical.Date = request.Data.DataValues[0].Date;
            valueInfoReal.Date = request.Data.DataValues[0].Date;
            valueInfoTheorical.Value = P0;
            
            Dictionary<String, double> deltas = new Dictionary<string, double>();
            double[] tabDelta = pricer.Price(timeToMaturity, spot).Deltas;
            for (int i = 1; i <= tabDelta.Length; i++)
                deltas["share_" + i] = tabDelta[i - 1];
            AutofinancingPortfolio book = new AutofinancingPortfolio(P0, listDatafeed[0].PriceList, deltas, startDate);
            portfolioRealValues.Add(new PortfolioPrice { DateOfPrice = listDatafeed[0].Date, Price = book.PfValueBeforeRebalancing(book.Composition, listDatafeed[0].PriceList, listDatafeed[0].Date) });
            valueInfoReal.Value = book.PfValueBeforeRebalancing(book.Composition, listDatafeed[0].PriceList, listDatafeed[0].Date);
            backtestOutput.PortfolioValues.Add(valueInfoReal);
            backtestOutput.TheoValues.Add(valueInfoTheorical);

            for (int i = 1; i < listDatafeed.Count; i++)
            {
                if (rebalancingOracle.RebalancingTime(listDatafeed[i].Date))
                {
                    for (int k = 1; k <= listDatafeed[i].PriceList.Count; k++)
                        spot[k - 1] = listDatafeed[i].PriceList["share_" + k];
                    PortfolioTheoreticalValues.Add(new PortfolioPrice { DateOfPrice = listDatafeed[i].Date, Price = pricer.Price(MathDateConverter.ConvertToMathDistance(listDatafeed[i].Date, maturity), spot).Price });
                    deltas = book.getDeltas(pricer, testParams, listDatafeed[i], listDatafeed[i].Date);
                    portfolioRealValues.Add(new PortfolioPrice { DateOfPrice = listDatafeed[i].Date, Price = book.PfValueBeforeRebalancing(book.Composition, listDatafeed[i].PriceList, listDatafeed[i].Date) });
                    valueInfoReal.Value = book.PfValueBeforeRebalancing(book.Composition, listDatafeed[i].PriceList, listDatafeed[i].Date);
                    book.Rebalancing(deltas, listDatafeed[i].PriceList, listDatafeed[i].Date);
                    valueInfoTheorical.Date = request.Data.DataValues[i].Date;
                    valueInfoReal.Date = request.Data.DataValues[i].Date;
                    valueInfoTheorical.Value = pricer.Price(MathDateConverter.ConvertToMathDistance(listDatafeed[i].Date, maturity), spot).Price;

                    backtestOutput.PortfolioValues.Add(valueInfoReal);
                    backtestOutput.TheoValues.Add(valueInfoTheorical);



                }
            }



       
            return Task.FromResult(backtestOutput
           ) ;
        }
    }
}