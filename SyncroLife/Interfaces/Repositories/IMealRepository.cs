using SyncroLife.Models;

namespace SyncroLife.Interfaces.Repositories
{
    public interface IMealRepository
    {
        Task<List<Meal>> GetAllAsync();
    }
}
