using System.Threading.Tasks;
using Grpc.Net.Client;
using BacktestGrpc.Protos;
using PricingLibrary.MarketDataFeed;
using AutofinancingSystematicPortfolio;
using Google.Protobuf.WellKnownTypes;
using Google.Protobuf;
using Google.Protobuf.Collections;

// The port number must match the port of the gRPC server.
using var channel = GrpcChannel.ForAddress("https://localhost:7177");
var client = new BacktestRunner.BacktestRunnerClient(channel);
TestParams testParams = new TestParams();

ListMarketData MyShareValues = new ListMarketData();
MyShareValues.ReadCSVFile("C:\\Users\\localuser\\Documents\\projet-couverture\\Projet-couverture\\MarketData\\InputCSVFile\\data_share_5_3.csv");
List<ShareValue> shareValues = MyShareValues.GetListSharedValue();
ShareValue shareValue1 = shareValues[0];
ShareData shareData = new ShareData();
DataParams dataParams = new DataParams();

//Console.WriteLine(shareValues.Count);
for (int i = 0; i < shareValues.Count; i++)
{
    shareValue1 = shareValues[i];
    shareData.Value = shareValue1.Value;
    //Console.WriteLine(shareValue1.Value);
    shareData.Date = Timestamp.FromDateTime(DateTime.SpecifyKind(shareValue1.DateOfPrice, DateTimeKind.Utc));//DateTime.UtcNow
    shareData.Id = shareValue1.Id;
    dataParams.DataValues.Add(shareData);
    //Console.WriteLine(dataParams.DataValues[i].Value);
}
for (int i = 0; i < shareValues.Count; i++)
{
    Console.WriteLine(dataParams.DataValues[i].Value);
}
var reply = await client.RunBacktestAsync(
                  new BacktestRequest { 
                        TstParams = testParams, Data = dataParams });
Console.WriteLine(reply);
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
