﻿using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.Geo;

public class GeoAdminUnitAutocomplete : MudAutocomplete<Guid>
{
    [Inject]
    private IStringLocalizer<GeoAdminUnitAutocomplete> L { get; set; } = default!;
    [Inject]
    private IGeoAdminUnitsClient GeoAdminUnitsClient { get; set; } = default!;
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private List<GeoAdminUnitDto> _entityList = new();

    // supply default parameters, but leave the possibility to override them

    [Parameter]
    public GeoAdminUnitType UnitType { get; set; }

    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = L["GeoAdminUnit"];
        Variant = Variant.Filled;
        Dense = true;
        Margin = Margin.None;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchGeoAdminUnits;
        ToStringFunc = GetGeoAdminUnitName;
        Clearable = true;
        return base.SetParametersAsync(parameters);
    }

    // when the value parameter is set, we have to load that one GeoAdminUnit to be able to show the name
    // we can't do that in OnInitialized because of a strange bug (https://github.com/MudBlazor/MudBlazor/issues/3818)
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender &&
            _value != default &&
            await ApiHelper.ExecuteCallGuardedAsync(
                () => GeoAdminUnitsClient.GetAsync(_value), Snackbar) is { } entityDetailsDto)
        {
            var entity = new GeoAdminUnitDto
            {
                Id = entityDetailsDto.Id,
                Code = entityDetailsDto.Code,
                Name = entityDetailsDto.Name
            };
            _entityList.Add(entity);
            ForceRender(true);
        }
    }

    private async Task<IEnumerable<Guid>> SearchGeoAdminUnits(string value)
    {
        var filter = new SearchGeoAdminUnitsRequest
        {
            Type = UnitType,
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "name" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => GeoAdminUnitsClient.SearchAsync(filter), Snackbar)
            is PaginationResponseOfGeoAdminUnitDto response)
        {
            _entityList = response.Data.ToList();
        }

        return _entityList.Select(x => x.Id);
    }

    private string GetGeoAdminUnitName(Guid id) =>
        _entityList.Find(e => e.Id == id)?.Name ?? string.Empty;
}
