using BCrypt.Net;
using SyncroLife.DTOs.Auth;
using SyncroLife.DTOs.User;
using SyncroLife.Helpers;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Interfaces.Services;
using SyncroLife.Models;
using Microsoft.Extensions.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth;

namespace SyncroLife.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly JwtHelper _jwtHelper;
    private readonly IConfiguration _configuration;
    private readonly IGoogleCalendarService _googleCalendarService;

    public AuthService(
        IAuthRepository authRepository, 
        JwtHelper jwtHelper,
        IConfiguration configuration,
        IGoogleCalendarService googleCalendarService)
    {
        _authRepository = authRepository;
        _jwtHelper = jwtHelper;
        _configuration = configuration;
        _googleCalendarService = googleCalendarService;
    }

    public async Task<LoginResponseDTO> GoogleLoginAsync(GoogleLoginRequestDTO request)
    {
        var clientId = _configuration["Google:ClientId"];
        var clientSecret = _configuration["Google:ClientSecret"];

        if (!string.IsNullOrEmpty(request.ClientId))
        {
            var webClientId = _configuration["Google:WebClientId"];
            if (request.ClientId == webClientId)
            {
                clientId = webClientId;
                clientSecret = _configuration["Google:WebClientSecret"];
            }
        }

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            throw new Exception("Google OAuth credentials are not configured in appsettings.json.");
        }

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            }
        });

        string redirectUri = "postmessage";
        if (request.RedirectUri != null)
        {
            redirectUri = request.RedirectUri;
        }
        else if (string.IsNullOrEmpty(request.ClientId))
        {
            redirectUri = "";
        }

        TokenResponse tokenResponse = await flow.ExchangeCodeForTokenAsync(
            userId: "user-id-placeholder",
            code: request.ServerAuthCode,
            redirectUri: redirectUri,
            CancellationToken.None
        );

        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.IdToken))
        {
            throw new Exception("Failed to exchange server auth code for tokens.");
        }

        var payload = await GoogleJsonWebSignature.ValidateAsync(tokenResponse.IdToken);
        if (payload == null)
        {
            throw new Exception("Invalid Google identity token.");
        }

        string googleId = payload.Subject;
        string email = payload.Email ?? request.Email ?? "";
        string name = payload.Name ?? request.FullName ?? email.Split('@')[0];

        var user = await _authRepository.GetByGoogleIdAsync(googleId);

        if (user == null)
        {
            var customerRole = await _authRepository.GetRoleByNameAsync("Customer");
            if (customerRole == null)
            {
                throw new Exception("Customer role not found.");
            }

            user = new User
            {
                UserId = Guid.NewGuid(),
                RoleId = customerRole.RoleId,
                Username = email,
                Email = email,
                FullName = name,
                GoogleId = googleId,
                GoogleAccessToken = tokenResponse.AccessToken,
                GoogleRefreshToken = tokenResponse.RefreshToken,
                GoogleTokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresInSeconds ?? 3600),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _authRepository.CreateUserAsync(user);
        }
        else
        {
            user.GoogleAccessToken = tokenResponse.AccessToken;
            if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
            {
                user.GoogleRefreshToken = tokenResponse.RefreshToken;
            }
            user.GoogleTokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresInSeconds ?? 3600);
            
            await _authRepository.UpdateUserAsync(user);
        }

        // Sync calendar synchronously to ensure the first fetch returns updated data
        try
        {
            await _googleCalendarService.SyncCalendarAsync(user.UserId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error syncing calendar on login: {ex.Message}");
        }

        string token = _jwtHelper.GenerateToken(
            user.UserId,
            user.Username,
            user.Role?.RoleName?.Trim() ?? "Customer"
        );

        return new LoginResponseDTO
        {
            UserId = user.UserId,
            Username = user.Username,
            Role = user.Role?.RoleName?.Trim() ?? "Customer",
            Token = token
        };
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
            Gender = user.Gender == "M" ? "Male" : (user.Gender == "F" ? "Female" : user.Gender),
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