
using System;
using System.Text.Json;
using PricingLibrary.DataClasses;
using PricingLibrary.RebalancingOracleDescriptions;

namespace AutofinancingSystematicPortfolio
{
    /// <summary>
    /// Classe permettant de récupérer les paramètres des sous-jacents de l'option
    /// </summary>
    public class JsonParser
    {
        string filepath { set; get; }

        /// <summary>
        /// Constructeur du parseur
        /// </summary>
        /// <param name="jsonFilepath">Chemin du fichier Json contenant les paramètres</param>
        public JsonParser(string jsonFilepath)
        {
            filepath = jsonFilepath;
        }

        /// <summary>
        /// Méthode parsant le fichier Json
        /// </summary>
        /// <returns></returns>
        public TestParameters GiveParameter()
        {
            StreamReader r = new StreamReader(filepath);
            string jsonString = r.ReadToEnd();
            RebalancingOracleDescriptionConverter descriptionConverter = new RebalancingOracleDescriptionConverter();
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(descriptionConverter);
            TestParameters testParameters = JsonSerializer.Deserialize<TestParameters>(jsonString, options);
            return testParameters;
        }
    }
}