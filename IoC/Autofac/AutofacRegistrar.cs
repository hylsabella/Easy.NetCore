using Autofac;
using Easy.Common.NetCore.Aop;
using Easy.Common.NetCore.Security;
using Easy.Common.NetCore.Service;
using System.ComponentModel.Composition;

namespace Easy.Common.NetCore.IoC.Autofac
{
    /// <summary>
    /// 通用AOP注册
    /// 例如：在需要拦截的类AdminRepository注册Autofac时，添加执行EnableInterfaceInterceptors()方法
    /// 如：builder.RegisterType<AdminRepository>().As<IAdminRepository>().SingleInstance().EnableInterfaceInterceptors()
    /// 并且在AdminRepository类添加拦截标记，如：[Intercept(typeof(LogInterceptor))]
    /// </summary>
    [Export(typeof(IAutofacRegistrar))]
    public class AutofacRegistrar : IAutofacRegistrar
    {
        public void Register(ContainerBuilder builder)
        {
            builder.Register(c => new LogInterceptor());
            builder.RegisterType<CookieAuthSvc>().As<ICookieAuthSvc>().SingleInstance();
            builder.RegisterType<TokenSvc>().As<ITokenSvc>().SingleInstance();
            builder.RegisterType<UserTreeSvc>().As<IUserTreeSvc>().SingleInstance();
        }
    }
}