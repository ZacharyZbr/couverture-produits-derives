namespace AutofinancingSystematicPortfolio
{
    /// <summary>
    /// Interface oracle
    /// </summary>
    public interface IRebalancingOracle
    {
        /// <summary>
        /// Accepte ou non le rebalancement
        /// Méthode à implémenter. 
        /// </summary>
        /// <param name="date">Date du jour du potentiel rebalancement</param>
        /// <returns>True si il faut rebalancer, false sinon</returns>
        public bool RebalancingTime(DateTime date);

    }
    /// <summary>
    /// Classe implémentant l'interface IRebalancing pour un rebalancement hebdomadaire
    /// </summary>
    public class WeeklyRebalancingOracle : IRebalancingOracle
    {
        /// <summary>
        /// Jour du rebalancement
        /// </summary>
        public DayOfWeek DayOfWeek;

        /// <summary>
        /// Constructeur de l'oracle de rebalancement hebdomadaire
        /// </summary>
        /// <param name="dayOfWeek">Jour du rebalancement</param>
        public WeeklyRebalancingOracle(DayOfWeek dayOfWeek)
        {
            this.DayOfWeek = dayOfWeek;
        }

        /// <summary>
        /// Accepte le rebalancement si la date en paramètre correspond au DayOfWeek
        /// </summary>
        /// <param name="date">Date du potentiel rebalancement</param>
        /// <returns>True si il faut rebalancer, false sinon</returns>
        public bool RebalancingTime(DateTime date)
        {
            if (date.DayOfWeek == DayOfWeek)
            {
                return true;
            }
            return false; 
        }
    }

    /// <summary>
    /// Classe implémentant l'interface IRebalancing pour un rebalancement regulier
    /// </summary>
    public class RegularRebalancingOracle : IRebalancingOracle
    {
        /// <summary>
        /// Periode entre chaque rebalancement
        /// </summary>
        public int Period;

        /// <summary>
        /// Nombre de jour ouvrés depuis le dernier rebalancement
        /// </summary>
        public int Compteur = 0;

        /// <summary>
        /// Constructeur de l'oracle de rebalancement regulier
        /// </summary>
        /// <param name="period">Periode entre chaque rebalancement</param>
        public RegularRebalancingOracle(int period)
        {
            Period = period;
        }

        /// <summary>
        /// Accepte le rebalancement si le nombre de jour ouvrés depuis le dernier rebalancement en paramètre correspond au DayOfWeek
        /// </summary>
        /// <param name="currentDate">Date du potentiel rebalancement</param>
        /// <returns></returns>
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
