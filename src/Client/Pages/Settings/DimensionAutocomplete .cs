using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.Settings;

public class DimensionAutocomplete : MudAutocomplete<Guid>
{
    [Inject]
    private IStringLocalizer<DimensionAutocomplete> L { get; set; } = default!;
    [Inject]
    private IDimensionsClient DimensionsClient { get; set; } = default!;
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private List<DimensionDto> _entityList = new();

    // supply default parameters, but leave the possibility to override them
    [Parameter]
    public string? DimensionType { get; set; }
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = L["Father Dimension"];
        Variant = Variant.Filled;
        Dense = true;
        Margin = Margin.None;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchDimensions;
        ToStringFunc = GetDimensionName;
        Clearable = true;
        return base.SetParametersAsync(parameters);
    }

    // when the value parameter is set, we have to load that one Dimension to be able to show the name
    // we can't do that in OnInitialized because of a strange bug (https://github.com/MudBlazor/MudBlazor/issues/3818)
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender &&
            _value != default &&
            await ApiHelper.ExecuteCallGuardedAsync(
                () => DimensionsClient.GetAsync(_value), Snackbar) is { } entityDetailsDto)
        {
            var entity = new DimensionDto
            {
                Id = entityDetailsDto.Id,
                Code = entityDetailsDto.Code,
                Name = entityDetailsDto.Name
            };
            _entityList.Add(entity);
            ForceRender(true);
        }
    }

    private async Task<IEnumerable<Guid>> SearchDimensions(string value)
    {
        var filter = new SearchDimensionsRequest
        {
            Type = DimensionType,
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "Code", "Name" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => DimensionsClient.SearchAsync(filter), Snackbar)
            is PaginationResponseOfDimensionDto response)
        {
            _entityList = response.Data.ToList();
        }

        return _entityList.Select(x => x.Id);
    }

    private string GetDimensionName(Guid id)
    {
        var entity = _entityList.Find(e => e.Id == id);
        return entity != null ? entity.Name : string.Empty;
    }
}