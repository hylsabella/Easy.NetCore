using Easy.Common.NetCore.Enums;
using System;

namespace Easy.Common.NetCore.Security
{
    public interface ITokenSvc
    {
        /// <summary>
        /// 生成新Token
        /// </summary>
        string CreateNewToken(TokenModel model);

        /// <summary>
        /// 解码Token
        /// </summary>
        TokenModel DecodeToken(string token);

        /// <summary>
        /// 更新用户Token
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="userName">用户名</param>
        /// <param name="deviceType">平台</param>
        /// <param name="isAdmin">是否是管理员</param>
        /// <returns>新的用户token</returns>
        string 更新用户Token(int userId, string userName, DeviceType deviceType, bool isAdmin, TimeSpan? tokenExpireTime = null);

        bool 检查用户登陆是否合法(int userId, DeviceType deviceType, string userToken, bool isAdmin, bool isSingleLogin, out string errorMsg);

        /// <summary>
        /// 清除指定用户token
        /// </summary>
        void ClearUserToken(int userId, DeviceType deviceType, bool isAdmin);

        /// <summary>
        /// 清除指定用户所有token
        /// </summary>
        void ClearUserAllToken(int userId);
    }
}