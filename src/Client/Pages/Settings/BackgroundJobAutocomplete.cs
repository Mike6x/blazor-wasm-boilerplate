using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.Settings;

public class BackgroundJobAutocomplete : MudAutocomplete<Guid>
{
    [Inject]
    private IStringLocalizer<BackgroundJobAutocomplete> L { get; set; } = default!;
    [Inject]
    private IBackgroundJobsClient BackgroundJobsClient { get; set; } = default!;
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private List<BackgroundJobDto> _entityList = new();

    // supply default parameters, but leave the possibility to override them
    [Parameter]
    public BackgroundJobType JobType { get; set; }

    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = L["BackgroundJob"];
        Variant = Variant.Filled;
        Dense = true;
        Margin = Margin.None;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchBackgroundJobs;
        ToStringFunc = GetBackgroundJobName;
        Clearable = true;
        return base.SetParametersAsync(parameters);
    }

    // when the value parameter is set, we have to load that one BackgroundJob to be able to show the name
    // we can't do that in OnInitialized because of a strange bug (https://github.com/MudBlazor/MudBlazor/issues/3818)
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender &&
            _value != default &&
            await ApiHelper.ExecuteCallGuardedAsync(
                () => BackgroundJobsClient.GetAsync(_value), Snackbar) is { } entityDetailsDto)
        {
            var entity = new BackgroundJobDto
            {
                Id = entityDetailsDto.Id,
                Code = entityDetailsDto.Code,
                Name = entityDetailsDto.Name
            };
            _entityList.Add(entity);
            ForceRender(true);
        }
    }

    private async Task<IEnumerable<Guid>> SearchBackgroundJobs(string value)
    {
        var filter = new SearchBackgroundJobsRequest
        {
            // Type = JobType == BackgroundJobType.All ? null : JobType,
            Type = JobType,
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "name" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => BackgroundJobsClient.SearchAsync(filter), Snackbar)
            is PaginationResponseOfBackgroundJobDto response)
        {
            _entityList = response.Data.ToList();
        }

        return _entityList.Select(x => x.Id);
    }

    private string GetBackgroundJobName(Guid id) =>
        _entityList.Find(e => e.Id == id)?.Name ?? string.Empty;
}