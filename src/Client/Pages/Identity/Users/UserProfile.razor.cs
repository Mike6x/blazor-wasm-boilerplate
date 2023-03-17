using FSH.BlazorWebAssembly.Client.Components.Common;
using FSH.BlazorWebAssembly.Client.Components.Dialogs;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Infrastructure.Auth;
using FSH.BlazorWebAssembly.Client.Infrastructure.Common;
using FSH.BlazorWebAssembly.Client.Shared;
using FSH.WebApi.Shared.Authorization;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.Identity.Users;

public partial class UserProfile
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;
    [Inject]
    protected IUsersClient UsersClient { get; set; } = default!;

    [Parameter]
    public string? Id { get; set; }
    [Parameter]
    public string? Title { get; set; }
    [Parameter]

    public string? Description { get; set; }

    private string Tenant { get; set; } = MultitenancyConstants.Root.Id;

    private readonly UpdateUserRequest _profileModel = new();

    private CustomValidation? _customValidation;

    private char _firstLetterOfName;
    private string? _imageUrl;
    private bool _isActive;

    private bool IsLocked { get; set; }
    private DateTime? LockoutEndDate { get; set; }
    private TimeSpan? LockoutEndTime { get; set; }

    private bool _loaded;

    protected override async Task OnInitializedAsync()
    {
        if (await ApiHelper.ExecuteCallGuardedAsync(
            () => UsersClient.GetByIdAsync(Id), Snackbar)
                is UserDetailsDto user)
        {
            _profileModel.Id = user.Id.ToString();
            _profileModel.FirstName = user.FirstName ?? string.Empty;
            _profileModel.LastName = user.LastName ?? string.Empty;
            _profileModel.UserName = user.UserName ?? string.Empty;
            _profileModel.Email = user.Email ?? string.Empty;
            _profileModel.PhoneNumber = user.PhoneNumber ?? string.Empty;

            _profileModel.ImageUrl = string.IsNullOrEmpty(user.ImageUrl)
                                        ? string.Empty
                                        : user.ImageUrl;

            _profileModel.IsActive = _isActive = user.IsActive;
            _profileModel.EmailConfirmed = user.EmailConfirmed;

            _profileModel.CreatedBy = user.CreatedBy ?? string.Empty;
            _profileModel.CreatedOn = user.CreatedOn;
            _profileModel.LastModifiedBy = user.LastModifiedBy ?? string.Empty;
            _profileModel.LastModifiedOn = user.LastModifiedOn ?? user.CreatedOn;

            if (user.LockoutEnd != null)
            {
                _profileModel.LockoutEnd = (DateTime)user.LockoutEnd;

                LockoutEndDate = user.LockoutEnd.Value.ToLocalTime().Date;
                LockoutEndTime = user.LockoutEnd.Value.ToLocalTime().TimeOfDay;
                var now = DateTimeOffset.Now;
                IsLocked = user.LockoutEnd > now;
            }

            _imageUrl = string.IsNullOrEmpty(user.ImageUrl)
                            ? string.Empty
                            : (Config[ConfigNames.ApiBaseUrl] + user.ImageUrl);

            if (_profileModel.FirstName.Length > 0)
            {
                _firstLetterOfName = _profileModel.FirstName.ToUpper().FirstOrDefault();
            }
        }

        var state = await AuthState;
        _loaded = true;
    }

    private void BackToUsers()
    {
        Navigation.NavigateTo("/users");
    }

    private async Task ToggleUserStatusAsync()
    {
        var request = new ToggleUserStatusRequest { ActivateUser = !_isActive, UserId = Id };
        if (await ApiHelper.ExecuteCallGuardedAsync(() => UsersClient.ToggleStatusAsync(Id, request), Snackbar))
        {
            string message = _isActive ? "The Account have disabled" : "The Account have activated";
            Snackbar.Add(_localizer[message], Severity.Success);
            _isActive = !_isActive!;
            _profileModel.IsActive = _isActive;
        }
        else { Snackbar.Add(_localizer["Internal error."], Severity.Error); }
    }

    private async Task SendVerificationEmailAsync()
    {
        string userId = Id ?? string.Empty;
        if (await ApiHelper.ExecuteCallGuardedAsync(() => UsersClient.SendVerificationEmailAsync(Id, userId), Snackbar))
        {
            Snackbar.Add(_localizer["Email for verification has been sent."], Severity.Success);
            _isActive = true;
            _profileModel.IsActive = true;
            _profileModel.EmailConfirmed = false;
        }
        else { Snackbar.Add(_localizer["Internal error."], Severity.Error); }
    }

    private async Task SendRecoveryPasswordEmailAsync()
    {
        var forgotPasswordRequest = new ForgotPasswordRequest
        {
            Email = _profileModel.Email
        };

        await ApiHelper.ExecuteCallGuardedAsync(
            () => UsersClient.ForgotPasswordAsync(Tenant, forgotPasswordRequest),
            Snackbar,
            _customValidation);

        Snackbar.Add(_localizer["Reset email has been sent."], Severity.Success);
    }

    private async Task UnlockUserAsync()
    {
        _profileModel.LockoutEnd = DateTime.UtcNow;
        if (await ApiHelper.ExecuteCallGuardedAsync(
            () => UsersClient.UpdateUserAsync(_profileModel), Snackbar, _customValidation))
        {
            Snackbar.Add(_localizer["User is unlocked."], Severity.Success);
            await OnInitializedAsync();
        }
        else { Snackbar.Add(_localizer["Internal error."], Severity.Error); }
    }

    private async Task UpdateUserAsync()
    {
        if (LockoutEndDate != null && LockoutEndTime != null)
        {
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            LockoutEndDate += LockoutEndTime;
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            var timeSpan = LockoutEndDate - DateTime.Now;
            _profileModel.LockoutEnd = DateTime.UtcNow.Add((TimeSpan)timeSpan);
        }

        if (await ApiHelper.ExecuteCallGuardedAsync(
            () => UsersClient.UpdateUserAsync(_profileModel), Snackbar, _customValidation))
        {
            Snackbar.Add(_localizer["Your Profile has been updated."], Severity.Success);
            await OnInitializedAsync();
        }
        else { Snackbar.Add(_localizer["Internal error."], Severity.Error); }
    }

    private async Task UploadFiles(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file is not null)
        {
            string? extension = Path.GetExtension(file.Name);
            if (!ApplicationConstants.SupportedImageFormats.Contains(extension.ToLower()))
            {
                Snackbar.Add("Image Format Not Supported.", Severity.Error);
                return;
            }

            string? fileName = $"{Id}-{Guid.NewGuid():N}";
            fileName = fileName[..Math.Min(fileName.Length, 90)];
            var imageFile = await file.RequestImageFileAsync(ApplicationConstants.StandardImageFormat, ApplicationConstants.MaxImageWidth, ApplicationConstants.MaxImageHeight);
            byte[]? buffer = new byte[imageFile.Size];
            await imageFile.OpenReadStream(ApplicationConstants.MaxImageFileSize).ReadAsync(buffer);
            string? base64String = $"data:{ApplicationConstants.StandardImageFormat};base64,{Convert.ToBase64String(buffer)}";
            _profileModel.Image = new FileUploadRequest() { Name = fileName, Data = base64String, Extension = extension };

            await UpdateUserAsync();
        }
    }

    private async Task RemoveImageAsync()
    {
        string deleteContent = _localizer["You're sure you want to delete your Profile Image?"];
        var parameters = new DialogParameters
        {
            { nameof(DeleteConfirmation.ContentText), deleteContent }
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
        var dialog = DialogService.Show<DeleteConfirmation>(_localizer["Delete"], parameters, options);
        var result = await dialog.Result;
        if (!result.Canceled)
        {
            _profileModel.DeleteCurrentImage = true;
            await UpdateUserAsync();
        }
    }

    private bool _passwordVisibility;
    private InputType _passwordInput = InputType.Password;
    private string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
    private void TogglePasswordVisibility()
    {
        if (_passwordVisibility)
        {
            _passwordVisibility = false;
            _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
            _passwordInput = InputType.Password;
        }
        else
        {
            _passwordVisibility = true;
            _passwordInputIcon = Icons.Material.Filled.Visibility;
            _passwordInput = InputType.Text;
        }
    }
}