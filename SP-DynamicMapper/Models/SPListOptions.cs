using CamlexNET;
using CamlexNET.Interfaces;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.News.DataModel;
using SP_DynamicMapper.Attributes;
using SP_DynamicMapper.Extentions.Internal;
using SP_DynamicMapper.Utilities;
using System.Linq.Expressions;
using System.Reflection;

namespace SP_DynamicMapper.Models
{
    public class SPListOptionField<T> where T : class, new()
    {
        private IQuery? IncludeQuery { get; set; }
        private IQuery? WhereQuery { get; set; }

        private IQuery ViewFields()
        {
            T Tobject = new T();
            Type? TobjectType = Tobject.GetType();
            PropertyInfo[] props = TobjectType.GetProperties();

            var fields = props.Select(x => x.GetCustomAttribute<SPFieldAttribute>()).Where(x => x != null).Select(x => x?.InternalName).ToArray();

            return Camlex.Query().ViewFields(fields);
        }

        internal CamlQuery CamlQuery
        {
            get
            {
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml += IncludeQuery?.ToCamlQuery()?.ViewXml?.Replace("<View>", "")?.Replace("</View>", "") ?? string.Empty;
                camlQuery.ViewXml += WhereQuery?.ToCamlQuery()?.ViewXml?.Replace("<View>", "").Replace("</View>", "") ?? string.Empty;
                camlQuery.ViewXml += ViewFields()?.ToCamlQuery()?.ViewXml?.Replace("<View>", "").Replace("</View>", "") ?? string.Empty;
                camlQuery.ViewXml = $"<View>{camlQuery.ViewXml}</View>";

                return camlQuery;
            }
        }

        public void Includes(params Expression<Func<T, object?>>[] expressions)
        {
            List<Tuple<string, string, string>> joins = new List<Tuple<string, string, string>>();

            foreach (var expression in expressions)
            {
                var memberExpression = ((expression.Body as UnaryExpression)?.Operand ?? expression.Body) as MemberExpression;
                var info = memberExpression?.Member as PropertyInfo;
                SPFieldAttribute? attribute = info?.GetCustomAttribute<SPFieldAttribute>();

                if (attribute == null || string.IsNullOrWhiteSpace(attribute.JoinFieldInternalName) || string.IsNullOrWhiteSpace(attribute.JoinListID))
                {
                    Console.WriteLine($"Attribute is null or JoinFieldInternalName or JoinListID is null or empty");
                    continue;
                }

                joins.Add(new Tuple<string, string, string>(attribute.InternalName, attribute.JoinFieldInternalName, attribute.JoinListID));
            }

            var tempCamlQuery = Camlex.Query();

            foreach (var groupedJoins in joins.GroupBy(x => x.Item2))
            {
                tempCamlQuery = tempCamlQuery
                    .LeftJoin(x => x[groupedJoins.Key].ForeignList(groupedJoins.First().Item3));

                foreach (Tuple<string, string, string> groupedJoin in groupedJoins)
                {
                    tempCamlQuery = tempCamlQuery.ProjectedField(x => x[groupedJoin.Item1].List(groupedJoin.Item3).ShowField(groupedJoin.Item1));
                };
            }

            this.IncludeQuery = tempCamlQuery;
        }

        public void Where(Expression<Func<T, bool>> filter)
        {
            var CamlexEpressionFilter = new ExpressionHelper().Visit(filter) as Expression<Func<ListItem, bool>>;

            WhereQuery = Camlex.Query().Where(CamlexEpressionFilter);
        }

    }
}
