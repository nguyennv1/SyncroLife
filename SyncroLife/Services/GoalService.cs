using SyncroLife.DTOs.Goal;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Interfaces.Services;
using SyncroLife.Models;

namespace SyncroLife.Services
{
    public class GoalService:IGoalService
    {
        private readonly IGoalRepository _goalRepository;

        public GoalService(IGoalRepository goalRepository)
        {
            _goalRepository = goalRepository;
        }

        public async Task<GoalResponseDTO> CreateGoalAsync(Guid userId,CreateGoalDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.GoalName))
            {
                throw new Exception("Goal name is required");
            }

            if (request.TargetValue <= 0)
            {
                throw new Exception("Target value must be greater than 0");
            }

            if (request.Deadline < request.StartDate)
            {
                throw new Exception("Deadline must be after Start Date");
            }

            var goal = new Goal
            {
                GoalId = Guid.NewGuid(),
                UserId = userId,

                GoalType = request.GoalType,
                GoalName = request.GoalName,

                TargetValue = request.TargetValue,
                Unit = request.Unit,

                StartDate = request.StartDate,
                Deadline = request.Deadline,

                CurrentValue = 0,
                ProgressPercentage = 0,

                IsActive = true,
                IsDeleted = false,

                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _goalRepository.AddAsync(goal);

            return new GoalResponseDTO
            {
                GoalId = goal.GoalId,

                GoalType = goal.GoalType,
                GoalName = goal.GoalName,

                TargetValue = goal.TargetValue,
                CurrentValue = goal.CurrentValue,

                Unit = goal.Unit,

                StartDate = goal.StartDate,
                Deadline = goal.Deadline,

                ProgressPercentage = goal.ProgressPercentage,

                IsActive = goal.IsActive,

                CreatedAt = goal.CreatedAt
            };
        }

        public async Task<List<GoalResponseDTO>> GetGoalsByUserAsync(Guid userId)
        {
            var goals = await _goalRepository.GetByUserIdAsync(userId);

            return goals.Select(x => new GoalResponseDTO
            {
                GoalId = x.GoalId,

                GoalType = x.GoalType,
                GoalName = x.GoalName,

                TargetValue = x.TargetValue,
                CurrentValue = x.CurrentValue,

                Unit = x.Unit,

                StartDate = x.StartDate,
                Deadline = x.Deadline,

                ProgressPercentage = x.ProgressPercentage,

                IsActive = x.IsActive,

                CreatedAt = x.CreatedAt

            }).ToList();
        }

        public async Task<GoalResponseDTO?> GetGoalByIdAsync(Guid userId, Guid goalId)
        {
            var goal = await _goalRepository.GetByIdAsync(goalId);

            if (goal == null)
                return null;

            if (goal.UserId != userId)
            {
                throw new Exception("You are not allowed to access this goal.");
            }

            return new GoalResponseDTO
            {
                GoalId = goal.GoalId,
                GoalType = goal.GoalType,
                GoalName = goal.GoalName,
                TargetValue = goal.TargetValue,
                CurrentValue = goal.CurrentValue,
                Unit = goal.Unit,
                StartDate = goal.StartDate,
                Deadline = goal.Deadline,
                ProgressPercentage = goal.ProgressPercentage,
                IsActive = goal.IsActive,
                CreatedAt = goal.CreatedAt
            };
        }

        public async Task<bool> UpdateGoalAsync(Guid userId, Guid goalId,UpdateGoalDTO request)
        {
            if (request.TargetValue <= 0)
            {
                throw new Exception("Target value must be greater than 0");
            }

            if (request.Deadline < request.StartDate)
            {
                throw new Exception("Deadline must be after Start Date");
            }

            var goal = await _goalRepository.GetByIdAsync(goalId);

            if (goal == null)
                return false;
            if (goal.UserId != userId)
            {
                throw new Exception("You are not allowed to update this goal.");
            }

            goal.GoalType = request.GoalType;

            goal.GoalName = request.GoalName;

            goal.TargetValue = request.TargetValue;

            goal.Unit = request.Unit;

            goal.StartDate = request.StartDate;

            goal.Deadline = request.Deadline;

            goal.IsActive = request.IsActive;

            goal.UpdatedAt = DateTime.UtcNow;

            await _goalRepository.UpdateAsync(goal);

            return true;
        }

        public async Task<bool> DeleteGoalAsync(Guid userId, Guid goalId)
        {
            var goal = await _goalRepository.GetByIdAsync(goalId);

            if (goal == null)
                return false;
            if (goal.UserId != userId)
            {
                throw new Exception("You are not allowed to delete this goal.");
            }

            goal.IsDeleted = true;
            goal.DeletedAt = DateTime.UtcNow;
            goal.UpdatedAt = DateTime.UtcNow;

            await _goalRepository.UpdateAsync(goal);

            return true;
        }
    }
}
