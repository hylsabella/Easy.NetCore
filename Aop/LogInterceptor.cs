using Castle.DynamicProxy;
using Newtonsoft.Json;
using NLog;
using System;
using System.Text;

namespace Easy.Common.NetCore.Aop
{
    public class LogInterceptor : IInterceptor
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public void Intercept(IInvocation invocation)
        {
            var methodName = $"{invocation.Method.DeclaringType}.{invocation.Method.Name}";

            try
            {
                StringBuilder sb = new StringBuilder();

                var parameters = invocation.Method.GetParameters();

                for (int i = 0; i < parameters.Length; i++)
                {
                    string parameterName = parameters[i].Name;
                    string paramValue = JsonConvert.SerializeObject(invocation.Arguments[i]);

                    sb.AppendLine($"【{parameterName}】:{paramValue}");
                }

                if (sb.Length > 0)
                {
                    logger.Trace($"【LogInterceptor】【{methodName}】{Environment.NewLine}输入参数：{Environment.NewLine}{sb}");
                }
                else
                {
                    logger.Trace($"【LogInterceptor】【{methodName}】输入参数：空");
                }

                var watch = System.Diagnostics.Stopwatch.StartNew();

                invocation.Proceed();

                watch.Stop();

                var execTime = watch.ElapsedMilliseconds;

                string returnValueJson = "该方法无返回值";

                if (invocation.ReturnValue != null)
                {
                    returnValueJson = JsonConvert.SerializeObject(invocation.ReturnValue);
                }

                logger.Trace($"【LogInterceptor】【{methodName}】{Environment.NewLine}返回值【执行耗时 {execTime}毫秒】：{Environment.NewLine}{returnValueJson}");
            }
            catch (Exception ex)
            {
                logger.Trace(ex, $"【LogInterceptor】【{methodName}】异常：{ex.Message}");
            }
        }
    }
}