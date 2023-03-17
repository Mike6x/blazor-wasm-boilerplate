using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Purchase;

public partial class Vendors
{
    protected EntityServerTableContext<VendorDto, Guid, UpdateVendorRequest> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["Vendor"],
            entityNamePlural: L["Vendors"],
            entityResource: FSHResource.Vendors,
            fields: new()
            {
                // new(Vendor => Vendor.Id, L["Id"], "Id"),
                new(Vendor => Vendor.Code, L["Code"], "Code"),
                new(Vendor => Vendor.Name, L["Name"], "Name"),
                new(Vendor => Vendor.ContactPerson, L["Contact"], "Contact"),
                new(Vendor => Vendor.Phone, L["Phone"], "Phone"),
                new(Vendor => Vendor.Email, L["Email"], "Email"),
                new(Vendor => Vendor.TaxCode, L["TaxCode"], "TaxCode"),

                // new(Vendor => Vendor.Description, L["Description"], "Description"),
                new(Vendor => Vendor.IsActive, L["Active"], Type: typeof(bool)),
            },
            idFunc: Vendor => Vendor.Id,
            searchFunc: async filter => (await VendorsClient
                .SearchAsync(filter.Adapt<SearchVendorsRequest>()))
                .Adapt<PaginationResponse<VendorDto>>(),
            createFunc: async Vendor => await VendorsClient.CreateAsync(Vendor.Adapt<CreateVendorRequest>()),
            updateFunc: async (id, Vendor) => await VendorsClient.UpdateAsync(id, Vendor),
            deleteFunc: async id => await VendorsClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportVendorsRequest>();
                return await VendorsClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportVendorsRequest() { ExcelFile = FileUploadRequest };
                await VendorsClient.ImportAsync(request);
            }
            );
}