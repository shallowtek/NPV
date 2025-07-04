using Bunit;
using Microsoft.JSInterop;
using NPV.UI.Pages;
using RichardSzalay.MockHttp;
using Microsoft.Extensions.DependencyInjection;
using NPV.Shared.Models;
using System.Net;
using System.Text.Json;

namespace NPV.UI.Tests;

public class HomeComponentTests : TestContext
{
    [Fact]
    public void RendersCashFlowInput()
    {
        var cut = RenderComponent<Home>();
        Assert.Contains("Cash Flows (comma-separated)", cut.Markup);
    }

    [Fact]
    public void ShowsCalculateButton()
    {
        var cut = RenderComponent<Home>();
        Assert.Contains("Calculate", cut.Find("button").TextContent);
    }

    [Fact]
    public void InputsRenderWithDefaultValues()
    {
        var cut = RenderComponent<Home>();
        var inputs = cut.FindAll("input");

        Assert.Equal("1.0", inputs[1].GetAttribute("value"));
        Assert.Equal("15.0", inputs[2].GetAttribute("value"));
        Assert.Equal("0.25", inputs[3].GetAttribute("value"));
    }

    [Fact]
    public void UpdatesBoundValues_WhenUserInputsData()
    {
        var cut = RenderComponent<Home>();
        cut.Find("input").Change("100,200");
        Assert.Equal("100,200", cut.Instance.CashFlows);
    }

    [Fact]
    public void CalculateButton_Disables_WhenIsLoading()
    {
        var cut = RenderComponent<Home>();
        cut.Instance.IsLoading = true;
        cut.Render();
        Assert.True(cut.Find("button").HasAttribute("disabled"));
    }

    [Fact]
    public void DoesNotRenderChart_WhenResultsAreEmpty()
    {
        var cut = RenderComponent<Home>();
        cut.Instance.Results.Clear();
        cut.Render();
        Assert.DoesNotContain("NPV Chart", cut.Markup);
    }

    [Fact]
    public async Task CalculateNPV_CallsApiAndRendersResults()
    {
        var mockHttp = new MockHttpMessageHandler();

        var responseObj = new NpvResponse
        {
            NpvResults = new List<NpvResult>
            {
                new() { DiscountRate = 5.0m, NPV = 1200m },
                new() { DiscountRate = 10.0m, NPV = 900m }
            }
        };

        string jsonString = JsonSerializer.Serialize(responseObj);

        mockHttp.When("/api/npv/calculate")
            .Respond(HttpStatusCode.OK, "application/json", jsonString);

        Services.AddSingleton(new HttpClient(mockHttp)
        {
            BaseAddress = new Uri("http://localhost")
        });

        var cut = RenderComponent<Home>();

        cut.Find("input").Change("1000, 2000, -500");
        cut.FindAll("input")[1].Change("1");
        cut.FindAll("input")[2].Change("10");
        cut.FindAll("input")[3].Change("1");

        await cut.InvokeAsync(() => cut.Find("button").Click());

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("NPV Chart", cut.Markup);
            Assert.Contains("1200", cut.Markup);
            Assert.Contains("900", cut.Markup);
        });
    }

    [Fact]
    public void SpinnerAppears_WhenIsLoadingTrue()
    {
        var cut = RenderComponent<Home>();
        cut.Instance.IsLoading = true;
        cut.Render();
        Assert.Contains("spinner-border", cut.Markup);
    }
}
