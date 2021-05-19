using System;
using System.Collections.Generic;
using System.Reflection;

namespace Easy.Common.NetCore
{
    public static class AppDomainExt
    {
        public static IEnumerable<Type> GetAllTypes(this AppDomain appDomain)
        {
            foreach (var assembly in appDomain.GetAssemblies())
            {
                foreach (var t in GetTypes(assembly))
                {
                    yield return t;
                }
            }
        }

        private static IEnumerable<Type> GetTypes(Assembly assembly)
        {
            Type[] types;

            try
            {
                types = assembly.GetTypes();
            }
            catch
            {
                yield break;
            }

            foreach (var t in types)
            {
                yield return t;
            }
        }
    }
}