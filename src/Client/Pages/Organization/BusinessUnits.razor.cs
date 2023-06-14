using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Organization;

public partial class BusinessUnits
{
    protected EntityServerTableContext<BusinessUnitDto, Guid, UpdateBusinessUnitRequest> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["BusinessUnit"],
            entityNamePlural: L["BusinessUnits"],
            entityResource: FSHResource.BusinessUnits,
            fields: new()
            {
                // new(BusinessUnit => BusinessUnit.Id, L["Id"], "Id"),
                new(BusinessUnit => BusinessUnit.Order, L["Order"], "Order"),
                new(BusinessUnit => BusinessUnit.Code, L["Code"], "Code"),
                new(BusinessUnit => BusinessUnit.Name, L["Name"], "Name"),
                new(BusinessUnit => BusinessUnit.Description, L["Description"], "Description"),
                new(BusinessUnit => BusinessUnit.IsActive, L["Active"], Type: typeof(bool)),
            },
            idFunc: BusinessUnit => BusinessUnit.Id,
            searchFunc: async filter => (await BusinessUnitsClient
                .SearchAsync(filter.Adapt<SearchBusinessUnitsRequest>()))
                .Adapt<PaginationResponse<BusinessUnitDto>>(),
            createFunc: async BusinessUnit => await BusinessUnitsClient.CreateAsync(BusinessUnit.Adapt<CreateBusinessUnitRequest>()),
            updateFunc: async (id, BusinessUnit) => await BusinessUnitsClient.UpdateAsync(id, BusinessUnit),
            deleteFunc: async id => await BusinessUnitsClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportBusinessUnitsRequest>();
                return await BusinessUnitsClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportBusinessUnitsRequest() { ExcelFile = FileUploadRequest };
                await BusinessUnitsClient.ImportAsync(request);
            }
            );
}