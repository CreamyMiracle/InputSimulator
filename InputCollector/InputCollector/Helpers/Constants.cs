using InputCollector.Flags;
using InputCollector.Inputs;
using InputCollector.Model;
using InputCollector.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputCollector.Helpers
{
    public static class Constants
    {
        public enum MouseButton
        {
            None,
            Left,
            Right,
            Middle,
            Other,
            WindowInit,
        }

        public enum MouseAction
        {
            //Primary actions
            Move,
            Up,
            Down,
            Scroll,
            Other,
            //Secondary actions
            Click,
            DoubleClick,
            Drag,
            WindowInit,
        }

        public static MouseEventFlag? ConvertMouseEventFlag(MouseEvent mEvent)
        {
            MouseButton btn = mEvent.Button;
            MouseAction act = mEvent.Action;
            int delta = mEvent.Delta;

            if (btn == MouseButton.None)
            {
                switch (act)
                {
                    case MouseAction.Move:
                        return MouseEventFlag.Move;
                    default:
                        throw new ArgumentException("Unknown button-action combination");
                }
            }
            else if (btn == MouseButton.Left)
            {
                switch (act)
                {
                    case MouseAction.Down:
                        return MouseEventFlag.LeftDown;
                    case MouseAction.Up:
                        return MouseEventFlag.LeftUp;
                    default:
                        throw new ArgumentException("Unknown button-action combination");
                }
            }
            else if (btn == MouseButton.Right)
            {
                switch (act)
                {
                    case MouseAction.Down:
                        return MouseEventFlag.RightDown;
                    case MouseAction.Up:
                        return MouseEventFlag.RightUp;
                    default:
                        throw new ArgumentException("Unknown button-action combination");
                }
            }
            else if (btn == MouseButton.Middle)
            {
                switch (act)
                {
                    case MouseAction.Down:
                        return MouseEventFlag.MiddleDown;
                    case MouseAction.Up:
                        return MouseEventFlag.MiddleUp;
                    case MouseAction.Scroll:
                        return MouseEventFlag.Wheel;
                    default:
                        throw new ArgumentException("Unknown button-action combination");
                }
            }
            else if (btn == MouseButton.Other)
            {
                switch (act)
                {
                    case MouseAction.Down:
                        return MouseEventFlag.XDown;
                    case MouseAction.Up:
                        return MouseEventFlag.XUp;
                    default:
                        throw new ArgumentException("Unknown button-action combination");
                }
            }
            return null;
        }

        public static Input ConvertMouseInput(MouseEvent mEvent, Func<IntPtr> extraInfo)
        {
            InputType _type = InputType.Mouse;
            MouseInput _mi = new MouseInput();


            return new Input { type = (int)_type, u = new InputUnion { mi = _mi } };
        }

        public static string DefaultDatabasePath
        {
            get
            {
                DirectoryInfo workDir = new DirectoryInfo(Environment.CurrentDirectory);
                return workDir.Parent.Parent.Parent.FullName;
            }
        }

        public static string DefaultDatabaseName
        {
            get
            {
                return "input_events_" + TimeExtensions.GetCurrentTimeStamp();
            }
        }

        public static int DefaultDababaseBufferSize
        {
            get
            {
                return 100;
            }
        }

        public static double DefaultSubstractedMilliseconds
        {
            get
            {
                return 0.1;
            }
        }
    }
}
