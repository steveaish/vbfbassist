using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Reflection;

namespace VBFB_API.Classes
{
    /// <summary>
    /// Copies the allowed fields from a JSON body onto an entity - the equivalent of Laravel's
    /// $model->fill($request->all()) with a $fillable list. Field names match case-insensitively.
    /// </summary>
    public static class EntityUpdater
    {
        public static void Fill(object entity, JObject data, IEnumerable<string> allowedFields)
        {
            HashSet<string> allowed = new(allowedFields, StringComparer.OrdinalIgnoreCase);
            Type type = entity.GetType();

            foreach (JProperty prop in data.Properties())
            {
                if (!allowed.Contains(prop.Name)) continue;

                PropertyInfo? pi = type.GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (pi == null || !pi.CanWrite) continue;

                pi.SetValue(entity, Convert(prop.Value, pi.PropertyType));
            }
        }

        private static object? Convert(JToken token, Type target)
        {
            Type? underlying = Nullable.GetUnderlyingType(target);
            bool nullable = underlying != null || !target.IsValueType;
            Type t = underlying ?? target;

            if (token.Type == JTokenType.Null || (token.Type == JTokenType.String && (string?)token == "" && t != typeof(string)))
            {
                if (nullable) return null;
                return Activator.CreateInstance(t);
            }

            if (t == typeof(string)) return token.ToString();
            if (t == typeof(int)) return System.Convert.ToInt32(((JValue)token).Value, CultureInfo.InvariantCulture);
            if (t == typeof(decimal)) return System.Convert.ToDecimal(((JValue)token).Value, CultureInfo.InvariantCulture);
            if (t == typeof(double)) return System.Convert.ToDouble(((JValue)token).Value, CultureInfo.InvariantCulture);
            if (t == typeof(bool)) return System.Convert.ToBoolean(((JValue)token).Value, CultureInfo.InvariantCulture);
            if (t == typeof(DateTime)) return token.ToObject<DateTime>();
            return token.ToObject(t);
        }
    }
}
