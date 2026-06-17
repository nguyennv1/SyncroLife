using SyncroLife.Models;

namespace SyncroLife.Interfaces.Repositories
{
    public interface IUserDietaryPreferenceRepository
    {
        Task<List<DietaryPreference>>GetPreferencesByUserIdAsync(Guid userId);
    }
}
