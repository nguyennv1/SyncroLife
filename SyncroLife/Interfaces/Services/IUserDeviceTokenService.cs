using SyncroLife.DTOs.DeviceToken;

namespace SyncroLife.Interfaces.Services
{
    public interface IUserDeviceTokenService
    {
        Task RegisterTokenAsync(Guid userId, RegisterDeviceTokenDTO dto);
    }
}
