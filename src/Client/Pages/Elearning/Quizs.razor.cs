using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Infrastructure.Common;
using FSH.WebApi.Shared.Authorization;
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
    private IBrowserFile? UploadFile { get; set; }

    private EntityTable<QuizDto, Guid, QuizViewModel>? _table;

    protected override async Task OnInitializedAsync()
    {
        if ((await AuthState).User is { } user)
        {
            _currentUserId = user.GetUserId() ?? string.Empty;
        }

        Context = new(
        entityName: L["Quiz"],
        entityNamePlural: L["Quizs"],
        entityResource: FSHResource.Quizs,
        fields: new()
            {
                // new(Quiz => Quiz.Id, L["Id"], "Id"),
                new(Quiz => Quiz.Code, L["Code"], "Code"),
                new(Quiz => Quiz.Name, L["Name"], "Name"),
                new(Quiz => Quiz.Description, L["Description"], "Description"),

                new(Quiz => Quiz.StartTime.ToLocalTime(), L["StartTime"], "StartTime", Type: typeof(DateTime)),
                new(Quiz => Quiz.EndTime.ToLocalTime(), L["EndTime"], "EndTime", Type: typeof(DateTime)),

                new(Quiz => Quiz.QuizTopic, L["Topic"], "QuizTopic"),
                new(Quiz => Quiz.QuizMode, L["Mode"], "QuizMode"),
                new(Quiz => Quiz.QuizType, L["Type"], "QuizType"),

                // new(Quiz => Quiz.QuizPath, L["QuizPath"], "QuizPath"),
                new(Quiz => Quiz.Price, L["Price"], "Price"),
                new(Quiz => Quiz.Sale, L["Sale"], "Sale"),
                new(Quiz => Quiz.Rating, L["Rating"], "Rating"),
                new(Quiz => Quiz.RatingCount, L["RatingCount"], "Reviewers"),

                new(Quiz => Quiz.IsActive, L["Active"], Type: typeof(bool)),
            },
        enableAdvancedSearch: true,
        idFunc: Quiz => Quiz.Id,
        searchFunc: async filter =>
        {
            var quizFilter = filter.Adapt<SearchQuizsRequest>();

            quizFilter.QuizType = SearchQuizTypeId == QuizType.All ? null : SearchQuizTypeId;
            quizFilter.QuizTopic = SearchQuizTopicId == QuizTopic.All ? null : SearchQuizTopicId;
            quizFilter.QuizMode = SearchQuizModeId == QuizMode.All ? null : SearchQuizModeId;

            var result = await QuizsClient.SearchAsync(quizFilter);
            return result.Adapt<PaginationResponse<QuizDto>>();
        },

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

            GetStartTime();
            GetEndTime();
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

            GetStartTime();
            GetEndTime();
            await QuizsClient.UpdateAsync(id, quiz.Adapt<UpdateQuizRequest>());
            quiz.QuizInBytes = string.Empty;
        },

        deleteFunc: async id => await QuizsClient.DeleteAsync(id),

        exportFunc: async filter =>
        {
            var exportFilter = filter.Adapt<ExportQuizsRequest>();

            // exportFilter.BrandId = SearchBrandId == default ? null : SearchBrandId;
            // exportFilter.MinimumRate = SearchMinimumRate;
            // exportFilter.MaximumRate = SearchMaximumRate;

            return await QuizsClient.ExportAsync(exportFilter);
        },
        importFunc: async FileUploadRequest =>
        {
            var request = new ImportQuizsRequest() { ExcelFile = FileUploadRequest };
            await QuizsClient.ImportAsync(request);
        }
        );
    }

    // Advanced Search
    private QuizType _searchQuizTypeId = QuizType.All;
    private QuizType SearchQuizTypeId
    {
        get => _searchQuizTypeId;
        set
        {
            _searchQuizTypeId = value;
            _ = _table?.ReloadDataAsync();
        }
    }

    private QuizTopic _searchQuizTopicId = QuizTopic.All;
    private QuizTopic SearchQuizTopicId
    {
        get => _searchQuizTopicId;
        set
        {
            _searchQuizTopicId = value;
            _ = _table?.ReloadDataAsync();
        }
    }

    private QuizMode _searchQuizModeId = QuizMode.All;
    private QuizMode SearchQuizModeId
    {
        get => _searchQuizModeId;
        set
        {
            _searchQuizModeId = value;
            _ = _table?.ReloadDataAsync();
        }
    }

    // Take a Quiz

    private string _currentUserId = string.Empty;
    private string QuizUrl => Config[ConfigNames.ApiBaseUrl] + Context.AddEditModal.RequestModel.QuizPath
                                    + "/index.html?QuizId=" + Context.AddEditModal.RequestModel.Id
                                    + "&SId=" + _currentUserId;

    // TODO : Make this as a shared service or something? Since it's used by Profile Component also for now, and literally any other component that will have image upload.
    // The new service should ideally return $"data:{ApplicationConstants.StandardImageFormat};base64,{Convert.ToBase64String(buffer)}"

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

    private TimeSpan? _startTimeSpan;
    public TimeSpan? StartTimeSpan
    {
        get
        {
            if (_startTimeSpan == null && Context.AddEditModal.RequestModel.StartTime != null)
                _startTimeSpan = Context.AddEditModal.RequestModel.StartTime.Value.ToLocalTime().TimeOfDay;
            return _startTimeSpan;
        }
        set
        {
            _startTimeSpan = value;
        }
    }

    private TimeSpan? _endTimeSpan;
    public TimeSpan? EndTimeSpan
    {
        get
        {
            if (_endTimeSpan == null && Context.AddEditModal.RequestModel.EndTime != null)
                _endTimeSpan = Context.AddEditModal.RequestModel.EndTime.Value.ToLocalTime().TimeOfDay;
            return _endTimeSpan;
        }
        set
        {
            _endTimeSpan = value;
        }
    }

    private void GetStartTime()
    {

        if (Context.AddEditModal.RequestModel.StartTime == null)
        {
            Context.AddEditModal.RequestModel.StartTime = DateTime.Now.ToLocalTime().Date;
            StartTimeSpan = DateTime.Now.ToLocalTime().TimeOfDay;
        }

        DateTime localTime = Context.AddEditModal.RequestModel.StartTime.Value.Date.Add(StartTimeSpan ?? TimeSpan.Zero);
        Context.AddEditModal.RequestModel.StartTime = TimeZoneInfo.ConvertTimeToUtc(localTime);
    }

    private void GetEndTime()
    {
        if (Context.AddEditModal.RequestModel.EndTime == null)
        {
            Context.AddEditModal.RequestModel.EndTime = DateTime.Now.ToLocalTime().Date;
            EndTimeSpan = DateTime.Now.ToLocalTime().TimeOfDay;
        }

        DateTime localTime = Context.AddEditModal.RequestModel.EndTime.Value.Date.Add(EndTimeSpan ?? TimeSpan.Zero);
        Context.AddEditModal.RequestModel.EndTime = TimeZoneInfo.ConvertTimeToUtc(localTime);
    }
}

public class QuizViewModel : UpdateQuizRequest
{
    public string? QuizPath { get; set; }
    public string? QuizInBytes { get; set; }
    public string? QuizExtension { get; set; }
}
