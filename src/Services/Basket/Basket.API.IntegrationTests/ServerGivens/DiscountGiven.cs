using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Basket.API.IntegrationTests.ServerGivens;

public class DiscountGiven
{
    private readonly WireMockServer _discountWireMockServer;
    private static string ProtoFilePath => Directory.GetCurrentDirectory() + "/protos/discount.proto";
    private const string ProtoDefinitionId = "DiscountTestProto";

    public DiscountGiven(WireMockServer discountWireMockServer)
    {
        _discountWireMockServer = discountWireMockServer;
        
        var protoDefinitionText = File.ReadAllText(ProtoFilePath);
        

        
        _discountWireMockServer.AddProtoDefinition(ProtoDefinitionId, protoDefinitionText);
    }

    public DiscountGiven GetDiscountGiven(string productName)
    {
        _discountWireMockServer
            .Given(Request.Create()
                .UsingPost()
                .WithPath("/discount.DiscountProtoService/GetDiscount")
                .WithBodyAsProtoBuf("discount.GetDiscountRequest", new JsonMatcher(new { productName = productName }))
            )
            .WithProtoDefinition(ProtoDefinitionId)
            .RespondWith(Response.Create()
                .WithHeader("Content-Type", "application/grpc")
                .WithTrailingHeader("grpc-status", "0")
                .WithBodyAsProtoBuf("discount.CouponModel",
                    new
                    {
                        id = 1,
                        productName = "productName",
                        description = "test description",
                        amount = 10
                    })
                .WithTransformer()
            );
        
        return this;
    }
}