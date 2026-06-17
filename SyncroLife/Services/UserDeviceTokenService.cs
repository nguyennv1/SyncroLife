using SyncroLife.DTOs.DeviceToken;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Interfaces.Services;
using SyncroLife.Models;

namespace SyncroLife.Services
{
    public class UserDeviceTokenService : IUserDeviceTokenService
    {
        private readonly IUserDeviceTokenRepository _deviceTokenRepository;

        public UserDeviceTokenService(
            IUserDeviceTokenRepository deviceTokenRepository)
        {
            _deviceTokenRepository = deviceTokenRepository;
        }

        public async Task RegisterTokenAsync(
            Guid userId,
            RegisterDeviceTokenDTO dto)
        {
            var existingToken =
                await _deviceTokenRepository
                    .GetByUserIdAsync(userId);

            if (existingToken == null)
            {
                var token = new UserDeviceToken
                {
                    DeviceTokenId = Guid.NewGuid(),
                    UserId = userId,
                    FcmToken = dto.FcmToken,
                };

                await _deviceTokenRepository.AddAsync(token);
            }
            else
            {
                existingToken.FcmToken = dto.FcmToken;

                await _deviceTokenRepository
                    .UpdateAsync(existingToken);
            }
        }
    }
}