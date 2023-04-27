using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.People;

public class EmployeeAutocomplete : MudAutocomplete<Guid>
{
    [Inject]
    private IStringLocalizer<EmployeeAutocomplete> L { get; set; } = default!;
    [Inject]
    private IEmployeesClient EmployeesClient { get; set; } = default!;
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;
    [Inject]
    private ITitlesClient TitlesClient { get; set; } = default!;

    private List<EmployeeDto> _entityList = new();

    // supply default parameters, but leave the possibility to override them
    [Parameter]
    public Guid TitleId { get; set; }
    [Parameter]
    public Guid BusinessUnitId { get; set; }
    [Parameter]
    public Guid DepartmentId { get; set; }
    [Parameter]
    public Guid SubDepartmentId { get; set; }
    [Parameter]
    public Guid TeamId { get; set; }

    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = L["Employee"];
        Variant = Variant.Filled;
        Dense = true;
        Margin = Margin.None;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchEmployees;
        ToStringFunc = GetEmployeeName;
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

    private async Task<IEnumerable<Guid>> SearchEmployees(string value)
    {
        TitleDetailsDto? empTitle = null;
        if (TitleId != default)
        {
            empTitle = await TitlesClient.GetAsync(TitleId);
        }

        var filter = new SearchEmployeesRequest
        {
            EmployeeGrade = empTitle?.Grade ?? 0,

            BusinessUnitId = BusinessUnitId == Guid.Empty ? null : BusinessUnitId,
            DepartmentId = DepartmentId == Guid.Empty ? null : DepartmentId,
            SubDepartmentId = SubDepartmentId == Guid.Empty ? null : SubDepartmentId,
            TeamId = TeamId == Guid.Empty ? null : TeamId,

            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "FirstName", "LastName" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => EmployeesClient.SearchAsync(filter), Snackbar)
            is PaginationResponseOfEmployeeDto response)
        {
            _entityList = response.Data.ToList();
        }

        return _entityList.Select(x => x.Id);
    }

    private string GetEmployeeName(Guid id)
    {
        var emp = _entityList.Find(e => e.Id == id);
        return emp != null ? emp.TitleCode + ": " + emp.FirstName + " " + emp.LastName : string.Empty;
    }
}