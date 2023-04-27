using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Catalog;

public partial class SubCategories
{
    protected EntityServerTableContext<SubCategorieDto, Guid, UpdateSubCategorieRequest> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["SubCategorie"],
            entityNamePlural: L["SubCategories"],
            entityResource: FSHResource.SubCategories,
            fields: new()
            {
                // new(SubCategorie => SubCategorie.Id, L["Id"], "Id"),
                new(SubCategorie => SubCategorie.Order, L["Order"], "Order"),
                new(SubCategorie => SubCategorie.Code, L["Code"], "Code"),
                new(SubCategorie => SubCategorie.Name, L["Name"], "Name"),
                new(SubCategorie => SubCategorie.Description, L["Description"], "Description"),

                new(SubCategorie => SubCategorie.CategorieName, L["CategorieName"], "CategorieName"),
                new(SubCategorie => SubCategorie.CategorieId, L["CategorieId"], "CategorieId"),

                new(SubCategorie => SubCategorie.Type, L["Type"], "Type" ),
                new(SubCategorie => SubCategorie.IsActive, L["Active"], Type: typeof(bool)),
            },
            idFunc: SubCategorie => SubCategorie.Id,
            searchFunc: async filter => (await SubCategoriesClient
                .SearchAsync(filter.Adapt<SearchSubCategoriesRequest>()))
                .Adapt<PaginationResponse<SubCategorieDto>>(),
            createFunc: async SubCategorie => await SubCategoriesClient.CreateAsync(SubCategorie.Adapt<CreateSubCategorieRequest>()),
            updateFunc: async (id, SubCategorie) => await SubCategoriesClient.UpdateAsync(id, SubCategorie),
            deleteFunc: async id => await SubCategoriesClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportSubCategoriesRequest>();
                return await SubCategoriesClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportSubCategoriesRequest() { ExcelFile = FileUploadRequest };
                await SubCategoriesClient.ImportAsync(request);
            }
            );
}