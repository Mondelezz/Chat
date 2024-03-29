﻿using Quantum.GroupFolder.Models;
using System.Text.Json.Serialization;

namespace Quantum.UserP.Models
{
    public class UserInfoOutput
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        [JsonIgnore]
        public ICollection<UserFriends> Friend { get; set; } = new List<UserFriends>();
        [JsonIgnore]
        public ICollection<GroupRequestUserInfoOutput> GroupRequests { get; set; } = new List<GroupRequestUserInfoOutput>();
    }
}
