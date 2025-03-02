using Cross.UI.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Components
{
    public static class Extensions
    {
        public static T GetAttributeOrDefault<T>(this IImmutableAttributeContext context, Key<T> key, T defaultValue)
        {
            if (context.TryGetAttribute(key, out var val))
                return val!;
            else
                return defaultValue;
        }

        public static T GetAttributeOrDefault<T>(this IImmutableAttributeContext context, IComponentTreeNode descendant, Key<T> key, T defaultValue)
        {
            if (context.TryGetAttribute(descendant, key, out var val))
                return val!;
            else
                return defaultValue;
        }
    }
}
