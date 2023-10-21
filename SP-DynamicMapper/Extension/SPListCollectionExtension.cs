using Microsoft.SharePoint.Client;

namespace SP_DynamicMapper.Extentions
{
    public static class SPListCollectionExtension
    {
        public static List GetById<T>(this ListCollection lists) where T : class
        {
            Guid guid = typeof(T).GetListGuid();

            if (guid == Guid.Empty)
            {
                throw new System.Exception("List guid must be set");
            }
            return lists.GetById(guid);
        }

        public static List GetByTitle<T>(this ListCollection lists) where T : class
        {
            string title = typeof(T).GetListTitle();

            if (string.IsNullOrEmpty(title))
            {
                throw new System.Exception("List title must be set");
            }
            return lists.GetByTitle(title);
        }

        public static List GetListByType<T>(this ListCollection lists) where T : class, new()
        {
            Guid guid = typeof(T).GetListGuid();

            if (guid != Guid.Empty)
            {
                return lists.GetById(guid);
            }

            string title = typeof(T).GetListTitle();

            if (!string.IsNullOrEmpty(title))
            {
                return lists.GetByTitle(title);
            }

            throw new Exception("List guid or title must be set");
        }
    }
}
