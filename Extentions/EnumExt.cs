using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Easy.Common.NetCore.Extentions
{
    public static class EnumExt
    {
        public static string GetEnumDescription(this Enum enumValue)
        {
            string str = enumValue.ToString();

            FieldInfo field = enumValue.GetType().GetField(str);

            if (field == null)
            {
                return str;
            }

            object[] objs = field.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);

            if (objs == null || objs.Length == 0)
            {
                return str;
            }

            var da = (System.ComponentModel.DescriptionAttribute)objs[0];

            return da.Description;
        }

        public static bool IsInDefined(this Enum enumValue)
        {
            var isOk = Enum.IsDefined(enumValue.GetType(), enumValue);

            return isOk;
        }

        public static Dictionary<string, int> GetEnumList(this Type enumType, bool isNeedAddAll = true)
        {
            if (!enumType.IsEnum)
            {
                return new Dictionary<string, int>();
            }

            var dic = new Dictionary<string, int>();

            if (isNeedAddAll)
            {
                dic.Add("全部", -1);
            }


            foreach (var value in enumType.GetEnumValues())
            {
                string enumName = Enum.GetName(enumType, value);
                dic.Add(enumName, (int)value);
            }

            return dic;
        }

        /// <summary>
        /// 获取枚举列表
        /// </summary>
        /// <param name="mustKeyWord">必须含有的关键字（用来剔除，只保留部分选项）</param>
        public static List<SelectListItem> GetEnumList(this Enum enumValue, Enum selectedValue = null, bool isNeedAddAll = true,
            string mustKeyWord = "")
        {
            var list = new List<SelectListItem>();

            Type type = enumValue.GetType();

            foreach (int value in Enum.GetValues(type))
            {
                string name = Enum.GetName(type, value);

                if (!string.IsNullOrWhiteSpace(mustKeyWord))
                {
                    //剔除不符合的
                    if (name.Contains(mustKeyWord))
                    {
                        list.Add(new SelectListItem { Text = name, Value = value.ToString() });
                    }
                }
                else
                {
                    list.Add(new SelectListItem { Text = name, Value = value.ToString() });
                }
            }

            if (isNeedAddAll)
            {
                list.Insert(0, new SelectListItem { Text = "全部", Value = "-1" });
            }

            if (null != selectedValue)
            {
                var find = list.Where(i => (Convert.ToInt32(selectedValue).ToString() == i.Value));

                if (find.Any())
                {
                    find.First().Selected = true;
                }
            }
            else
            {
                if (isNeedAddAll)
                {
                    var find = list.Where(i => "-1" == i.Value);

                    if (find.Any())
                    {
                        find.First().Selected = true;
                    }
                }
            }

            return list;
        }
    }
}