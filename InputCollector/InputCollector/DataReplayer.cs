using InputCollector.Helpers;
using InputCollector.Inputs;
using InputCollector.Misc;
using InputCollector.Model;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
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
                foreach (KeyboardEvent e in KeyboardEvents)
                {
                    var options = new JsonSerializerOptions { IncludeFields = true };
                    e.VirtualKeyCodes = JsonSerializer.Deserialize<List<int>>(e.VirtualKeyCodesBlob, options)!;
                }
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
                    Input[] inputs = Convert(kEvent.VirtualKeyCodes, kEvent.Type);
                    Win32API.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
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

        #endregion

        #region Private Methods
        private static Input[] Convert(List<int> vkCodes, KeyEventType eventType)
        {
            List<Input> inputs = new List<Input>();

            foreach (int vk in vkCodes)
            {
                Input input = new Input()
                {
                    u = new InputUnion()
                    {
                        ki = new KeyboardInput()
                        {
                            wVk = 0,
                            wScan = (ushort)Win32API.MapVirtualKey((uint)vk, 0),
                            dwFlags = (uint)(eventType | KeyEventType.Scancode),
                            dwExtraInfo = Win32API.GetMessageExtraInfo()
                        }
                    },
                    type = 1
                };

                inputs.Add(input);
            }

            return inputs.ToArray();
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
