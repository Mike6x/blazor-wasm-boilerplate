using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using FSH.WebApi.Shared.Helper;
using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using System.Security.Claims;
using static MudBlazor.CategoryTypes;

namespace FSH.BlazorWebAssembly.Client.Pages.Leave;

public partial class LeaveApplications
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;

    [Inject]
    protected ILeaveApplicationsClient LeaveApplicationsClient { get; set; } = default!;

    protected EntityServerTableContext<LeaveApplicationDto, Guid, LeaveApplicationViewModel> Context { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        if ((await AuthState).User is { } user)
        {
            _currentUserId = user.GetUserId() ?? string.Empty;
        }

        Context = new(
        fields:
            [
                new(LeaveApplication => LeaveApplication.LeaveAllocationPeriod, L["Period"], "LeaveAllocation.Period"),
                new(LeaveApplication => LeaveApplication.EmployeeCode, L["Employee Code"], "Employee.Code"),
                new(LeaveApplication => LeaveApplication.EmployeeFirstName, L["Employee Name"], "Employee.FirstName"),

                new(LeaveApplication => LeaveApplication.LeaveTypeName, L["Type"], "LeaveType.name"),
                new(LeaveApplication => LeaveApplication.FromDate.ToLocalTime().ToString("yyyy-MM-dd"), L["FromDate"], "FromDate" ),
                new(LeaveApplication => LeaveApplication.ToDate.ToLocalTime().ToString("yyyy-MM-dd"), L["ToDate"], "ToDate" ),
                new(LeaveApplication => LeaveApplication.OffTime, L["OffTime"], "OffTime"),
                new(LeaveApplication => LeaveApplication.RequestOn.ToLocalTime().ToString("yyyy-MM-dd"), L["RequestOn"], "RequestOn" ),

                new(LeaveApplication => LeaveApplication.Status, L["Status"], "Status"),

                // new(LeaveApplication => LeaveApplication.ApprovedOn?.ToLocalTime().ToString("yyyy-MM-dd"), L["ApprovedOn"], "ApprovedOn" ),
                // new(LeaveApplication => LeaveApplication.ApproverCode, L["Approver Code"], "Approver.Code"),
                new(LeaveApplication => LeaveApplication.ApproverFirstName, L["Approver"], "Approver.FirstName"),

                new(LeaveApplication => LeaveApplication.NumberOfValidDays, L["Valid Days"], "NumberOfValidDays"),
                new(LeaveApplication => LeaveApplication.NumberOfOnHandDays, L["OnHand Days"], "NumberOfOnHandDays"),

                // new(LeaveApplication => LeaveApplication.LastModifiedOn?.ToLocalTime(), L["Last Update Time"], "lastModifiedOn", Type: typeof(DateTime)),
            ],
        searchFunc: async filter =>
        {
            var searchFilter = filter.Adapt<SearchLeaveApplicationsRequest>();
            searchFilter.LeaveTypeId = SearchLeaveTypeId == default ? null : SearchLeaveTypeId;
            searchFilter.EmployeeId = SearchEmployeeId == default ? null : SearchEmployeeId;

            searchFilter.FromDate = SearchFromDate;
            searchFilter.ToDate = SearchToDate;

            var result = await LeaveApplicationsClient.SearchAsync(searchFilter);
            return result.Adapt<PaginationResponse<LeaveApplicationDto>>();
        },

        exportFunc: async filter =>
        {
            var exportFilter = filter.Adapt<ExportLeaveApplicationsRequest>();

            exportFilter.LeaveTypeId = SearchLeaveTypeId == default ? null : SearchLeaveTypeId;
            exportFilter.EmployeeId = SearchEmployeeId == default ? null : SearchEmployeeId;
            exportFilter.FromDate = SearchFromDate;
            exportFilter.ToDate = SearchToDate;

            return await LeaveApplicationsClient.ExportAsync(exportFilter);
        },
        enableAdvancedSearch: true,
        idFunc: LeaveApplication => LeaveApplication.Id,

        createFunc: async leaveApplication =>
        {
            leaveApplication.FromDate = DateTimeHelper.Local2UTC(leaveApplication.LocalFromDate ?? DateTime.Now, TimeSpan.Zero);
            leaveApplication.ToDate = DateTimeHelper.Local2UTC(leaveApplication.LocalToDate ?? DateTime.Now, TimeSpan.Zero);
            leaveApplication.Status = RequestStatus.Edited;

            await LeaveApplicationsClient.CreateAsync(leaveApplication.Adapt<CreateLeaveApplicationRequest>());
        },

        updateFunc: async (id, leaveApplication) =>
        {
            leaveApplication.FromDate = DateTimeHelper.Local2UTC(leaveApplication.LocalFromDate ?? DateTime.Now, TimeSpan.Zero);
            leaveApplication.ToDate = DateTimeHelper.Local2UTC(leaveApplication.LocalToDate ?? DateTime.Now, TimeSpan.Zero);

            await LeaveApplicationsClient.SubmitAsync(id, leaveApplication.Adapt<SubmitLeaveApplicationRequest>());
        },
        deleteFunc: async id => await LeaveApplicationsClient.DeleteAsync(id),
        importFunc: async FileUploadRequest =>
        {
            var request = new ImportLeaveApplicationsRequest() { ExcelFile = FileUploadRequest };
            await LeaveApplicationsClient.ImportAsync(request);
        },
        entityName: L["LeaveApplication"],
        entityNamePlural: L["LeaveApplications"],
        entityResource: FSHResource.LeaveApplications);
    }

    // Take a LeaveApplication

    private string _currentUserId = string.Empty;

    // TODO : Make this as a shared service or something? Since it's used by Profile Component also for now, and literally any other component that will have image upload.
    // The new service should ideally return $"data:{ApplicationConstants.StandardImageFormat};base64,{Convert.ToBase64String(buffer)}"

    #region Advanced Search
    private EntityTable<LeaveApplicationDto, Guid, LeaveApplicationViewModel>? _table;

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

    private Guid _searchApproverId;
    private Guid SearchAppoverId
    {
        get => _searchApproverId;
        set
        {
            _searchApproverId = value;
            _ = _table?.ReloadDataAsync();
        }
    }

    private Guid _searchLeaveTypeId;
    private Guid SearchLeaveTypeId
    {
        get => _searchLeaveTypeId;
        set
        {
            _searchLeaveTypeId = value;
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

    private RequestStatus _searchRequestStatus = RequestStatus.All;
    private RequestStatus SearchRequestStatus
    {
        get => _searchRequestStatus;
        set
        {
            _searchRequestStatus = value;
            _ = _table?.ReloadDataAsync();
        }
    }

    #endregion

    #region Extra actions
    private async Task SubmitRequest()
    {
        var leaveApplication = Context.AddEditModal.RequestModel;
        leaveApplication.FromDate = DateTimeHelper.Local2UTC(leaveApplication.LocalFromDate ?? DateTime.Now, TimeSpan.Zero);
        leaveApplication.ToDate = DateTimeHelper.Local2UTC(leaveApplication.LocalToDate ?? DateTime.Now, TimeSpan.Zero);
        leaveApplication.Status = RequestStatus.Submited;

        await LeaveApplicationsClient.CreateAsync(leaveApplication.Adapt<CreateLeaveApplicationRequest>());

        Snackbar.Add("Request have submited", Severity.Success);

        _ = _table?.ReloadDataAsync();
        Navigation.NavigateTo("/leave/LeaveApplications");
    }

    private async Task ApproveRequest(Guid requestId)
    {
        await LeaveApplicationsClient.ApproveAsync(requestId, new ApproveLeaveApplicationRequest()
        {
            Id = requestId,
            ApproverLogonId = _currentUserId,
            ApproverRemarks = string.Empty,
            IsApproved = true
        });

        Snackbar.Add("Request have approved!", Severity.Success);
        _ = _table?.ReloadDataAsync();
    }
    private async Task RejectRequest(Guid requestId)
    {
        await LeaveApplicationsClient.ApproveAsync(requestId, new ApproveLeaveApplicationRequest()
                {
                    Id = requestId,
                    ApproverLogonId = _currentUserId,
                    ApproverRemarks = string.Empty,
                    IsApproved = false
                });
        Snackbar.Add("Request have Rejected!", Severity.Success);
        _ = _table?.ReloadDataAsync();
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

public class LeaveApplicationViewModel : UpdateLeaveApplicationRequest
{
    #region Date and Time Picker
    private DateTime? _localFromDate;
    public DateTime? LocalFromDate
    {
        get
        {
            if (_localFromDate == null && FromDate != null)
                _localFromDate = FromDate.Value.ToLocalTime();

            return _localFromDate;
        }
        set
        {
            _localFromDate = value;
        }
    }

    private DateTime? _localToDate;
    public DateTime? LocalToDate
    {
        get
        {
            if (_localToDate == null && ToDate != null)
                _localToDate = ToDate.Value.ToLocalTime();

            return _localToDate;
        }
        set
        {
            _localToDate = value;
        }
    }
    #endregion
}