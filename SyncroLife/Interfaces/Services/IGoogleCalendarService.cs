using System;
using System.Threading.Tasks;

namespace SyncroLife.Interfaces.Services;

public interface IGoogleCalendarService
{
    Task SyncCalendarAsync(Guid userId);
}
