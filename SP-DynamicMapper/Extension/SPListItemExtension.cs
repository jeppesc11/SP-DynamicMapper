using Microsoft.SharePoint.Client;

namespace SP_DynamicMapper.Extentions
{
    public static class SPListItemExtension
    {
        public static ListItem PopulateFromDictionary(this ListItem item, IDictionary<string, object?> dic)
        {

            foreach (var keyValuePair in dic)
            {

                if (keyValuePair.Key.ToLower() == "id")
                {
                    continue;
                }

                item[keyValuePair.Key] = keyValuePair.Value;
            }
            return item;
        }
    }
}
