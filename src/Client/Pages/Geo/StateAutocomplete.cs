using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.Geo;

public class StateAutocomplete : MudAutocomplete<Guid>
{
    [Inject]
    private IStringLocalizer<StateAutocomplete> L { get; set; } = default!;
    [Inject]
    private IStatesClient StatesClient { get; set; } = default!;
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private List<StateDto> _entityList = new();

    // supply default parameters, but leave the possibility to override them
    [Parameter]
    public Guid FatherId { get; set; }
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = L["State"];
        Variant = Variant.Filled;
        Dense = true;
        Margin = Margin.None;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchStates;
        ToStringFunc = GetStateName;
        Clearable = true;
        return base.SetParametersAsync(parameters);
    }

    // when the value parameter is set, we have to load that one State to be able to show the name
    // we can't do that in OnInitialized because of a strange bug (https://github.com/MudBlazor/MudBlazor/issues/3818)
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender &&
            _value != default &&
            await ApiHelper.ExecuteCallGuardedAsync(
                () => StatesClient.GetAsync(_value), Snackbar) is { } entityDetailsDto)
        {
            var entity = new StateDto
            {
                Id = entityDetailsDto.Id,
                Code = entityDetailsDto.Code,
                Name = entityDetailsDto.Name
            };
            _entityList.Add(entity);
            ForceRender(true);
        }
    }

    private async Task<IEnumerable<Guid>> SearchStates(string value)
    {
        var filter = new SearchStatesRequest
        {
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "name" }, Keyword = value },
            CountryId = FatherId == Guid.Empty ? null : FatherId,
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => StatesClient.SearchAsync(filter), Snackbar)
            is PaginationResponseOfStateDto response)
        {
            _entityList = response.Data.ToList();
        }

        return _entityList.Select(x => x.Id);
    }

    private string GetStateName(Guid id) =>
        _entityList.Find(e => e.Id == id)?.Name ?? string.Empty;
}
