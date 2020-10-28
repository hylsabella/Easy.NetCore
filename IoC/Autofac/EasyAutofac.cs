using Autofac;
using Autofac.Extras.CommonServiceLocator;
using CommonServiceLocator;
using Easy.Common.NetCore.Startup;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Easy.Common.NetCore.IoC.Autofac
{
    public class EasyAutofac
    {
        [ImportMany]
        private IEnumerable<IAutofacRegistrar> _autofacRegList = null;
        public static ContainerBuilder ContainerBuilder { get; private set; } = new ContainerBuilder();
        private static object _lock = new object();
        private static IServiceLocator _serviceLocator;
        public static IContainer Container { get; set; }

        public EasyAutofac()
        {
        }

        public EasyAutofac(ContainerBuilder builder)
        {
            ContainerBuilder = builder;
        }

        /// <summary>
        /// 初始化IServiceLocator对象
        /// </summary>
        /// <param name="hasExtraIocReg">是否有额外的IOC注册服务</param>
        public void RegExtraIoc(bool hasExtraIocReg)
        {
            if (hasExtraIocReg && _autofacRegList == null)
            {
                if (EasyMefContainer.Container == null) throw new Exception("请先初始化MEF容器");

                try
                {
                    //MEF导入初始化_autofacRegistrar变量
                    EasyMefContainer.Container.SatisfyImportsOnce(this);
                }
                catch (CompositionException ex)
                {
                    throw new Exception("当参数【hasExtraIocReg】为true时，请先实现IAutofacRegistrar接口。", ex);
                }
            }

            if (_autofacRegList != null)
            {
                foreach (var autofacReg in _autofacRegList)
                {
                    autofacReg.Register(ContainerBuilder);
                }
            }
        }

        /// <summary>
        /// 获取IServiceLocator（会执行Build操作）
        /// </summary>
        public static IServiceLocator GetServiceLocator()
        {
            if (_serviceLocator == null)
            {
                lock (_lock)
                {
                    if (_serviceLocator == null)
                    {
                        //生成容器
                        Container = ContainerBuilder.Build();

                        var serviceLocator = new AutofacServiceLocator(Container);

                        //设置通用IOC适配器
                        ServiceLocator.SetLocatorProvider(() => serviceLocator);

                        _serviceLocator = serviceLocator;
                    }
                }
            }

            return _serviceLocator;
        }
    }
}