using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SyncroLife.Models;

namespace SyncroLife.Data;

public partial class SyncroLifeDbContext : DbContext
{
    public SyncroLifeDbContext(DbContextOptions<SyncroLifeDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActiveSubscription> ActiveSubscriptions { get; set; }

    public virtual DbSet<AiRecommendation> AiRecommendations { get; set; }

    public virtual DbSet<DietaryPreference> DietaryPreferences { get; set; }

    public virtual DbSet<FoodAnalysis> FoodAnalyses { get; set; }

    public virtual DbSet<Goal> Goals { get; set; }

    public virtual DbSet<Habit> Habits { get; set; }

    public virtual DbSet<Meal> Meals { get; set; }

    public virtual DbSet<MealPlan> MealPlans { get; set; }

    public virtual DbSet<MealPlanDetail> MealPlanDetails { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Reminder> Reminders { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<ScheduleType> ScheduleTypes { get; set; }

    public virtual DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserDailySchedule> UserDailySchedules { get; set; }
    public virtual DbSet<UserDeviceToken> UserDeviceTokens { get; set; }

    public virtual DbSet<UserDietaryPreference> UserDietaryPreferences { get; set; }

    public virtual DbSet<UserProfileSummary> UserProfileSummaries { get; set; }

    public virtual DbSet<UserSubscription> UserSubscriptions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<ActiveSubscription>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("active_subscriptions");

            entity.Property(e => e.DaysRemaining).HasColumnName("days_remaining");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.PlanName)
                .HasMaxLength(100)
                .HasColumnName("plan_name");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserSubId).HasColumnName("user_sub_id");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");
        });

        modelBuilder.Entity<AiRecommendation>(entity =>
        {
            entity.HasKey(e => e.RecommendationId).HasName("ai_recommendation_pkey");

            entity.ToTable("ai_recommendation");

            entity.HasIndex(e => e.ContextSnapshot, "idx_ai_recommendation_context").HasMethod("gin");

            entity.HasIndex(e => e.CreatedAt, "idx_ai_recommendation_created_at").IsDescending();

            entity.HasIndex(e => new { e.UserId, e.UserFeedback }, "idx_ai_recommendation_feedback");

            entity.HasIndex(e => e.Score, "idx_ai_recommendation_score");

            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "idx_ai_recommendation_user_date").IsDescending(false, true);

            entity.HasIndex(e => new { e.UserId, e.UserFeedback }, "idx_ai_recommendation_user_feedback");

            entity.HasIndex(e => e.UserId, "idx_ai_recommendation_user_id");

            entity.Property(e => e.RecommendationId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("recommendation_id");
            entity.Property(e => e.ContextSnapshot)
                .HasColumnType("jsonb")
                .HasColumnName("context_snapshot");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.GoalId).HasColumnName("goal_id");
            entity.Property(e => e.HabitId).HasColumnName("habit_id");
            entity.Property(e => e.MealId).HasColumnName("meal_id");
            entity.Property(e => e.PreferenceId).HasColumnName("preference_id");
            entity.Property(e => e.Reasoning)
                .HasMaxLength(1000)
                .HasColumnName("reasoning");
            entity.Property(e => e.RecommendationType)
                .HasMaxLength(50)
                .HasColumnName("recommendation_type");
            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
            entity.Property(e => e.Score)
                .HasPrecision(3, 2)
                .HasDefaultValue(0.5m)
                .HasColumnName("score");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserFeedback)
                .HasMaxLength(50)
                .HasColumnName("user_feedback");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Goal).WithMany(p => p.AiRecommendations)
                .HasForeignKey(d => d.GoalId)
                .HasConstraintName("ai_recommendation_goal_id_fkey");

            entity.HasOne(d => d.Habit).WithMany(p => p.AiRecommendations)
                .HasForeignKey(d => d.HabitId)
                .HasConstraintName("ai_recommendation_habit_id_fkey");

            entity.HasOne(d => d.Meal).WithMany(p => p.AiRecommendations)
                .HasForeignKey(d => d.MealId)
                .HasConstraintName("ai_recommendation_meal_id_fkey");

            entity.HasOne(d => d.Preference).WithMany(p => p.AiRecommendations)
                .HasForeignKey(d => d.PreferenceId)
                .HasConstraintName("ai_recommendation_preference_id_fkey");

            entity.HasOne(d => d.Schedule).WithMany(p => p.AiRecommendations)
                .HasForeignKey(d => d.ScheduleId)
                .HasConstraintName("ai_recommendation_schedule_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.AiRecommendations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ai_recommendation_user_id_fkey");
        });

        modelBuilder.Entity<DietaryPreference>(entity =>
        {
            entity.HasKey(e => e.PreferenceId).HasName("dietary_preference_pkey");

            entity.ToTable("dietary_preference");

            entity.HasIndex(e => e.PreferenceName, "dietary_preference_preference_name_key").IsUnique();

            entity.Property(e => e.PreferenceId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("preference_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.PreferenceName)
                .HasMaxLength(255)
                .HasColumnName("preference_name");
            entity.Property(e => e.PreferenceType)
                .HasMaxLength(50)
                .HasColumnName("preference_type");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<FoodAnalysis>(entity =>
        {
            entity.HasKey(e => e.AnalysisId).HasName("food_analysis_pkey");

            entity.ToTable("food_analysis");

            entity.HasIndex(e => e.CreatedAt, "idx_food_analysis_created_at");

            entity.HasIndex(e => e.MealId, "idx_food_analysis_meal_id");

            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "idx_food_analysis_user_date").IsDescending(false, true);

            entity.HasIndex(e => e.UserId, "idx_food_analysis_user_id");

            entity.Property(e => e.AnalysisId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("analysis_id");
            entity.Property(e => e.AiNotes).HasColumnName("ai_notes");
            entity.Property(e => e.AnalysisResult).HasColumnName("analysis_result");
            entity.Property(e => e.Calories).HasColumnName("calories");
            entity.Property(e => e.Carbs)
                .HasPrecision(5, 2)
                .HasColumnName("carbs");
            entity.Property(e => e.ConfidenceScore)
                .HasPrecision(3, 2)
                .HasColumnName("confidence_score");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DetectedMealName)
                .HasMaxLength(255)
                .HasColumnName("detected_meal_name");
            entity.Property(e => e.Fats)
                .HasPrecision(5, 2)
                .HasColumnName("fats");
            entity.Property(e => e.Fiber)
                .HasPrecision(5, 2)
                .HasColumnName("fiber");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.MealId).HasColumnName("meal_id");
            entity.Property(e => e.Protein)
                .HasPrecision(5, 2)
                .HasColumnName("protein");
            entity.Property(e => e.Sodium)
                .HasPrecision(5, 2)
                .HasColumnName("sodium");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Meal).WithMany(p => p.FoodAnalyses)
                .HasForeignKey(d => d.MealId)
                .HasConstraintName("food_analysis_meal_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.FoodAnalyses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("food_analysis_user_id_fkey");
        });

        modelBuilder.Entity<Goal>(entity =>
        {
            entity.HasKey(e => e.GoalId).HasName("goal_pkey");

            entity.ToTable("goal");

            entity.HasIndex(e => e.IsActive, "idx_goal_is_active");

            entity.HasIndex(e => e.IsDeleted, "idx_goal_is_deleted");

            entity.HasIndex(e => new { e.UserId, e.IsDeleted }, "idx_goal_user_deleted");

            entity.HasIndex(e => e.UserId, "idx_goal_user_id");

            entity.Property(e => e.GoalId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("goal_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrentValue)
                .HasPrecision(10, 2)
                .HasColumnName("current_value");
            entity.Property(e => e.Deadline).HasColumnName("deadline");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.GoalName)
                .HasMaxLength(255)
                .HasColumnName("goal_name");
            entity.Property(e => e.GoalType)
                .HasMaxLength(50)
                .HasColumnName("goal_type");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.ProgressPercentage)
                .HasDefaultValue(0)
                .HasColumnName("progress_percentage");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.TargetValue)
                .HasPrecision(10, 2)
                .HasColumnName("target_value");
            entity.Property(e => e.Unit)
                .HasMaxLength(50)
                .HasColumnName("unit");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Goals)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("goal_user_id_fkey");
        });

        modelBuilder.Entity<Habit>(entity =>
        {
            entity.HasKey(e => e.HabitId).HasName("habit_pkey");

            entity.ToTable("habit");

            entity.HasIndex(e => e.IsDeleted, "idx_habit_is_deleted");

            entity.HasIndex(e => new { e.UserId, e.IsDeleted }, "idx_habit_user_deleted");

            entity.HasIndex(e => e.UserId, "idx_habit_user_id");

            entity.Property(e => e.HabitId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("habit_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.FrequencyCount).HasColumnName("frequency_count");
            entity.Property(e => e.FrequencyUnit)
                .HasMaxLength(50)
                .HasColumnName("frequency_unit");
            entity.Property(e => e.HabitName)
                .HasMaxLength(255)
                .HasColumnName("habit_name");
            entity.Property(e => e.HabitType)
                .HasMaxLength(50)
                .HasColumnName("habit_type");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.IsPositive)
                .HasDefaultValue(true)
                .HasColumnName("is_positive");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Habits)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("habit_user_id_fkey");
        });

        modelBuilder.Entity<Meal>(entity =>
        {
            entity.HasKey(e => e.MealId).HasName("meal_pkey");

            entity.ToTable("meal");

            entity.HasIndex(e => e.Calories, "idx_meal_calories");

            entity.HasIndex(e => e.Category, "idx_meal_category");

            entity.HasIndex(e => e.IsDeleted, "idx_meal_deleted");

            entity.HasIndex(e => e.IsDeleted, "idx_meal_is_deleted");

            entity.HasIndex(e => e.Protein, "idx_meal_protein");

            entity.Property(e => e.MealId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("meal_id");
            entity.Property(e => e.Calories).HasColumnName("calories");
            entity.Property(e => e.Carbs)
                .HasPrecision(5, 2)
                .HasColumnName("carbs");
            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .HasColumnName("category");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Fats)
                .HasPrecision(5, 2)
                .HasColumnName("fats");
            entity.Property(e => e.Fiber)
                .HasPrecision(5, 2)
                .HasColumnName("fiber");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.IsVegan)
                .HasDefaultValue(false)
                .HasColumnName("is_vegan");
            entity.Property(e => e.IsVegetarian)
                .HasDefaultValue(false)
                .HasColumnName("is_vegetarian");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.PortionSize)
                .HasMaxLength(100)
                .HasColumnName("portion_size");
            entity.Property(e => e.Protein)
                .HasPrecision(5, 2)
                .HasColumnName("protein");
            entity.Property(e => e.Sodium)
                .HasPrecision(5, 2)
                .HasColumnName("sodium");
            entity.Property(e => e.Tags).HasColumnName("tags");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<MealPlan>(entity =>
        {
            entity.HasKey(e => e.MealPlanId).HasName("meal_plan_pkey");

            entity.ToTable("meal_plan");

            entity.HasIndex(e => e.PlanDate, "idx_meal_plan_plan_date");

            entity.HasIndex(e => e.UserId, "idx_meal_plan_user_id");

            entity.HasIndex(e => new { e.UserId, e.PlanDate }, "unique_user_plan_date").IsUnique();

            entity.Property(e => e.MealPlanId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("meal_plan_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.IsCompleted)
                .HasDefaultValue(false)
                .HasColumnName("is_completed");
            entity.Property(e => e.Notes)
                .HasMaxLength(500)
                .HasColumnName("notes");
            entity.Property(e => e.PlanDate).HasColumnName("plan_date");
            entity.Property(e => e.PlanType)
                .HasMaxLength(50)
                .HasDefaultValueSql("'standard'::character varying")
                .HasColumnName("plan_type");
            entity.Property(e => e.TotalCalories).HasColumnName("total_calories");
            entity.Property(e => e.TotalCarbs)
                .HasPrecision(7, 2)
                .HasColumnName("total_carbs");
            entity.Property(e => e.TotalFats)
                .HasPrecision(7, 2)
                .HasColumnName("total_fats");
            entity.Property(e => e.TotalProtein)
                .HasPrecision(7, 2)
                .HasColumnName("total_protein");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.MealPlans)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("meal_plan_user_id_fkey");
        });

        modelBuilder.Entity<MealPlanDetail>(entity =>
        {
            entity.HasKey(e => e.DetailId).HasName("meal_plan_detail_pkey");

            entity.ToTable("meal_plan_detail");

            entity.HasIndex(e => e.MealPlanId, "idx_meal_plan_detail_meal_plan_id");

            entity.HasIndex(e => e.MealTime, "idx_meal_plan_detail_meal_time");

            entity.HasIndex(e => new { e.MealPlanId, e.MealId, e.MealTime }, "unique_meal_plan_detail").IsUnique();

            entity.Property(e => e.DetailId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("detail_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.IsCompleted)
                .HasDefaultValue(false)
                .HasColumnName("is_completed");
            entity.Property(e => e.MealId).HasColumnName("meal_id");
            entity.Property(e => e.MealPlanId).HasColumnName("meal_plan_id");
            entity.Property(e => e.MealTime)
                .HasMaxLength(50)
                .HasColumnName("meal_time");
            entity.Property(e => e.Notes)
                .HasMaxLength(500)
                .HasColumnName("notes");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");

            entity.HasOne(d => d.Meal).WithMany(p => p.MealPlanDetails)
                .HasForeignKey(d => d.MealId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("meal_plan_detail_meal_id_fkey");

            entity.HasOne(d => d.MealPlan).WithMany(p => p.MealPlanDetails)
                .HasForeignKey(d => d.MealPlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("meal_plan_detail_meal_plan_id_fkey");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("notification_pkey");

            entity.ToTable("notification");

            entity.HasIndex(e => e.IsRead, "idx_notification_is_read");

            entity.HasIndex(e => e.SentAt, "idx_notification_sent_at");

            entity.HasIndex(e => new { e.Status, e.UserId }, "idx_notification_status");

            entity.HasIndex(e => e.NotificationType, "idx_notification_type");

            entity.HasIndex(e => e.UserId, "idx_notification_user_id");

            entity.Property(e => e.NotificationId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("notification_id");
            entity.Property(e => e.Channel)
                .HasMaxLength(50)
                .HasDefaultValueSql("'push'::character varying")
                .HasColumnName("channel");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.NotificationType)
                .HasMaxLength(50)
                .HasDefaultValueSql("'reminder'::character varying")
                .HasColumnName("notification_type");
            entity.Property(e => e.ReadAt).HasColumnName("read_at");
            entity.Property(e => e.RecommendationId).HasColumnName("recommendation_id");
            entity.Property(e => e.ReminderId).HasColumnName("reminder_id");
            entity.Property(e => e.SentAt).HasColumnName("sent_at");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'pending'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Recommendation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.RecommendationId)
                .HasConstraintName("fk_notification_ai_recommendation");

            entity.HasOne(d => d.Reminder).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.ReminderId)
                .HasConstraintName("notification_reminder_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("notification_user_id_fkey");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("payment_pkey");

            entity.ToTable("payment");

            entity.HasIndex(e => e.PaymentDate, "idx_payment_payment_date");

            entity.HasIndex(e => e.Status, "idx_payment_status");

            entity.HasIndex(e => new { e.Status, e.PaymentDate }, "idx_payment_status_date").IsDescending(false, true);

            entity.HasIndex(e => e.TransactionId, "idx_payment_transaction_id");

            entity.HasIndex(e => e.UserSubId, "idx_payment_user_sub_id");

            entity.Property(e => e.PaymentId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("payment_id");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("payment_date");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("payment_method");
            entity.Property(e => e.RetryCount)
                .HasDefaultValue(0)
                .HasColumnName("retry_count");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'pending'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TransactionId)
                .HasMaxLength(255)
                .HasColumnName("transaction_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserSubId).HasColumnName("user_sub_id");

            entity.HasOne(d => d.UserSub).WithMany(p => p.Payments)
                .HasForeignKey(d => d.UserSubId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("payment_user_sub_id_fkey");
        });

        modelBuilder.Entity<Reminder>(entity =>
        {
            entity.HasKey(e => e.ReminderId).HasName("reminder_pkey");

            entity.ToTable("reminder");

            entity.HasIndex(e => e.IsSent, "idx_reminder_is_sent");

            entity.HasIndex(e => new { e.RemindAt, e.IsSent }, "idx_reminder_pending");

            entity.HasIndex(e => e.RemindAt, "idx_reminder_remind_at");

            entity.HasIndex(e => e.ScheduleId, "idx_reminder_schedule_id");

            entity.Property(e => e.ReminderId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("reminder_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.IsSent)
                .HasDefaultValue(false)
                .HasColumnName("is_sent");
            entity.Property(e => e.RemindAt).HasColumnName("remind_at");
            entity.Property(e => e.ReminderType)
                .HasMaxLength(50)
                .HasDefaultValueSql("'before_activity'::character varying")
                .HasColumnName("reminder_type");
            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");

            entity.HasOne(d => d.Schedule).WithMany(p => p.Reminders)
                .HasForeignKey(d => d.ScheduleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("reminder_schedule_id_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.HasIndex(e => e.RoleName, "roles_role_name_key").IsUnique();

            entity.Property(e => e.RoleId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("role_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("schedule_pkey");

            entity.ToTable("schedule");

            entity.HasIndex(e => e.EndTime, "idx_schedule_end_time");

            entity.HasIndex(e => e.IsDeleted, "idx_schedule_is_deleted");

            entity.HasIndex(e => e.StartTime, "idx_schedule_start_time");

            entity.HasIndex(e => new { e.UserId, e.IsDeleted }, "idx_schedule_user_deleted");

            entity.HasIndex(e => e.UserId, "idx_schedule_user_id");

            entity.Property(e => e.ScheduleId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("schedule_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.IsCompleted)
                .HasDefaultValue(false)
                .HasColumnName("is_completed");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.TypeId).HasColumnName("type_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.GoogleEventId)
                .HasMaxLength(255)
                .HasColumnName("google_event_id");

            entity.HasOne(d => d.Type).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("schedule_type_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("schedule_user_id_fkey");
        });

        modelBuilder.Entity<ScheduleType>(entity =>
        {
            entity.HasKey(e => e.TypeId).HasName("schedule_type_pkey");

            entity.ToTable("schedule_type");

            entity.Property(e => e.TypeId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("type_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.EnergyLevel)
                .HasMaxLength(50)
                .HasDefaultValueSql("'medium'::character varying")
                .HasColumnName("energy_level");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.TypeName)
                .HasMaxLength(100)
                .HasColumnName("type_name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.HasKey(e => e.PlanId).HasName("subscription_plan_pkey");

            entity.ToTable("subscription_plan");

            entity.Property(e => e.PlanId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("plan_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.DurationDays).HasColumnName("duration_days");
            entity.Property(e => e.Features).HasColumnName("features");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.PlanName)
                .HasMaxLength(100)
                .HasColumnName("plan_name");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.DateOfBirth, "idx_users_date_of_birth");

            entity.HasIndex(e => e.Email, "idx_users_email");

            entity.HasIndex(e => e.IsDeleted, "idx_users_is_deleted");

            entity.HasIndex(e => e.Username, "idx_users_username");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.HasIndex(e => e.Username, "users_username_key").IsUnique();

            entity.HasIndex(e => e.GoogleId, "users_google_id_key").IsUnique();

            entity.Property(e => e.UserId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.Gender)
                .HasMaxLength(1)
                .HasColumnName("gender");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");

            entity.Property(e => e.GoogleId)
                .HasMaxLength(255)
                .HasColumnName("google_id");

            entity.Property(e => e.GoogleAccessToken)
                .HasColumnName("google_access_token");

            entity.Property(e => e.GoogleRefreshToken)
                .HasColumnName("google_refresh_token");

            entity.Property(e => e.GoogleTokenExpiresAt)
                .HasColumnName("google_token_expires_at");

            entity.Property(e => e.Height)
                .HasColumnName("height");

            entity.Property(e => e.Weight)
                .HasColumnName("weight");

            entity.Property(e => e.TargetCalories)
                .HasColumnName("target_calories");

            entity.Property(e => e.MonthlyBudget)
                .HasColumnName("monthly_budget");


            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("users_role_id_fkey");
        });

        modelBuilder.Entity<UserDailySchedule>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("user_daily_schedule");

            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.EnergyLevel)
                .HasMaxLength(50)
                .HasColumnName("energy_level");
            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.TypeName)
                .HasMaxLength(100)
                .HasColumnName("type_name");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");
        });

        modelBuilder.Entity<UserDeviceToken>(entity =>
        {
            entity.HasKey(e => e.DeviceTokenId).HasName("user_device_token_pkey");

            entity.ToTable("user_device_token");

            entity.Property(e => e.DeviceTokenId)
                .ValueGeneratedNever()
                .HasColumnName("device_token_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.FcmToken).HasColumnName("fcm_token");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserDeviceTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_user_device_token_user");
        });

        modelBuilder.Entity<UserDietaryPreference>(entity =>
        {
            entity.HasKey(e => e.UserDietaryId).HasName("user_dietary_preference_pkey");

            entity.ToTable("user_dietary_preference");

            entity.HasIndex(e => e.UserId, "idx_user_dietary_preference_user_id");

            entity.HasIndex(e => new { e.UserId, e.PreferenceId }, "unique_user_preference").IsUnique();

            entity.Property(e => e.UserDietaryId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("user_dietary_id");
            entity.Property(e => e.AddedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("added_at");
            entity.Property(e => e.PreferenceId).HasColumnName("preference_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Preference).WithMany(p => p.UserDietaryPreferences)
                .HasForeignKey(d => d.PreferenceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_dietary_preference_preference_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserDietaryPreferences)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_dietary_preference_user_id_fkey");
        });

        modelBuilder.Entity<UserProfileSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("user_profile_summary");

            entity.Property(e => e.ActiveGoalsCount).HasColumnName("active_goals_count");
            entity.Property(e => e.Age).HasColumnName("age");
            entity.Property(e => e.DietaryPreferencesCount).HasColumnName("dietary_preferences_count");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.Gender)
                .HasMaxLength(1)
                .HasColumnName("gender");
            entity.Property(e => e.HabitsCount).HasColumnName("habits_count");
            entity.Property(e => e.PlanName)
                .HasMaxLength(100)
                .HasColumnName("plan_name");
            entity.Property(e => e.SubscriptionEndDate).HasColumnName("subscription_end_date");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");
        });

        modelBuilder.Entity<UserSubscription>(entity =>
        {
            entity.HasKey(e => e.UserSubId).HasName("user_subscription_pkey");

            entity.ToTable("user_subscription");

            entity.HasIndex(e => e.EndDate, "idx_user_subscription_end_date");

            entity.HasIndex(e => e.Status, "idx_user_subscription_status");

            entity.HasIndex(e => e.UserId, "idx_user_subscription_user_id");

            entity.HasIndex(e => new { e.UserId, e.Status }, "idx_user_subscription_user_status");

            entity.HasIndex(e => e.UserId, "ux_active_subscription")
                .IsUnique()
                .HasFilter("((status)::text = 'active'::text)");

            entity.Property(e => e.UserSubId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("user_sub_id");
            entity.Property(e => e.AutoRenew)
                .HasDefaultValue(true)
                .HasColumnName("auto_renew");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'pending'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Plan).WithMany(p => p.UserSubscriptions)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_subscription_plan_id_fkey");

            entity.HasOne(d => d.User).WithOne(p => p.UserSubscription)
                .HasForeignKey<UserSubscription>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_subscription_user_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
