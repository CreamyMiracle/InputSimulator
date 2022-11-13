using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common.Helpers.Constants;

namespace Common.Model
{
    public class MouseEvent
    {
        public MouseEvent()
        {

        }
        public MouseEvent(MouseButton button, MouseAction action, int x, int y, int delta, string window = "")
        {
            Button = button;
            Action = action;
            X = x;
            Y = y;
            Delta = delta;
            Window = window;
            Timestamp = DateTime.Now;
        }
        public double Distance(MouseEvent destination)
        {
            return Math.Sqrt(Math.Pow((destination.X - X), 2) + Math.Pow((destination.Y - Y), 2));
        }
        public MouseButton Button { get; set; }
        public MouseAction Action { get; set; }
        public int Delta { get; set; }
        public DateTime Timestamp { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Window { get; set; }
    }
}
