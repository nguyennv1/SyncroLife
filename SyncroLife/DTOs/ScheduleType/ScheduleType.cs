namespace SyncroLife.DTOs.ScheduleType
{
    public class ScheduleTypeResponseDTO
    {
        public Guid TypeId { get; set; }

        public string TypeName { get; set; } = null!;

        public string? Description { get; set; }

        public string? EnergyLevel { get; set; }
    }
}