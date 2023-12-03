using Quantum.Models;
using System.ComponentModel.DataAnnotations;

public class UserFriends
{
    [Key]
    public int Id { get; set; }

    public User User { get; set; }
    public Guid UserId { get; set; }

    public UserInfoOutput Friends { get; set; }
    public Guid FriendId { get; set; }
}
