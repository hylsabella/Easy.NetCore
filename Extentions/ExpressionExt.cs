using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Easy.Common.NetCore.Extentions
{
    public static class ExpressionExt
    {
        /// <summary>
        /// 获取匿名对象 'object' 的属性名称
        /// </summary>
        /// <typeparam name="T">item，实体类</typeparam>
        /// <param name="propertyExpression">item => new {item.Propert1, item.Propert2, ..}</param>
        /// <returns></returns>
        public static List<string> ToPropertyNames<T>(this Expression<Func<T, object>> propertyExpression)
            where T : class
        {

            CheckHelper.MustEqual(propertyExpression.NodeType, ExpressionType.Lambda, "updatePropertys.Body.NodeType", "Lambda");

            var lambdaExp = (LambdaExpression)propertyExpression;

            CheckHelper.MustEqual(lambdaExp.Body.NodeType, ExpressionType.New, "lambdaExp.Body.NodeType", "New");


            NewExpression newExp = (NewExpression)lambdaExp.Body;

            List<string> updatePtyNamelst = new List<string>();

            for (int i = 0; i < newExp.Arguments.Count; i++)
            {
                var expItem = newExp.Arguments[i];

                CheckHelper.MustEqual(expItem.NodeType, ExpressionType.MemberAccess, "expItem.NodeType", "MemberAccess");

                updatePtyNamelst.Add(((MemberExpression)expItem).Member.Name);
            }

            return updatePtyNamelst;
        }
    }
}
