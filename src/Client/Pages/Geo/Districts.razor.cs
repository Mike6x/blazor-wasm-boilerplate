using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Geo;

public partial class Districts
{
    private Guid _countryId;
    public Guid CountryId
    {
        get => _countryId;
        set
        {
            if (value == Guid.Empty)
            {
                StateId = Guid.NewGuid();
            }

            _countryId = value;
            Context.AddEditModal.ForceRender();
        }
    }

    private Guid _stateId;
    public Guid StateId
    {
        get => _stateId;
        set
        {
            _stateId = value;
            Context.AddEditModal.ForceRender();
        }
    }

    protected EntityServerTableContext<DistrictDto, Guid, UpdateDistrictRequest> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["District"],
            entityNamePlural: L["Districts"],
            entityResource: FSHResource.Districts,
            fields: new()
            {
                new(District => District.Order, L["Order"], "Order"),
                new(District => District.NumericCode, L["Numeric"], "NumericCode"),
                new(District => District.Code, L["Code"], "Code"),
                new(District => District.Name, L["Name"], "Name"),

                // new(District => District.FullName, L["Formal Name"], "FullName"),
                new(District => District.TypeName, L["Type"], "Type" ),
                new(District => District.ProvinceName, L["City"], "City"),

                // new(District => District.NativeName, L["Navtive Name"], "NativeName"),

                new(District => District.FullNativeName, L["Full Native"], "FullNativeName"),

                // new(District => District.Description, L["Description"], "Description"),
            },
            idFunc: District => District.Id,
            searchFunc: async filter => (await DistrictsClient
                .SearchAsync(filter.Adapt<SearchDistrictsRequest>()))
                .Adapt<PaginationResponse<DistrictDto>>(),
            createFunc: async District => await DistrictsClient.CreateAsync(District.Adapt<CreateDistrictRequest>()),
            updateFunc: async (id, District) => await DistrictsClient.UpdateAsync(id, District),
            deleteFunc: async id => await DistrictsClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportDistrictsRequest>();
                return await DistrictsClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportDistrictsRequest() { ExcelFile = FileUploadRequest };
                await DistrictsClient.ImportAsync(request);
            }
            );
}