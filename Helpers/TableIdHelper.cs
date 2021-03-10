using Easy.Common.NetCore.Exceptions;
using System;
using System.Collections.Generic;

namespace Easy.Common.NetCore.Helpers
{
    public sealed class TableIdHelper
    {
        /// <summary>
        /// 用户对应的流水表id分表策略（第一次临界值为0，默认为空即可，如果有新的临界值直接参与新的计算即可）
        /// 第一次默认分10个表0-9 （第一次：table0----->table9）将0-100000分布到0-9表(每个表理论是10000个用户)
        /// 第二次还没到需要的时候，如果到了，这个地方给定要进行第二次分表的起始用户id（第一次：table10----->table19） 将100000-200000分布到10-19表(每个表理论是10000个用户)
        /// 第三次....(table20-table29)
        /// </summary>
        public static readonly string 用户分表依次对应的用户Id临界值 = "100000|200000|300000|400000|500000";

        /// <summary>
        /// 当前的最大分表Id（注：扩充表时记得修改此值）
        /// </summary>
        public const int MaxTableIndex = 9;

        /// <summary>
        /// 严重警告【分表的ID算法是不能改变的！！！】
        /// 用户流水分表策略
        /// 根据用户的id，进行策略，映射到每个用户的流水表id
        /// </summary>
        public static int GetUserLiuShuiTableMapId(int userId)
        {
            if (userId <= 0) throw new FException("用户Id必须大于0");

            var 用户Id临界值集合 = new List<int>();

            //解析分表区间配置
            if (!string.IsNullOrWhiteSpace(用户分表依次对应的用户Id临界值))
            {
                var splitList = 用户分表依次对应的用户Id临界值.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

                if (splitList != null && splitList.Length > 0)
                {
                    foreach (string 用户Id临界值 in splitList)
                    {
                        用户Id临界值集合.Add(Convert.ToInt32(用户Id临界值));
                    }
                }
            }

            //计算出来是那个区间内
            int index = 0;

            for (int i = 0; i < 用户Id临界值集合.Count; i++)
            {
                if (i == 0 && (userId < 用户Id临界值集合[i]))
                {
                    //比第一个界限还小
                    break;
                }

                if (i < 用户Id临界值集合.Count - 1
                    && (用户Id临界值集合[i] <= userId)
                    && (用户Id临界值集合[i + 1] > userId))
                {
                    //处理指定区间
                    index = i + 1;
                    break;
                }

                if (i == 用户Id临界值集合.Count - 1)
                {
                    //大于最后一个界限
                    index = i + 1;
                    break;
                }
            }

            //计算出实际的table id
            int yuShu = userId % 10;
            int tableId = 10 * index + yuShu;

            return tableId;
        }
    }
}