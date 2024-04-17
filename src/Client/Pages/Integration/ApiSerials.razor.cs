using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Integration;

public partial class ApiSerials
{
    protected EntityServerTableContext<ApiSerialDto, Guid, ApiSerialViewModel> Context { get; set; } = default!;
    protected override void OnInitialized() =>
        Context = new(
            fields:
            [
                new(ApiSerial => ApiSerial.ItemSerial, L["ItemSerial"], "ItemSerial"),
                new(ApiSerial => ApiSerial.ItemCode, L["ItemCode"], "ItemCode"),
                new(ApiSerial => ApiSerial.PoNumber, L["PoNumber"], "PoNumber"),
                new(ApiSerial => ApiSerial.ImportStatus, L["ImportStatus"], "ImportStatus" ),

                new(ApiSerial => ApiSerial.ItemName, L["ItemName"], "ItemName" ),
                new(ApiSerial => ApiSerial.ItemClass, L["ItemClass"], "ItemClass" ),
                new(ApiSerial => ApiSerial.ItemBrand, L["ItemBrand"], "ItemBrand" ),

                new(ApiSerial => ApiSerial.PoCreatedDate.ToLocalTime(), L["PoCreatedDate"], "PoCreatedDate", Type: typeof(DateTime)),                new(ApiSerial => ApiSerial.PoCreatedDate.ToLocalTime(), L["PoCreatedDate"], "PoCreatedDate", Type: typeof(DateTime)),
                new(ApiSerial => ApiSerial.PoModifiedDate.ToLocalTime(), L["PoModifiedDate"], "PoModifiedDate", Type: typeof(DateTime)),                new(ApiSerial => ApiSerial.PoCreatedDate.ToLocalTime(), L["PoCreatedDate"], "PoCreatedDate", Type: typeof(DateTime)),

                new(ApiSerial => ApiSerial.CronJobName, L["CronJobName"], "CronJobName"),
            ],
            searchFunc: async filter =>
            {
                var searchFilter = filter.Adapt<SearchApiSerialsRequest>();
                searchFilter.CronJobId = SearchCronJobId == default ? null : SearchCronJobId;
                searchFilter.PoNumber = SearchPoNumberString;
                searchFilter.ItemCode = SearchItemCodeString;

                var result = await ApiSerialsClient.SearchAsync(searchFilter);
                return result.Adapt<PaginationResponse<ApiSerialDto>>();
            },
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportApiSerialsRequest>();
                exportFilter.CronJobId = SearchCronJobId == default ? null : SearchCronJobId;
                exportFilter.PoNumber = SearchPoNumberString;
                exportFilter.ItemCode = SearchItemCodeString;

                return await ApiSerialsClient.ExportAsync(exportFilter);
            },
            enableAdvancedSearch: true,
            idFunc: ApiSerial => ApiSerial.Id,
            createFunc: async ApiSerial =>
            {
                var request = ApiSerial.Adapt<CreateApiSerialRequest>();

                // request.Code = request.Name = request.Command.ToString() + "_" + request.StartTime.ToString("yyyyMMdd_HHmm");
                await ApiSerialsClient.CreateAsync(request);
            },
            updateFunc: async (id, ApiSerial) =>
            {
                var request = ApiSerial.Adapt<UpdateApiSerialRequest>();

                await ApiSerialsClient.UpdateAsync(id, request);
            },
            deleteFunc: async id => await ApiSerialsClient.DeleteAsync(id),
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportApiSerialsRequest() { ExcelFile = FileUploadRequest };
                await ApiSerialsClient.ImportAsync(request);
            },
            entityName: L["ApiSerial"],
            entityNamePlural: L["ApiSerials"],
            entityResource: FSHResource.ApiSerials);

    #region Advanced Search
    private EntityTable<ApiSerialDto, Guid, ApiSerialViewModel>? _table;

    private Guid _searchCronJobId;
    private Guid SearchCronJobId
    {
        get => _searchCronJobId;
        set
        {
            _searchCronJobId = value;
            _ = _table?.ReloadDataAsync();
        }
    }

    private string? SearchPoNumberString { get; set; }
    private void OnPoNumberStringChanged()
    {
        _ = _table?.ReloadDataAsync();
    }

    private string? SearchItemCodeString { get; set; }
    private void OnItemCodeStringChanged()
    {
        _ = _table?.ReloadDataAsync();
    }
    #endregion
}

public class ApiSerialViewModel : UpdateApiSerialRequest
{
}