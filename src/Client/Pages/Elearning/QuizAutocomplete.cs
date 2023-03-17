using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Shared;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Reflection.Emit;

namespace FSH.BlazorWebAssembly.Client.Pages.Elearning;

public class QuizAutocomplete : MudAutocomplete<Guid>
{
    [Inject]
    private IStringLocalizer<QuizAutocomplete> L { get; set; } = default!;
    [Inject]
    private IQuizsClient QuizsClient { get; set; } = default!;
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private List<QuizDto> _quizs = new();

    // supply default parameters, but leave the possibility to override them
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = L["Quiz"];
        Variant = Variant.Filled;
        Dense = true;
        Margin = Margin.None;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchQuizs;
        ToStringFunc = GetQuizName;
        Clearable = true;
        return base.SetParametersAsync(parameters);
    }

    // when the value parameter is set, we have to load that one Quiz to be able to show the name
    // we can't do that in OnInitialized because of a strange bug (https://github.com/MudBlazor/MudBlazor/issues/3818)
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender &&
            _value != default &&
            await ApiHelper.ExecuteCallGuardedAsync(
                () => QuizsClient.GetAsync(_value), Snackbar) is { } quizDetailsDto)
        {
            var quiz = new QuizDto
            {
                Id = quizDetailsDto.Id,
                Code = quizDetailsDto.Code,
                Name = quizDetailsDto.Name
            };
            _quizs.Add(quiz);
            ForceRender(true);
        }
    }

    private async Task<IEnumerable<Guid>> SearchQuizs(string value)
    {
        var filter = new SearchQuizsRequest
        {
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "name" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => QuizsClient.SearchAsync(filter), Snackbar)
            is PaginationResponseOfQuizDto response)
        {
            _quizs = response.Data.ToList();
        }

        return _quizs.Select(x => x.Id);
    }

    private string GetQuizName(Guid id) =>
        _quizs.Find(e => e.Id == id)?.Name ?? string.Empty;
}
