using PricingLibrary.Computations;
using PricingLibrary.DataClasses;
using PricingLibrary.MarketDataFeed;
using PricingLibrary.TimeHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutofinancingSystematicPortfolio
{
    public class AutofinancingPortfolio
    {
        public double RiskFreeQuantity { get; private set; }
        public Dictionary<String, double> Composition;
        public DateTime LastRebalancingDate;
        ///public double Value;


        public AutofinancingPortfolio(double Value, Dictionary<String, double> UnderAssetPrice, Dictionary<String, double> composition, DateTime startDate)
        {
            Composition = composition;
            double produitSaclaire = 0;
            for (int i = 1; i <= composition.Count; i++)
                produitSaclaire += composition["share_" + i] * UnderAssetPrice["share_" + i];
            RiskFreeQuantity = Value - produitSaclaire;
            LastRebalancingDate = startDate;
        }

        public void Rebalancing(Dictionary<String, double> newComposition, Dictionary<String, double> UnderAssetPrice, DateTime rebalancingDate)
        {
            double produitSaclaire = 0;
            for (int i = 1; i <= Composition.Count; i++)
                produitSaclaire += Composition["share_" + i] * UnderAssetPrice["share_" + i] - newComposition["share_" + i] * UnderAssetPrice["share_" + i];


            RiskFreeQuantity = produitSaclaire + RiskFreeQuantity * RiskFreeRateProvider.GetRiskFreeRateAccruedValue(LastRebalancingDate, rebalancingDate);
            Composition = newComposition;
            LastRebalancingDate = rebalancingDate;

        }

        public double PfValueBeforeRebalancing(Dictionary<String, double> Composition, Dictionary<String, double> UnderAssetPrice, DateTime currentDate)

        {

            double produitSaclaire = 0;
            double Value = 0;
            for (int i = 1; i <= Composition.Count; i++)
                produitSaclaire += Composition["share_" + i] * UnderAssetPrice["share_" + i];

            Value = RiskFreeQuantity * RiskFreeRateProvider.GetRiskFreeRateAccruedValue(LastRebalancingDate, currentDate) + produitSaclaire;
            return Value;
        }


        public Dictionary<String, double> getDeltas(Pricer pricer, TestParameters testParams, DataFeed datafeed, DateTime currentDate)
        {
            DateTime maturity = testParams.BasketOption.Maturity;
            double timeToMaturity = MathDateConverter.ConvertToMathDistance(currentDate, maturity);
            double[] spot = new double[datafeed.PriceList.Count];
            for (int i = 1; i <= datafeed.PriceList.Count; i++)
                spot[i - 1] = datafeed.PriceList["share_" + i];
            Dictionary<String, double> deltas = new Dictionary<string, double>();
            double[] tabDelta = pricer.Price(timeToMaturity, spot).Deltas;
            for (int i = 1; i <= tabDelta.Length; i++)
                deltas["share_" + i] = tabDelta[i - 1];
            return deltas;
        }


        /*    public double getRiskFreeQuantity(Dictionary<String, double> newDeltas, DataFeed fromDataFeed)
            {
                Dictionary<string, double> newPrices = fromDataFeed.PriceList;
                double produitSaclaire = 0;
                for (int i = 1; i <= newDeltas.Count; i++)
                    produitSaclaire +=  newDeltas["share_"+i] * newPrices["share_" + i];
                double RiskFreeQuantity = Value - produitSaclaire;

                return RiskFreeQuantity;
            }*/

    }
}
