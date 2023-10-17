using Microsoft.SharePoint.Client;
using SP_DynamicMapper.Attributes;
using SP_DynamicMapper.Models;
using System.Reflection;

namespace SP_DynamicMapper.Extentions
{
    public static class ObjectExtensions
    {

        public static T ToObject<T>(this ListItem source) where T : class, new()
        {
            T objToReturn = new T();
            objToReturn = source.FieldValues.ToObject<T>();
            PropertyInfo? prop = objToReturn.GetType().GetProperty("Id");

            switch (prop?.PropertyType.Name)
            {
                case nameof(Int32):
                case nameof(Int16):
                    prop.SetValue(objToReturn, int.Parse(source.Id.ToString()), null);
                    break;
                case nameof(Int64):
                    prop.SetValue(objToReturn, long.Parse(source.Id.ToString()), null);
                    break;
                case nameof(Guid):
                    prop.SetValue(objToReturn, Guid.Parse(source.Id.ToString()), null);
                    break;
                case nameof(String):
                    prop.SetValue(objToReturn, source.Id.ToString(), null);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(prop.PropertyType.Name));
            }

            if (typeof(T).BaseType == typeof(TrackModel<T>))
            {
                objToReturn = (objToReturn as TrackModel<T>).StartTrackingChanges();
            }

            return objToReturn;
        }

        public static IDictionary<string, object?> AsDictionary(this object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return source.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.GetCustomAttribute<SPFieldAttribute>()?.InternalName ?? propInfo.Name,
                propInfo => propInfo.GetValue(source, null)
            );
        }

        public static Guid GetListGuid<T>(this T type) where T : class, new()
        {
            var listGuid = new T().GetType().GetCustomAttribute<SPListAttribute>()?.ListGuid;

            if (listGuid == null)
            {
                return Guid.Empty;
            }

            return listGuid.Value;
        }

        public static string GetListTitle<T>(this T type) where T : class, new()
        {
            var listTitle = new T().GetType().GetCustomAttribute<SPListAttribute>()?.ListTitle;

            if (string.IsNullOrWhiteSpace(listTitle))
            {
                return string.Empty;
            }

            return listTitle;
        }

        public static Guid GetListGuid(this Type type)
        {
            var listGuid = type.GetCustomAttribute<SPListAttribute>()?.ListGuid;

            if (listGuid == null)
            {
                return Guid.Empty;
            }

            return listGuid.Value;
        }

        public static string GetListTitle(this Type type)
        {
            var listTitle = type.GetCustomAttribute<SPListAttribute>()?.ListTitle;

            if (string.IsNullOrWhiteSpace(listTitle))
            {
                return string.Empty;
            }

            return listTitle;
        }

        #region Private Methods

        private static T ToObject<T>(this IDictionary<string, object> source) where T : class, new()
        {
            var someObject = new T();
            var someObjectType = someObject.GetType();

            var props = someObjectType.GetProperties();


            foreach (var item in source)
            {
                var selectedPropsByInternalName = props.Where(x => x.GetCustomAttribute<SPFieldAttribute>()?.InternalName.ToLower() == item.Key.ToLower());

                if (selectedPropsByInternalName.Count() > 1)
                {
                    throw new Exception("Multiple properties with same internal name");
                }

                var prop = selectedPropsByInternalName.FirstOrDefault();

                if (prop != null)
                {
                    if (item.Value.GetType() == typeof(FieldLookupValue))
                    {
                        if (someObjectType?.GetProperty(prop.Name)?.PropertyType == typeof(String))
                        {
                            someObjectType?.GetProperty(prop.Name)?.SetValue(someObject, ((FieldLookupValue)item.Value).LookupValue, null);
                            continue;
                        }

                        if (someObjectType?.GetProperty(prop.Name)?.PropertyType == typeof(Int32) || someObjectType?.GetProperty(prop.Name)?.PropertyType == typeof(Nullable<Int32>))
                        {
                            someObjectType?.GetProperty(prop.Name)?.SetValue(someObject, ((FieldLookupValue)item.Value).LookupId, null);
                            continue;
                        }

                        someObjectType?.GetProperty(prop.Name)?.SetValue(someObject, item.Value, null);
                        continue;
                    }

                    someObjectType?.GetProperty(prop.Name)?.SetValue(someObject, item.Value, null);
                }
            }

            return someObject;
        }

        #endregion Private Methods

    }
}
