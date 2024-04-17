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

    private bool _canViewCronJobs;
    private bool _canViewApiSerials;
    private bool CanViewIntegrationGroup => _canViewCronJobs || _canViewApiSerials;

    private bool _canViewHangfire;
    private bool _canViewSwagger;
    private bool _canViewAuditTrails;

    private bool _canViewMenus;
    private bool _canViewDimensions;
    private bool _canViewBackgroundJobs;
    private bool _canViewEntityCodes;
    private bool CanViewSystemGroup => _canViewHangfire || _canViewSwagger || _canViewAuditTrails || _canViewMenus || _canViewDimensions || _canViewBackgroundJobs || _canViewEntityCodes;

    private bool _canViewChatMessages;

    private bool _canViewRoles;
    private bool _canViewUsers;
    private bool _canViewTenants;
    private bool CanViewUserGroup => _canViewUsers || _canViewRoles || _canViewTenants || _canViewMenus || _canViewChatMessages;

    private bool _canViewUserStats;
    private bool _canViewProductStats;
    private bool _canViewAssetStats;
    private bool _canViewEmployeeStats;
    private bool _canViewDistributionStats;

    private bool _canViewGeoAdminUnits;
    private bool _canViewCountries;
    private bool _canViewStates;
    private bool _canViewRegions;
    private bool _canViewProvinces;
    private bool _canViewDistricts;
    private bool _canViewWards;
    private bool CanViewGeographyGroup => _canViewGeoAdminUnits || _canViewCountries || _canViewStates || _canViewRegions || _canViewProvinces || _canViewDistricts || _canViewWards;

    private bool _canViewBusinessUnits;
    private bool _canViewDepartments;
    private bool _canViewSubDepartments;
    private bool _canViewTeams;
    private bool _canViewEmployees;
    private bool CanViewOrganizationGroup => _canViewBusinessUnits || _canViewDepartments || _canViewSubDepartments || _canViewTeams || _canViewEmployees || _canViewEmployeeStats;

    private bool _canViewLeaveAllocations;
    private bool _canViewLeaveApplications;
    private bool CanViewTimeOffGroup => _canViewLeaveAllocations || _canViewLeaveApplications;

    private bool _canViewQuizs;
    private bool _canViewQuizResults;
    private bool CanViewElearningGroup => _canViewQuizs || _canViewQuizResults;

    private bool _canViewVendors;
    private bool CanViewPurchaseGroup => _canViewVendors;

    private bool _canViewBrands;
    private bool _canViewBusinessLines;
    private bool _canViewGroupCategories;
    private bool _canViewCategories;
    private bool _canViewSubCategories;
    private bool CanViewCatalogGroup => _canViewBrands || _canViewBusinessLines || _canViewGroupCategories || _canViewCategories || _canViewSubCategories;

    private bool _canViewAssets;
    private bool _canViewAssetHistorys;
    private bool CanViewPropertyGroup => _canViewAssets || _canViewAssetHistorys || _canViewAssetStats;

    private bool _canViewChannels;
    private bool _canViewRetailers;
    private bool _canViewStores;
    private bool CanViewPlaceGroup => _canViewChannels || _canViewRetailers || _canViewStores || _canViewDistributionStats;

    private bool _canViewPriceGroups;
    private bool _canViewPricePlans;

    private bool _canViewProducts;
    private bool CanViewProductionGroup => _canViewProducts || _canViewPriceGroups || _canViewPricePlans || _canViewProductStats;

    private bool _canViewVnPowers;
    private bool _canViewVnPowerForcasts;
    private bool CanViewGameGroup => _canViewVnPowers || _canViewVnPowerForcasts;

    protected override async Task OnParametersSetAsync()
    {
        var user = (await AuthState).User;

        _hangfireUrl = Config[ConfigNames.ApiBaseUrl] + "jobs";

        _canViewHangfire = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Hangfire);
        _canViewBackgroundJobs = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.BackgroundJobs);

        _canViewCronJobs = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.CronJobs);
        _canViewApiSerials = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.ApiSerials);

        _canViewMenus = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Menus);
        _canViewDimensions = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Dimensions);
        _canViewEntityCodes = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.EntityCodes);

        _canViewSwagger = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Swagger);
        _canViewAuditTrails = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.AuditTrails);

        _canViewUserStats = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.UserStats);
        _canViewProductStats = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.ProductStats);
        _canViewAssetStats = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.AssetStats);
        _canViewEmployeeStats = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.EmployeeStats);
        _canViewDistributionStats = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.DistributionStats);

        _canViewTenants = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Tenants);
        _canViewRoles = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Roles);
        _canViewUsers = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Users);

        _canViewChatMessages = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.ChatMessages);

        _canViewGeoAdminUnits = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.GeoAdminUnits);
        _canViewCountries = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Countries);
        _canViewStates = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.States);
        _canViewRegions = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Regions);
        _canViewProvinces = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Provinces);
        _canViewDistricts = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Districts);
        _canViewWards = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Wards);

        _canViewBusinessUnits = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.BusinessUnits);
        _canViewDepartments = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Departments);
        _canViewSubDepartments = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.SubDepartments);
        _canViewTeams = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Teams);

        _canViewEmployees = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Employees);
        _canViewLeaveAllocations = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.LeaveAllocations);
        _canViewLeaveApplications = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.LeaveApplications);

        _canViewQuizs = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Quizs);
        _canViewQuizResults = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.QuizResults);

        _canViewVendors = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Vendors);

        _canViewBrands = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Brands);
        _canViewBusinessLines = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.BusinessLines);
        _canViewGroupCategories = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.GroupCategories);
        _canViewCategories = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Categories);
        _canViewSubCategories = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.SubCategories);

        _canViewAssets = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Assets);
        _canViewAssetHistorys = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.AssetHistorys);
        _canViewChannels = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Channels);
        _canViewRetailers = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Retailers);
        _canViewStores = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Stores);

        _canViewPriceGroups = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.PriceGroups);
        _canViewPricePlans = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.PricePlans);

        _canViewProducts = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Products);

        _canViewVnPowers = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.VnPowers);
        _canViewVnPowerForcasts = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.VnPowerForcasts);
    }
}