using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Infrastructure.Notifications;
using FSH.BlazorWebAssembly.Client.Shared;
using FSH.WebApi.Shared.Notifications;
using MediatR.Courier;
using Microsoft.AspNetCore.Components;

namespace FSH.BlazorWebAssembly.Client.Pages.Dashboard;

public partial class ProductStats
{
    [Parameter]
    public int BrandCount { get; set; }
    [Parameter]
    public int BusinessLineCount { get; set; }
    [Parameter]
    public int GroupCategorieCount { get; set; }
    [Parameter]
    public int CategorieCount { get; set; }
    [Parameter]
    public int SubCategorieCount { get; set; }
    [Parameter]
    public int ProductCount { get; set; }

    [Inject]
    private IProductStatsClient ProductStatsClient { get; set; } = default!;
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
                () => ProductStatsClient.GetAsync(),
                Snackbar)
            is ProductStatsDto statsDto)
        {
            ProductCount = statsDto.ProductCount;
            BrandCount = statsDto.BrandCount;
            BusinessLineCount = statsDto.BusinessLineCount;
            GroupCategorieCount = statsDto.GroupCategorieCount;
            CategorieCount = statsDto.CategorieCount;
            SubCategorieCount = statsDto.SubCategorieCount;

            foreach (var item in statsDto.DataEnterBarChart)
            {
                _dataEnterBarChartSeries
                    .RemoveAll(x => x.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));
                _dataEnterBarChartSeries.Add(new MudBlazor.ChartSeries { Name = item.Name, Data = item.Data?.ToArray() });
            }
        }
    }
}