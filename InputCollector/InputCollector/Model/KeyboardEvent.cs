using InputCollector.Inputs;
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

        }
        public KeyboardEvent(int key)
        {
            Key = key;
            Timestamp = DateTime.UtcNow;
        }
        public int Key { get; set; }
    }
}
