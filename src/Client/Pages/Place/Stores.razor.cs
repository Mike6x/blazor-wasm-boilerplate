using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Pages.Production;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Place;

public partial class Stores
{
    private Guid _countryId;
    public Guid CountryId
    {
        get => _countryId;
        set
        {
            if (_countryId != value)
            {
                _countryId = value;
                _stateId = Guid.Empty;

                // _stateId = _provinceId = _districtId = _wardId = Guid.Empty;
                // Context.AddEditModal.RequestModel.ProvinceId = Guid.Empty;
                // Context.AddEditModal.RequestModel.DistrictId = Guid.Empty;
                // Context.AddEditModal.RequestModel.WardId = Guid.Empty;

                Context.AddEditModal.ForceRender();
            }
        }
    }

    private Guid _stateId;
    public Guid StateId
    {
        get => _stateId;
        set
        {
            if (_stateId != value)
            {
                _stateId = value;

                // _provinceId = _districtId = _wardId = Guid.Empty;
                // Context.AddEditModal.RequestModel.ProvinceId = Guid.Empty;
                // Context.AddEditModal.RequestModel.DistrictId = Guid.Empty;
                // Context.AddEditModal.RequestModel.WardId = Guid.Empty;

                Context.AddEditModal.ForceRender();
            }
        }
    }

    private Guid _provinceId;
    public Guid ProvinceId
    {
        get
        {
            _provinceId = Context.AddEditModal.RequestModel.ProvinceId;
            return _provinceId;
        }
        set
        {
            if (value != Guid.Empty && value != _provinceId)
            {
                _provinceId = value;
                _districtId = _wardId = Guid.Empty;

                Context.AddEditModal.RequestModel.ProvinceId = value;
                Context.AddEditModal.RequestModel.DistrictId = Guid.Empty;
                Context.AddEditModal.RequestModel.WardId = Guid.Empty;

                Context.AddEditModal.ForceRender();
            }
        }
    }

    private Guid _districtId;
    public Guid DistrictId
    {
        get
        {
            _districtId = Context.AddEditModal.RequestModel.DistrictId;
            return _districtId;
        }
        set
        {
            if (value != _districtId)
            {

                _districtId = value;
                _wardId = Guid.Empty;

                Context.AddEditModal.RequestModel.DistrictId = value;
                Context.AddEditModal.RequestModel.WardId = Guid.Empty;

                Context.AddEditModal.ForceRender();
            }
        }
    }

    private Guid _wardId;
    public Guid WardId
    {
        get
        {
            _wardId = Context.AddEditModal.RequestModel.WardId;
            return _wardId;
        }
        set
        {
            if (value != _wardId)
            {
                _wardId = value;
                Context.AddEditModal.RequestModel.WardId = value;
            }
        }
    }

    protected EntityServerTableContext<StoreDto, Guid, UpdateStoreRequest> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["Store"],
            entityNamePlural: L["Stores"],
            entityResource: FSHResource.Stores,
            fields: new()
            {
                // new(Store => Store.Id, L["Id"], "Id"),
                new(Store => Store.Order, L["Order"], "Order"),
                new(Store => Store.Code, L["Code"], "Code"),
                new(Store => Store.Name, L["Name"], "Name"),
                new(Store => Store.Description, L["Description"], "Description"),

                new(Store => Store.RetailerName, L["Retailer"], "Retailer.Name"),

                new(Store => Store.IsActive, L["Active"], Type: typeof(bool)),
            },
            idFunc: Store => Store.Id,
            searchFunc: async filter => (await StoresClient
                .SearchAsync(filter.Adapt<SearchStoresRequest>()))
                .Adapt<PaginationResponse<StoreDto>>(),
            createFunc: async Store => await StoresClient.CreateAsync(Store.Adapt<CreateStoreRequest>()),
            updateFunc: async (id, Store) => await StoresClient.UpdateAsync(id, Store),
            deleteFunc: async id => await StoresClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportStoresRequest>();
                return await StoresClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportStoresRequest() { ExcelFile = FileUploadRequest };
                await StoresClient.ImportAsync(request);
            }
            );
}