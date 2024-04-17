using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using FSH.WebApi.Shared.Helper;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Settings;

public partial class BackgroundJobs
{
    protected EntityServerTableContext<BackgroundJobDto, Guid, BackgroundJobViewModel> Context { get; set; } = default!;
    protected override void OnInitialized() =>
        Context = new(
            fields:
            [
                new(BackgroundJob => BackgroundJob.Code, L["Code"], "Code"),
                new(BackgroundJob => BackgroundJob.Name, L["Name"], "Name"),
                new(BackgroundJob => BackgroundJob.Type, L["Type"], "Type" ),
                new(BackgroundJob => BackgroundJob.Command, L["Command"], "Command" ),
                new(BackgroundJob => BackgroundJob.DayOfMonth, L["DoM"], "DayOfMonth" ),
                new(BackgroundJob => BackgroundJob.DayOfWeek, L["DoW"], "DayOfWeek" ),
                new(BackgroundJob => BackgroundJob.RunTime.ToLocalTime(), L["RunTime"], "RunTime", Type: typeof(DateTime)),
                new(BackgroundJob => BackgroundJob.FromDate.ToLocalTime(), L["FromDate"], "FromDate" ),
                new(BackgroundJob => BackgroundJob.ToDate.ToLocalTime(), L["ToDate"], "ToDate" ),
                new(BackgroundJob => BackgroundJob.RepeatTimes, L["Repeat"], "RepeatTimes" ),

                // new(BackgroundJob => BackgroundJob.Description, L["Description"], "Description"),
            ],
            searchFunc: async filter =>
            {
                var searchFilter = filter.Adapt<SearchBackgroundJobsRequest>();
                searchFilter.Type = SearchBackgroundJobType == default ? null : SearchBackgroundJobType;
                searchFilter.FromDate = SearchFromDate;
                searchFilter.ToDate = SearchToDate;

                var result = await BackgroundJobsClient.SearchAsync(searchFilter);
                return result.Adapt<PaginationResponse<BackgroundJobDto>>();
            },
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportBackgroundJobsRequest>();
                exportFilter.Type = SearchBackgroundJobType == default ? null : SearchBackgroundJobType;
                exportFilter.FromDate = SearchFromDate;
                exportFilter.ToDate = SearchToDate;

                return await BackgroundJobsClient.ExportAsync(exportFilter);
            },
            enableAdvancedSearch: true,
            idFunc: BackgroundJob => BackgroundJob.Id,
            createFunc: async BackgroundJob =>
            {
                var request = BackgroundJob.Adapt<CreateBackgroundJobRequest>();
                request.RunTime = DateTimeHelper.Local2UTC(
                    BackgroundJob.LocalDate ?? DateTime.Now,
                    BackgroundJob.LocalTimeOfDay ?? DateTime.Now.TimeOfDay);
                request.Code = request.Name = request.Command.ToString() + "_" + request.RunTime.ToString("yyyyMMdd_HHmm");
                await BackgroundJobsClient.CreateAsync(request);
            },
            updateFunc: async (id, BackgroundJob) =>
            {
                var request = BackgroundJob.Adapt<UpdateBackgroundJobRequest>();
                request.RunTime = DateTimeHelper.Local2UTC(
                    BackgroundJob.LocalDate ?? DateTime.Now,
                    BackgroundJob.LocalTimeOfDay ?? DateTime.Now.TimeOfDay);
                await BackgroundJobsClient.UpdateAsync(id, request);
            },
            deleteFunc: async id => await BackgroundJobsClient.DeleteAsync(id),
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportBackgroundJobsRequest() { ExcelFile = FileUploadRequest };
                await BackgroundJobsClient.ImportAsync(request);
            },
            entityName: L["BackgroundJob"],
            entityNamePlural: L["BackgroundJobs"],
            entityResource: FSHResource.BackgroundJobs);

    #region Advanced Search
    private EntityTable<BackgroundJobDto, Guid, BackgroundJobViewModel>? _table;

    private BackgroundJobType _searchBackgroundJobType = BackgroundJobType.All;
    private BackgroundJobType SearchBackgroundJobType
    {
        get => _searchBackgroundJobType;
        set
        {
            _searchBackgroundJobType = value;
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
}

public class BackgroundJobViewModel : UpdateBackgroundJobRequest
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