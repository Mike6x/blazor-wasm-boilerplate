using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Infrastructure.Notifications;
using FSH.BlazorWebAssembly.Client.Shared;
using FSH.WebApi.Shared.Notifications;

using MediatR.Courier;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.Dashboard;

public partial class DistributionStats
{
    [Parameter]
    public int ChannelCount { get; set; }
    [Parameter]
    public int RetailerCount { get; set; }
    [Parameter]
    public int StoreCount { get; set; }
    [Parameter]
    public int PriceGroupCount { get; set; }

    [Inject]
    private IDistributionStatsClient DistributionStatsClient { get; set; } = default!;
    [Inject]
    private ICourier Courier { get; set; } = default!;

    private readonly string[] _dataEnterBarChartXAxisLabels = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
    private readonly List<MudBlazor.ChartSeries> _dataEnterBarChartSeries = new();
    private bool _loaded;

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
        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => DistributionStatsClient.GetAsync(),
                Snackbar)
            is DistributionStatsDto statsDto)
        {
            ChannelCount = statsDto.ChannelCount;
            RetailerCount = statsDto.RetailerCount;
            StoreCount = statsDto.StoreCount;

            PriceGroupCount = statsDto.PriceGroupCount;

            foreach (var item in statsDto.DataEnterBarChart)
            {
                _dataEnterBarChartSeries
                    .RemoveAll(x => x.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));
                _dataEnterBarChartSeries.Add(new MudBlazor.ChartSeries { Name = item.Name, Data = item.Data?.ToArray() });
            }
        }
    }
}