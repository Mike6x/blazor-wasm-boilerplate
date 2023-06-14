using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Geo;

public partial class Countries
{
    protected EntityServerTableContext<CountryDto, Guid, UpdateCountryRequest> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["Country"],
            entityNamePlural: L["Countries"],
            entityResource: FSHResource.Countries,
            fields: new()
            {
                new(Country => Country.Order, L["Order"], "Order"),
                new(Country => Country.NumericCode, L["Numeric"], "NumericCode"),
                new(Country => Country.Code, L["Code"], "Code"),
                new(Country => Country.Name, L["Name"], "Name"),
                new(Country => Country.FullName, L["Formal Name"], "FullName"),
                new(Country => Country.NativeName, L["Navtive Name"], "NativeName"),
                new(Country => Country.ContinentName, L["Continent"], "Continent.Name"),

                // new(Country => Country.FullNativeName, L["Full Native"], "FullNativeName"),
                // (Country => Country.Description, L["Description"], "Description"),
            },
            idFunc: Country => Country.Id,
            searchFunc: async filter => (await CountriesClient
                .SearchAsync(filter.Adapt<SearchCountriesRequest>()))
                .Adapt<PaginationResponse<CountryDto>>(),
            createFunc: async Country => await CountriesClient.CreateAsync(Country.Adapt<CreateCountryRequest>()),
            updateFunc: async (id, Country) => await CountriesClient.UpdateAsync(id, Country),
            deleteFunc: async id => await CountriesClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportCountriesRequest>();
                return await CountriesClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportCountriesRequest() { ExcelFile = FileUploadRequest };
                await CountriesClient.ImportAsync(request);
            }
            );
}