using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using FSH.WebApi.Shared.Helper;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Integration;

public partial class CronJobs
{
    protected EntityServerTableContext<CronJobDto, Guid, CronJobViewModel> Context { get; set; } = default!;
    protected override void OnInitialized() =>
        Context = new(
            fields:
            [

                // new(CronJob => CronJob.Code, L["Code"], "Code"),
                new(CronJob => CronJob.Name, L["Name"], "Name"),
                new(CronJob => CronJob.FromDate.ToLocalTime(), L["FromDate"], "FromDate" ),
                new(CronJob => CronJob.ToDate.ToLocalTime(), L["ToDate"], "ToDate" ),
                new(CronJob => CronJob.RunTime.ToLocalTime(), L["RunTime"], "RunTime", Type: typeof(DateTime)),
                new(CronJob => CronJob.TotalRecord, L["TotalRecord"], "TotalRecord" ),
                new(CronJob => CronJob.NumberOfSuccessed, L["NumberOfSuccessed"], "NumberOfSuccessed" ),
                new(CronJob => CronJob.NumberOfDuplicated, L["NumberOfDuplicated"], "NumberOfDuplicated" ),
                new(CronJob => CronJob.NumberOfExisted, L["NumberOfExisted"], "NumberOfExisted" ),
                new(CronJob => CronJob.NumberOfFailed, L["NumberOfFailed"], "NumberOfFailed" ),

                // new(CronJob => CronJob.Description, L["Description"], "Description"),
            ],
            searchFunc: async filter =>
            {
                var searchFilter = filter.Adapt<SearchCronJobsRequest>();

                searchFilter.FromDate = SearchFromDate;
                searchFilter.ToDate = SearchToDate;

                var result = await CronJobsClient.SearchAsync(searchFilter);
                return result.Adapt<PaginationResponse<CronJobDto>>();
            },
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportCronJobsRequest>();
                exportFilter.FromDate = SearchFromDate;
                exportFilter.ToDate = SearchToDate;

                return await CronJobsClient.ExportAsync(exportFilter);
            },
            enableAdvancedSearch: true,
            idFunc: CronJob => CronJob.Id,
            createFunc: async CronJob =>
            {
                var request = CronJob.Adapt<CreateCronJobRequest>();
                request.RunTime = DateTimeHelper.Local2UTC(
                    CronJob.LocalDate ?? DateTime.Now,
                    CronJob.LocalTimeOfDay ?? TimeSpan.Zero);

               // request.Code = request.Name = request.Command.ToString() + "_" + request.StartTime.ToString("yyyyMMdd_HHmm");
                await CronJobsClient.CreateAsync(request);
            },
            updateFunc: async (id, CronJob) =>
            {
                var request = CronJob.Adapt<UpdateCronJobRequest>();
                request.RunTime = DateTimeHelper.Local2UTC(
                    CronJob.LocalDate ?? DateTime.Now,
                    CronJob.LocalTimeOfDay ?? TimeSpan.Zero);
                await CronJobsClient.UpdateAsync(id, request);
            },
            deleteFunc: async id => await CronJobsClient.DeleteAsync(id),
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportCronJobsRequest() { ExcelFile = FileUploadRequest };
                await CronJobsClient.ImportAsync(request);
            },
            entityName: L["CronJob"],
            entityNamePlural: L["CronJobs"],
            entityResource: FSHResource.CronJobs);

    #region Advanced Search
    private EntityTable<CronJobDto, Guid, CronJobViewModel>? _table;

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
}

public class CronJobViewModel : UpdateCronJobRequest
{
    #region Date and Time Picker
    private TimeSpan? _localTimeOfDay;
    public TimeSpan? LocalTimeOfDay
    {
        get
        {
            if (_localTimeOfDay == null && RunTime != null)
                _localTimeOfDay = RunTime.Value.ToLocalTime().TimeOfDay;

            return _localTimeOfDay;
        }
        set
        {
            _localTimeOfDay = value;
        }
    }

    private DateTime? _localDate;
    public DateTime? LocalDate
    {
        get
        {
            if (_localDate == null && RunTime != null)
                _localDate = RunTime.Value.ToLocalTime();

            return _localDate;
        }
        set
        {
            _localDate = value;
        }
    }
    #endregion
}