using InputSimulator.Flags;
using InputSimulator.Inputs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static InputSimulator.Helpers.Constants;

namespace InputSimulator.Model
{
    public class MouseEvent : InputEvent
    {
        public MouseEvent()
        {

        }
        public MouseEvent(MouseEventFlag type, int x, int y, int delta)
        {
            Type = type;
            X = x;
            Y = y;
            Delta = delta;
            Timestamp = DateTime.UtcNow;
        }
        public MouseEventFlag Type { get; set; }
        public int Delta { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
