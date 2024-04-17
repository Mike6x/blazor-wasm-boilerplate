using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.People;

public class EntityCodeAutocomplete : MudAutocomplete<Guid>
{
    [Inject]
    private IStringLocalizer<EntityCodeAutocomplete> L { get; set; } = default!;
    [Inject]
    private IEntityCodesClient EntityCodesClient { get; set; } = default!;
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private List<EntityCodeDto> _entityList = new();

    // supply default parameters, but leave the possibility to override them
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = L["EntityCode"];
        Variant = Variant.Filled;
        Dense = true;
        Margin = Margin.None;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchEntityCodes;
        ToStringFunc = GetEntityCodeName;
        Clearable = true;
        return base.SetParametersAsync(parameters);
    }

    // when the value parameter is set, we have to load that one EntityCode to be able to show the name
    // we can't do that in OnInitialized because of a strange bug (https://github.com/MudBlazor/MudBlazor/issues/3818)
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender &&
            _value != default &&
            await ApiHelper.ExecuteCallGuardedAsync(
                () => EntityCodesClient.GetAsync(_value), Snackbar) is { } entityDetailsDto)
        {
            var entity = new EntityCodeDto
            {
                Id = entityDetailsDto.Id,
                Code = entityDetailsDto.Code,
                Name = entityDetailsDto.Name
            };
            _entityList.Add(entity);
            ForceRender(true);
        }
    }

    private async Task<IEnumerable<Guid>> SearchEntityCodes(string value)
    {
        var filter = new SearchEntityCodesRequest
        {
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "code", "name"}, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => EntityCodesClient.SearchAsync(filter), Snackbar)
            is PaginationResponseOfEntityCodeDto response)
        {
            _entityList = response.Data.ToList();
        }

        return _entityList.Select(x => x.Id);
    }

    private string GetEntityCodeName(Guid id) =>
        _entityList.Find(e => e.Id == id)?.Name ?? string.Empty;
}