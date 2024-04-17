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
    protected override void OnInitialized() =>
    Context = new(
        fields:
        [

                // new(QuizResult => QuizResult.Id, L["Id"], "Id"),
                new(QuizResult => QuizResult.QuizCode, L["Quiz Code"], "Quiz.Code"),
                new(QuizResult => QuizResult.QuizName, L["Quiz Name"], "Quiz.Name"),

                // new(QuizResult => QuizResult.T, L["Quiz Type"], "T"),

                new(QuizResult => QuizResult.SId, L["Student Id"], "SId"),
                new(QuizResult => QuizResult.Sp, L["Student Point"], "Sp"),

                new(QuizResult => QuizResult.StartTime.ToLocalTime(), L["StartTime"], "StartTime"),
                new(QuizResult => QuizResult.EndTime.ToLocalTime(), L["EndTime"], "EndTime"),
                new(QuizResult => QuizResult.Ut, L["Used Time in Seconds"], "Ut"),

                new(QuizResult => QuizResult.Tp, L["Total Score"], "Tp"),

                new(QuizResult => QuizResult.Ps, L["Passing Score"], "Ps"),
                new(QuizResult => QuizResult.Psp, L["Require Score in %"], "Psp"),
                new(QuizResult => QuizResult.Tl, L["Time Limit"], "Tl"),

                new(QuizResult => QuizResult.Rating, L["Rating"], "Rating"),
                new(QuizResult => QuizResult.IsPass, L["Passed"], Type: typeof(bool)),
        ],
        searchFunc: async filter =>
        {
            var searchFilter = filter.Adapt<SearchQuizResultsRequest>();

            searchFilter.QuizId = SearchQuizId == default ? null : SearchQuizId;

            var result = await QuizResultsClient.SearchAsync(searchFilter);
            return result.Adapt<PaginationResponse<QuizResultDto>>();
        },
        exportFunc: async filter =>
        {
            var exportFilter = filter.Adapt<ExportQuizResultsRequest>();

            exportFilter.QuizId = SearchQuizId == default ? null : SearchQuizId;

            return await QuizResultsClient.ExportAsync(exportFilter);
        },
        enableAdvancedSearch: true,
        idFunc: QuizResult => QuizResult.Id,

        createFunc: async QuizResult =>
        {
            await QuizResultsClient.CreateAsync(QuizResult.Adapt<CreateQuizResultRequest>());
        },

        updateFunc: async (id, QuizResult) =>
        {
            await QuizResultsClient.UpdateAsync(id, QuizResult.Adapt<UpdateQuizResultRequest>());
        },

        deleteFunc: async id => await QuizResultsClient.DeleteAsync(id),
        entityName: L["QuizResult"],
        entityNamePlural: L["QuizResults"],
        entityResource: FSHResource.QuizResults);

    #region Advanced Search

    private EntityTable<QuizResultDto, Guid, QuizResultViewModel>? _table;

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

    #endregion

    /// <summary>
    /// View Student info.
    /// </summary>
    /// <param name="userId"></param>
    private void ViewProfile(string userId)
    {
        // if (!string.IsNullOrEmpty(userId))
        Navigation.NavigateTo($"/users/{userId}/profile");
    }
}

public class QuizResultViewModel : UpdateQuizResultRequest
{
}