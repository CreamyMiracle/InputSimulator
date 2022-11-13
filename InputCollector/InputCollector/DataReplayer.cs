using InputCollector.Helpers;
using InputCollector.Inputs;
using InputCollector.Model;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static InputCollector.Helpers.Constants;
using static InputCollector.Program;

namespace InputCollector
{
    public class DataReplayer
    {
        public DataReplayer(string dbPath)
        {
            db_async = new SQLiteAsyncConnection(dbPath);
        }

        #region Public Methods
        public async Task ReadEvents(InputTypeMode type)
        {
            if (type == InputTypeMode.M || type == InputTypeMode.B)
            {
                MouseEvents = await db_async.GetAllWithChildrenAsync<MouseEvent>();
            }

            if (type == InputTypeMode.K || type == InputTypeMode.B)
            {
                KeyboardEvents = await db_async.GetAllWithChildrenAsync<KeyboardEvent>();
            }
            
            AllEvents.AddRange(MouseEvents);
            AllEvents.AddRange(KeyboardEvents);
        }
        public async Task ReplayEvents()
        {
            List<InputEvent> orderedEvents = AllEvents.OrderBy(e => e.Timestamp).ToList();
            DateTime prev = DateTime.MinValue;

            foreach (InputEvent input in orderedEvents)
            {
                if (input is MouseEvent)
                {
                    MouseEvent mEvent = input as MouseEvent;
                    InputType _type = InputType.Mouse;
                    MouseInput _mi = new MouseInput();
                    _mi.dwFlags = (uint)Constants.ConvertMouseEventFlag(mEvent);
                    _mi.dwExtraInfo = Win32API.GetMessageExtraInfo();
                    _mi.mouseData = (uint)mEvent.Delta;

                    if (mEvent.Action == MouseAction.Move)
                    {
                        Win32API.SetCursorPos(mEvent.X, mEvent.Y);
                    }
                    else
                    {
                        _mi.dx = mEvent.X;
                        _mi.dy = mEvent.Y;

                        Input[] inputs = new Input[] { new Input { type = (int)_type, u = new InputUnion { mi = _mi } } };
                        Win32API.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
                    }
                }
                else if (input is KeyboardEvent)
                {
                    KeyboardEvent kEvent = input as KeyboardEvent;
                    KeyPress(kEvent.Key);
                }

                if (prev == DateTime.MinValue)
                {
                    prev = input.Timestamp;
                    continue;
                }

                TimeSpan diff = input.Timestamp - prev;

                prev = input.Timestamp;

                await Task.Delay(diff);
            }
        }
        public static void KeyPress(int keyCode)
        {
            Input input = new Input
            {
                type = (int)InputType.Keyboard,
                u = new InputUnion
                {
                    ki = new KeyboardInput
                    {
                        wVk = (ushort)keyCode,
                        wScan = 0,
                        dwFlags = 0, // if nothing, key down
                        time = 0,
                        dwExtraInfo = Win32API.GetMessageExtraInfo(),
                    }
                }
            };

            Input input2 = new Input
            {
                type = (int)InputType.Keyboard,
                u = new InputUnion
                {
                    ki = new KeyboardInput
                    {
                        wVk = (ushort)keyCode,
                        wScan = 0,
                        dwFlags = 2, // key up
                        time = 0,
                        dwExtraInfo = Win32API.GetMessageExtraInfo(),
                    }
                }
            };

            Input[] inputs = new Input[] { input, input2 }; // Combined, it's a keystroke
            Win32API.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
        }

        #endregion

        #region Private Fields
        private SQLiteAsyncConnection db_async;
        #endregion

        #region Public Properties
        public List<MouseEvent> MouseEvents = new List<MouseEvent>();
        public List<KeyboardEvent> KeyboardEvents = new List<KeyboardEvent>();
        public List<InputEvent> AllEvents = new List<InputEvent>();
        #endregion
    }
}
