using System;

namespace Easy.Common.NetCore.Security
{
    [Serializable]
    public class UserIdentity
    {
        public int UserId { get; set; }

        public string UserName { get; set; }

        public string Token { get; set; }

        public DateTime? TokenExpireTime { get; set; }

        public bool Equals(UserIdentity other)
        {
            return this.UserId == other.UserId;
        }
    }
}