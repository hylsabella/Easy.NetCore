using Autofac;
using Easy.Common.NetCore.Attributes;
using Easy.Common.NetCore.Extentions;
using Easy.Common.NetCore.Repository;
using System;
using System.Reflection;

namespace Easy.Common.NetCore.IoC.Autofac
{
    public static class ContainerBuilderExt
    {
        public static void RegisterRepoAndSvc(this ContainerBuilder builder, DataBaseType dataBaseType, Assembly repoAssembly, Assembly unitSvcAssembly, Assembly integrateSvcAssembly)
        {
            if (!dataBaseType.IsInDefined()) throw new Exception($"数据库类型dataBaseType：{dataBaseType}不合法");
            if (repoAssembly == null) throw new Exception($"repoAssembly程序集不能为空");
            if (unitSvcAssembly == null) throw new Exception($"unitSvcAssembly程序集不能为空");
            if (integrateSvcAssembly == null) throw new Exception($"integrateSvcAssembly程序集不能为空");

            //注册范型仓储
            if (dataBaseType == DataBaseType.SqlServer)
            {
                builder.RegisterGeneric(typeof(SqlServerRepository<>)).As(typeof(IRepository<>)).PropertiesAutowired().InstancePerLifetimeScope();
            }
            else if (dataBaseType == DataBaseType.PostgreSQL)
            {
                builder.RegisterGeneric(typeof(PgSqlRepository<>)).As(typeof(IRepository<>)).PropertiesAutowired().InstancePerLifetimeScope();
            }

            var allTypes = AppDomain.CurrentDomain.GetAllTypes();

            foreach (Type type in allTypes)
            {
                var attr = type.GetCustomAttribute<AutoRegisterAttribute>();
                bool isPromissoryAutoRegType = attr != null;//是约定自动注册

                //约定是一个类，且是约定自动注册
                if (type.IsClass && isPromissoryAutoRegType)
                {
                    Type interfaceType = !string.IsNullOrWhiteSpace(attr.InterfaceName) ?
                                                  type.GetInterface(attr.InterfaceName) :
                                                  type.GetInterface($"I{type.Name}");

                    if (interfaceType != null)
                    {
                        if (!string.IsNullOrWhiteSpace(attr.RegKey))
                        {
                            builder.RegisterType(type).As(interfaceType).Named(attr.RegKey, interfaceType).PropertiesAutowired().SingleInstance();
                        }
                        else
                        {
                            builder.RegisterType(type).As(interfaceType).PropertiesAutowired().SingleInstance();
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(attr.RegKey))
                        {
                            builder.RegisterType(type).As(type).Named(attr.RegKey, type).PropertiesAutowired().SingleInstance();
                        }
                        else
                        {
                            builder.RegisterType(type).PropertiesAutowired().SingleInstance();
                        }
                    }
                }

                bool isSubClass = typeof(Microsoft.AspNetCore.Mvc.ControllerBase).IsAssignableFrom(type);

                if (isSubClass && !type.Assembly.FullName.Contains("Microsoft.AspNetCore", StringComparison.OrdinalIgnoreCase))
                {
                    builder.RegisterType(type).PropertiesAutowired();
                }
            }
        }
    }
}