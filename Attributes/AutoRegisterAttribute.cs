using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace Easy.Common.NetCore.Attributes
{
    /// <summary>
    /// 标记为自动IOC注册
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AutoRegisterAttribute : Attribute
    {
    }
}