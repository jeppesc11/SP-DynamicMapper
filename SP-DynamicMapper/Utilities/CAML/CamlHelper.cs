using CamlexNET;
using Microsoft.SharePoint.Client;
using SP_DynamicMapper.Attributes;
using System.Reflection;

namespace SP_DynamicMapper.Utilities.CAML
{
    internal static class CamlHelper
    {

        internal static CamlQuery MergeQueries(params CamlQuery[] queries)
        {
            CamlQuery camlQuery = new CamlQuery();

            for (int i = 0; i < queries.Length; i++)
            {
                camlQuery.ViewXml += queries[i].ViewXml?.Replace("<View>", "")?.Replace("</View>", "");
            }

            camlQuery.ViewXml = $"<View>{camlQuery.ViewXml}</View>";

            return camlQuery;
        }

        internal static CamlQuery GetByIdQuery(string id)
        {
            CamlQuery camlQuery = new CamlQuery();

            camlQuery.ViewXml = $"<View><Query><Where><Eq><FieldRef Name='ID'/><Value Type='Number'>{id}</Value></Eq></Where></Query></View>";

            return camlQuery;
        }

        internal static CamlQuery GenerateIncludeQueryFromType<T>() where T : class, new()
        {
            T type = new T();
            Type? TobjectType = type.GetType();
            PropertyInfo[] props = TobjectType.GetProperties();
            IEnumerable<SPFieldAttribute?> attributes = props.Select(x => x.GetCustomAttribute<SPFieldAttribute>()).ToList().Where(x => !string.IsNullOrWhiteSpace(x.JoinFieldInternalName));

            List<Tuple<string, string, string>> joins = new List<Tuple<string, string, string>>();

            foreach (SPFieldAttribute? attribute in attributes)
            {
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

            return tempCamlQuery.ToCamlQuery();
        }

        internal static CamlQuery GenerateViewFieldsQueryFromType<T>() where T : class, new()
        {
            T type = new T();
            Type? TobjectType = type.GetType();
            PropertyInfo[] props = TobjectType.GetProperties();

            var fields = props.Select(x => x.GetCustomAttribute<SPFieldAttribute>()).Where(x => x != null).Select(x => x?.InternalName).ToArray();

            return Camlex.Query().ViewFields(fields).ToCamlQuery();
        }

    }
}
