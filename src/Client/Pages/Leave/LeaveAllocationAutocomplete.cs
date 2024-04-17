using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.Leave;

public class LeaveAllocationAutocomplete : MudAutocomplete<Guid>
{
    [Inject]
    private IStringLocalizer<LeaveAllocationAutocomplete> L { get; set; } = default!;
    [Inject]
    private ILeaveAllocationsClient LeaveAllocationsClient { get; set; } = default!;
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;
    [Inject]

    private IDimensionsClient DimensionsClient { get; set; } = default!;

    private List<LeaveAllocationDto> _entityList = new();

    // supply default parameters, but leave the possibility to override them
    [Parameter]
    public Guid EmployeeId { get; set; }

    [Parameter]
    public Guid LeaveTypeId { get; set; }
    [Parameter]
    public int Period { get; set; }

    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = L["LeaveAllocation"];
        Variant = Variant.Filled;
        Dense = true;
        Margin = Margin.None;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchLeaveAllocations;
        ToStringFunc = GetLeaveAllocationName;
        Clearable = true;
        return base.SetParametersAsync(parameters);
    }

    // when the value parameter is set, we have to load that one LeaveAllocation to be able to show the name
    // we can't do that in OnInitialized because of a strange bug (https://github.com/MudBlazor/MudBlazor/issues/3818)
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender &&
            _value != default &&
            await ApiHelper.ExecuteCallGuardedAsync(
                () => LeaveAllocationsClient.GetAsync(_value), Snackbar) is { } entityDetailsDto)
        {
            var entity = new LeaveAllocationDto
            {
                Id = entityDetailsDto.Id,
                EmployeeCode = entityDetailsDto.EmployeeCode,
                EmployeeFirstName = entityDetailsDto.EmployeeFirstName
            };
            _entityList.Add(entity);
            ForceRender(true);
        }
    }

    private async Task<IEnumerable<Guid>> SearchLeaveAllocations(string value)
    {
       // DimensionDetailsDto? empTitle = null;

        // if (TitleId != Guid.Empty)
        // {
        //    empTitle = await DimensionsClient.GetAsync(TitleId);
        // }

        var filter = new SearchLeaveAllocationsRequest
        {
            Period = Period,
            IsActive = true,
            IsLocked = false,
            EmployeeId = EmployeeId == Guid.Empty ? null : EmployeeId,
            LeaveAllocationTypeId = LeaveTypeId == Guid.Empty ? null : LeaveTypeId,

            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "FirstName", "LastName" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => LeaveAllocationsClient.SearchAsync(filter), Snackbar)
            is PaginationResponseOfLeaveAllocationDto response)
        {
            _entityList = response.Data.ToList();
        }

        return _entityList.Select(x => x.Id);
    }

    private string GetLeaveAllocationName(Guid id)
    {
        var entity = _entityList.Find(e => e.Id == id);
        return entity != null ? entity.Period + "_" + entity.LeaveAllocationTypeName + ": " + entity.EmployeeCode + " " + entity.EmployeeFirstName : string.Empty;
    }
}
