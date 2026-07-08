using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Interfaces.Services;
using SyncroLife.Models;

namespace SyncroLife.Services
{
    public class GoogleCalendarService : IGoogleCalendarService
    {
        private readonly IUserRepository _userRepository;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IScheduleTypeRepository _scheduleTypeRepository;
        private readonly IConfiguration _configuration;

        public GoogleCalendarService(
            IUserRepository userRepository,
            IScheduleRepository scheduleRepository,
            IScheduleTypeRepository scheduleTypeRepository,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _scheduleRepository = scheduleRepository;
            _scheduleTypeRepository = scheduleTypeRepository;
            _configuration = configuration;
        }

        public async Task SyncCalendarAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return;
            }

            // 1. Check if access token is expired or missing
            string accessToken = user.GoogleAccessToken ?? "";
            bool isTokenExpired = user.GoogleTokenExpiresAt == null || user.GoogleTokenExpiresAt.Value <= DateTime.UtcNow.AddMinutes(5);

            if (isTokenExpired || string.IsNullOrEmpty(accessToken))
            {
                if (string.IsNullOrEmpty(user.GoogleRefreshToken))
                {
                    throw new Exception("Google Calendar authorization has expired. Please sign out and sign in again to re-authorize Google Calendar.");
                }
                accessToken = await RefreshGoogleTokenAsync(user);
            }

            if (string.IsNullOrEmpty(accessToken))
            {
                throw new Exception("Unable to refresh Google access token.");
            }

            // 2. Initialize Google Calendar Service
            var credential = GoogleCredential.FromAccessToken(accessToken);
            var calendarService = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "SyncroLife",
            });

            // 3. Fetch list of events (from -180 days to +180 days)
            var request = calendarService.Events.List("primary");
            request.TimeMinDateTimeOffset = DateTimeOffset.UtcNow.AddDays(-180);
            request.TimeMaxDateTimeOffset = DateTimeOffset.UtcNow.AddDays(180);
            request.SingleEvents = true;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            var events = await request.ExecuteAsync();
            if (events?.Items == null)
            {
                return;
            }

            // 4. Get default and specific schedule types
            var types = await _scheduleTypeRepository.GetAllAsync();
            var gymType = types.FirstOrDefault(t => t.TypeName.Trim().ToLower().Contains("gym") || t.TypeName.Trim().ToLower().Contains("workout") || t.TypeName.Trim().ToLower().Contains("exercise") || t.TypeName.Trim().ToLower().Contains("cardio") || t.TypeName.Trim().ToLower().Contains("run"));
            var mealType = types.FirstOrDefault(t => t.TypeName.Trim().ToLower().Contains("meal") || t.TypeName.Trim().ToLower().Contains("food") || t.TypeName.Trim().ToLower().Contains("diet"));
            var sleepType = types.FirstOrDefault(t => t.TypeName.Trim().ToLower().Contains("sleep") || t.TypeName.Trim().ToLower().Contains("rest"));
            var workType = types.FirstOrDefault(t => t.TypeName.Trim().ToLower().Contains("work") || t.TypeName.Trim().ToLower().Contains("study") || t.TypeName.Trim().ToLower().Contains("meet") || t.TypeName.Trim().ToLower().Contains("office"));

            var defaultType = types.FirstOrDefault(t => t.TypeName.Trim().ToLower().Contains("task"))
                           ?? types.FirstOrDefault(t => t.TypeName.Trim().ToLower().Contains("todo"))
                           ?? types.FirstOrDefault(t => t.TypeName.Trim().Equals("Google Calendar", StringComparison.OrdinalIgnoreCase))
                           ?? types.FirstOrDefault(t => t.TypeName.Trim().ToLower().Contains("general"))
                           ?? types.FirstOrDefault(t => t.TypeName.Trim().ToLower().Contains("default"))
                           ?? types.FirstOrDefault(t => !t.TypeName.Trim().ToLower().Contains("gym") && 
                                                        !t.TypeName.Trim().ToLower().Contains("workout") && 
                                                        !t.TypeName.Trim().ToLower().Contains("cardio") && 
                                                        !t.TypeName.Trim().ToLower().Contains("run") && 
                                                        !t.TypeName.Trim().ToLower().Contains("exercise") && 
                                                        !t.TypeName.Trim().ToLower().Contains("meal") && 
                                                        !t.TypeName.Trim().ToLower().Contains("food") && 
                                                        !t.TypeName.Trim().ToLower().Contains("sleep"))
                           ?? workType
                           ?? types.FirstOrDefault();

            if (defaultType == null)
            {
                throw new Exception("No active schedule type found in the system.");
            }

            // 5. Sync events to database
            foreach (var ev in events.Items)
            {
                if (ev.Start == null || ev.End == null) continue;

                // Determine start and end times (using DateTimeOffset to parse timezone correctly, then convert to UTC)
                DateTime startTimeUtc;
                DateTime endTimeUtc;

                if (ev.Start.DateTimeDateTimeOffset.HasValue)
                {
                    startTimeUtc = ev.Start.DateTimeDateTimeOffset.Value.UtcDateTime;
                }
                else if (!string.IsNullOrEmpty(ev.Start.Date))
                {
                    // All-day event
                    startTimeUtc = DateTime.SpecifyKind(DateTime.Parse(ev.Start.Date), DateTimeKind.Utc);
                }
                else
                {
                    continue;
                }

                if (ev.End.DateTimeDateTimeOffset.HasValue)
                {
                    endTimeUtc = ev.End.DateTimeDateTimeOffset.Value.UtcDateTime;
                }
                else if (!string.IsNullOrEmpty(ev.End.Date))
                {
                    endTimeUtc = DateTime.SpecifyKind(DateTime.Parse(ev.End.Date), DateTimeKind.Utc);
                }
                else
                {
                    endTimeUtc = startTimeUtc.AddHours(1);
                }

                // Analyze title and description to map to correct schedule type
                string titleLower = (ev.Summary ?? "").ToLower();
                string descLower = (ev.Description ?? "").ToLower();
                Guid typeId = defaultType.TypeId;

                bool isGym = titleLower.Contains("gym") || titleLower.Contains("workout") || titleLower.Contains("exercise") ||
                             titleLower.Contains("cardio") || titleLower.Contains("yoga") || titleLower.Contains("football") ||
                             titleLower.Contains("swimming") || titleLower.Contains("sports") || titleLower.Contains("tập gym") ||
                             titleLower.Contains("chạy bộ") || titleLower.Contains("vận động") ||
                             descLower.Contains("gym") || descLower.Contains("workout") || descLower.Contains("tập gym") || descLower.Contains("chạy bộ");

                bool isMeal = titleLower.Contains("meal") || titleLower.Contains("food") || titleLower.Contains("lunch") ||
                              titleLower.Contains("dinner") || titleLower.Contains("breakfast") || titleLower.Contains("ăn") ||
                              titleLower.Contains("uống") || descLower.Contains("meal") || descLower.Contains("food");

                bool isSleep = titleLower.Contains("sleep") || titleLower.Contains("nap") || titleLower.Contains("ngủ") ||
                               descLower.Contains("sleep") || descLower.Contains("nap") || descLower.Contains("ngủ");

                bool isWork = titleLower.Contains("work") || titleLower.Contains("meet") || titleLower.Contains("study") ||
                              titleLower.Contains("office") || titleLower.Contains("làm việc") || titleLower.Contains("họp") ||
                              descLower.Contains("work") || descLower.Contains("meet") || descLower.Contains("họp");

                if (isGym && gymType != null)
                {
                    typeId = gymType.TypeId;
                }
                else if (isMeal && mealType != null)
                {
                    typeId = mealType.TypeId;
                }
                else if (isSleep && sleepType != null)
                {
                    typeId = sleepType.TypeId;
                }
                else if (isWork && workType != null)
                {
                    typeId = workType.TypeId;
                }

                // Check if already exists by GoogleEventId
                var existing = await _scheduleRepository.GetByGoogleEventIdAsync(userId, ev.Id);
                string descriptionText = !string.IsNullOrWhiteSpace(ev.Description) ? ev.Description : "No description";
                if (descriptionText.Length > 500)
                {
                    descriptionText = descriptionText.Substring(0, 497) + "...";
                }

                if (existing != null)
                {
                    // Update
                    existing.Title = !string.IsNullOrEmpty(ev.Summary) ? ev.Summary : "(No title)";
                    existing.Description = descriptionText;
                    existing.StartTime = startTimeUtc;
                    existing.EndTime = endTimeUtc;
                    existing.TypeId = typeId;
                    existing.UpdatedAt = DateTime.UtcNow;

                    await _scheduleRepository.UpdateAsync(existing);
                }
                else
                {
                    // Create new
                    var newSchedule = new Schedule
                    {
                        ScheduleId = Guid.NewGuid(),
                        UserId = userId,
                        TypeId = typeId,
                        Title = !string.IsNullOrEmpty(ev.Summary) ? ev.Summary : "(No title)",
                        Description = descriptionText,
                        StartTime = startTimeUtc,
                        EndTime = endTimeUtc,
                        GoogleEventId = ev.Id,
                        IsCompleted = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _scheduleRepository.AddAsync(newSchedule);
                }
            }

            // 6. Identify and soft-delete events that were removed from Google Calendar
            var timeMin = DateTime.UtcNow.AddDays(-180);
            var timeMax = DateTime.UtcNow.AddDays(180);

            var googleEventIds = new HashSet<string>(
                events.Items
                    .Where(ev => !string.IsNullOrEmpty(ev.Id))
                    .Select(ev => ev.Id)
            );

            var dbSchedules = await _scheduleRepository.GetByUserIdAsync(userId);
            foreach (var schedule in dbSchedules)
            {
                if (!string.IsNullOrEmpty(schedule.GoogleEventId) &&
                    schedule.EndTime > timeMin &&
                    schedule.StartTime < timeMax &&
                    !googleEventIds.Contains(schedule.GoogleEventId))
                {
                    schedule.IsDeleted = true;
                    schedule.DeletedAt = DateTime.UtcNow;
                    await _scheduleRepository.UpdateAsync(schedule);
                }
            }
        }

        private async Task<string> RefreshGoogleTokenAsync(User user)
        {
            var clientId = _configuration["Google:ClientId"];
            var clientSecret = _configuration["Google:ClientSecret"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                throw new Exception("Google OAuth ClientId or ClientSecret is not configured in appsettings.json.");
            }

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                }
            });

            var tokenResponse = new TokenResponse
            {
                RefreshToken = user.GoogleRefreshToken,
                AccessToken = user.GoogleAccessToken,
                IssuedUtc = DateTime.UtcNow
            };

            var credential = new UserCredential(flow, user.UserId.ToString(), tokenResponse);
            bool success = false;
            try
            {
                success = await credential.RefreshTokenAsync(CancellationToken.None);
            }
            catch
            {
                // Ignore and try fallback
            }

            if (!success)
            {
                var webClientId = _configuration["Google:WebClientId"];
                var webClientSecret = _configuration["Google:WebClientSecret"];
                if (!string.IsNullOrEmpty(webClientId) && !string.IsNullOrEmpty(webClientSecret))
                {
                    var webFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                    {
                        ClientSecrets = new ClientSecrets
                        {
                            ClientId = webClientId,
                            ClientSecret = webClientSecret
                        }
                    });

                    var webCredential = new UserCredential(webFlow, user.UserId.ToString(), tokenResponse);
                    try
                    {
                        success = await webCredential.RefreshTokenAsync(CancellationToken.None);
                        if (success && webCredential.Token != null)
                        {
                            credential = webCredential;
                        }
                    }
                    catch
                    {
                        // Ignore
                    }
                }
            }

            if (success && credential.Token != null)
            {
                user.GoogleAccessToken = credential.Token.AccessToken;
                if (!string.IsNullOrEmpty(credential.Token.RefreshToken))
                {
                    user.GoogleRefreshToken = credential.Token.RefreshToken;
                }
                user.GoogleTokenExpiresAt = DateTime.UtcNow.AddSeconds(credential.Token.ExpiresInSeconds ?? 3600);

                await _userRepository.UpdateAsync(user);
                return credential.Token.AccessToken;
            }

            return "";
        }
    }
}
