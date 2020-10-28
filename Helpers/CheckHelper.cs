using System;
using System.Collections.Generic;
using System.Linq;

namespace Easy.Common.NetCore.Helpers
{
    public static class CheckHelper
    {
        public static string NotEmpty(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"参数{parameterName}不能为空或者空字符！", parameterName);
            }

            return value;
        }

        public static T NotNull<T>(T value, string parameterName, string message = "") where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName, message);
            }

            return value;
        }

        public static T? NotNull<T>(T? value, string parameterName, string message = "") where T : struct
        {
            if (!value.HasValue)
            {
                throw new ArgumentNullException(parameterName, message);
            }

            return value;
        }

        public static IEnumerable<T> ArrayNotHasNull<T>(IEnumerable<T> value, string parameterName)
        {
            NotNull(value, parameterName);

            if (!value.Any())
            {
                throw new ArgumentException($"参数{parameterName}没有包含任何对象！");
            }

            if (value.Where(item => item == null).Any())
            {
                throw new ArgumentException($"参数 {parameterName} 集合包含null对象！");
            }

            return value;
        }

        public static void MustEqual<T>(T value1, T value2, string parameterName1, string parameterName2)
        {
            if (!object.Equals(value1, value2))
            {
                throw new ArgumentException($"参数 {parameterName1} 与参数{parameterName2}不相等！");
            }
        }

        public static void MustIn<T>(T value, IEnumerable<T> list, string parameterName1, string parameterName2)
        {
            NotNull(list, "list");

            bool flag = false;

            foreach (var itemValue in list)
            {
                if (object.Equals(value, itemValue))
                {
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                throw new ArgumentException($"参数 {parameterName1} 不在集合{parameterName2}中！");
            }
        }
    }
}