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
    /// <summary>
    /// Classe représentant un portefeuille autofinancé
    /// </summary>
    public class AutofinancingPortfolio
    {
        /// <summary>
        /// Quantité de d'argent positionné au taux sans risque 
        /// </summary>
        public double RiskFreeQuantity { get; private set; }
        /// <summary>
        /// Composition du Portefeuille : Quantité d'argent placé pour chaque actions
        /// </summary>
        public Dictionary<String, double> Composition;
        /// <summary>
        /// Date du jour du dernier rebalancement
        /// </summary>
        public DateTime LastRebalancingDate;

        /// <summary>
        /// Constructeur du portefeuille autofinancé 
        /// </summary>
        /// <param name="Value">Valeur du portefeuille à sa création</param>
        /// <param name="UnderAssetPrice">Dictionnaire donnant le prix de chaque action à la création du portefeuille autofinancé</param>
        /// <param name="composition">Dictionnaire donnant le delta de chaque action à la création du portefeuille autofinancé</param>
        /// <param name="startDate">Jour de la création du portefeuille autofinancé</param>
        public AutofinancingPortfolio(double Value, Dictionary<String, double> UnderAssetPrice, Dictionary<String, double> composition, DateTime startDate)
        {
            Composition = composition;
            double produitSaclaire = 0;
            for (int i = 1; i <= composition.Count; i++)
                produitSaclaire += composition["share_" + i] * UnderAssetPrice["share_" + i];
            RiskFreeQuantity = Value - produitSaclaire;
            LastRebalancingDate = startDate;
        }

        /// <summary>
        /// Methode de rebalancement du portefeuille
        /// </summary>
        /// <param name="newComposition">Dictionnaire donnant le nouveau delta pour chaque action</param>
        /// <param name="UnderAssetPrice">Dictionnaire donnant le prix actuel pour chaque action</param>
        /// <param name="rebalancingDate">Date du rebalancement</param>
        public void Rebalancing(Dictionary<String, double> newComposition, Dictionary<String, double> UnderAssetPrice, DateTime rebalancingDate)
        {
            double produitSaclaire = 0;
            for (int i = 1; i <= Composition.Count; i++)
                produitSaclaire += Composition["share_" + i] * UnderAssetPrice["share_" + i] - newComposition["share_" + i] * UnderAssetPrice["share_" + i];


            RiskFreeQuantity = produitSaclaire + RiskFreeQuantity * RiskFreeRateProvider.GetRiskFreeRateAccruedValue(LastRebalancingDate, rebalancingDate);
            Composition = newComposition;
            LastRebalancingDate = rebalancingDate;

        }
        /// <summary>
        /// Calcule la valeur du portefeuille juste avant le rebalancement
        /// </summary>
        /// <param name="Composition">Dictionnaire donnant le delta pour chaque action</param>
        /// <param name="UnderAssetPrice">Dictionnaire donnant le prix actuel pour chaque action</param>
        /// <param name="currentDate">Date du calcul</param>
        /// <returns>Retourne la valeur du portefeuille</returns>
        public double PfValueBeforeRebalancing(Dictionary<String, double> Composition, Dictionary<String, double> UnderAssetPrice, DateTime currentDate)

        {

            double produitSaclaire = 0;
            double Value = 0;
            for (int i = 1; i <= Composition.Count; i++)
                produitSaclaire += Composition["share_" + i] * UnderAssetPrice["share_" + i];

            Value = RiskFreeQuantity * RiskFreeRateProvider.GetRiskFreeRateAccruedValue(LastRebalancingDate, currentDate) + produitSaclaire;
            return Value;
        }

        /// <summary>
        /// Retourne un dictionnaire donnant pour chaque action le nouveau delta
        /// </summary>
        /// <param name="pricer">Pricer pour calculer les deltas</param>
        /// <param name="maturity">Maturité de l'option</param>
        /// <param name="datafeed">Données des prix des actions pour la date de calcul</param>
        /// <returns></returns>
        public Dictionary<String, double> getDeltas(Pricer pricer, DateTime maturity, DataFeed datafeed)
        {
            double timeToMaturity = MathDateConverter.ConvertToMathDistance(datafeed.Date, maturity);
            double[] spot = new double[datafeed.PriceList.Count];
            for (int i = 1; i <= datafeed.PriceList.Count; i++)
                spot[i - 1] = datafeed.PriceList["share_" + i];
            Dictionary<String, double> deltas = new Dictionary<string, double>();
            double[] tabDelta = pricer.Price(timeToMaturity, spot).Deltas;
            for (int i = 1; i <= tabDelta.Length; i++)
                deltas["share_" + i] = tabDelta[i - 1];
            return deltas;
        }

    }
    /// <summary>
    /// Class permettant d'avoir la valeur du portefeuille en une date
    /// </summary>
    public class PortfolioPrice
    {
        /// <summary>
        /// Valeur du portefeuille
        /// </summary>
        public double Price { get; set; }
        /// <summary>
        /// Date correspondante
        /// </summary>
        public DateTime DateOfPrice { get; set; }

    }
}
