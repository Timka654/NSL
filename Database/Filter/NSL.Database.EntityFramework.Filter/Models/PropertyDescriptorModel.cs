using System.Reflection;

namespace NSL.Database.EntityFramework.Filter.Models
{
    public class PropertyDescriptorModel
    {
        public PropertyInfo Property { get; set; }

        public string Path { get; set; }

        public string Name { get; set; }

        public bool Show { get; set; } = true;

        public PropertyDescriptorModel(PropertyInfo p, string path)
        {
            Property = p;
            Path = path;
            Name = p.Name;
        }

        public PropertyDescriptorModel SetName(string name)
        {
            Name = name;
            return this;
        }

        public PropertyDescriptorModel SetEnabled()
        {
            Show = true;
            return this;
        }

        public PropertyDescriptorModel SetDisabled()
        {
            Show = false;
            return this;
        }

        //public PropertyDescriptor SetDelegate(Func<string, bool>)
    }
}
