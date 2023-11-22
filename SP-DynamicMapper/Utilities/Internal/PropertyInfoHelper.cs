using SP_DynamicMapper.Attributes;
using System.Reflection;

namespace SP_DynamicMapper.Utilities.Internal
{
    internal static class PropertyInfoHelper
    {
        internal static SPFieldAttribute? GetSPFieldAttribute(this PropertyInfo propertyInfo) =>
            (SPFieldAttribute?)Attribute.GetCustomAttribute(propertyInfo, typeof(SPFieldAttribute));
    }
}