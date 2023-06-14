using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Organization;

public partial class Teams
{
    protected EntityServerTableContext<TeamDto, Guid, UpdateTeamRequest> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["Team"],
            entityNamePlural: L["Teams"],
            entityResource: FSHResource.Teams,
            fields: new()
            {
                // new(Team => Team.Id, L["Id"], "Id"),
                new(Team => Team.Order, L["Order"], "Order"),
                new(Team => Team.Code, L["Code"], "Code"),
                new(Team => Team.Name, L["Name"], "Name"),
                new(Team => Team.Description, L["Description"], "Description"),

                new(Team => Team.SubDepartmentName, L["SubDepartment"], "SubDepartment.Name"),
                new(Team => Team.IsActive, L["Active"], Type: typeof(bool)),

            },
            idFunc: Team => Team.Id,
            searchFunc: async filter => (await TeamsClient
                .SearchAsync(filter.Adapt<SearchTeamsRequest>()))
                .Adapt<PaginationResponse<TeamDto>>(),
            createFunc: async Team => await TeamsClient.CreateAsync(Team.Adapt<CreateTeamRequest>()),
            updateFunc: async (id, Team) => await TeamsClient.UpdateAsync(id, Team),
            deleteFunc: async id => await TeamsClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportTeamsRequest>();
                return await TeamsClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportTeamsRequest() { ExcelFile = FileUploadRequest };
                await TeamsClient.ImportAsync(request);
            }
            );
}