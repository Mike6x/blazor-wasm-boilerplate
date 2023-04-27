using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Catalog;

public partial class GroupCategories
{
    protected EntityServerTableContext<GroupCategorieDto, Guid, UpdateGroupCategorieRequest> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["GroupCategorie"],
            entityNamePlural: L["GroupCategories"],
            entityResource: FSHResource.GroupCategories,
            fields: new()
            {
                // new(GroupCategorie => GroupCategorie.Id, L["Id"], "Id"),
                new(GroupCategorie => GroupCategorie.Order, L["Order"], "Order"),
                new(GroupCategorie => GroupCategorie.Code, L["Code"], "Code"),
                new(GroupCategorie => GroupCategorie.Name, L["Name"], "Name"),
                new(GroupCategorie => GroupCategorie.Description, L["Description"], "Description"),

                new(GroupCategorie => GroupCategorie.BusinessLineName, L["BusinessLineName"], "BusinessLineName"),
                new(GroupCategorie => GroupCategorie.BusinessLineId, L["BusinessLineId"], "BusinessLineId"),

                new(GroupCategorie => GroupCategorie.Type, L["Type"], "Type" ),
                new(GroupCategorie => GroupCategorie.IsActive, L["Active"], Type: typeof(bool)),
            },
            idFunc: GroupCategorie => GroupCategorie.Id,
            searchFunc: async filter => (await GroupCategoriesClient
                .SearchAsync(filter.Adapt<SearchGroupCategoriesRequest>()))
                .Adapt<PaginationResponse<GroupCategorieDto>>(),
            createFunc: async GroupCategorie => await GroupCategoriesClient.CreateAsync(GroupCategorie.Adapt<CreateGroupCategorieRequest>()),
            updateFunc: async (id, GroupCategorie) => await GroupCategoriesClient.UpdateAsync(id, GroupCategorie),
            deleteFunc: async id => await GroupCategoriesClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportGroupCategoriesRequest>();
                return await GroupCategoriesClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportGroupCategoriesRequest() { ExcelFile = FileUploadRequest };
                await GroupCategoriesClient.ImportAsync(request);
            }
            );
}