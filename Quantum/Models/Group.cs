namespace Quantum.Models
{
    public class Group
    {
        public string GroupName { get; set; } = string.Empty;
        public Guid GroupId { get; set; }
        public List<UserInfoOutput> Users { get; set; } = new List<UserInfoOutput>();
        public UserInfoOutput User { get; set; } = new UserInfoOutput();
        public Guid UserId { get; set; }
     

    }
}
