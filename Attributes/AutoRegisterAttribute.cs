using System;

namespace Easy.Common.NetCore.Attributes
{
    /// <summary>
    /// 标记为自动IOC注册
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AutoRegisterAttribute : Attribute
    {
        /// <summary>
        /// 注册值
        /// </summary>
        public string RegKey { get; }

        /// <summary>
        /// 接口名称
        /// </summary>
        public string InterfaceName { get; }

        public AutoRegisterAttribute()
        {
        }

        public AutoRegisterAttribute(string regKey, string interfaceName)
        {
            this.RegKey = regKey;
            this.InterfaceName = interfaceName;
        }
    }
}