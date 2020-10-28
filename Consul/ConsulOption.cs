using System.Collections.Generic;

namespace Easy.Common.NetCore.Consul
{
    public class ConsulOption
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 服务IP
        /// </summary>
        public string ServiceIP { get; set; }

        /// <summary>
        /// 服务端口
        /// </summary>
        public int ServicePort { get; set; }

        /// <summary>
        /// 服务健康检查地址
        /// </summary>
        public string ServiceHealthCheck { get; set; }

        /// <summary>
        /// 服务备注信息
        /// </summary>
        public string ServiceRemark { get; set; }

        /// <summary>
        /// 服务元数据（例如可存储该服务的最大并发访问量之类的信息，以便调用方进行筛选）
        /// </summary>
        public IDictionary<string, string> Meta { get; set; }

        /// <summary>
        /// Consul地址
        /// </summary>
        public string ConsulAddress { get; set; }
    }
}