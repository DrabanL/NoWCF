using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NoWCF.Utilities
{
    public static class ReflectionExtensions
    {

        public static object Invoke(this MethodBase self, object obj, IDictionary<string, object> namedParameters)
        {
            return self.Invoke(obj, MapParameters(self, namedParameters));
        }

        public static object[] MapParameters(MethodBase method, IDictionary<string, object> namedParameters)
        {
            var methodparameters = method.GetParameters();
            var paramTypes = methodparameters.Select(p => p.ParameterType).ToArray();
            var paramNames = methodparameters.Select(p => p.Name).ToArray();
            var parameters = new object[paramNames.Length];
            for (int i = 0; i < parameters.Length; ++i)
            {
                parameters[i] = Type.Missing;
            }
            foreach (var item in namedParameters)
            {
                var paramName = item.Key;
                var paramIndex = Array.IndexOf(paramNames, paramName);
                if (paramIndex >= 0)
                {
                    if (item.Value is Newtonsoft.Json.Linq.JToken)
                    {
                        parameters[paramIndex] = JsonConvert.DeserializeObject(item.Value.ToString(), paramTypes[paramIndex]);
                    }
                    else
                        parameters[paramIndex] =  item.Value;
                }
            }
            return parameters;
        }
    }
}
