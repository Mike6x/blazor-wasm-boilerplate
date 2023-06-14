using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Organization;

public partial class SubDepartments
{
    protected EntityServerTableContext<SubDepartmentDto, Guid, UpdateSubDepartmentRequest> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["SubDepartment"],
            entityNamePlural: L["SubDepartments"],
            entityResource: FSHResource.SubDepartments,
            fields: new()
            {
                // new(SubDepartment => SubDepartment.Id, L["Id"], "Id"),
                new(SubDepartment => SubDepartment.Order, L["Order"], "Order"),
                new(SubDepartment => SubDepartment.Code, L["Code"], "Code"),
                new(SubDepartment => SubDepartment.Name, L["Name"], "Name"),
                new(SubDepartment => SubDepartment.Description, L["Description"], "Description"),

                new(SubDepartment => SubDepartment.DepartmentName, L["Department"], "Department.Name"),
                new(SubDepartment => SubDepartment.IsActive, L["Active"], Type: typeof(bool)),
            },
            idFunc: SubDepartment => SubDepartment.Id,
            searchFunc: async filter => (await SubDepartmentsClient
                .SearchAsync(filter.Adapt<SearchSubDepartmentsRequest>()))
                .Adapt<PaginationResponse<SubDepartmentDto>>(),
            createFunc: async SubDepartment => await SubDepartmentsClient.CreateAsync(SubDepartment.Adapt<CreateSubDepartmentRequest>()),
            updateFunc: async (id, SubDepartment) => await SubDepartmentsClient.UpdateAsync(id, SubDepartment),
            deleteFunc: async id => await SubDepartmentsClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportSubDepartmentsRequest>();
                return await SubDepartmentsClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportSubDepartmentsRequest() { ExcelFile = FileUploadRequest };
                await SubDepartmentsClient.ImportAsync(request);
            }
            );
}