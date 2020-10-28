using Autofac;

namespace Easy.Common.NetCore.IoC.Autofac
{
    /// <summary>
    /// Autofac自动注册接口
    /// </summary>
    public interface IAutofacRegistrar
    {
        void Register(ContainerBuilder builder);
    }
}