using InputSimulator.Inputs;
using InputSimulator.Misc;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static InputSimulator.Helpers.Constants;

namespace InputSimulator.Model
{
    public class KeyboardEvent : InputEvent
    {
        public KeyboardEvent(int virtualCode, KeyEventType type)
        {
            Timestamp = DateTime.UtcNow;
            VirtualKeyCode = virtualCode;
            Type = type;
        }
        public KeyboardEvent()
        {

        }

        public KeyEventType Type { get; set; }

        public int VirtualKeyCode { get; set; }
    }
}
