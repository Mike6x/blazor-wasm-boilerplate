using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.People;

public partial class Titles
{
    protected EntityServerTableContext<TitleDto, Guid, UpdateTitleRequest> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["Title"],
            entityNamePlural: L["Titles"],
            entityResource: FSHResource.Titles,
            fields: new()
            {
                // new(Title => Title.Id, L["Id"], "Id"),
                new(Title => Title.Code, L["Code"], "Code"),
                new(Title => Title.Name, L["Name"], "Name"),
                new(Title => Title.Grade, L["Grade"], "Grade"),
                new(Title => Title.Description, L["Description"], "Description"),
                new(Title => Title.IsActive, L["Active"], Type: typeof(bool)),
            },
            idFunc: Title => Title.Id,
            searchFunc: async filter => (await TitlesClient
                .SearchAsync(filter.Adapt<SearchTitlesRequest>()))
                .Adapt<PaginationResponse<TitleDto>>(),
            createFunc: async Title => await TitlesClient.CreateAsync(Title.Adapt<CreateTitleRequest>()),
            updateFunc: async (id, Title) => await TitlesClient.UpdateAsync(id, Title),
            deleteFunc: async id => await TitlesClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportTitlesRequest>();
                return await TitlesClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportTitlesRequest() { ExcelFile = FileUploadRequest };
                await TitlesClient.ImportAsync(request);
            }
            );
}