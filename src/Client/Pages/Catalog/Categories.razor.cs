using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Catalog;

public partial class Categories
{
    protected EntityServerTableContext<CategorieDto, Guid, UpdateCategorieRequest> Context { get; set; } = default!;
    private EntityTable<CategorieDto, Guid, UpdateCategorieRequest>? _table;
    protected override void OnInitialized() =>
        Context = new(
            fields:
            [

                // new(Categorie => Categorie.Id, L["Id"], "Id"),
                new(Categorie => Categorie.Order, L["Order"], "Order"),
                new(Categorie => Categorie.Code, L["Code"], "Code"),
                new(Categorie => Categorie.Name, L["Name"], "Name"),
                new(Categorie => Categorie.Description, L["Description"], "Description"),

                new(Categorie => Categorie.GroupCategorieName, L["GroupCategorie"], "GroupCategorie.Name"),

                new(Categorie => Categorie.Type, L["Type"], "Type" ),
                new(Categorie => Categorie.IsActive, L["Active"], Type: typeof(bool)),
            ],
            searchFunc: async filter =>
            {
                var searchFilter = filter.Adapt<SearchCategoriesRequest>();
                searchFilter.GroupCategorieId = SearchGroupCategorieId == default ? null : SearchGroupCategorieId;
                searchFilter.Type = SearchCatalogType == default ? null : SearchCatalogType;

                var result = await CategoriesClient.SearchAsync(searchFilter);
                return result.Adapt<PaginationResponse<CategorieDto>>();
            },
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportCategoriesRequest>();
                exportFilter.GroupCategorieId = SearchGroupCategorieId == default ? null : SearchGroupCategorieId;
                exportFilter.Type = SearchCatalogType == default ? null : SearchCatalogType;

                return await CategoriesClient.ExportAsync(exportFilter);
            },
            enableAdvancedSearch: true,
            idFunc: Categorie => Categorie.Id,
            createFunc: async Categorie => await CategoriesClient.CreateAsync(Categorie.Adapt<CreateCategorieRequest>()),
            updateFunc: async (id, Categorie) => await CategoriesClient.UpdateAsync(id, Categorie),
            deleteFunc: async id => await CategoriesClient.DeleteAsync(id),
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportCategoriesRequest() { ExcelFile = FileUploadRequest };
                await CategoriesClient.ImportAsync(request);
            },
            entityName: L["Categorie"],
            entityNamePlural: L["Categories"],
            entityResource: FSHResource.Categories);

    #region Advanced Search

    private Guid _searchGroupCategorieId;
    private Guid SearchGroupCategorieId
    {
        get => _searchGroupCategorieId;
        set
        {
            _searchGroupCategorieId = value;
            _ = _table?.ReloadDataAsync();
        }
    }

    private CatalogType _searchCatalogType = CatalogType.All;
    private CatalogType SearchCatalogType
    {
        get => _searchCatalogType;
        set
        {
            _searchCatalogType = value;
            _ = _table?.ReloadDataAsync();
        }
    }

    #endregion
}