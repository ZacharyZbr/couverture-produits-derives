using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PricingLibrary.RebalancingOracleDescriptions;


namespace AutofinancingSystematicPortfolio
{
    public interface IRebalancingOracle
    {
        public bool RebalancingTime(DateTime date);

    }
    public class WeeklyRebalancingOracle : IRebalancingOracle
    {
        public DayOfWeek DayOfWeek;
        public WeeklyRebalancingOracle(DayOfWeek dayOfWeek)
        {
            this.DayOfWeek = dayOfWeek;
        }
        public bool RebalancingTime(DateTime date)
        {
            if (date.DayOfWeek == DayOfWeek)
            {
                return true;
            }
            return false; 
        }
    }

    public class RegularRebalancingOracle : IRebalancingOracle
    {
        public int Period;
        public int Compteur = 0; 
        public RegularRebalancingOracle(int period)
        {
            Period = period;
        }
        public bool RebalancingTime(DateTime currentDate)
        {
            Compteur++;
            if (Compteur%Period == 0)
            {
                return true;
            }
            return false;
        }
    }

}
