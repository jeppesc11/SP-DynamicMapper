using Microsoft.SharePoint.Client;
using SP_DynamicMapper.Attributes;
using SP_DynamicMapper.Models;
using System.Reflection;

namespace SP_DynamicMapper.Extentions.Internal
{
    internal static class ObjectParser
    {
        internal static T ToObject<T>(this ListItem source) where T : class, new()
        {
            T objToReturn = new T();
            objToReturn = source.FieldValues.ToObject<T>();
            PropertyInfo prop = objToReturn.GetType().GetProperty("Id") ?? throw new Exception("Something went wrong");

            switch (prop.PropertyType.Name)
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

            return TrackObjectChangesIfPossible(objToReturn);
        }

        internal static T ToObject<T>(this IDictionary<string, object> source) where T : class, new()
        {
            T Tobject = new T();
            Type? TobjectType = Tobject.GetType();
            PropertyInfo[] props = TobjectType.GetProperties();

            foreach (var item in source)
            {
                PropertyInfo prop = GetPropertyInfoByAttributeInternalName(props, item);

                if (item.Value.GetType() == typeof(FieldLookupValue))
                {
                    switch (TobjectType?.GetProperty(prop.Name)?.PropertyType.Name)
                    {
                        case nameof(String):
                            TobjectType?.GetProperty(prop.Name)?.SetValue(Tobject, ((FieldLookupValue)item.Value).LookupValue, null);
                            break;
                        case nameof(Int32):
                        case nameof(Nullable<Int32>):
                            TobjectType?.GetProperty(prop.Name)?.SetValue(Tobject, ((FieldLookupValue)item.Value).LookupId, null);
                            break;
                        default:
                            TobjectType?.GetProperty(prop.Name)?.SetValue(Tobject, item.Value, null);
                            break;
                    }

                    continue;
                }

                TobjectType?.GetProperty(prop.Name)?.SetValue(Tobject, item.Value, null);
            }

            return Tobject;
        }

        #region Private Methods

        private static PropertyInfo GetPropertyInfoByAttributeInternalName(PropertyInfo[] props, KeyValuePair<string, object> item)
        {
            IEnumerable<PropertyInfo> selectedPropsByInternalName = props.Where(x => x.GetCustomAttribute<SPFieldAttribute>()?.InternalName.ToLower() == item.Key.ToLower());

            if (selectedPropsByInternalName.Count() > 1)
            {
                throw new Exception("Multiple properties with same internal name");
            }

            return selectedPropsByInternalName.FirstOrDefault() ?? throw new Exception("Property not found");
        }

        private static T TrackObjectChangesIfPossible<T>(T objToReturn) where T : class, new()
        {
            if (typeof(T).BaseType == typeof(TrackModel<T>))
            {
                objToReturn = (objToReturn as TrackModel<T>)!.StartTrackingChanges();
            }

            return objToReturn;
        }

        #endregion Private Methods
    }
}
