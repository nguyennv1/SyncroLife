using BCrypt.Net;
using SyncroLife.DTOs.Auth;
using SyncroLife.DTOs.User;
using SyncroLife.Helpers;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Interfaces.Services;
using SyncroLife.Models;

namespace SyncroLife.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly JwtHelper _jwtHelper;

    public AuthService(IAuthRepository authRepository, JwtHelper jwtHelper)
    {
        _authRepository = authRepository;
        _jwtHelper = jwtHelper;
    }

    public async Task<UserResponseDTO> RegisterAsync(RegisterRequestDTO request)
    {
        var existingUser =
            await _authRepository.GetByUsernameAsync(request.Username);

        if (existingUser != null)
        {
            throw new Exception("Username already exists");
        }

        if (request.Password != request.ConfirmPassword)
        {
            throw new Exception("Passwords do not match");
        }

        var customerRole =
    await _authRepository.GetRoleByNameAsync("Customer");

        if (customerRole == null)
        {
            throw new Exception("Customer role not found");
        }

        var user = new User
        {
            UserId = Guid.NewGuid(),
            RoleId = customerRole.RoleId,
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _authRepository.CreateUserAsync(user);

        return new UserResponseDTO
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth
        };
    }

    public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request)
    {
        var user = await _authRepository.GetByUsernameAsync(request.Username);

        if (user == null)
        {
            throw new Exception("Invalid username or password");
        }

        bool verify = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash);

        if (!verify)
        {
            throw new Exception("Invalid username or password");
        }

        string token = _jwtHelper.GenerateToken(
            user.UserId,
            user.Username,
            user.Role.RoleName.Trim());

        return new LoginResponseDTO
        {
            UserId = user.UserId,
            Username = user.Username,
            Role = user.Role.RoleName,
            Token = token
        };
    }
}