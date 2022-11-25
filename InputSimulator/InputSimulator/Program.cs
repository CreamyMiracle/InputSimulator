using InputSimulator.Helpers;
using InputSimulator.Model;
using Fclp;
using InputSimulator.Hooks;
using Fclp.Internals;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.ComponentModel;

namespace InputSimulator
{
    public class Program
    {
        private static Task _cvTask;

        private static MouseHook? _mh;
        private static KeyboardHook? _kh;

        private static DataCollector? _collector;
        private static DataReplayer? _replayer;
        private static ScreenCam? _screenCam;

        private static int _testCase = 0;

        private static string _workDir = Constants.DefaultDatabasePath;
        private static string _dbName = Constants.DefaultDatabaseName;
        private static int _dbBatchSize = Constants.DefaultDababaseBufferSize;
        private static int _startDelay = Constants.DefaultStartDelaySeconds;
        private static List<int> _quitVirtualKeys = Constants.DefaultQuitVirtualKeys;
        private static List<int> _scVirtualKeys = Constants.DefaultScreenShotVirtualKeys;
        private static Mode _mode = Mode.C;
        private static InputTypeMode _type = InputTypeMode.B;
        private static string[] _debugArgs = new string[] { "-m", "C", "-t", "B" };

        private static bool _parsingOk = true;

        public enum Mode
        {
            C,
            R
        }

        public enum InputTypeMode
        {
            M,
            K,
            B
        }

        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);

            if (Parse(args.Count() == 0 ? _debugArgs : args))
            {
                Console.WriteLine("Process starts in {0} seconds", _startDelay);
                TimeExtensions.NOP(_startDelay);

                string dbPath = Path.Combine(_workDir, _dbName);
                _screenCam = new ScreenCam(_workDir, dbPath);

                if (_mode == Mode.C)
                {
                    Collect(args);
                }
                else if (_mode == Mode.R)
                {
                    Replay(args);
                }

                if (_type == InputTypeMode.M || _type == InputTypeMode.B)
                {
                    _mh = new MouseHook();
                    _mh.SetHook();
                    _mh.MouseEvent += mh_MouseEvent;
                }

                if (_type == InputTypeMode.K || _type == InputTypeMode.B)
                {
                    _kh = new KeyboardHook();
                    _kh.KeyboardEvent += kh_KeyboardEvent;
                    _kh.Start();
                }
            }

            Thread.CurrentThread.Join();
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            _mh?.UnHook();
            _kh?.Stop();

            if (_cvTask.IsCompleted)
            {
                Environment.Exit(0);
            }
            else
            {
                _cvTask.ContinueWith(_ => Environment.Exit(0));
            }

            if (_collector != null)
            {
                _collector.Dispose();
            }
        }

        private static bool Parse(string[] args)
        {
            var p = new FluentCommandLineParser();

            p.Setup<Mode>('m', "mode")
             .Callback(value => _mode = value)
             .Required()
             .SetDefault(Mode.C)
             .WithDescription("C to collect, R to replay");

            p.Setup<InputTypeMode>('t', "type")
             .Callback(value => _type = value)
             .Required()
             .SetDefault(InputTypeMode.B)
             .WithDescription("M for mouse, K for keyboard, B for both");

            p.Setup<string>('n', "name")
             .Callback(value => _dbName = value)
             .SetDefault(Constants.DefaultDatabaseName)
             .WithDescription("Name of the database file");

            p.Setup<string>('d', "dir")
             .Callback(value => _workDir = value)
             .SetDefault(Constants.DefaultDatabasePath)
             .WithDescription("Path of the database file");

            p.Setup<int>('b', "buffer")
             .Callback(value => _dbBatchSize = value)
             .SetDefault(Constants.DefaultDababaseBufferSize)
             .WithDescription("Event collector buffer size");

            p.Setup<int>("delay")
             .Callback(value => _startDelay = value)
             .SetDefault(Constants.DefaultStartDelaySeconds)
             .WithDescription("Delay in seconds after which the process starts");

            p.Setup<List<int>>('q', "quit")
             .Callback(value => _quitVirtualKeys = value)
             .SetDefault(Constants.DefaultQuitVirtualKeys)
             .WithDescription("Any combination of virtual key codes that stops the process");

            p.Setup<List<int>>('c', "capture")
             .Callback(value => _scVirtualKeys = value)
             .SetDefault(Constants.DefaultScreenShotVirtualKeys)
             .WithDescription("Any combination of virtual key codes that takes a screen capture");

            p.SetupHelp("?", "help")
             .Callback(text =>
             {
                 _parsingOk = false;
                 Console.WriteLine(text);
             });

            var parseResult = p.Parse(args);
            if (parseResult.HasErrors)
            {
                p.HelpOption.ShowHelp(p.Options);
                _parsingOk = false;
            }
            return _parsingOk;
        }

        private static void Collect(string[] args)
        {
            string dbPath = Path.Combine(_workDir, _dbName);
            _collector = new DataCollector(dbPath, _dbBatchSize);

            Console.WriteLine("Collecting events to '{0}'", dbPath);
        }

        private static async Task Replay(string[] args)
        {
            string dbPath = Path.Combine(_workDir, _dbName);
            FileInfo file = new FileInfo(dbPath);
            if (!file.Exists)
            {
                throw new FileNotFoundException("Database not found!", dbPath);

            }
            _replayer = new DataReplayer(dbPath);

            DateTime readStart = await _replayer.ReadEvents(_type);
            Console.WriteLine("Reading events from '{0}' took '{1}'", dbPath, (DateTime.UtcNow - readStart));

            DateTime replayStart = await _replayer.ReplayEvents();
            Console.WriteLine("Replaying events from '{0}' took '{1}'", dbPath, (DateTime.UtcNow - replayStart));

            Environment.Exit(0);
        }

        #region Mouse stuff
        private static void mh_MouseEvent(object sender, MouseEvent me)
        {
            try
            {
                _collector?.CollectMouseEvent(me);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void kh_KeyboardEvent(object sender, KeyboardEvent ke)
        {
            try
            {
                _collector?.CollectKeyboardEvent(ke);

                _cvTask = Task.Run(async () =>
                {
                    CaptureOrVerify();
                });

                CheckIfExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void CaptureOrVerify()
        {
            if (AreKeysDown(_scVirtualKeys))
            {
                Bitmap? sc = _screenCam?.CaptureScreen();

                if (_mode == Mode.C)
                {
                    string path = _screenCam?.SaveImageToDisk(sc);
                    _screenCam.SaveImageToDB(path, _testCase);
                }
                else if (_mode == Mode.R)
                {
                    Bitmap saved = _screenCam?.ReadFromDisk(_testCase);
                    double sim = BitmapHelpers.Similarity(sc, saved);
                    Console.WriteLine(sim);
                }
                _testCase++;
            }
        }

        private static void CheckIfExit()
        {
            if (AreKeysDown(_quitVirtualKeys))
            {
                Environment.Exit(0);
            }
        }

        private static bool AreKeysDown(List<int> vkCodes)
        {
            foreach (int quitVk in vkCodes)
            {
                if (!((Win32API.GetKeyState(quitVk) & 0x80) == 0x80))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion
    }
}
