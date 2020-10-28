using Easy.Common.NetCore.Enums;

namespace Easy.Common.NetCore.Setting
{
    /// <summary>
    /// Cache Key统一管理处
    /// </summary>
    public static class CommonCacheKeys
    {
        /// <summary>
        /// 构建用户TokenKey
        /// </summary>
        public static string Build_UserToken_Key(int userId, DeviceType deviceType, bool isAdmin)
        {
            return $"UserToken:uid:{userId}|device:{deviceType}|admin:{isAdmin}";
        }

        /// <summary>
        /// 构建用于【查找指定父节点下的直接用户】的Key
        /// </summary>
        public static string Build_UserTree_PartOf_Puid_Key(long parentUserId)
        {
            return $"UserTree:PartOf:puid:{parentUserId}";
        }

        /// <summary>
        /// 构建用于【设置和获取用户树缓存节点】的Key
        /// </summary>
        public static string Build_UserTree_Key(long platformUserId)
        {
            return $"UserTree:uid:{platformUserId}";
        }
    }
}