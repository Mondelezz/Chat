using Quantum.Models;

public class UserFriends
{
    public User? User { get; set; }
    public Guid UserId { get; set; }

    public UserInfoOutput? Friend { get; set; }
    public Guid FriendId { get; set; }
}
