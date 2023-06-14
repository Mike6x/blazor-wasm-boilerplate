using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Infrastructure.Notifications;
using FSH.BlazorWebAssembly.Client.Shared;
using FSH.WebApi.Shared.Notifications;
using MediatR.Courier;
using Microsoft.AspNetCore.Components;

namespace FSH.BlazorWebAssembly.Client.Pages.Dashboard;

public partial class EmployeeStats
{
    [Parameter]
    public int BusinessUnitCount { get; set; }
    [Parameter]
    public int DepartmentCount { get; set; }
    [Parameter]
    public int SubDepartmentCount { get; set; }
    [Parameter]
    public int TeamCount { get; set; }
    [Parameter]
    public int TitleCount { get; set; }
    [Parameter]
    public int EmployeeCount { get; set; }

    [Inject]
    private IEmployeeStatsClient EmployeeStatsClient { get; set; } = default!;
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
                () => EmployeeStatsClient.GetAsync(),
                Snackbar)
            is EmployeeStatsDto statsDto)
        {
            EmployeeCount = statsDto.EmployeeCount;
            BusinessUnitCount = statsDto.BusinessUnitCount;
            DepartmentCount = statsDto.DepartmentCount;
            SubDepartmentCount = statsDto.SubDepartmentCount;
            TeamCount = statsDto.TeamCount;
            TitleCount = statsDto.TitleCount;

            foreach (var item in statsDto.DataEnterBarChart)
            {
                _dataEnterBarChartSeries
                    .RemoveAll(x => x.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));
                _dataEnterBarChartSeries.Add(new MudBlazor.ChartSeries { Name = item.Name, Data = item.Data?.ToArray() });
            }
        }
    }
}