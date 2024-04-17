using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.People;

public partial class Employees
{
    [Inject]
    protected IUsersClient UsersClient { get; set; } = default!;

    protected EntityServerTableContext<EmployeeDto, Guid, EmployeeViewModel> Context { get; set; } = default!;

    private EntityTable<EmployeeDto, Guid, EmployeeViewModel>? _table;

    protected override void OnInitialized() =>
        Context = new(
            fields:
            [

                // new(Employee => Employee.Id, L["Id"], "Id"),
                new(Employee => Employee.Code, L["Code"], "Code"),
                new(Employee => Employee.FirstName, L["F.Name"], "FirstName"),
                new(Employee => Employee.LastName, L["L.Name"], "LastName"),
                new(Employee => Employee.PhoneNumber, L["Phone"], "Phone"),

                // new(Employee => Employee.Email, L["Email"], "Email"),
                // new(Employee => Employee.Address, L["Address"], "Address"),
                // new(Employee => Employee.Description, L["Description"], "Description"),
                new(Employee => Employee.TitleValue, L["Grade"], "Title.Value"),
                new(Employee => Employee.TitleName, L["Title"], "Title.Name"),
                new(Employee => Employee.SuperiorFirstName, L["Superior F.Name "], "Superior.FirstName"),
                new(Employee => Employee.SuperiorLastName, L["Superior L.Name"], "Superior.LastName"),
                new(Employee => Employee.BusinessUnitName, L["BU."], "BusinessUnit.Name"),
                new(Employee => Employee.DepartmentName, L["Department"], "Department.Name"),
                new(Employee => Employee.SubDepartmentName, L["Section"], "SubDepartment.Name"),
                new(Employee => Employee.TeamName, L["Team"], "Team.Name"),
                new(Employee => Employee.IsActive, L["Active"], Type: typeof(bool)),
            ],
            searchFunc: async filter =>
            {
                var employeeFilter = filter.Adapt<SearchEmployeesRequest>();

                employeeFilter.BusinessUnitId = SearchBusinessUnitId == default ? null : SearchBusinessUnitId;
                employeeFilter.DepartmentId = SearchDepartmentId == default ? null : SearchDepartmentId;
                employeeFilter.SubDepartmentId = SearchSubDepartmentId == default ? null : SearchSubDepartmentId;
                employeeFilter.TeamId = SearchTeamId == default ? null : SearchTeamId;
                employeeFilter.Gender = SearchGender == default ? null : SearchGender;

                var result = await EmployeesClient.SearchAsync(employeeFilter);
                return result.Adapt<PaginationResponse<EmployeeDto>>();
            },
            exportFunc: async filter =>
            {
                var employeeFilter = filter.Adapt<ExportEmployeesRequest>();

                employeeFilter.BusinessUnitId = SearchBusinessUnitId == default ? null : SearchBusinessUnitId;
                employeeFilter.DepartmentId = SearchDepartmentId == default ? null : SearchDepartmentId;
                employeeFilter.SubDepartmentId = SearchSubDepartmentId == default ? null : SearchSubDepartmentId;
                employeeFilter.TeamId = SearchTeamId == default ? null : SearchTeamId;

                employeeFilter.Gender = SearchGender == Gender.All ? null : SearchGender;

                return await EmployeesClient.ExportAsync(employeeFilter);
            },
            enableAdvancedSearch: true,
            idFunc: Employee => Employee.Id,
            createFunc: async Employee =>
            {
                if (Employee.CreateOrDeleteUser)
                {
                    Employee.UserId = await CreateUserRelationAsync(Employee);
                }

                await EmployeesClient.CreateAsync(Employee.Adapt<CreateEmployeeRequest>());
            },
            updateFunc: async (id, Employee) =>
            {
                if (Employee.CreateOrDeleteUser && string.IsNullOrEmpty(Employee.UserId))
                {
                    Employee.UserId = await CreateUserRelationAsync(Employee);
                }
                else
                if (Employee.CreateOrDeleteUser && !string.IsNullOrEmpty(Employee.UserId))
                {
                    await DeleteUserRelationAsync(Employee);
                    Employee.UserId = null;
                }

                await EmployeesClient.UpdateAsync(id, Employee);
            },
            deleteFunc: async id => await EmployeesClient.DeleteAsync(id),
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportEmployeesRequest() { ExcelFile = FileUploadRequest };
                await EmployeesClient.ImportAsync(request);
            },
            entityName: L["Employee"],
            entityNamePlural: L["Employees"],
            entityResource: FSHResource.Employees);

    #region Advanced Search

    private Guid _searchBusinessUnitId;
    private Guid SearchBusinessUnitId
    {
        get => _searchBusinessUnitId;
        set
        {
            _searchBusinessUnitId = value;
            _ = _table?.ReloadDataAsync();
        }
    }

    private Guid _searchDepartmentId;
    private Guid SearchDepartmentId
    {
        get => _searchDepartmentId;
        set
        {
            _searchDepartmentId = value;
            _ = _table?.ReloadDataAsync();
        }
    }

    private Guid _searchSubDepartmentId;
    private Guid SearchSubDepartmentId
    {
        get => _searchSubDepartmentId;
        set
        {
            _searchSubDepartmentId = value;
            _ = _table?.ReloadDataAsync();
        }
    }

    private Guid _searchTeamId;
    private Guid SearchTeamId
    {
        get => _searchTeamId;
        set
        {
            _searchTeamId = value;
            _ = _table?.ReloadDataAsync();
        }
    }

    private Gender _searchGender = Gender.All;
    private Gender SearchGender
    {
        get => _searchGender;
        set
        {
            _searchGender = value;
            _ = _table?.ReloadDataAsync();
        }
    }

    #endregion
    private void UpdateUser(in string userCode) =>
            Navigation.NavigateTo($"/users/{userCode}/Emp2User");

    private void ViewProfile(in Guid userId) =>
            Navigation.NavigateTo($"/users/{userId}/profile");

    private async Task<string> DeleteUserRelationAsync(EmployeeViewModel employeeViewModel)
    {
        UserDetailsDto? user;

        try
        {
            user = await UsersClient.GetByUserNameAsync(employeeViewModel.Code);
        }
        catch (Exception)
        {
            return string.Empty;
        }

        await UsersClient.DeleteAsync(user.Id.ToString());

        return user.Id.ToString();
    }

    private async Task<string> CreateUserRelationAsync(EmployeeViewModel employeeViewModel)
    {
        UserDetailsDto? user;

        try
        {
            user = await UsersClient.GetByUserNameAsync(employeeViewModel.Code);
        }
        catch (Exception)
        {
            CreateUserRequest creatUserRequest = new()
            {
                UserName = employeeViewModel.Code,
                Email = employeeViewModel.Email!,
                FirstName = employeeViewModel.FirstName,
                LastName = employeeViewModel.LastName,
                PhoneNumber = employeeViewModel.PhoneNumber,
                Password = employeeViewModel.Password,
                ConfirmPassword = employeeViewModel.ConfirmPassword
            };
            await UsersClient.CreateAsync(creatUserRequest);
            user = await UsersClient.GetByUserNameAsync(employeeViewModel.Code);
        }

        return user.Id.ToString();
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

        Context.AddEditModal.ForceRender();
    }

    private void FormRender()
    {
        Context.AddEditModal.ForceRender();
    }
}

public class EmployeeViewModel : UpdateEmployeeRequest
{
    public bool CreateOrDeleteUser { get; set; }
    public string Password { get; set; } = default!;
    public string ConfirmPassword { get; set; } = default!;

    public bool PasswordCheck => !(
        !string.IsNullOrEmpty(Password) &&
        !string.IsNullOrEmpty(ConfirmPassword) &&
        (Password.Length > 5) &&
        (ConfirmPassword.Length > 5)
        && (Password == ConfirmPassword)
        ) && string.IsNullOrEmpty(UserId);

    public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
}