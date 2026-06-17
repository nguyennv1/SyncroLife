namespace SyncroLife.Models
{
    public class UserDeviceToken
    {
        public Guid DeviceTokenId { get; set; }

        public Guid UserId { get; set; }

        public string FcmToken { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
