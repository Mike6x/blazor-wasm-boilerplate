using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Infrastructure.Notifications;
using FSH.BlazorWebAssembly.Client.Shared;
using FSH.WebApi.Shared.Notifications;
using MediatR.Courier;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace FSH.BlazorWebAssembly.Client.Pages.Dashboard;

public partial class VnPowerStats
{
    [Parameter]
    public int VnPowerCount { get; set; }

    // [Inject]
    // private IVnPowerStatsClient VnPowerStatsClient { get; set; } = default!;
    [Inject]
    private ICourier Courier { get; set; } = default!;

    private readonly string[] _dataEnterBarChartXAxisLabels = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
    private readonly List<MudBlazor.ChartSeries> _dataEnterBarChartSeries = new();
    private static readonly string[] Value = [
        "01|",
        "02|",
        "03|",
        "04|",
        "05|",
        "06|",
        "07|",
        "08|",
        "09|",
        "10|",
        "11|",
        "12|",
        "13|",
        "14|",
        "15|",
        "16|",
        "17|",
        "18|",
        "19|",
        "20|",
        "21|",
        "22|",
        "23|",
        "24|",
        "25|",
        "26|",
        "27|",
        "28|",
        "29|",
        "30|",
        "31|",
        "32|",
        "33|",
        "34|",
        "35|",
        "36|",
        "37|",
        "38|",
        "39|",
        "40|",
        "41|",
        "42|",
        "43|",
        "44|",
        "45|",
        "46|",
        "47|",
        "48|",
        "49|",
        "50|",
        "51|",
        "52|",
        "53|",
        "54|",
        "55|",
    ];
    private readonly string[] _barChartXAxisLabels = Value;
    private readonly List<MudBlazor.ChartSeries> _series =
    [
        new MudBlazor.ChartSeries()
        {
            Name = "Power 655", Data =[
            40, 20, 25, 27, 46,
                60, 48, 80, 15, 20,
            25, 27, 46, 60, 48, 80, 15, 20, 25, 27,
            40, 20, 25, 27, 46, 60, 48, 80, 15, 20,
            25, 27, 46, 60, 48, 80, 15, 20, 25, 27,
            46, 60, 48, 80, 15, 80, 15, 20, 25, 27,
            60, 48, 80, 15, 30]
        },
    ];
    private bool _loaded;
    private int _index = -1; // default value cannot be 0 -> first selectedindex is 0.
    protected override async Task OnInitializedAsync()
    {
        Courier.SubscribeWeak<NotificationWrapper<StatsChangedNotification>>(async _ =>
        {
            await LoadDataAsync();
            StateHasChanged();
        });

        await LoadDataAsync();

        _loaded = true;
    }

    private async Task LoadDataAsync()
    {
        //if (await ApiHelper.ExecuteCallGuardedAsync(
        //        () => VnPowerStatsClient.GetAsync(),
        //        Snackbar)
        //    is VnPowerStatsDto statsDto)
        //{
        //    VnPowerCount = statsDto.VnPowerCount;

        //    foreach (var item in statsDto.DataEnterBarChart)
        //    {
        //        _dataEnterBarChartSeries
        //            .RemoveAll(x => x.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));
        //        _dataEnterBarChartSeries.Add(new MudBlazor.ChartSeries { Name = item.Name, Data = item.Data?.ToArray() });
        //    }
        //}
    }
}