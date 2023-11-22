using CamlexNET;
using Microsoft.SharePoint.Client;
using SP_DynamicMapper.Attributes;
using SP_DynamicMapper.Extentions.Internal;
using SP_DynamicMapper.Models;
using SP_DynamicMapper.Utilities.CAML;
using System.Linq.Expressions;
using System.Reflection;

namespace SP_DynamicMapper.Extentions
{
    public static class SPWebExtension
    {

        public static IEnumerable<T> GetItems<T>(this Web web, Action<SPListOptionField<T>> expression) where T : class, new()
        {
            var expressionTemp = new SPListOptionField<T>();
            expression.Invoke(expressionTemp);

            return web.GetItems<T>(expressionTemp.CamlQuery);
        }

        public static IEnumerable<T> GetItems<T>(this Web web, CamlQuery? camlQuery = null) where T : class, new()
        {
            ListItemCollection items = web.Lists.GetListByType<T>().GetItems(camlQuery ?? CamlQuery.CreateAllItemsQuery());

            web.Context.Load(items);
            web.Context.ExecuteQuery();

            return items.Select(p => p.ToObject<T>());
        }

        public static T? GetItemById<T>(this Web web, string id, bool expandFields = false) where T : class, new()
        {
            return web.GetListItemById<T>(id, expandFields)?.ToObject<T>() ?? default;
        }

        public static ListItem? GetListItemById<T>(this Web web, string id, bool expandFields = false) where T : class, new()
        {
            CamlQuery includeCaml = new CamlQuery();
            CamlQuery viewFieldsCaml = CamlHelper.GenerateViewFieldsQueryFromType<T>();
            CamlQuery idCaml = CamlHelper.GetByIdQuery(id);

            if (expandFields)
            {
                includeCaml = CamlHelper.GenerateIncludeQueryFromType<T>();
            }

            CamlQuery mergedCaml = CamlHelper.MergeQueries(idCaml, includeCaml, viewFieldsCaml);

            var items = web.Lists.GetListByType<T>().GetItems(mergedCaml);

            web.Context.Load(items);
            web.Context.ExecuteQuery();

            return items.FirstOrDefault();
        }

        public static ListItemCollection GetListItems<T>(this Web web, string id) where T : class, new()
        {
            CamlQuery includeCaml = CamlHelper.GenerateIncludeQueryFromType<T>();
            CamlQuery viewFieldsCaml = CamlHelper.GenerateViewFieldsQueryFromType<T>();
            CamlQuery idCaml = CamlHelper.GetByIdQuery(id);
            CamlQuery mergedCaml = CamlHelper.MergeQueries(idCaml, includeCaml, viewFieldsCaml);

            var items = web.Lists.GetListByType<T>().GetItems(mergedCaml);

            web.Context.Load(items);
            web.Context.ExecuteQuery();

            return items;
        }

        public static T? UpdateItemExpandedFields<T>(this Web web, T item) where T : class, new()
        {
            Dictionary<string, object?>? propsToUpdate = GetTrackedChanges(item);
            if (propsToUpdate == null || !propsToUpdate.Any())
            {
                return item;
            }

            T updatedItem = web.UpdateItem(item);
            var Id = updatedItem.GetType().GetProperty("Id")?.GetValue(updatedItem, null)?.ToString() ?? throw new Exception("Id must be set");

            return web.GetItemById<T>(Id, true);
        }

        public static T UpdateItem<T>(this Web web, T item) where T : class, new()
        {
            Dictionary<string, object?>? propsToUpdate = GetTrackedChanges(item);
            if (propsToUpdate == null || !propsToUpdate.Any())
            {
                return item;
            }

            return web.UpdateListItem(item, propsToUpdate).ToObject<T>();
        }

        #region PRIVATE

        private static Dictionary<string, object?>? GetTrackedChanges<T>(T item) where T : class, new()
        {
            Dictionary<string, object?>? propsToUpdate = null;

            if (typeof(T).BaseType == typeof(TrackModel<T>))
            {
                TrackModel<T>? obj = (item as TrackModel<T>);
                Dictionary<string, object?>? changes = obj?.GetChanges();
                propsToUpdate = obj?.GetType()?.GetProperties()?.Where(x => changes?.ContainsKey(x.Name) ?? false)?.ToDictionary(x => x.GetCustomAttribute<SPFieldAttribute>()?.InternalName!, x => changes?[x.Name]);

            }

            return propsToUpdate;
        }

        private static ListItem UpdateListItem<T>(this Web web, T item, Dictionary<string, object?>? propsToUpdate) where T : class, new()
        {
            List list = web.Lists.GetListByType<T>();
            string idAsString = item.GetType().GetProperty("Id")?.GetValue(item, null)?.ToString() ?? throw new Exception("Id must be set");

            int id = int.Parse(idAsString);
            ListItem listItem = list.GetItemById(id);
            web.Context.Load(listItem);

            web.Context.ExecuteQuery();
            listItem.PopulateFromDictionary(propsToUpdate ?? item.AsDictionary()).Update();

            web.Context.ExecuteQuery();

            return listItem;
        }

        #endregion PRIVATE
    }
}
