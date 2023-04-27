using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Catalog;

public partial class Categories
{
    protected EntityServerTableContext<CategorieDto, Guid, UpdateCategorieRequest> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["Categorie"],
            entityNamePlural: L["Categories"],
            entityResource: FSHResource.Categories,
            fields: new()
            {
                // new(Categorie => Categorie.Id, L["Id"], "Id"),
                new(Categorie => Categorie.Order, L["Order"], "Order"),
                new(Categorie => Categorie.Code, L["Code"], "Code"),
                new(Categorie => Categorie.Name, L["Name"], "Name"),
                new(Categorie => Categorie.Description, L["Description"], "Description"),

                new(Categorie => Categorie.GroupCategorieName, L["GroupCategorieName"], "GroupCategorieName"),
                new(Categorie => Categorie.GroupCategorieId, L["GroupCategorieId"], "GroupCategorieId"),

                new(Categorie => Categorie.Type, L["Type"], "Type" ),
                new(Categorie => Categorie.IsActive, L["Active"], Type: typeof(bool)),
            },
            idFunc: Categorie => Categorie.Id,
            searchFunc: async filter => (await CategoriesClient
                .SearchAsync(filter.Adapt<SearchCategoriesRequest>()))
                .Adapt<PaginationResponse<CategorieDto>>(),
            createFunc: async Categorie => await CategoriesClient.CreateAsync(Categorie.Adapt<CreateCategorieRequest>()),
            updateFunc: async (id, Categorie) => await CategoriesClient.UpdateAsync(id, Categorie),
            deleteFunc: async id => await CategoriesClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportCategoriesRequest>();
                return await CategoriesClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportCategoriesRequest() { ExcelFile = FileUploadRequest };
                await CategoriesClient.ImportAsync(request);
            }
            );
}