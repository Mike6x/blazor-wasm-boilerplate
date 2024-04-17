using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Game;

public partial class VnPowerResults
{
    protected EntityServerTableContext<VnPowerResultDto, Guid, UpdateVnPowerResultRequest> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["VnPowerResult"],
            entityNamePlural: L["VnPowerResults"],
            entityResource: FSHResource.VnPowers,
            fields: new()
            {
                new(VnPowerResult => VnPowerResult.VnPowerDrawId, L["DrawId"], "VnPowerDrawId"),
                new(VnPowerResult => VnPowerResult.VnPowerDrawDate, L["DrawDate"], "VnPowerDrawDate"),
                new(VnPowerResult => VnPowerResult.RoundId, L["RoundId"], "RoundId"),
                new(VnPowerResult => VnPowerResult.SubRoundId, L["SRoundId"], "SubRoundId"),

                new(VnPowerResult => VnPowerResult.Number1, L["N01"], "Number1"),
                new(VnPowerResult => VnPowerResult.Number2, L["N02"], "Number2"),
                new(VnPowerResult => VnPowerResult.Number3, L["N03"], "Number3"),
                new(VnPowerResult => VnPowerResult.Number4, L["N04"], "Number4"),
                new(VnPowerResult => VnPowerResult.Number5, L["N05"], "Number5"),
                new(VnPowerResult => VnPowerResult.Number6, L["N06"], "Number6"),
                new(VnPowerResult => VnPowerResult.Number7, L["N07"], "Number7"),
                new(VnPowerResult => VnPowerResult.Number8, L["N08"], "Number8"),
                new(VnPowerResult => VnPowerResult.Number9, L["N09"], "Number9"),
                new(VnPowerResult => VnPowerResult.Number10, L["N10"], "Number10"),

                new(VnPowerResult => VnPowerResult.Number11, L["N11"], "Number11"),
                new(VnPowerResult => VnPowerResult.Number12, L["N12"], "Number12"),
                new(VnPowerResult => VnPowerResult.Number13, L["N13"], "Number13"),
                new(VnPowerResult => VnPowerResult.Number14, L["N14"], "Number14"),
                new(VnPowerResult => VnPowerResult.Number15, L["N15"], "Number15"),
                new(VnPowerResult => VnPowerResult.Number16, L["N16"], "Number16"),
                new(VnPowerResult => VnPowerResult.Number17, L["N17"], "Number17"),
                new(VnPowerResult => VnPowerResult.Number18, L["N18"], "Number18"),
                new(VnPowerResult => VnPowerResult.Number19, L["N19"], "Number19"),
                new(VnPowerResult => VnPowerResult.Number10, L["N20"], "Number20"),

                new(VnPowerResult => VnPowerResult.Number21, L["N21"], "Number21"),
                new(VnPowerResult => VnPowerResult.Number22, L["N22"], "Number22"),
                new(VnPowerResult => VnPowerResult.Number23, L["N23"], "Number23"),
                new(VnPowerResult => VnPowerResult.Number24, L["N24"], "Number24"),
                new(VnPowerResult => VnPowerResult.Number25, L["N25"], "Number25"),
                new(VnPowerResult => VnPowerResult.Number26, L["N26"], "Number26"),
                new(VnPowerResult => VnPowerResult.Number27, L["N27"], "Number27"),
                new(VnPowerResult => VnPowerResult.Number28, L["N28"], "Number28"),
                new(VnPowerResult => VnPowerResult.Number29, L["N29"], "Number29"),
                new(VnPowerResult => VnPowerResult.Number30, L["N30"], "Number30"),

                new(VnPowerResult => VnPowerResult.Number31, L["N31"], "Number31"),
                new(VnPowerResult => VnPowerResult.Number32, L["N32"], "Number32"),
                new(VnPowerResult => VnPowerResult.Number33, L["N33"], "Number33"),
                new(VnPowerResult => VnPowerResult.Number34, L["N34"], "Number34"),
                new(VnPowerResult => VnPowerResult.Number35, L["N35"], "Number35"),
                new(VnPowerResult => VnPowerResult.Number36, L["N36"], "Number36"),
                new(VnPowerResult => VnPowerResult.Number37, L["N37"], "Number37"),
                new(VnPowerResult => VnPowerResult.Number38, L["N38"], "Number38"),
                new(VnPowerResult => VnPowerResult.Number39, L["N39"], "Number39"),
                new(VnPowerResult => VnPowerResult.Number40, L["N40"], "Number40"),

                new(VnPowerResult => VnPowerResult.Number41, L["N41"], "Number41"),
                new(VnPowerResult => VnPowerResult.Number42, L["N42"], "Number42"),
                new(VnPowerResult => VnPowerResult.Number43, L["N43"], "Number43"),
                new(VnPowerResult => VnPowerResult.Number44, L["N44"], "Number44"),
                new(VnPowerResult => VnPowerResult.Number45, L["N45"], "Number45"),
                new(VnPowerResult => VnPowerResult.Number46, L["N46"], "Number46"),
                new(VnPowerResult => VnPowerResult.Number47, L["N47"], "Number47"),
                new(VnPowerResult => VnPowerResult.Number48, L["N48"], "Number48"),
                new(VnPowerResult => VnPowerResult.Number49, L["N49"], "Number49"),
                new(VnPowerResult => VnPowerResult.Number50, L["N50"], "Number50"),

                new(VnPowerResult => VnPowerResult.Number51, L["N51"], "Number51"),
                new(VnPowerResult => VnPowerResult.Number52, L["N52"], "Number52"),
                new(VnPowerResult => VnPowerResult.Number53, L["N53"], "Number53"),
                new(VnPowerResult => VnPowerResult.Number54, L["N54"], "Number54"),
                new(VnPowerResult => VnPowerResult.Number55, L["N55"], "Number55"),
            },
            idFunc: VnPowerResult => VnPowerResult.Id,
            searchFunc: async filter => (await VnPowerResultsClient
                .SearchAsync(filter.Adapt<SearchVnPowerResultsRequest>()))
                .Adapt<PaginationResponse<VnPowerResultDto>>(),
            createFunc: async VnPowerResult => await VnPowerResultsClient.CreateAsync(VnPowerResult.Adapt<CreateVnPowerResultRequest>()),
            updateFunc: async (id, VnPowerResult) => await VnPowerResultsClient.UpdateAsync(id, VnPowerResult),
            deleteFunc: async id => await VnPowerResultsClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportVnPowerResultsRequest>();
                return await VnPowerResultsClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportVnPowerResultsRequest() { ExcelFile = FileUploadRequest };
                await VnPowerResultsClient.ImportAsync(request);
            }
        );
}
