using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.Catalog;

public class AssetSubCategorieAutocomplete : MudAutocomplete<Guid>
{
    [Inject]
    private IStringLocalizer<AssetSubCategorieAutocomplete> L { get; set; } = default!;
    [Inject]
    private ISubCategoriesClient SubCategoriesClient { get; set; } = default!;
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private List<SubCategorieDto> _entityList = new();

    // supply default parameters, but leave the possibility to override them
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = L["SubCategorie"];
        Variant = Variant.Filled;
        Dense = true;
        Margin = Margin.None;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchSubCategories;
        ToStringFunc = GetSubCategorieName;
        Clearable = true;
        return base.SetParametersAsync(parameters);
    }

    // when the value parameter is set, we have to load that one SubCategorie to be able to show the name
    // we can't do that in OnInitialized because of a strange bug (https://github.com/MudBlazor/MudBlazor/issues/3818)
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender &&
            _value != default &&
            await ApiHelper.ExecuteCallGuardedAsync(
                () => SubCategoriesClient.GetAsync(_value), Snackbar) is { } entityDetailsDto)
        {
            var entity = new SubCategorieDto
            {
                Id = entityDetailsDto.Id,
                Code = entityDetailsDto.Code,
                Name = entityDetailsDto.Name
            };
            _entityList.Add(entity);
            ForceRender(true);
        }
    }

    private async Task<IEnumerable<Guid>> SearchSubCategories(string value)
    {
        var filter = new SearchSubCategoriesRequest
        {
            Type = CatalogType.Asset,
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "name" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => SubCategoriesClient.SearchAsync(filter), Snackbar)
            is PaginationResponseOfSubCategorieDto response)
        {
            _entityList = response.Data.ToList();
        }

        return _entityList.Select(x => x.Id);
    }

    private string GetSubCategorieName(Guid id) =>
        _entityList.Find(e => e.Id == id)?.Name ?? string.Empty;
}