using InputCollector.Flags;
using InputCollector.Helpers;
using InputCollector.Inputs;
using InputCollector.Misc;
using InputCollector.Model;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            }

            AllEvents.AddRange(MouseEvents);
            AllEvents.AddRange(KeyboardEvents);
        }

        public async Task ReplayEvents()
        {
            List<InputEvent> orderedEvents = AllEvents.OrderBy(e => e.Timestamp).ToList();
            DateTime prevEventTime = DateTime.MinValue;
            DateTime loopTime = DateTime.UtcNow;

            DateTime loopStart = DateTime.UtcNow;
            PeriodicTimer periodicTimer = null;

            foreach (InputEvent input in orderedEvents)
            {
                loopTime = DateTime.UtcNow;

                if (input is MouseEvent)
                {
                    MouseEvent mEvent = input as MouseEvent;

                    if (mEvent.Type == MouseEventFlag.MoveNoCoalesce)
                    {
                        Win32API.SetCursorPos(mEvent.X, mEvent.Y);
                    }
                    else
                    {
                        Input[] inputs = CreateMouseInputs(mEvent);
                        Win32API.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
                    }
                }
                else if (input is KeyboardEvent)
                {
                    KeyboardEvent kEvent = input as KeyboardEvent;
                    Input[] inputs = CreateKeyboardInputs(kEvent, kEvent.Type);

                    Win32API.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
                }

                if (prevEventTime == DateTime.MinValue)
                {
                    prevEventTime = input.Timestamp;
                    continue;
                }


                TimeSpan eventDiff = input.Timestamp - prevEventTime;
                prevEventTime = input.Timestamp;

                // Loop execution time
                TimeSpan loopSpan = DateTime.UtcNow - loopTime;

                TimeSpan wait = eventDiff - loopSpan;
                wait = wait - TimeSpan.FromMilliseconds(Constants.DefaultSubstractedMilliseconds);
                wait = (wait <= TimeSpan.Zero) ? TimeSpan.Zero : wait;

                NOP(wait.TotalSeconds);
            }
            Console.WriteLine((DateTime.UtcNow - loopStart).TotalSeconds);
        }
        #endregion

        #region Private Methods
        private static void NOP(double durationSeconds)
        {
            var durationTicks = Math.Round(durationSeconds * Stopwatch.Frequency);
            var sw = Stopwatch.StartNew();

            while (sw.ElapsedTicks < durationTicks)
            {

            }
        }

        private static Input[] CreateKeyboardInputs(KeyboardEvent kEvent, KeyEventType eventType)
        {
            Input input = new Input()
            {
                u = new InputUnion()
                {
                    ki = new KeyboardInput()
                    {
                        wVk = (ushort)kEvent.VirtualKeyCode,
                        dwFlags = (uint)eventType,
                        dwExtraInfo = IntPtr.Zero
                        //wVk = 0,
                        //wScan = (ushort)Win32API.MapVirtualKey((uint)vk, 0),
                        //dwFlags = (uint)(eventType | KeyEventType.Scancode),
                        //dwExtraInfo = Win32API.GetMessageExtraInfo()
                    }
                },
                type = 1
            };

            return new Input[] { input };
        }

        private static Input[] CreateMouseInputs(MouseEvent mEvent)
        {
            InputType _type = InputType.Mouse;
            MouseInput _mi = new MouseInput();
            _mi.dwFlags = (uint)mEvent.Type;
            _mi.dwExtraInfo = IntPtr.Zero;
            _mi.mouseData = (uint)mEvent.Delta;

            _mi.dx = mEvent.X;
            _mi.dy = mEvent.Y;

            return new Input[] { new Input { type = (int)_type, u = new InputUnion { mi = _mi } } };
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
