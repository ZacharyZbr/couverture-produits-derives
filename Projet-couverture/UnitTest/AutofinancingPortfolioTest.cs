using NUnit.Framework;
using AutofinancingSystematicPortfolio;
using MathNet.Numerics.Financial;
using PricingLibrary.MarketDataFeed;

namespace UnitTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestPfInitialValue()
        {
            /* Vérifie si tout marche bien pour la valeur initiale */

            // ARRANGE
            Dictionary<string, double> composition = new Dictionary<string, double>();
            Dictionary<string, double> underAssetPrice = new Dictionary<string, double>();
            DateTime dateCreationPfTest = DateTime.Today;
            double val = 12;
            AutofinancingPortfolio pfTest = new AutofinancingPortfolio(val, underAssetPrice, composition, dateCreationPfTest);

            // ACT
            double value = pfTest.PfValueBeforeRebalancing(composition, underAssetPrice, dateCreationPfTest);

            // ASSERT
            Assert.IsTrue(Double.Equals(val, value));
        }

        [Test]
        public void TestPfValue()
        {
            /* Test qui vérifie si la valeur est bonne avec les nouveaux prix et avant rebalancement */

            // ARRANGE
            Dictionary<string, double> compositionPfStepN = new Dictionary<string, double>
            {
                {"share_1", 0.3},
                {"share_2", 0.5},
                {"share_3", 1.1},
            };

            Dictionary<string, double> underAssetPriceStepN = new Dictionary<string, double>
            {
                {"share_1", 19},
                {"share_2", 6.45},
                {"share_3", 9},
            };

            Dictionary<string, double> underAssetPriceStepNplus1 = new Dictionary<string, double>
            {
                {"share_1", 21},
                {"share_2", 6.3},
                {"share_3", 7.3},
            };

            DateTime dateCreationPfTest = DateTime.Today;
            DateTime datePfNplus1 = dateCreationPfTest.AddDays(1);
            double val = 30;
            AutofinancingPortfolio pfTest = new AutofinancingPortfolio(val, underAssetPriceStepN, compositionPfStepN, dateCreationPfTest); // invests 11,175 in risk free

            // ACT
            double value = pfTest.PfValueBeforeRebalancing(compositionPfStepN, underAssetPriceStepNplus1, datePfNplus1);

            // ASSERT 
            double expected = 17.48 + 11.175 * RiskFreeRateProvider.GetRiskFreeRateAccruedValue(dateCreationPfTest, datePfNplus1);
            Assert.IsTrue(Math.Abs(expected - value)<0.001); 
        }

        [Test]
        public void TestPfRebalancing()
        {
            /* Test qui vérifie si le rebalancement se fait bien de manière autofinancée */

            // ARRANGE
            Dictionary<string, double> compositionPfStepN = new Dictionary<string, double>
            {
                {"share_1", 0.3},
                {"share_2", 0.5},
                {"share_3", 1.1},
            };

            Dictionary<string, double> compositionPfStepNplus1 = new Dictionary<string, double>
            {
                {"share_1", 0.15},
                {"share_2", 0.68},
                {"share_3", 1.69},
            };

            Dictionary<string, double> underAssetPriceStepNplus1 = new Dictionary<string, double>
            {
                {"share_1", 21},
                {"share_2", 6.3},
                {"share_3", 7.3},
            };

            DateTime dateCreationPfTest = DateTime.Today;
            DateTime datePfNplus1 = dateCreationPfTest.AddDays(1);
            double val = 30;
            AutofinancingPortfolio pfTest = new AutofinancingPortfolio(val, underAssetPriceStepNplus1, compositionPfStepN, dateCreationPfTest); // invests 12,52 in risk free

            // ACT
            double valueBeforeRebalancing = pfTest.PfValueBeforeRebalancing(compositionPfStepN, underAssetPriceStepNplus1, datePfNplus1);
            pfTest.Rebalancing(compositionPfStepNplus1, underAssetPriceStepNplus1, datePfNplus1);
            double valueAfterRebalancing = pfTest.PfValueBeforeRebalancing(compositionPfStepNplus1, underAssetPriceStepNplus1, datePfNplus1);

            // ASSERT 
            double expected = 17.48 + 11.175 * RiskFreeRateProvider.GetRiskFreeRateAccruedValue(dateCreationPfTest, datePfNplus1);
            Assert.IsTrue(Math.Abs(valueBeforeRebalancing - valueAfterRebalancing) < 0.001);
        }

    }
}