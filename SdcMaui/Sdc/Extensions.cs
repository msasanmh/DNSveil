using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SdcMaui
{
    public static class Extensions
    {
        public static void DispatchIt(this IDispatcher dispatcher, Action action)
        {
            if (!dispatcher.IsDispatchRequired)
            {
                action();
                return;
            }
            dispatcher.Dispatch(action);
        }

        public static async Task DispatchItAsync(this IDispatcher dispatcher, Action action)
        {
            if (!dispatcher.IsDispatchRequired)
            {
                action();
                return;
            }
            await dispatcher.DispatchAsync(action);
        }
    }
}
