using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Place;

public partial class Retailers
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

    protected EntityServerTableContext<RetailerDto, Guid, UpdateRetailerRequest> Context { get; set; } = default!;

    protected override void OnInitialized()
    {
        Context = new(
            entityName: L["Retailer"],
            entityNamePlural: L["Retailers"],
            entityResource: FSHResource.Retailers,
            fields: new()
            {
                // new(Retailer => Retailer.Id, L["Id"], "Id"),
                new(Retailer => Retailer.Order, L["Order"], "Order"),
                new(Retailer => Retailer.Code, L["Code"], "Code"),
                new(Retailer => Retailer.Name, L["Name"], "Name"),
                new(Retailer => Retailer.Description, L["Description"], "Description"),
                new(Retailer => Retailer.ChannelName, L["Channel"], "Channel.Name"),
                new(Retailer => Retailer.SellGroup, L["Group"], "SellGroup"),
                new(Retailer => Retailer.PriceGroupName, L["PriceGroup"], "PriceGroup.Name"),

                // new(Retailer => Retailer.PriceGroupId, L["PriceGroupId"], "PriceGroupId"),

                new(Retailer => Retailer.IsActive, L["Active"], Type: typeof(bool)),

            },
            idFunc: Retailer => Retailer.Id,
            searchFunc: async filter => (await RetailersClient
                .SearchAsync(filter.Adapt<SearchRetailersRequest>()))
                .Adapt<PaginationResponse<RetailerDto>>(),
            createFunc: async Retailer =>
            {
               // CountryId = StateId = ProvinceId = DistrictId = WardId = Guid.Empty;
                await RetailersClient.CreateAsync(Retailer.Adapt<CreateRetailerRequest>());
            },
            updateFunc: async (id, Retailer) =>
            {
                await RetailersClient.UpdateAsync(id, Retailer);
            },
            deleteFunc: async id => await RetailersClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportRetailersRequest>();
                return await RetailersClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportRetailersRequest() { ExcelFile = FileUploadRequest };
                await RetailersClient.ImportAsync(request);
            }
            );
    }
}