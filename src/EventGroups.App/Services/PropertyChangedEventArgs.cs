using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventGroups.App.Services
{
    public class PropertyChangedEventArgs
    {
        public string Name { get; set; }
        public object NewValue { get; set; }
        public object OldValue { get; set; }
        public bool IsGlobal { get; set; }
    }
}
