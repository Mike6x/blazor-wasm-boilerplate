using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Catalog;

public partial class BusinessLines
{
    protected EntityServerTableContext<BusinessLineDto, Guid, UpdateBusinessLineRequest> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["BusinessLine"],
            entityNamePlural: L["BusinessLines"],
            entityResource: FSHResource.BusinessLines,
            fields: new()
            {
                // new(BusinessLine => BusinessLine.Id, L["Id"], "Id"),
                new(BusinessLine => BusinessLine.Code, L["Code"], "Code"),
                new(BusinessLine => BusinessLine.Name, L["Name"], "Name"),
                new(BusinessLine => BusinessLine.Description, L["Description"], "Description"),
                new(BusinessLine => BusinessLine.IsActive, L["Active"], Type: typeof(bool)),
            },
            idFunc: BusinessLine => BusinessLine.Id,
            searchFunc: async filter => (await BusinessLinesClient
                .SearchAsync(filter.Adapt<SearchBusinessLinesRequest>()))
                .Adapt<PaginationResponse<BusinessLineDto>>(),
            createFunc: async BusinessLine => await BusinessLinesClient.CreateAsync(BusinessLine.Adapt<CreateBusinessLineRequest>()),
            updateFunc: async (id, BusinessLine) => await BusinessLinesClient.UpdateAsync(id, BusinessLine),
            deleteFunc: async id => await BusinessLinesClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportBusinessLinesRequest>();
                return await BusinessLinesClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportBusinessLinesRequest() { ExcelFile = FileUploadRequest };
                await BusinessLinesClient.ImportAsync(request);
            }
            );
}