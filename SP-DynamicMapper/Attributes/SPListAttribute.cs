namespace SP_DynamicMapper.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class SPListAttribute : System.Attribute
    {
        public SPListAttribute(string? listGuid, string? listTitle)
        {
            ListGuid = listGuid != null ? new Guid(listGuid) : null;
            ListTitle = listTitle;
        }

        public Guid? ListGuid { get; set; }
        public string? ListTitle { get; set; }
    }
}