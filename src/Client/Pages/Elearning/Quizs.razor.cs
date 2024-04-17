using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Infrastructure.Common;
using FSH.WebApi.Shared.Authorization;
using FSH.WebApi.Shared.Helper;
using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System.Security.Claims;

namespace FSH.BlazorWebAssembly.Client.Pages.Elearning;

public partial class Quizs
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;

    [Inject]
    protected IQuizsClient QuizsClient { get; set; } = default!;

    protected EntityServerTableContext<QuizDto, Guid, QuizViewModel> Context { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        if ((await AuthState).User is { } user)
        {
            _currentUserId = user.GetUserId() ?? string.Empty;
        }

        Context = new(
        fields:
            [

                // new(Quiz => Quiz.Id, L["Id"], "Id"),
                new(Quiz => Quiz.Code, L["Code"], "Code"),
                new(Quiz => Quiz.Name, L["Name"], "Name"),
                new(Quiz => Quiz.Description, L["Description"], "Description"),

                new(Quiz => Quiz.StartTime.ToLocalTime(), L["StartTime"], "StartTime", Type: typeof(DateTime)),
                new(Quiz => Quiz.EndTime.ToLocalTime(), L["EndTime"], "EndTime", Type: typeof(DateTime)),

                new(Quiz => Quiz.QuizTopicName, L["Topic"], "QuizTopic.Name"),
                new(Quiz => Quiz.QuizModeName, L["Mode"], "QuizMode.Name"),
                new(Quiz => Quiz.QuizTypeName, L["Type"], "QuizType>name"),

                // new(Quiz => Quiz.QuizPath, L["QuizPath"], "QuizPath"),
                new(Quiz => Quiz.Price, L["Price"], "Price"),
                new(Quiz => Quiz.Sale, L["Sale"], "Sale"),
                new(Quiz => Quiz.Rating, L["Rating"], "Rating"),
                new(Quiz => Quiz.RatingCount, L["RatingCount"], "Reviewers"),

                new(Quiz => Quiz.IsActive, L["Active"], Type: typeof(bool)),
            ],
        searchFunc: async filter =>
        {
            var searchFilter = filter.Adapt<SearchQuizsRequest>();
            searchFilter.QuizTypeId = SearchQuizTypeId == default ? null : SearchQuizTypeId;
            searchFilter.QuizTopicId = SearchQuizTopicId == default ? null : SearchQuizTopicId;
            searchFilter.QuizModeId = SearchQuizModeId == default ? null : SearchQuizModeId;

            searchFilter.FromDate = SearchFromDate;
            searchFilter.ToDate = SearchToDate;

            var result = await QuizsClient.SearchAsync(searchFilter);
            return result.Adapt<PaginationResponse<QuizDto>>();
        },

        exportFunc: async filter =>
        {
            var exportFilter = filter.Adapt<ExportQuizsRequest>();
            exportFilter.QuizTypeId = SearchQuizTypeId == default ? null : SearchQuizTypeId;
            exportFilter.QuizTopicId = SearchQuizTopicId == default ? null : SearchQuizTopicId;
            exportFilter.QuizModeId = SearchQuizModeId == default ? null : SearchQuizModeId;
            exportFilter.FromDate = SearchFromDate;
            exportFilter.ToDate = SearchToDate;

            return await QuizsClient.ExportAsync(exportFilter);
        },
        enableAdvancedSearch: true,
        idFunc: Quiz => Quiz.Id,

        createFunc: async quiz =>
        {
            if (!string.IsNullOrEmpty(quiz.QuizInBytes))
            {
                quiz.QuizMedia = new FileUploadRequest()
                {
                    Data = quiz.QuizInBytes,
                    Extension = quiz.QuizExtension ?? string.Empty,
                    Name = $"{quiz.Code}"

                    // Name = $"{Quiz.Name}_{Guid.NewGuid():N}"
                };
            }

            quiz.StartTime = DateTimeHelper.Local2UTC(quiz.LocalStartDate ?? DateTime.Now, quiz.LocalStartTimeOfDay ?? TimeSpan.Zero);
            quiz.EndTime = DateTimeHelper.Local2UTC(quiz.LocalEndDate ?? DateTime.Now, quiz.LocalEndTimeOfDay ?? TimeSpan.Zero);

            await QuizsClient.CreateAsync(quiz.Adapt<CreateQuizRequest>());
            quiz.QuizInBytes = string.Empty;
        },

        updateFunc: async (id, quiz) =>
        {
            if (!string.IsNullOrEmpty(quiz.QuizInBytes))
            {
                quiz.DeleteCurrentQuiz = true;
                quiz.QuizMedia = new FileUploadRequest()
                {
                    Data = quiz.QuizInBytes,
                    Extension = quiz.QuizExtension ?? string.Empty,
                    Name = $"{quiz.Code}"

                    // Name = $"{quiz.Name}_{Guid.NewGuid():N}"
                };
            }

            quiz.StartTime = DateTimeHelper.Local2UTC(quiz.LocalStartDate ?? DateTime.Now, quiz.LocalStartTimeOfDay ?? TimeSpan.Zero);
            quiz.EndTime = DateTimeHelper.Local2UTC(quiz.LocalEndDate ?? DateTime.Now, quiz.LocalEndTimeOfDay ?? TimeSpan.Zero);

            await QuizsClient.UpdateAsync(id, quiz.Adapt<UpdateQuizRequest>());
            quiz.QuizInBytes = string.Empty;
        },
        deleteFunc: async id => await QuizsClient.DeleteAsync(id),
        importFunc: async FileUploadRequest =>
        {
            var request = new ImportQuizsRequest() { ExcelFile = FileUploadRequest };
            await QuizsClient.ImportAsync(request);
        },
        entityName: L["Quiz"],
        entityNamePlural: L["Quizs"],
        entityResource: FSHResource.Quizs);
    }

    // Take a Quiz

    private string _currentUserId = string.Empty;
    private string QuizUrl => Config[ConfigNames.ApiBaseUrl] + Context.AddEditModal.RequestModel.QuizPath
                                    + "/index.html?QuizId=" + Context.AddEditModal.RequestModel.Id
                                    + "&SId=" + _currentUserId;

    // TODO : Make this as a shared service or something? Since it's used by Profile Component also for now, and literally any other component that will have image upload.
    // The new service should ideally return $"data:{ApplicationConstants.StandardImageFormat};base64,{Convert.ToBase64String(buffer)}"

    #region Quiz Upload
    private IBrowserFile? UploadFile { get; set; }
    public void ClearQuizInBytes()
    {
        UploadFile = null;
        Context.AddEditModal.RequestModel.QuizInBytes = string.Empty;
        Context.AddEditModal.ForceRender();
    }

    public void SetDeleteCurrentQuizFlag()
    {
        Context.AddEditModal.RequestModel.QuizInBytes = string.Empty;
        Context.AddEditModal.RequestModel.QuizPath = string.Empty;
        Context.AddEditModal.RequestModel.DeleteCurrentQuiz = true;
        Context.AddEditModal.ForceRender();
    }

    private async Task UploadFiles(InputFileChangeEventArgs e)
    {
        if (e.File != null)
        {
            UploadFile = e.File;
            string? extension = Path.GetExtension(e.File.Name);
            if (!ApplicationConstants.SupportedQuizMediaFormats.Contains(extension.ToLower()))
            {
                Snackbar.Add("QuizMedia Format Not Supported.", Severity.Error);
                UploadFile = null;
                return;
            }

            Context.AddEditModal.RequestModel.QuizExtension = extension;
            byte[]? buffer = new byte[UploadFile.Size];
            await UploadFile.OpenReadStream(ApplicationConstants.MaxQuizMediaFileSize).ReadAsync(buffer);
            Context.AddEditModal.RequestModel.QuizInBytes = $"data:{ApplicationConstants.StandardQuizMediaFormat};base64,{Convert.ToBase64String(buffer)}";

            Context.AddEditModal.ForceRender();
        }
    }
    #endregion

    #region Advanced Search
    private EntityTable<QuizDto, Guid, QuizViewModel>? _table;

    private Guid _searchQuizTypeId;
    private Guid SearchQuizTypeId
    {
        get => _searchQuizTypeId;
        set
        {
            _searchQuizTypeId = value;
            _ = _table?.ReloadDataAsync();
        }
    }

    private Guid _searchQuizTopicId;
    private Guid SearchQuizTopicId
    {
        get => _searchQuizTopicId;
        set
        {
            _searchQuizTopicId = value;
            _ = _table?.ReloadDataAsync();
        }
    }

    private Guid _searchQuizModeId;
    private Guid SearchQuizModeId
    {
        get => _searchQuizModeId;
        set
        {
            _searchQuizModeId = value;
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

public class QuizViewModel : UpdateQuizRequest
{
    #region File Upload
    public string? QuizPath { get; set; }
    public string? QuizInBytes { get; set; }
    public string? QuizExtension { get; set; }
    #endregion

    #region Date and Time Picker

    private TimeSpan? _localStartTimeOfDay;
    public TimeSpan? LocalStartTimeOfDay
    {
        get
        {
            if (_localStartTimeOfDay == null && StartTime != null)
                _localStartTimeOfDay = StartTime.Value.ToLocalTime().TimeOfDay;

            return _localStartTimeOfDay;
        }
        set
        {
            _localStartTimeOfDay = value;
        }
    }

    private DateTime? _localStartDate;
    public DateTime? LocalStartDate
    {
        get
        {
            if (_localStartDate == null && StartTime != null)
                _localStartDate = StartTime.Value.ToLocalTime();

            return _localStartDate;
        }
        set
        {
            _localStartDate = value;
        }
    }

    private TimeSpan? _localEndTimeOfDay;
    public TimeSpan? LocalEndTimeOfDay
    {
        get
        {
            if (_localEndTimeOfDay == null && EndTime != null)
                _localEndTimeOfDay = EndTime.Value.ToLocalTime().TimeOfDay;

            return _localEndTimeOfDay;
        }
        set
        {
            _localEndTimeOfDay = value;
        }
    }

    private DateTime? _localEndDate;
    public DateTime? LocalEndDate
    {
        get
        {
            if (_localEndDate == null && EndTime != null)
                _localEndDate = EndTime.Value.ToLocalTime();

            return _localEndDate;
        }
        set
        {
            _localEndDate = value;
        }
    }
    #endregion
}
