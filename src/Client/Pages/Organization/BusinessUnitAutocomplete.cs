using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.Organization;

public class BusinessUnitAutocomplete : MudAutocomplete<Guid>
{
    [Inject]
    private IStringLocalizer<BusinessUnitAutocomplete> L { get; set; } = default!;
    [Inject]
    private IBusinessUnitsClient BusinessUnitsClient { get; set; } = default!;
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private List<BusinessUnitDto> _entityList = new();

    // supply default parameters, but leave the possibility to override them
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = L["BusinessUnit"];
        Variant = Variant.Filled;
        Dense = true;
        Margin = Margin.None;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchBusinessUnits;
        ToStringFunc = GetBusinessUnitName;
        Clearable = true;
        return base.SetParametersAsync(parameters);
    }

    // when the value parameter is set, we have to load that one BusinessUnit to be able to show the name
    // we can't do that in OnInitialized because of a strange bug (https://github.com/MudBlazor/MudBlazor/issues/3818)
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender &&
            _value != default &&
            await ApiHelper.ExecuteCallGuardedAsync(
                () => BusinessUnitsClient.GetAsync(_value), Snackbar) is { } entityDetailsDto)
        {
            var entity = new BusinessUnitDto
            {
                Id = entityDetailsDto.Id,
                Code = entityDetailsDto.Code,
                Name = entityDetailsDto.Name
            };
            _entityList.Add(entity);
            ForceRender(true);
        }
    }

    private async Task<IEnumerable<Guid>> SearchBusinessUnits(string value)
    {
        var filter = new SearchBusinessUnitsRequest
        {
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "name" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => BusinessUnitsClient.SearchAsync(filter), Snackbar)
            is PaginationResponseOfBusinessUnitDto response)
        {
            _entityList = response.Data.ToList();
        }

        return _entityList.Select(x => x.Id);
    }

    private string GetBusinessUnitName(Guid id) =>
        _entityList.Find(e => e.Id == id)?.Name ?? string.Empty;
}