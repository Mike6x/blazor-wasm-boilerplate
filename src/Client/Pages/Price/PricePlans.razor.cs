using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Price;

public partial class PricePlans
{
    protected EntityServerTableContext<PricePlanDto, Guid, UpdatePricePlanRequest> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["PricePlan"],
            entityNamePlural: L["PricePlans"],
            entityResource: FSHResource.PricePlans,
            fields: new()
            {
                // new(PricePlan => PricePlan.Id, L["Id"], "Id"),
                new(PricePlan => PricePlan.Order, L["Order"], "Order"),
                new(PricePlan => PricePlan.Code, L["Code"], "Code"),

                new(PricePlan => PricePlan.PriceGroupName, L["PriceGroup"], "PriceGroup.Name"),
                new(PricePlan => PricePlan.ProductName, L["Product"], "Product.Name"),
                new(PricePlan => PricePlan.PackOfMea, L["PackOfMea"], "PackOfMea"),
                new(PricePlan => PricePlan.PackQty, L["PackQty"], "PackQty"),
                new(PricePlan => PricePlan.UnitPrice, L["Price"], "UnitPrice"),
                new(PricePlan => PricePlan.ListPrice, L["ListPrice"], "ListPrice"),
                new(PricePlan => PricePlan.IsActive, L["Active"], Type: typeof(bool)),
            },
            idFunc: PricePlan => PricePlan.Id,
            searchFunc: async filter => (await PricePlansClient
                .SearchAsync(filter.Adapt<SearchPricePlansRequest>()))
                .Adapt<PaginationResponse<PricePlanDto>>(),
            createFunc: async PricePlan => await PricePlansClient.CreateAsync(PricePlan.Adapt<CreatePricePlanRequest>()),
            updateFunc: async (id, PricePlan) => await PricePlansClient.UpdateAsync(id, PricePlan),
            deleteFunc: async id => await PricePlansClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportPricePlansRequest>();
                return await PricePlansClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportPricePlansRequest() { ExcelFile = FileUploadRequest };
                await PricePlansClient.ImportAsync(request);
            }
            );
}