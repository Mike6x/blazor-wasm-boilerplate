using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;
using Microsoft.AspNetCore.Components;

namespace FSH.BlazorWebAssembly.Client.Pages.Elearning;

public partial class QuizResults
{
    [Inject]
    protected IQuizResultsClient QuizResultsClient { get; set; } = default!;
    [Inject]
    protected IQuizsClient QuizsClient { get; set; } = default!;

    protected EntityServerTableContext<QuizResultDto, Guid, QuizResultViewModel> Context { get; set; } = default!;

    private EntityTable<QuizResultDto, Guid, QuizResultViewModel>? _table;

    protected override void OnInitialized() =>
    Context = new(
        entityName: L["QuizResult"],
        entityNamePlural: L["QuizResults"],
        entityResource: FSHResource.QuizResults,
        fields: new()
        {
                new(QuizResult => QuizResult.Id, L["Id"], "Id"),

                new(QuizResult => QuizResult.QuizName, L["Quiz Name"], "Quiz Name"),
                new(QuizResult => QuizResult.T, L["QuizType"], "Quiz Type"),

                new(QuizResult => QuizResult.Sp, L["Student Point"], "Student Point"),
                new(QuizResult => QuizResult.Ut, L["Used Time"], "Used Time"),

                new(QuizResult => QuizResult.Tp, L["Total Score"], "Total Score"),
                new(QuizResult => QuizResult.Ps, L["Passing Score"], "Passing Score"),
                new(QuizResult => QuizResult.Psp, L["%"], "%"),
                new(QuizResult => QuizResult.Tl, L["Time Limit"], "Time Limit"),

        },
        enableAdvancedSearch: true,
        idFunc: QuizResult => QuizResult.Id,
        searchFunc: async filter =>
        {
            var quizResultFilter = filter.Adapt<SearchQuizResultsRequest>();

            quizResultFilter.QuizId = SearchQuizId == default ? null : SearchQuizId;

            var result = await QuizResultsClient.SearchAsync(quizResultFilter);
            return result.Adapt<PaginationResponse<QuizResultDto>>();
        },

        createFunc: async QuizResult =>
        {
            await QuizResultsClient.CreateAsync(QuizResult.Adapt<CreateQuizResultRequest>());
        },

        updateFunc: async (id, QuizResult) =>
        {
            await QuizResultsClient.UpdateAsync(id, QuizResult.Adapt<UpdateQuizResultRequest>());
        },

        deleteFunc: async id => await QuizResultsClient.DeleteAsync(id),

        exportFunc: async filter =>
        {
            var exportFilter = filter.Adapt<ExportQuizResultsRequest>();

            exportFilter.QuizId = SearchQuizId == default ? null : SearchQuizId;

            return await QuizResultsClient.ExportAsync(exportFilter);
        });

    // Advanced Search
    private Guid _searchQuizId;
    private Guid SearchQuizId
    {
        get => _searchQuizId;
        set
        {
            _searchQuizId = value;
            _ = _table?.ReloadDataAsync();
        }
    }
}

public class QuizResultViewModel : UpdateQuizResultRequest
{
}