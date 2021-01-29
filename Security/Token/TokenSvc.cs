using Easy.Common.NetCore.Cache;
using Easy.Common.NetCore.Enums;
using Easy.Common.NetCore.Helpers;
using Easy.Common.NetCore.Setting;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Exceptions;
using Newtonsoft.Json;
using System;

namespace Easy.Common.NetCore.Security
{
    public class TokenSvc : ITokenSvc
    {
        private readonly IEasyCache _easyCache;
        private readonly TimeSpan _tokenExpiresTimeSpan = TimeSpan.FromDays(30);
        private readonly static string Secret = "730aa20593084799b3f1ea49417293cb";

        public TokenSvc(IEasyCache easyCache)
        {
            _easyCache = easyCache;
        }

        /// <summary>
        /// 生成新Token
        /// </summary>
        public string CreateNewToken(TokenModel model)
        {
            CheckHelper.NotNull(model, "model");
            CheckHelper.NotEmpty(model.UserName, "model.UserName");
            if (model.UserId <= 0) throw new ArgumentException("userId不能小于0");

            var token = new JwtBuilder()
                            .WithAlgorithm(new HMACSHA256Algorithm())
                            .WithSecret(Secret)
                            //.AddClaim("exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds())
                            .AddClaim("UserName", model.UserName)
                            .AddClaim("UserId", model.UserId)
                            .AddClaim("TokenExpireTime", model.TokenExpireTime)
                            .Encode();

            return token;
        }

        /// <summary>
        /// 解码Token
        /// </summary>
        public TokenModel DecodeToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            TokenModel tokenModel = null;

            try
            {
                var json = new JwtBuilder()
                    .WithAlgorithm(new HMACSHA256Algorithm())
                    .WithSecret(Secret)
                    .MustVerifySignature()
                    .Decode(token);

                tokenModel = JsonConvert.DeserializeObject<TokenModel>(json);

                return tokenModel;
            }
            catch (TokenExpiredException)
            {
            }
            catch (SignatureVerificationException)
            {
            }

            return tokenModel;
        }

        /// <summary>
        /// 更新用户Token
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="userName">用户名</param>
        /// <param name="deviceType">平台</param>
        /// <param name="isAdmin">是否是管理员</param>
        /// <param name="tokenExpireTime">token过期时间</param>
        /// <returns>新的用户token</returns>
        public string 更新用户Token(int userId, string userName, DeviceType deviceType, bool isAdmin, TimeSpan? tokenExpireTime = null)
        {
            var expire = tokenExpireTime ?? this._tokenExpiresTimeSpan;

            string newToken = CreateNewToken(new TokenModel
            {
                UserId = userId,
                UserName = userName,
                TokenExpireTime = DateTime.Now.Add(expire),
            });

            string tokenCacheKey = CommonCacheKeys.Build_UserToken_Key(userId, deviceType, isAdmin);

            this._easyCache.Set(tokenCacheKey, newToken, expire);

            return newToken;
        }

        public bool 检查用户登陆是否合法(int userId, DeviceType deviceType, string userToken, bool isAdmin, bool isSingleLogin, out string errorMsg)
        {
            errorMsg = string.Empty;

            //只能一个用户登录
            if (isAdmin || isSingleLogin)
            {
                string key = CommonCacheKeys.Build_UserToken_Key(userId, deviceType, isAdmin);

                string tokenInCache = this._easyCache.Get<string>(key);

                //只有token相同才是合法的
                if (!string.IsNullOrWhiteSpace(tokenInCache) && tokenInCache == userToken)
                {
                    return true;
                }
                else
                {
                    errorMsg = "您的帐号已经在其他地方登录，如不是您本人的操作，请及时修改账户密码。";

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 清除指定用户token
        /// </summary>
        public void ClearUserToken(int userId, DeviceType deviceType, bool isAdmin)
        {
            string userTokenKey = CommonCacheKeys.Build_UserToken_Key(userId, deviceType, isAdmin);

            this._easyCache.Remove(userTokenKey);
        }

        /// <summary>
        /// 清除指定用户所有token
        /// </summary>
        public void ClearUserAllToken(int userId)
        {
            foreach (DeviceType deviceType in Enum.GetValues(typeof(DeviceType)))
            {
                string key = CommonCacheKeys.Build_UserToken_Key(userId, deviceType, false);

                this._easyCache.Remove(key);
            }
        }
    }
}