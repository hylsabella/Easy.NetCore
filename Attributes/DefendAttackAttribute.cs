using System;

namespace Easy.Common.NetCore.Attributes
{
    /// <summary>
    /// 防御流量攻击（注：WebApi工程拦截带 RouteAttribute 特性的方法）
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class DefendAttackAttribute : Attribute
    {

    }
}