﻿using System;
using System.Collections.Generic;

namespace Easy.Common.NetCore.Consul
{
    public class ConsulOption
    {
        /// <summary>
        /// 全局唯一注册Id
        /// </summary>
        public string GlobalRegId { get; set; }

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
        /// 服务健康检查地址（GET请求）
        /// </summary>
        public string ServiceHealthUrlCheck { get; set; }

        /// <summary>
        /// 健康检查时间间隔
        /// </summary>
        public TimeSpan? ServiceHealthCheckInterval { get; set; }

        /// <summary>
        /// 服务是否存在检查时间间隔（可能由于Consul服务器丢失服务信息数据，需要服务主动定时检查服务是否存在并注册）
        /// </summary>
        public TimeSpan? ServiceIsExistCheckInterval { get; set; }

        /// <summary>
        /// 服务状态异常后多久注销服务
        /// </summary>
        public TimeSpan? DeregisterCriticalServiceAfter { get; set; }

        /// <summary>
        /// 服务备注信息
        /// </summary>
        public string ServiceRemark { get; set; }

        /// <summary>
        /// 服务元数据（例如可存储该服务的最大并发访问量之类的信息，以便调用方进行筛选）
        /// </summary>
        public Dictionary<string, string> Meta { get; set; }

        /// <summary>
        /// Consul地址
        /// </summary>
        public string ConsulAddress { get; set; }
    }
}