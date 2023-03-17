using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Organization;

public partial class Departments
{
    protected EntityServerTableContext<DepartmentDto, Guid, UpdateDepartmentRequest> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["Department"],
            entityNamePlural: L["Departments"],
            entityResource: FSHResource.Departments,
            fields: new()
            {
                // new(Department => Department.Id, L["Id"], "Id"),
                new(Department => Department.Code, L["Code"], "Code"),
                new(Department => Department.Name, L["Name"], "Name"),
                new(Department => Department.Description, L["Description"], "Description"),

                new(Department => Department.BusinessUnitName, L["BusinessUnit Name"], "BusinessUnit Name"),
                new(Department => Department.BusinessUnitId, L["BusinessUnitId"], "BusinessUnitId"),

                new(Department => Department.IsActive, L["Active"], Type: typeof(bool)),

            },
            idFunc: Department => Department.Id,
            searchFunc: async filter => (await DepartmentsClient
                .SearchAsync(filter.Adapt<SearchDepartmentsRequest>()))
                .Adapt<PaginationResponse<DepartmentDto>>(),
            createFunc: async Department => await DepartmentsClient.CreateAsync(Department.Adapt<CreateDepartmentRequest>()),
            updateFunc: async (id, Department) => await DepartmentsClient.UpdateAsync(id, Department),
            deleteFunc: async id => await DepartmentsClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportDepartmentsRequest>();
                return await DepartmentsClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportDepartmentsRequest() { ExcelFile = FileUploadRequest };
                await DepartmentsClient.ImportAsync(request);
            }
            );
}