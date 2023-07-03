using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.Identity.Users;

public class UserAutocomplete : MudAutocomplete<string>
{
    [Inject]
    private IStringLocalizer<UserAutocomplete> L { get; set; } = default!;
    [Inject]
    private IUsersClient UsersClient { get; set; } = default!;
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private List<UserDetailsDto> _entityList = new();

    // supply default parameters, but leave the possibility to override them

    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = L["User"];
        Variant = Variant.Filled;
        Dense = true;
        Margin = Margin.None;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchUsers;
        ToStringFunc = GetUserName;
        Clearable = true;
        return base.SetParametersAsync(parameters);
    }

    // when the value parameter is set, we have to load that one User to be able to show the name
    // we can't do that in OnInitialized because of a strange bug (https://github.com/MudBlazor/MudBlazor/issues/3818)
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender &&
            _value != default &&
            await ApiHelper.ExecuteCallGuardedAsync(
                () => UsersClient.GetByIdAsync(_value), Snackbar) is { } entityDetailsDto)
        {
            var entity = new UserDetailsDto
            {
                Id = entityDetailsDto.Id,
                FirstName = entityDetailsDto.FirstName,
                LastName = entityDetailsDto.LastName,
            };
            _entityList.Add(entity);
            ForceRender(true);
        }
    }

    private async Task<IEnumerable<string>> SearchUsers(string value)
    {

        var filter = new UserListFilter
        {
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "FirstName", "LastName" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => UsersClient.SearchAsync(filter), Snackbar)
            is PaginationResponseOfUserDetailsDto response)
        {
            _entityList = response.Data.ToList();
        }

        var userList = new List<string>();
        _entityList.ForEach(e => userList.Add(e.Id.ToString()));
        return userList;

        // return _entityList.Select(x => x.Id);
    }

    private string GetUserName(string id)
    {
        var emp = _entityList.Find(e => e.Id.ToString() == id);
        return emp != null ? emp.FirstName + " " + emp.LastName : string.Empty;
    }
}