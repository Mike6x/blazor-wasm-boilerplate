using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.Place;

public class StoreAutocomplete : MudAutocomplete<Guid>
{
    [Inject]
    private IStringLocalizer<StoreAutocomplete> L { get; set; } = default!;
    [Inject]
    private IStoresClient StoresClient { get; set; } = default!;
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private List<StoreDto> _entityList = new();

    // supply default parameters, but leave the possibility to override them
    [Parameter]
    public Guid FatherId { get; set; }
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = L["Store"];
        Variant = Variant.Filled;
        Dense = true;
        Margin = Margin.None;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchStores;
        ToStringFunc = GetStoreName;
        Clearable = true;
        return base.SetParametersAsync(parameters);
    }

    // when the value parameter is set, we have to load that one Store to be able to show the name
    // we can't do that in OnInitialized because of a strange bug (https://github.com/MudBlazor/MudBlazor/issues/3818)
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender &&
            _value != default &&
            await ApiHelper.ExecuteCallGuardedAsync(
                () => StoresClient.GetAsync(_value), Snackbar) is { } entityDetailsDto)
        {
            var entity = new StoreDto
            {
                Id = entityDetailsDto.Id,
                Code = entityDetailsDto.Code,
                Name = entityDetailsDto.Name
            };
            _entityList.Add(entity);
            ForceRender(true);
        }
    }

    private async Task<IEnumerable<Guid>> SearchStores(string value)
    {
        var filter = new SearchStoresRequest
        {
            RetailerId = FatherId == Guid.Empty ? null : FatherId,
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "name" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => StoresClient.SearchAsync(filter), Snackbar)
            is PaginationResponseOfStoreDto response)
        {
            _entityList = response.Data.ToList();
        }

        return _entityList.Select(x => x.Id);
    }

    private string GetStoreName(Guid id) =>
        _entityList.Find(e => e.Id == id)?.Name ?? string.Empty;
}