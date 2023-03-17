using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.Catalog;

public class GroupCategorieAutocomplete : MudAutocomplete<Guid>
{
    [Inject]
    private IStringLocalizer<GroupCategorieAutocomplete> L { get; set; } = default!;
    [Inject]
    private IGroupCategoriesClient GroupCategoriesClient { get; set; } = default!;
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private List<GroupCategorieDto> _entityList = new();

    // supply default parameters, but leave the possibility to override them
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = L["Group Categorie"];
        Variant = Variant.Filled;
        Dense = true;
        Margin = Margin.None;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchGroupCategories;
        ToStringFunc = GetGroupCategorieName;
        Clearable = true;
        return base.SetParametersAsync(parameters);
    }

    // when the value parameter is set, we have to load that one GroupCategorie to be able to show the name
    // we can't do that in OnInitialized because of a strange bug (https://github.com/MudBlazor/MudBlazor/issues/3818)
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender &&
            _value != default &&
            await ApiHelper.ExecuteCallGuardedAsync(
                () => GroupCategoriesClient.GetAsync(_value), Snackbar) is { } entityDetailsDto)
        {
            var entity = new GroupCategorieDto
            {
                Id = entityDetailsDto.Id,
                Code = entityDetailsDto.Code,
                Name = entityDetailsDto.Name
            };
            _entityList.Add(entity);
            ForceRender(true);
        }
    }

    private async Task<IEnumerable<Guid>> SearchGroupCategories(string value)
    {
        var filter = new SearchGroupCategoriesRequest
        {
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "name" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => GroupCategoriesClient.SearchAsync(filter), Snackbar)
            is PaginationResponseOfGroupCategorieDto response)
        {
            _entityList = response.Data.ToList();
        }

        return _entityList.Select(x => x.Id);
    }

    private string GetGroupCategorieName(Guid id) =>
        _entityList.Find(e => e.Id == id)?.Name ?? string.Empty;
}