using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Game;

public partial class VnPowerForcasts
{
    protected EntityServerTableContext<VnPowerForcastDto, Guid, UpdateVnPowerForcastRequest> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["VnPowerForcast"],
            entityNamePlural: L["VnPowerForcasts"],
            entityResource: FSHResource.VnPowerForcasts,
            fields: new()
            {
                new(VnPowerForcast => VnPowerForcast.VnPowerDrawId, L["DrawId"], "VnPowerDrawId"),
                new(VnPowerForcast => VnPowerForcast.VnPowerDrawDate, L["DrawDate"], "VnPowerDrawDate"),
                new(VnPowerForcast => VnPowerForcast.Number1, L["N01"], "Number1"),
                new(VnPowerForcast => VnPowerForcast.Number2, L["N02"], "Number2"),
                new(VnPowerForcast => VnPowerForcast.Number3, L["N03"], "Number3"),
                new(VnPowerForcast => VnPowerForcast.Number4, L["N04"], "Number4"),
                new(VnPowerForcast => VnPowerForcast.Number5, L["N05"], "Number5"),
                new(VnPowerForcast => VnPowerForcast.Number6, L["N06"], "Number6"),
                new(VnPowerForcast => VnPowerForcast.Number7, L["N07"], "Number7"),
                new(VnPowerForcast => VnPowerForcast.Number8, L["N08"], "Number8"),
                new(VnPowerForcast => VnPowerForcast.Number9, L["N09"], "Number9"),
                new(VnPowerForcast => VnPowerForcast.Number10, L["N10"], "Number10"),

                new(VnPowerForcast => VnPowerForcast.Number11, L["N11"], "Number11"),
                new(VnPowerForcast => VnPowerForcast.Number12, L["N12"], "Number12"),
                new(VnPowerForcast => VnPowerForcast.Number13, L["N13"], "Number13"),
                new(VnPowerForcast => VnPowerForcast.Number14, L["N14"], "Number14"),
                new(VnPowerForcast => VnPowerForcast.Number15, L["N15"], "Number15"),
                new(VnPowerForcast => VnPowerForcast.Number16, L["N16"], "Number16"),
                new(VnPowerForcast => VnPowerForcast.Number17, L["N17"], "Number17"),
                new(VnPowerForcast => VnPowerForcast.Number18, L["N18"], "Number18"),
                new(VnPowerForcast => VnPowerForcast.Number19, L["N19"], "Number19"),
                new(VnPowerForcast => VnPowerForcast.Number10, L["N20"], "Number20"),

                new(VnPowerForcast => VnPowerForcast.Number21, L["N21"], "Number21"),
                new(VnPowerForcast => VnPowerForcast.Number22, L["N22"], "Number22"),
                new(VnPowerForcast => VnPowerForcast.Number23, L["N23"], "Number23"),
                new(VnPowerForcast => VnPowerForcast.Number24, L["N24"], "Number24"),
                new(VnPowerForcast => VnPowerForcast.Number25, L["N25"], "Number25"),
                new(VnPowerForcast => VnPowerForcast.Number26, L["N26"], "Number26"),
                new(VnPowerForcast => VnPowerForcast.Number27, L["N27"], "Number27"),
                new(VnPowerForcast => VnPowerForcast.Number28, L["N28"], "Number28"),
                new(VnPowerForcast => VnPowerForcast.Number29, L["N29"], "Number29"),
                new(VnPowerForcast => VnPowerForcast.Number30, L["N30"], "Number30"),

                new(VnPowerForcast => VnPowerForcast.Number31, L["N31"], "Number31"),
                new(VnPowerForcast => VnPowerForcast.Number32, L["N32"], "Number32"),
                new(VnPowerForcast => VnPowerForcast.Number33, L["N33"], "Number33"),
                new(VnPowerForcast => VnPowerForcast.Number34, L["N34"], "Number34"),
                new(VnPowerForcast => VnPowerForcast.Number35, L["N35"], "Number35"),
                new(VnPowerForcast => VnPowerForcast.Number36, L["N36"], "Number36"),
                new(VnPowerForcast => VnPowerForcast.Number37, L["N37"], "Number37"),
                new(VnPowerForcast => VnPowerForcast.Number38, L["N38"], "Number38"),
                new(VnPowerForcast => VnPowerForcast.Number39, L["N39"], "Number39"),
                new(VnPowerForcast => VnPowerForcast.Number40, L["N40"], "Number40"),

                new(VnPowerForcast => VnPowerForcast.Number41, L["N41"], "Number41"),
                new(VnPowerForcast => VnPowerForcast.Number42, L["N42"], "Number42"),
                new(VnPowerForcast => VnPowerForcast.Number43, L["N43"], "Number43"),
                new(VnPowerForcast => VnPowerForcast.Number44, L["N44"], "Number44"),
                new(VnPowerForcast => VnPowerForcast.Number45, L["N45"], "Number45"),
                new(VnPowerForcast => VnPowerForcast.Number46, L["N46"], "Number46"),
                new(VnPowerForcast => VnPowerForcast.Number47, L["N47"], "Number47"),
                new(VnPowerForcast => VnPowerForcast.Number48, L["N48"], "Number48"),
                new(VnPowerForcast => VnPowerForcast.Number49, L["N49"], "Number49"),
                new(VnPowerForcast => VnPowerForcast.Number50, L["N50"], "Number50"),

                new(VnPowerForcast => VnPowerForcast.Number51, L["N51"], "Number51"),
                new(VnPowerForcast => VnPowerForcast.Number52, L["N52"], "Number52"),
                new(VnPowerForcast => VnPowerForcast.Number53, L["N53"], "Number53"),
                new(VnPowerForcast => VnPowerForcast.Number54, L["N54"], "Number54"),
                new(VnPowerForcast => VnPowerForcast.Number55, L["N55"], "Number55"),
            },
            idFunc: VnPowerForcast => VnPowerForcast.Id,
            searchFunc: async filter => (await VnPowerForcastsClient
                .SearchAsync(filter.Adapt<SearchVnPowerForcastsRequest>()))
                .Adapt<PaginationResponse<VnPowerForcastDto>>(),
            createFunc: async VnPowerForcast => await VnPowerForcastsClient.CreateAsync(VnPowerForcast.Adapt<CreateVnPowerForcastRequest>()),
            updateFunc: async (id, VnPowerForcast) => await VnPowerForcastsClient.UpdateAsync(id, VnPowerForcast),
            deleteFunc: async id => await VnPowerForcastsClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportVnPowerForcastsRequest>();
                return await VnPowerForcastsClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportVnPowerForcastsRequest() { ExcelFile = FileUploadRequest };
                await VnPowerForcastsClient.ImportAsync(request);
            }
        );
}
