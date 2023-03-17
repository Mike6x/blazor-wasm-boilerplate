using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Geo;

public partial class GeoAdminUnits
{
    protected EntityServerTableContext<GeoAdminUnitDto, Guid, UpdateGeoAdminUnitRequest> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["GeoAdminUnit"],
            entityNamePlural: L["GeoAdminUnits"],
            entityResource: FSHResource.GeoAdminUnits,
            fields: new()
            {
                new(GeoAdminUnit => GeoAdminUnit.Order, L["Order"], "Order"),

                // new(GeoAdminUnit => GeoAdminUnit.Code, L["Code"], "Code"),
                new(GeoAdminUnit => GeoAdminUnit.Name, L["Name"], "Name"),
                new(GeoAdminUnit => GeoAdminUnit.FullName, L["Formal Name"], "FullName"),
                new(GeoAdminUnit => GeoAdminUnit.NativeName, L["Navtive Name"], "NativeName"),
                new(GeoAdminUnit => GeoAdminUnit.FullNativeName, L["Full Native"], "FullNativeName"),

                new(GeoAdminUnit => GeoAdminUnit.Type, L["Type"], "Type" ),
                new(GeoAdminUnit => GeoAdminUnit.Description, L["Description"], "Description"),

            },
            idFunc: GeoAdminUnit => GeoAdminUnit.Id,
            searchFunc: async filter => (await GeoAdminUnitsClient
                .SearchAsync(filter.Adapt<SearchGeoAdminUnitsRequest>()))
                .Adapt<PaginationResponse<GeoAdminUnitDto>>(),
            createFunc: async GeoAdminUnit => await GeoAdminUnitsClient.CreateAsync(GeoAdminUnit.Adapt<CreateGeoAdminUnitRequest>()),
            updateFunc: async (id, GeoAdminUnit) => await GeoAdminUnitsClient.UpdateAsync(id, GeoAdminUnit),
            deleteFunc: async id => await GeoAdminUnitsClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportGeoAdminUnitsRequest>();
                return await GeoAdminUnitsClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportGeoAdminUnitsRequest() { ExcelFile = FileUploadRequest };
                await GeoAdminUnitsClient.ImportAsync(request);
            }
            );
}