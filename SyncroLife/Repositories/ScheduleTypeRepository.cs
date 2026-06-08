using Microsoft.EntityFrameworkCore;
using SyncroLife.Data;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Models;

namespace SyncroLife.Repositories
{
    public class ScheduleTypeRepository : IScheduleTypeRepository
    {
        private readonly SyncroLifeDbContext _context;

        public ScheduleTypeRepository(
            SyncroLifeDbContext context)
        {
            _context = context;
        }
        public async Task<List<ScheduleType>> GetAllAsync()
        {
            return await _context.ScheduleTypes
            .Where(x => x.IsActive != false)
            .ToListAsync();
        }
        public async Task<ScheduleType?> GetByIdAsync(Guid typeId)
        {
            return await _context.ScheduleTypes
                .FirstOrDefaultAsync(x =>
                    x.TypeId == typeId &&
                    x.IsActive == true);
        }
    }
}
