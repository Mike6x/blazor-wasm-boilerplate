using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Geo;

public partial class Wards
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
                ProvinceId = Guid.NewGuid();
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
            if (value == Guid.Empty)
            {
                 ProvinceId = Guid.NewGuid();
            }

            _stateId = value;
            Context.AddEditModal.ForceRender();
        }
    }

    private Guid _provinceId;
    public Guid ProvinceId
    {
        get => _provinceId;
        set
        {
            _provinceId = value;
            Context.AddEditModal.ForceRender();
        }
    }

    protected EntityServerTableContext<WardDto, Guid, UpdateWardRequest> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["Ward"],
            entityNamePlural: L["Wards"],
            entityResource: FSHResource.Wards,
            fields: new()
            {
                new(Ward => Ward.Order, L["Order"], "Order"),
                new(Ward => Ward.NumericCode, L["Numeric"], "NumericCode"),
                new(Ward => Ward.Code, L["Code"], "Code"),
                new(Ward => Ward.Name, L["Name"], "Name"),
                new(Ward => Ward.TypeName, L["Type"], "Type.Name" ),
                new(Ward => Ward.DistrictName, L["District"], "District.Name"),

                new(Ward => Ward.NativeName, L["Navtive Name"], "NativeName"),
                new(Ward => Ward.TypeNativeName, L["Native Type"], "Type.NativeName" ),
                new(Ward => Ward.DistrictNativeName, L["Native District"], "District.NativeName"),

                // new(Ward => Ward.FullName, L["Formal Name"], "FullName"),
                // new(Ward => Ward.FullNativeName, L["Full Native"], "FullNativeName"),
                // new(Ward => Ward.Description, L["Description"], "Description"),
            },
            idFunc: Ward => Ward.Id,
            searchFunc: async filter => (await WardsClient
                .SearchAsync(filter.Adapt<SearchWardsRequest>()))
                .Adapt<PaginationResponse<WardDto>>(),
            createFunc: async Ward => await WardsClient.CreateAsync(Ward.Adapt<CreateWardRequest>()),
            updateFunc: async (id, Ward) => await WardsClient.UpdateAsync(id, Ward),
            deleteFunc: async id => await WardsClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportWardsRequest>();
                return await WardsClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportWardsRequest() { ExcelFile = FileUploadRequest };
                await WardsClient.ImportAsync(request);
            }
            );
}