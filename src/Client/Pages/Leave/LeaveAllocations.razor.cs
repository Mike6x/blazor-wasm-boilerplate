using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace FSH.BlazorWebAssembly.Client.Pages.Leave;

public partial class LeaveAllocations
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;

    [Inject]
    protected ILeaveAllocationsClient LeaveAllocationsClient { get; set; } = default!;

    protected EntityServerTableContext<LeaveAllocationDto, Guid, LeaveAllocationViewModel> Context { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        if ((await AuthState).User is { } user)
        {
            _currentUserId = user.GetUserId() ?? string.Empty;
        }

        Context = new(
        fields:
            [
                new(LeaveAllocation => LeaveAllocation.Period, L["Period"], "Period"),
                new(LeaveAllocation => LeaveAllocation.EmployeeCode, L["Emp Code"], "Employee.Code"),
                new(LeaveAllocation => LeaveAllocation.EmployeeFirstName, L["Emp Name"], "Employee.FirstName"),
                new(LeaveAllocation => LeaveAllocation.LeaveAllocationTypeName, L["Type"], "LeaveAllocationType.name"),

                new(LeaveAllocation => LeaveAllocation.NumberOfAnnualDays, L["Annual Days"], "NumberOfAnnualDays"),
                new(LeaveAllocation => LeaveAllocation.NumberOfExtraDays, L["Extra Days"], "NumberOfExtraDays"),
                new(LeaveAllocation => LeaveAllocation.NumberOfCarryOverDays, L["CarryOver Days"], "NumberOfCarryOverDays"),
                new(LeaveAllocation => LeaveAllocation.NumberOfCompensationDays, L["Compensation Days"], "NumberOfCompensationDays"),
                new(LeaveAllocation => LeaveAllocation.NumberOfValidDays, L["Valid Days"], "NumberOfValidDays"),
                new(LeaveAllocation => LeaveAllocation.NumberOfOnHandDays, L["OnHand Days"], "NumberOfOnHandDays"),

                new(LeaveAllocation => LeaveAllocation.IsActive, L["Active"], Type: typeof(bool)),
                new(LeaveAllocation => LeaveAllocation.IsLocked, L["Locked"], Type: typeof(bool)),

                new(LeaveAllocation => LeaveAllocation.LastModifiedOn?.ToLocalTime(), L["Last Modified"], "lastModifiedOn", Type: typeof(DateTime)),
            ],
        searchFunc: async filter =>
        {
            var searchFilter = filter.Adapt<SearchLeaveAllocationsRequest>();
            searchFilter.Period = SearchPeriod;
            searchFilter.LeaveAllocationTypeId = SearchTypeId == default ? null : SearchTypeId;
            searchFilter.EmployeeId = SearchEmployeeId == default ? null : SearchEmployeeId;

            searchFilter.FromDate = SearchFromDate;
            searchFilter.ToDate = SearchToDate;

            var result = await LeaveAllocationsClient.SearchAsync(searchFilter);
            return result.Adapt<PaginationResponse<LeaveAllocationDto>>();
        },

        exportFunc: async filter =>
        {
            var exportFilter = filter.Adapt<ExportLeaveAllocationsRequest>();
            exportFilter.Period = SearchPeriod;
            exportFilter.LeaveAllocationTypeId = SearchTypeId == default ? null : SearchTypeId;
            exportFilter.EmployeeId = SearchEmployeeId == default ? null : SearchEmployeeId;
            exportFilter.FromDate = SearchFromDate;
            exportFilter.ToDate = SearchToDate;

            return await LeaveAllocationsClient.ExportAsync(exportFilter);
        },
        enableAdvancedSearch: true,
        idFunc: LeaveAllocation => LeaveAllocation.Id,

        createFunc: async leaveAllocation =>
        {
            await LeaveAllocationsClient.CreateAsync(leaveAllocation.Adapt<CreateLeaveAllocationRequest>());
        },

        updateFunc: async (id, leaveAllocation) =>
        {
            await LeaveAllocationsClient.UpdateAsync(id, leaveAllocation.Adapt<UpdateLeaveAllocationRequest>());
        },
        deleteFunc: async id => await LeaveAllocationsClient.DeleteAsync(id),
        importFunc: async FileUploadRequest =>
        {
            var request = new ImportLeaveAllocationsRequest() { ExcelFile = FileUploadRequest };
            await LeaveAllocationsClient.ImportAsync(request);
        },
        entityName: L["LeaveAllocation"],
        entityNamePlural: L["LeaveAllocations"],
        entityResource: FSHResource.LeaveAllocations);
    }

    // Take a LeaveAllocation

    private string _currentUserId = string.Empty;

    // TODO : Make this as a shared service or something? Since it's used by Profile Component also for now, and literally any other component that will have image upload.
    // The new service should ideally return $"data:{ApplicationConstants.StandardImageFormat};base64,{Convert.ToBase64String(buffer)}"

    #region ExtraActions
    private async Task<Guid> UnlockLeaveAllocation(Guid id)
    {
       var result = await LeaveAllocationsClient.UnlockAsync(id, new UnlockLeaveAllocationRequest() { Id = id });
       _ = _table?.ReloadDataAsync();

       return result;
    }
    #endregion

    #region Advanced Search
    private EntityTable<LeaveAllocationDto, Guid, LeaveAllocationViewModel>? _table;

    private int _searchPeriod = DateTime.UtcNow.Year;
    private int SearchPeriod
    {
        get => _searchPeriod;
        set
        {
            _searchPeriod = value;
            _ = _table?.ReloadDataAsync();
        }
    }

    private Guid _searchEmployeeId;
    private Guid SearchEmployeeId
    {
        get => _searchEmployeeId;
        set
        {
            _searchEmployeeId = value;
            _ = _table?.ReloadDataAsync();
        }
    }

    private Guid _searchTypeId;
    private Guid SearchTypeId
    {
        get => _searchTypeId;
        set
        {
            _searchTypeId = value;
            _ = _table?.ReloadDataAsync();
        }
    }

    private DateTime? _searchFromDate;
    private DateTime? SearchFromDate
    {
        get => _searchFromDate;
        set
        {
            _searchFromDate = value;
            _ = _table?.ReloadDataAsync();
        }
    }

    private DateTime? _searchToDate;
    private DateTime? SearchToDate
    {
        get => _searchToDate;
        set
        {
            _searchToDate = value;
            _ = _table?.ReloadDataAsync();
        }
    }
    #endregion

    #region Organization select
    private Guid _businessUnitId;
    private Guid BusinessUnitId
    {
        get => _businessUnitId;
        set
        {
            if (value == Guid.Empty)
            {
                DepartmentId = Guid.NewGuid();
                SubDepartmentId = Guid.NewGuid();
                TeamId = Guid.NewGuid();
            }

            _businessUnitId = value;
            Context.AddEditModal.ForceRender();
        }
    }

    private Guid _departmentId;
    private Guid DepartmentId
    {
        get => _departmentId;
        set
        {
            if (value == Guid.Empty)
            {
                SubDepartmentId = Guid.NewGuid();
                TeamId = Guid.NewGuid();
            }

            _departmentId = value;
            Context.AddEditModal.ForceRender();
        }
    }

    private Guid _subDepartmentId;
    private Guid SubDepartmentId
    {
        get => _subDepartmentId;
        set
        {
            if (value == Guid.Empty)
            {
                TeamId = Guid.NewGuid();
            }

            _subDepartmentId = value;
            Context.AddEditModal.ForceRender();
        }
    }

    private Guid _teamId;
    private Guid TeamId
    {
        get => _teamId;
        set
        {
            _teamId = value;
            Context.AddEditModal.ForceRender();
        }
    }

    #endregion
}

public class LeaveAllocationViewModel : UpdateLeaveAllocationRequest
{
}
