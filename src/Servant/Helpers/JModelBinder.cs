using Newtonsoft.Json.Linq;
using System;
using System.Reflection;

namespace Servant.Helpers
{
    public class JModelBinder
    {
        private readonly JObject _paramObj;

        public JModelBinder(JObject paramObj)
        {
            _paramObj = paramObj;
        }

        public T GetModel<T>()
        {
            var obj = Activator.CreateInstance<T>();
            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var rawValue = GetValue<string>(prop.Name);
                prop.SetValue(obj, SafeConvert(rawValue, prop.PropertyType));
            }
            return obj;
        }

        public T GetValue<T>(string paramName)
        {
            var token = _paramObj.GetValue(paramName, StringComparison.OrdinalIgnoreCase);
            return token != null ? token.Value<T>() : default(T);
        }

        private static object SafeConvert(object value, Type conversionType)
        {
            try
            {
                return Convert.ChangeType(value, conversionType);
            }
            catch
            {
                return null;
            }
        }
    }
}
