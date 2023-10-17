using System.Reflection;

namespace SP_DynamicMapper.Models
{
    public class TrackModel<T> where T : class, new()
    {
        private Dictionary<string, object?> _OriginalValues = new Dictionary<string, object?>();

        public TrackModel() { }

        public T StartTrackingChanges()
        {
            _OriginalValues = new Dictionary<string, object?>();

            GetType().GetProperties().ToList().ForEach(property => {
                _OriginalValues.Add(property.Name, property.GetValue(this));
            });

            return this as T ?? new T();
        }

        public Dictionary<string, object?> GetChanges()
        {
            Dictionary<string, object?> changes = new Dictionary<string, object?>();
            IEnumerable<PropertyInfo> properties = GetType().GetProperties().ToArray().Where(p => !Equals(p.GetValue(this, null), _OriginalValues[p.Name]));

            foreach (PropertyInfo property in properties)
            {
                changes.Add(property.Name, property.GetValue(this));
            };

            return changes;
        }
    }
}