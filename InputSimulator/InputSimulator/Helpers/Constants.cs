using InputSimulator.Flags;
using InputSimulator.Inputs;
using InputSimulator.Model;
using InputSimulator.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputSimulator.Helpers
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
                return "events_" + TimeExtensions.GetCurrentTimeStamp();
            }
        }

        public static int DefaultDababaseBufferSize
        {
            get
            {
                return 100;
            }
        }

        public static int DefaultStartDelaySeconds
        {
            get
            {
                return 5;
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
