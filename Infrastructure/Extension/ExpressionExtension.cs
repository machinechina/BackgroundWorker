using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Extensions
{
    public partial class Extension
    {
        public static T Then<T>(this T @this, Action<T> action)
        {
            action?.Invoke(@this);
            return @this;
        }
 
        public static bool WhenTrue(this bool @this,Action action)
        {
            if (@this)
                action?.Invoke();
            return @this;
        }

    }
}
