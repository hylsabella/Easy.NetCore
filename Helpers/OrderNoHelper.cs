using Easy.Common.NetCore.Exceptions;
using Easy.Common.NetCore.Extentions;
using System;

namespace Easy.Common.NetCore.Helpers
{
    public static class OrderNoHelper
    {
        /// <summary>
        /// 生成订单号
        /// </summary>
        /// <param name="prefix">前缀（只能一个字符，且是小写的/底层会自动小写）</param>
        /// <param name="userId">用户Id</param>
        public static string CreateNewOrderNo(string prefix = "x", int? userId = null)
        {
            prefix = string.IsNullOrWhiteSpace(prefix) ? "x" : prefix;
            if (prefix.Length > 1) throw new FException("订单前缀只能一个字符");

            //订单号特用的时间戳（从2016年开始）
            var orderTimeStamp = GetTimeStamp();

            var tableIndex = userId.HasValue ? TableIdHelper.GetUserLiuShuiTableMapId(userId.Value).ToString() : string.Empty;

            int random = RandomHelper.GetRandom(99999);

            string result = tableIndex + prefix.ToLower() + orderTimeStamp.ToString() + random.ToString();

            return result;
        }

        /// <summary>
        /// 从订单号提取分表索引值（小于0表示不存在分表）
        /// </summary>
        public static long GetTableIndexFromOrderNo(string orderNo)
        {
            if (string.IsNullOrWhiteSpace(orderNo)) throw new FException("订单号不能为空");

            var splits = orderNo.Split(new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" }, StringSplitOptions.RemoveEmptyEntries);

            if (splits.Length != 2) throw new FException("订单号不符合规则");

            if (splits[0].Length >= 1)
            {
                if (!int.TryParse(splits[0], out int oIndex)) throw new FException("分表索引值必须是整型");

                return oIndex;
            }

            return -1;
        }

        /// <summary>
        /// 是否该订单存在分表
        /// </summary>
        public static bool HasTableIndex(string orderNo)
        {
            return GetTableIndexFromOrderNo(orderNo) >= 0;
        }

        public static bool 检测是否具有订单的特征码(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId)) return false;

            bool isOrder = false;
            bool isOrderPrefix = false;

            //订单前缀判断 OrderPrefix
            if (orderId.Length > 2)
            {
                var prefix = orderId.Substring(1, 1);
                if (prefix == OrderPrefix.充值申请表前缀)
                {
                    isOrderPrefix = true;
                }
            }

            if (!isOrderPrefix) return false;

            //订单前缀符合要求
            //订单号特用的时间戳（从2016年开始）
            var curTimeLong = GetTimeStamp() + "";

            int minOrderLen = 1 + 1 + curTimeLong.Length + 1; //预计是15
            int maxOrderLen = 1 + 1 + curTimeLong.Length + 5; //预计是19

            //长度检测合格（15-19位）
            if (orderId.Length >= minOrderLen && orderId.Length <= maxOrderLen)
            {
                long tabldIndex = -1;

                try
                {
                    tabldIndex = GetTableIndexFromOrderNo(orderId);
                }
                catch (Exception)
                {
                }

                //解析出来了表索引（0-9之间）
                if (tabldIndex >= 0 && tabldIndex.ToString().Length == 1)
                {
                    isOrder = true;
                }
            }

            return isOrder;
        }

        /// <summary>
        /// 订单号特用的时间戳（从2016年开始）
        /// </summary>
        private static long GetTimeStamp()
        {
            return DateTime.Now.GetTimeStamp(2016, 1, 1);
        }
    }

    public static class OrderPrefix
    {
        public const string 充值申请表前缀 = "r";

        public const string 提现申请表前缀 = "w";

        public const string 流水表前缀 = "l";
    }
}