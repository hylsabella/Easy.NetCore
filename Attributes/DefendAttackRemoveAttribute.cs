using System;

namespace Easy.Common.NetCore.Attributes
{
    /// <summary>
    /// 不参与防御流量攻击
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class DefendAttackRemoveAttribute : Attribute
    {

    }
}