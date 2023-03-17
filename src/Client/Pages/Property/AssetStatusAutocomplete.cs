using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.Property;

public class AssetStatusAutocomplete : MudAutocomplete<Guid>
{
    [Inject]
    private IStringLocalizer<AssetStatusAutocomplete> L { get; set; } = default!;
    [Inject]
    private IAssetStatusesClient AssetStatusesClient { get; set; } = default!;
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private List<AssetStatusDto> _entityList = new();

    // supply default parameters, but leave the possibility to override them
    [Parameter]
    public AssetStatusType StatusType { get; set; }

    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = L["AssetStatus"];
        Variant = Variant.Filled;
        Dense = true;
        Margin = Margin.None;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchAssetStatuses;
        ToStringFunc = GetAssetStatusName;
        Clearable = true;
        return base.SetParametersAsync(parameters);
    }

    // when the value parameter is set, we have to load that one AssetStatus to be able to show the name
    // we can't do that in OnInitialized because of a strange bug (https://github.com/MudBlazor/MudBlazor/issues/3818)
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender &&
            _value != default &&
            await ApiHelper.ExecuteCallGuardedAsync(
                () => AssetStatusesClient.GetAsync(_value), Snackbar) is { } entityDetailsDto)
        {
            var entity = new AssetStatusDto
            {
                Id = entityDetailsDto.Id,
                Code = entityDetailsDto.Code,
                Name = entityDetailsDto.Name
            };
            _entityList.Add(entity);
            ForceRender(true);
        }
    }

    private async Task<IEnumerable<Guid>> SearchAssetStatuses(string value)
    {
        var filter = new SearchAssetStatusesRequest
        {
            Type = StatusType,
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "name" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => AssetStatusesClient.SearchAsync(filter), Snackbar)
            is PaginationResponseOfAssetStatusDto response)
        {
            _entityList = response.Data.ToList();
        }

        return _entityList.Select(x => x.Id);
    }

    private string GetAssetStatusName(Guid id) =>
        _entityList.Find(e => e.Id == id)?.Name ?? string.Empty;
}