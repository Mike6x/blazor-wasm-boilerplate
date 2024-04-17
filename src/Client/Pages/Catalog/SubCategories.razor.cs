using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Catalog;

public partial class SubCategories
{
    protected EntityServerTableContext<SubCategorieDto, Guid, UpdateSubCategorieRequest> Context { get; set; } = default!;
    private EntityTable<SubCategorieDto, Guid, UpdateSubCategorieRequest>? _table;
    protected override void OnInitialized() =>
        Context = new(
            fields:
            [

                // new(SubCategorie => SubCategorie.Id, L["Id"], "Id"),
                new(SubCategorie => SubCategorie.Order, L["Order"], "Order"),
                new(SubCategorie => SubCategorie.Code, L["Code"], "Code"),
                new(SubCategorie => SubCategorie.Name, L["Name"], "Name"),
                new(SubCategorie => SubCategorie.Description, L["Description"], "Description"),

                new(SubCategorie => SubCategorie.CategorieName, L["Categorie"], "Categorie.Name"),
                new(SubCategorie => SubCategorie.Type, L["Type"], "Type" ),
                new(SubCategorie => SubCategorie.IsActive, L["Active"], Type: typeof(bool)),
            ],
            searchFunc: async filter =>
            {
                var searchFilter = filter.Adapt<SearchSubCategoriesRequest>();
                searchFilter.CategorieId = SearchCategorieId == default ? null : SearchCategorieId;
                searchFilter.Type = SearchCatalogType == default ? null : SearchCatalogType;

                var result = await SubCategoriesClient.SearchAsync(searchFilter);
                return result.Adapt<PaginationResponse<SubCategorieDto>>();
            },
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportSubCategoriesRequest>();
                exportFilter.CategorieId = SearchCategorieId == default ? null : SearchCategorieId;
                exportFilter.Type = SearchCatalogType == default ? null : SearchCatalogType;

                return await SubCategoriesClient.ExportAsync(exportFilter);
            },
            enableAdvancedSearch: true,
            idFunc: SubCategorie => SubCategorie.Id,
            createFunc: async SubCategorie => await SubCategoriesClient.CreateAsync(SubCategorie.Adapt<CreateSubCategorieRequest>()),
            updateFunc: async (id, SubCategorie) => await SubCategoriesClient.UpdateAsync(id, SubCategorie),
            deleteFunc: async id => await SubCategoriesClient.DeleteAsync(id),
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportSubCategoriesRequest() { ExcelFile = FileUploadRequest };
                await SubCategoriesClient.ImportAsync(request);
            },
            entityName: L["SubCategorie"],
            entityNamePlural: L["SubCategories"],
            entityResource: FSHResource.SubCategories);

    #region Advanced Search

    private Guid _searchCategorieId;
    private Guid SearchCategorieId
    {
        get => _searchCategorieId;
        set
        {
            _searchCategorieId = value;
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