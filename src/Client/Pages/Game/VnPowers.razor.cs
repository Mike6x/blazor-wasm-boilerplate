using FSH.BlazorWebAssembly.Client.Components.Dialogs;
using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;
using MudBlazor;
using System.Globalization;

namespace FSH.BlazorWebAssembly.Client.Pages.Game;

public partial class VnPowers
{
    protected EntityServerTableContext<VnPowerDto, Guid, UpdateVnPowerRequest> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["VnPower"],
            entityNamePlural: L["VnPowers"],
            entityResource: FSHResource.VnPowers,
            fields: new()
            {
                // new(VnPower => VnPower.Id, L["Id"], "Id"),
                new(VnPower => VnPower.DrawId, L["DrawId"], "DrawId", Type: typeof(int)),
                new(VnPower => VnPower.DrawDate, L["Date"], "DrawDate"),
                new(VnPower => VnPower.VnPowerResultRoundId, L["RId"], "VnPowerResultRoundId"),
                new(VnPower => VnPower.VnPowerResultSubRoundId, L["SRId"], "VnPowerResultSubRoundId"),
                new(VnPower => VnPower.WinNumber1, L["W1"], "WinNumber1"),
                new(VnPower => VnPower.WinNumber2, L["W2"], "WinNumber2"),
                new(VnPower => VnPower.WinNumber3, L["W3"], "WinNumber3"),
                new(VnPower => VnPower.WinNumber4, L["W4"], "WinNumber4"),
                new(VnPower => VnPower.WinNumber5, L["W5"], "WinNumber5"),
                new(VnPower => VnPower.WinNumber6, L["W6"], "WinNumber6"),
                new(VnPower => VnPower.BonusNumber, L["BN"], "BonusNumber"),
                new(VnPower => VnPower.Jackpot1, L["JP1"], "Jackpot1"),
                new(VnPower => VnPower.Jackpot1, L["JP2"], "Jackpot2"),

                // new(VnPower => VnPower.IsActive, L["Active"], Type: typeof(bool)),
            },
            idFunc: VnPower => VnPower.Id,
            searchFunc: async filter => (await VnPowersClient
                .SearchAsync(filter.Adapt<SearchVnPowersRequest>()))
                .Adapt<PaginationResponse<VnPowerDto>>(),
            createFunc: async VnPower => await VnPowersClient.CreateAsync(VnPower.Adapt<CreateVnPowerRequest>()),
            updateFunc: async (id, VnPower) => await VnPowersClient.UpdateAsync(id, VnPower),
            deleteFunc: async id => await VnPowersClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportVnPowersRequest>();
                return await VnPowersClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportVnPowersRequest() { ExcelFile = FileUploadRequest };
                await VnPowersClient.ImportAsync(request);
            }
            );

    private async Task GetVnPowerForcastsAsync(Guid id, int drawId, int roundId, int subRoundId)
    {
        ForcastVnPowerRequest forcastReq = new ForcastVnPowerRequest()
        {
            Id = id,
            DrawId = drawId,
            RoundId = roundId,
            SubRoundId = null
        };

        // var forcast = new VnPowerForcast();

        var forcast = await VnPowersClient.ForcastVnPowerAsync(forcastReq);

        if (forcast == Guid.Empty)
        {
            Snackbar.Add(L["Internal error."], Severity.Error);
        }
        else
        {
            const string contentText = "Would you like to see ?";
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
            var parameters = new DialogParameters
                    {
                        { "ContentText", contentText},
                        { "ButtonText", "Go" },
                        { "ButtonColor", Color.Info},
                        { "titleIcon", Icons.Material.Filled.OpenInFull},
                        { "titleText", "Forcast calculation have done !"}
                    };
            var dialog = DialogService.Show<DialogComfirmation>("Forcast", parameters, options);

            var result = await dialog.Result;

            if (!result.Canceled)
            {

                Navigation.NavigateTo($"/game/VnPowerForcasts");
            }
        }
    }

}