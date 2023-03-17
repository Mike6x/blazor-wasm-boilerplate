using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.Geo;

public class WardAutocomplete : MudAutocomplete<Guid>
{
    [Inject]
    private IStringLocalizer<WardAutocomplete> L { get; set; } = default!;
    [Inject]
    private IWardsClient WardsClient { get; set; } = default!;
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private List<WardDto> _entityList = new();

    // supply default parameters, but leave the possibility to override them

    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = L["Ward"];
        Variant = Variant.Filled;
        Dense = true;
        Margin = Margin.None;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchWards;
        ToStringFunc = GetWardName;
        Clearable = true;
        return base.SetParametersAsync(parameters);
    }

    // when the value parameter is set, we have to load that one Ward to be able to show the name
    // we can't do that in OnInitialized because of a strange bug (https://github.com/MudBlazor/MudBlazor/issues/3818)
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender &&
            _value != default &&
            await ApiHelper.ExecuteCallGuardedAsync(
                () => WardsClient.GetAsync(_value), Snackbar) is { } entityDetailsDto)
        {
            var entity = new WardDto
            {
                Id = entityDetailsDto.Id,
                Code = entityDetailsDto.Code,
                Name = entityDetailsDto.Name
            };
            _entityList.Add(entity);
            ForceRender(true);
        }
    }

    private async Task<IEnumerable<Guid>> SearchWards(string value)
    {
        var filter = new SearchWardsRequest
        {
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "name" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => WardsClient.SearchAsync(filter), Snackbar)
            is PaginationResponseOfWardDto response)
        {
            _entityList = response.Data.ToList();
        }

        return _entityList.Select(x => x.Id);
    }

    private string GetWardName(Guid id) =>
        _entityList.Find(e => e.Id == id)?.Name ?? string.Empty;
}
