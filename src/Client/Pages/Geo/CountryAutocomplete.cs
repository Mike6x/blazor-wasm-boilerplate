﻿using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.Geo;

public class CountryAutocomplete : MudAutocomplete<Guid>
{
    [Inject]
    private IStringLocalizer<CountryAutocomplete> L { get; set; } = default!;
    [Inject]
    private ICountriesClient CountriesClient { get; set; } = default!;
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private List<CountryDto> _entityList = new();

    // supply default parameters, but leave the possibility to override them
    [Parameter]
    public Guid ContinentIdStr { get; set; }
    [Parameter]
    public Guid SubContinentIdStr { get; set; }

    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = L["Country"];
        Variant = Variant.Filled;
        Dense = true;
        Margin = Margin.None;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchCountries;
        ToStringFunc = GetCountryName;
        Clearable = true;
        return base.SetParametersAsync(parameters);
    }

    // when the value parameter is set, we have to load that one Country to be able to show the name
    // we can't do that in OnInitialized because of a strange bug (https://github.com/MudBlazor/MudBlazor/issues/3818)
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender &&
            _value != default &&
            await ApiHelper.ExecuteCallGuardedAsync(
                () => CountriesClient.GetAsync(_value), Snackbar) is { } entityDetailsDto)
        {
            var entity = new CountryDto
            {
                Id = entityDetailsDto.Id,
                Code = entityDetailsDto.Code,
                Name = entityDetailsDto.Name
            };
            _entityList.Add(entity);
            ForceRender(true);
        }
    }

    private async Task<IEnumerable<Guid>> SearchCountries(string value)
    {
        var filter = new SearchCountriesRequest
        {
            ContinentId = ContinentIdStr == Guid.Empty ? null : ContinentIdStr,
            SubContinentId = SubContinentIdStr == Guid.Empty ? null : SubContinentIdStr,

            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "name" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => CountriesClient.SearchAsync(filter), Snackbar)
            is PaginationResponseOfCountryDto response)
        {
            _entityList = response.Data.ToList();
        }

        return _entityList.Select(x => x.Id);
    }

    private string GetCountryName(Guid id) =>
        _entityList.Find(e => e.Id == id)?.Name ?? string.Empty;
}
