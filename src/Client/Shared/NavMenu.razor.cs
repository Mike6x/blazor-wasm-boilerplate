using FSH.BlazorWebAssembly.Client.Infrastructure.Auth;
using FSH.BlazorWebAssembly.Client.Infrastructure.Common;
using FSH.WebApi.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace FSH.BlazorWebAssembly.Client.Shared;

public partial class NavMenu
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;

    private string? _hangfireUrl;

    private bool _canViewDashboard;

    private bool _canViewMenus;

    private bool _canViewRoles;
    private bool _canViewUsers;
    private bool _canViewTenants;

    private bool _canViewHangfire;
    private bool _canViewSwagger;
    private bool _canViewAuditTrails;

    private bool _canViewCountries;
    private bool _canViewStates;
    private bool _canViewRegions;
    private bool _canViewProvinces;
    private bool _canViewDistricts;
    private bool _canViewAwards;

    private bool _canViewBrands;
    private bool _canViewIndustries;
    private bool _canViewGroupCategories;
    private bool _canViewCategories;
    private bool _canViewSubCategories;
    private bool _canViewProducts;

    private bool _canViewChannels;
    private bool _canViewRetailers;
    private bool _canViewStores;

    private bool _canViewPriceGroups;
    private bool _canViewPricePlans;

    private bool CanViewAdministrationGroup => _canViewUsers || _canViewRoles || _canViewTenants || _canViewMenus;

    protected override async Task OnParametersSetAsync()
    {
        _hangfireUrl = Config[ConfigNames.ApiBaseUrl] + "jobs";
        var user = (await AuthState).User;
        _canViewHangfire = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Hangfire);
        _canViewDashboard = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Dashboard);

        _canViewMenus = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Menus);

        _canViewTenants = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Tenants);
        _canViewRoles = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Roles);
        _canViewUsers = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Users);

        _canViewProducts = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Products);
        _canViewBrands = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Brands);
    }
}