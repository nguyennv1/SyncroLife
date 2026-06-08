using SyncroLife.DTOs.ScheduleType;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Interfaces.Services;

namespace SyncroLife.Services
{
    public class ScheduleTypeService : IScheduleTypeService
    {
        private readonly IScheduleTypeRepository _repository;
        public ScheduleTypeService(IScheduleTypeRepository repository)
        {
            _repository = repository;
        }
        public async Task<List<ScheduleTypeResponseDTO>> GetAllAsync()
        {
            var types = await _repository.GetAllAsync();

            return types.Select(x => new ScheduleTypeResponseDTO
            {
                TypeId = x.TypeId,
                TypeName = x.TypeName,
                Description = x.Description,
                EnergyLevel = x.EnergyLevel
            }).ToList();
        }
    }
}
