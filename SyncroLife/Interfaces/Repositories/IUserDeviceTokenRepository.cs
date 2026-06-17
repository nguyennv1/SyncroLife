using SyncroLife.Models;

namespace SyncroLife.Interfaces.Repositories
{
    public interface IUserDeviceTokenRepository
    {
        Task<UserDeviceToken?> GetByUserIdAsync(Guid userId);

        Task AddAsync(UserDeviceToken token);

        Task UpdateAsync(UserDeviceToken token);
    }
}
