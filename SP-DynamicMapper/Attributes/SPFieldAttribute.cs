using SP_DynamicMapper.Enums;

namespace SP_DynamicMapper.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class SPFieldAttribute : System.Attribute
    {
        public string InternalName { get; set; }
        public SPFieldType? SPFieldType { get; set; }
        public string? JoinFieldInternalName { get; set; }
        public string? JoinListID { get; set; }

        public SPFieldAttribute(string internalName, string joinFieldInternalName, string joinListID)
        {
            InternalName = internalName;
            JoinFieldInternalName = joinFieldInternalName;
            JoinListID = joinListID;
        }

        public SPFieldAttribute(string internalName, SPFieldType sPFieldType)
        {
            InternalName = internalName;
            SPFieldType = sPFieldType;
        }

        public SPFieldAttribute(string internalName)
        {
            InternalName = internalName;
        }
    }
}