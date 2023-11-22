using SP_DynamicMapper.Attributes;
using System.Reflection;

namespace SP_DynamicMapper.Extentions
{
    public static class ObjectExtensions
    {
        public static IDictionary<string, object?> AsDictionary(this object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return source.GetType().GetProperties(bindingAttr)
                .Where(x => string.IsNullOrWhiteSpace(x.GetCustomAttribute<SPFieldAttribute>()?.JoinFieldInternalName))
                .ToDictionary
                (
                    propInfo => propInfo.GetCustomAttribute<SPFieldAttribute>()?.InternalName ?? propInfo.Name,
                    propInfo => propInfo.GetValue(source, null)
                );
        }

        public static IDictionary<string, object?> AsDictionaryWithExpandedFields(this object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return source.GetType().GetProperties(bindingAttr)
                .ToDictionary
                (
                    propInfo => propInfo.GetCustomAttribute<SPFieldAttribute>()?.InternalName ?? propInfo.Name,
                    propInfo => propInfo.GetValue(source, null)
                );
        }

        public static Guid GetListGuid<T>(this T type) where T : class, new()
        {
            return new T().GetType().GetCustomAttribute<SPListAttribute>()?.ListGuid ?? Guid.Empty;
        }

        public static string GetListTitle<T>(this T type) where T : class, new()
        {
            return new T().GetType().GetCustomAttribute<SPListAttribute>()?.ListTitle ?? string.Empty;
        }

        public static Guid GetListGuid(this Type type)
        {
            return type.GetCustomAttribute<SPListAttribute>()?.ListGuid ?? Guid.Empty;
        }

        public static string GetListTitle(this Type type)
        {
            return type.GetCustomAttribute<SPListAttribute>()?.ListTitle ?? string.Empty;
        }
    }
}
