using CamlexNET;
using Microsoft.SharePoint.Client;
using SP_DynamicMapper.Attributes;
using SP_DynamicMapper.Enums;
using SP_DynamicMapper.Utilities.Internal;
using System.Linq.Expressions;
using System.Reflection;

namespace SP_DynamicMapper.Utilities
{
    internal class ExpressionHelper : ExpressionVisitor
    {
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var newParameters = new List<ParameterExpression>() { ExpressionHelperInternal.ListItemParameter };
            return Expression.Lambda<Func<ListItem, bool>>(Visit(node.Body), node.Name, newParameters);
        }

        protected override Expression VisitBinary(BinaryExpression bExpr)
        {
            if (bExpr.Left is MemberExpression)
            {
                MemberExpression? mExpr = bExpr.Left as MemberExpression;

                if (mExpr?.Member.MemberType == MemberTypes.Property)
                {
                    SPFieldAttribute? FieldAttribute = mExpr.Member.ReflectedType?.GetProperty(mExpr.Member.Name)?.GetSPFieldAttribute();
                    if (FieldAttribute != null && (FieldAttribute.SPFieldType == SPFieldType.LookupId || FieldAttribute.SPFieldType == SPFieldType.LookupValue))
                        return Expression.MakeBinary(bExpr.NodeType, bExpr.Left.ToMappedField(FieldAttribute), bExpr.Right.ToSPFieldType(FieldAttribute));
                }
            }

            return base.VisitBinary(bExpr);
        }

        protected override Expression VisitMember(MemberExpression mExpr)
        {
            if (mExpr.Member.MemberType == MemberTypes.Property)
            {
                SPFieldAttribute FieldAttribute = mExpr.Member.ReflectedType?.GetProperty(mExpr.Member.Name)?.GetSPFieldAttribute() ?? throw new Exception();
                MethodCallExpression getItemMethod = Expression.Call(ExpressionHelperInternal.ListItemParameter, ExpressionHelperInternal.GetItemMethod, Expression.Constant(FieldAttribute.InternalName));
                UnaryExpression getItemConverted = Expression.Convert(getItemMethod, mExpr.Member.ReflectedType.GetProperty(mExpr.Member.Name)!.PropertyType);

                return getItemConverted;
            }

            return mExpr;
        }
    }
}