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
    public class MouseEvent : InputEvent
    {
        public MouseEvent()
        {

        }
        public MouseEvent(MouseButton button, MouseAction action, int x, int y, int delta)
        {
            Button = button;
            Action = action;
            X = x;
            Y = y;
            Delta = delta;
            Timestamp = DateTime.UtcNow;
        }
        public MouseButton Button { get; set; }
        public MouseAction Action { get; set; }
        public int Delta { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
