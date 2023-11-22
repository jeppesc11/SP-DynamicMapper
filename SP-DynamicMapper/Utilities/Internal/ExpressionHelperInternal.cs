using CamlexNET;
using Microsoft.SharePoint.Client;
using SP_DynamicMapper.Attributes;
using SP_DynamicMapper.Enums;
using System.Linq.Expressions;
using System.Reflection;

namespace SP_DynamicMapper.Utilities.Internal
{
    internal static class ExpressionHelperInternal
    {
        internal static readonly MethodInfo GetItemMethod = typeof(ListItem).GetMethod("get_Item") ?? throw new Exception();
        internal static readonly MethodInfo ToStringMethod = typeof(object).GetMethod("ToString") ?? throw new Exception();
        internal static readonly ParameterExpression ListItemParameter = Expression.Parameter(typeof(ListItem), "listItem");

        internal static Expression ToMappedField(this Expression e, SPFieldAttribute FieldAttribute) =>
            Expression.Call(ListItemParameter, GetItemMethod, Expression.Constant(FieldAttribute.InternalName));

        internal static Expression ToSPFieldType(this Expression e, SPFieldAttribute FieldAttribute)
        {
            UnaryExpression convertedToBaseFieldType = Expression.Convert(Expression.Call(e, ToStringMethod), typeof(BaseFieldType));

            return FieldAttribute.SPFieldType switch
            {
                SPFieldType.LookupId => Expression.Convert(convertedToBaseFieldType, typeof(DataTypes.LookupId)),
                SPFieldType.LookupValue => Expression.Convert(convertedToBaseFieldType, typeof(DataTypes.LookupValue)),
                _ => Expression.Constant(null, typeof(object)),
            };
        }
    }
}
