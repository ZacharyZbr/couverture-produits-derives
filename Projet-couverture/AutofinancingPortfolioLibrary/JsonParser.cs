
using System;
using System.Text.Json;
using PricingLibrary.DataClasses;
using PricingLibrary.RebalancingOracleDescriptions;

namespace AutofinancingSystematicPortfolio
{

    public class JsonParser
    {
        string filepath { set; get; }

        public JsonParser(string jsonFilepath)
        {
            filepath = jsonFilepath;
        }

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