using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NSL.ASPNET.Blazor.Components.EntityFilter
{
    public class PlatformEntityFilterBuilderComponentRegistry : IEntityFilterBuilderComponentRegistry
    {
        //public static PlatformEntityFilterBuilderComponentRegistry Instance { get; } = new PlatformEntityFilterBuilderComponentRegistry();

        private readonly Dictionary<Type, EntityFilterBuilderValueComponent> _editors = new();
        //private readonly Dictionary<EntityFilterEditorEnum, EntityFilterBuilderValueComponent> _customEditors = new();

        public PlatformEntityFilterBuilderComponentRegistry()
        {
            var asm = Assembly.GetEntryAssembly();

            foreach (var item in asm.GetReferencedAssemblies().Where(x => x.Name.Contains("Teaching")))
            {
                try
                {
                    var asm2 = Assembly.Load(item);
                    Collect(asm2);
                }
                catch (Exception)
                {

                }
            }

            Collect(asm);
        }

        protected virtual void Collect(Assembly asm)
        {
            var editorTypes = asm.GetTypes()
                .Select(t => new
                {
                    EditorType = t,
                    Attr = t.GetCustomAttributes<EntityFilterBuilderTypeValueComponentAttribute>()
                })
                .Where(x => x.Attr?.Any() == true);

            foreach (var item in editorTypes)
            {
                foreach (var a in item.Attr)
                {
                    _editors[a.Type] = new EntityFilterBuilderValueComponent(item.EditorType, a.ComponentData);
                }
            }

            //var customEditorTypes = asm.GetTypes()
            //    .Select(t => new
            //    {
            //        EditorType = t,
            //        Attr = t.GetCustomAttributes<EntityFilterBuilderCustomValueComponentAttribute>()
            //    })
            //    .Where(x => x.Attr?.Any() == true);

            //foreach (var item in customEditorTypes)
            //{
            //    foreach (var a in item.Attr)
            //    {
            //        _customEditors[a.Type] = new EntityFilterBuilderValueComponent(item.EditorType, a.ComponentData);
            //    }
            //}
        }

        public virtual EntityFilterBuilderValueComponent? GetValueComponent(EntityFilterBuilderBlockDataModel data)
        {
            //if (data.Field.Value.Value.Meta.TryGetValue("Editor", out var _ceditor))
            //{
            //    var ceeditor = (EntityFilterEditorEnum)_ceditor;

            //    if (_customEditors.TryGetValue(ceeditor, out var builder))
            //        return builder;
            //    throw new Exception($"Editor for custom type '{ceeditor}' is not registered");
            //}

            if (data.Field.Value.Value.PropertyType.IsEnum)
            {
                if (_editors.TryGetValue(typeof(Enum), out var builder))
                    return builder;
            }

            if (_editors.TryGetValue(data.Field.Value.Value.PropertyType, out var builder2))
                return builder2;

            return null;
        }
    }
}
