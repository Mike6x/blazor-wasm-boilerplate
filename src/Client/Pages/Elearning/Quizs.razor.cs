using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Infrastructure.Common;
using FSH.WebApi.Shared.Authorization;
using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.Elearning;

public partial class Quizs
{
    [Inject]
    protected IQuizsClient QuizsClient { get; set; } = default!;

    protected EntityServerTableContext<QuizDto, Guid, QuizViewModel> Context { get; set; } = default!;

    private EntityTable<QuizDto, Guid, QuizViewModel>? _table;

    public IBrowserFile? UploadFile { get; set; } = null;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["Quiz"],
            entityNamePlural: L["Quizs"],
            entityResource: FSHResource.Quizs,
            fields: new()
            {
                new(Quiz => Quiz.Id, L["Id"], "Id"),
                new(Quiz => Quiz.Code, L["Code"], "Code"),
                new(Quiz => Quiz.Name, L["Name"], "Name"),
                new(Quiz => Quiz.Description, L["Description"], "Description"),

                new(Quiz => Quiz.StartTime, L["StartTime"], "StartTime"),
                new(Quiz => Quiz.EndTime, L["EndTime"], "EndTime"),
                new(Quiz => Quiz.IsActive, L["IsActive"], "IsActive"),

                new(Quiz => Quiz.QuizPath, L["QuizPath"], "QuizPath"),
                new(Quiz => Quiz.QuizType, L["QuizType"], "QuizType"),
                new(Quiz => Quiz.QuizTopic, L["QuizTopic"], "QuizTopic"),
            },
            enableAdvancedSearch: true,
            idFunc: Quiz => Quiz.Id,
            searchFunc: async filter =>
            {
                var quizFilter = filter.Adapt<SearchQuizsRequest>();

                // quizFilter.QuizType = SearchQuizTypeId == default ? null : SearchQuizTypeId;
                // quizFilter.QuizTopic = SearchQuizTopic == default ? null : SearchQuizTopic;
                // quizFilter.QuizType = SearchQuizTypeId;
                // quizFilter.QuizTopic = SearchQuizTopic;

                var result = await QuizsClient.SearchAsync(quizFilter);
                return result.Adapt<PaginationResponse<QuizDto>>();
            },

            createFunc: async Quiz =>
            {
                if (!string.IsNullOrEmpty(Quiz.QuizInBytes))
                {
                    Quiz.QuizMedia = new FileUploadRequest()
                    {
                        Data = Quiz.QuizInBytes,
                        Extension = Quiz.QuizExtension ?? string.Empty,
                        Name = $"{Quiz.Name}_{Guid.NewGuid():N}"
                    };
                }

                await QuizsClient.CreateAsync(Quiz.Adapt<CreateQuizRequest>());
                Quiz.QuizInBytes = string.Empty;
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
                        Name = $"{quiz.Name}_{Guid.NewGuid():N}"
                    };
                }

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
             });

    // Advanced Search
    private QuizType _searchQuizTypeId;
    private QuizType SearchQuizTypeId
    {
        get => _searchQuizTypeId;
        set
        {
            _searchQuizTypeId = value;
            _ = _table?.ReloadDataAsync();
        }
    }

    private QuizTopic _searchQuizTopic;
    private QuizTopic SearchQuizTopic
    {
        get => _searchQuizTopic;
        set
        {
            _searchQuizTopic = value;
            _ = _table?.ReloadDataAsync();
        }
    }

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

}

public class QuizViewModel : UpdateQuizRequest
{
    public string? QuizPath { get; set; }
    public string? QuizInBytes { get; set; }
    public string? QuizExtension { get; set; }
}
