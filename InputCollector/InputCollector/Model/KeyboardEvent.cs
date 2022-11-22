using InputCollector.Inputs;
using InputCollector.Misc;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static InputCollector.Helpers.Constants;

namespace InputCollector.Model
{
    public class KeyboardEvent : InputEvent
    {
        public KeyboardEvent()
        {
            Timestamp = DateTime.UtcNow;
        }

        public KeyEventType Type { get; set; }

        [Ignore]
        public List<int> VirtualKeyCodes { get; set; } = new List<int>();

        public string VirtualKeyCodesBlob { get; set; }
    }
}
