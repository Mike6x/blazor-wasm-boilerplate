using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Geo;

public partial class States
{
    private Guid _continentId;
    public Guid ContinentId
    {
        get => _continentId;
        set
        {
            _continentId = value;
            Context.AddEditModal.ForceRender();
        }
    }

    private Guid _subContinentId;
    public Guid SubContinentId
    {
        get => _subContinentId;
        set
        {
            _subContinentId = value;
            Context.AddEditModal.ForceRender();
        }
    }

    protected EntityServerTableContext<StateDto, Guid, UpdateStateRequest> Context { get; set; } = default!;
    protected override void OnInitialized() =>
        Context = new(
            entityName: L["State"],
            entityNamePlural: L["States"],
            entityResource: FSHResource.States,
            fields: new()
            {
                new(State => State.Order, L["Order"], "Order"),
                new(State => State.NumericCode, L["Numeric"], "NumericCode"),
                new(State => State.Code, L["Code"], "Code"),
                new(State => State.Name, L["Name"], "Name"),

                // new(State => State.FullName, L["Formal Name"], "FullName"),
                new(State => State.NativeName, L["Navtive Name"], "NativeName"),

                // new(State => State.FullNativeName, L["Full Native"], "FullNativeName"),
                new(State => State.CountryName, L["Country"], "Country"),
                new(State => State.Metropolis, L["Metropolis"], "Metropolis"),

               // new(State => State.TypeName, L["Type"], "Type" ),
               // new(State => State.Description, L["Description"], "Description"),
            },
            idFunc: State => State.Id,
            searchFunc: async filter => (await StatesClient
                .SearchAsync(filter.Adapt<SearchStatesRequest>()))
                .Adapt<PaginationResponse<StateDto>>(),
            createFunc: async State => await StatesClient.CreateAsync(State.Adapt<CreateStateRequest>()),
            updateFunc: async (id, State) => await StatesClient.UpdateAsync(id, State),
            deleteFunc: async id => await StatesClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportStatesRequest>();
                return await StatesClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportStatesRequest() { ExcelFile = FileUploadRequest };
                await StatesClient.ImportAsync(request);
            }
        );
}