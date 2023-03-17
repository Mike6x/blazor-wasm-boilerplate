using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Reflection.Emit;

namespace FSH.BlazorWebAssembly.Client.Pages.People;

public class SuperiorAutocomplete : MudAutocomplete<Guid>
{
    [Inject]
    private IStringLocalizer<SuperiorAutocomplete> L { get; set; } = default!;
    [Inject]
    private IEmployeesClient EmployeesClient { get; set; } = default!;
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private List<EmployeeDto> _entityList = new();

    // supply default parameters, but leave the possibility to override them
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = L["Superior"];
        Variant = Variant.Filled;
        Dense = true;
        Margin = Margin.None;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchSuperiors;
        ToStringFunc = GetSuperiorName;
        Clearable = true;
        return base.SetParametersAsync(parameters);
    }

    // when the value parameter is set, we have to load that one Employee to be able to show the name
    // we can't do that in OnInitialized because of a strange bug (https://github.com/MudBlazor/MudBlazor/issues/3818)
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender &&
            _value != default &&
            await ApiHelper.ExecuteCallGuardedAsync(
                () => EmployeesClient.GetAsync(_value), Snackbar) is { } entityDetailsDto)
        {
            var entity = new EmployeeDto
            {
                Id = entityDetailsDto.Id,
                Code = entityDetailsDto.Code,
                FirstName = entityDetailsDto.FirstName
            };
            _entityList.Add(entity);
            ForceRender(true);
        }
    }

    private async Task<IEnumerable<Guid>> SearchSuperiors(string value)
    {
        var filter = new SearchEmployeesRequest
        {
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "name" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => EmployeesClient.SearchAsync(filter), Snackbar)
            is PaginationResponseOfEmployeeDto response)
        {
            _entityList = response.Data.ToList();
        }

        return _entityList.Select(x => x.Id);
    }

    private string GetSuperiorName(Guid id) =>
        _entityList.Find(e => e.Id == id)?.FirstName ?? string.Empty;
}