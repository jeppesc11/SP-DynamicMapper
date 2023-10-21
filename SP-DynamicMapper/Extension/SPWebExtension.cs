using CamlexNET;
using Microsoft.SharePoint.Client;
using SP_DynamicMapper.Attributes;
using SP_DynamicMapper.Extentions.Internal;
using System.Linq.Expressions;
using System.Reflection;

namespace SP_DynamicMapper.Extentions
{
    public static class SPWebExtension
    {
        public static IEnumerable<T> GetItems<T>(this Microsoft.SharePoint.Client.Web web, params Expression<Func<T, object?>>[] expressions) where T : class, new()
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

            ListItemCollection items = web.Lists.GetListByType<T>().GetItems(tempCamlQuery.ToCamlQuery());

            web.Context.Load(items);
            web.Context.ExecuteQuery();

            return items.Select(p => p.ToObject<T>());
        }

        public static IEnumerable<T> GetItems<T>(this Microsoft.SharePoint.Client.Web web) where T : class, new()
        {
            ListItemCollection items = web.Lists.GetListByType<T>().GetItems(CamlQuery.CreateAllItemsQuery());

            web.Context.Load(items);
            web.Context.ExecuteQuery();

            return items.Select(p => p.ToObject<T>());
        }

        public static T UpdateItem<T>(this Web web, T item) where T : class, new()
        {
            List list = web.Lists.GetListByType<T>();
            string? idAsString = item.GetType().GetProperty("Id")?.GetValue(item, null)?.ToString();

            if (string.IsNullOrEmpty(idAsString))
            {
                throw new System.Exception("Id must be set");
            }

            int id = int.Parse(idAsString);
            ListItem listItem = list.GetItemById(id);
            web.Context.Load(listItem);
            web.Context.ExecuteQuery();

            listItem.PopulateFromDictionary(item.AsDictionary()).Update(); // TODO

            web.Context.ExecuteQuery();

            return listItem.ToObject<T>();
        }
    }
}
