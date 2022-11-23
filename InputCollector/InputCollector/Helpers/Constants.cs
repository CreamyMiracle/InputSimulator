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
                return 0.047;
            }
        }
    }
}
