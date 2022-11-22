using InputCollector.Helpers;
using InputCollector.Model;
using Fclp;
using InputCollector.Hooks;
using Fclp.Internals;

namespace InputCollector
{
    public class Program
    {
        private static MouseHook _mh;
        private static KeyboardHook _kh;
        private static DataCollector _collector;
        private static DataReplayer _replayer;

        private static string _dbPath = Path.Combine(Constants.DefaultDatabasePath, Constants.DefaultDatabaseName + ".db");
        private static int _dbBatchSize = Constants.DefaultDababaseBufferSize;
        private static Mode _mode = Mode.C;
        private static InputTypeMode _type = InputTypeMode.B;
        private static string[] _debugArgs = new string[] { "-m", "R", "-t", "B" };

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

            foreach (string i in args)
            {
                Console.WriteLine(i);
            }

            if (Parse(args.Count() == 0 ? _debugArgs : args))
            {
                if (_mode == Mode.C)
                {
                    Collect(args);
                }
                else if (_mode == Mode.R)
                {
                    Replay(args);
                }
            }

            Thread.CurrentThread.Join();
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
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

            p.Setup<string>('p', "path")
             .Callback(value => _dbPath = value)
             .SetDefault(Path.Combine(Constants.DefaultDatabasePath, Constants.DefaultDatabaseName + ".db"))
             .WithDescription("Path of the database file");

            p.Setup<int>('b', "buffer")
             .Callback(value => _dbBatchSize = value)
             .SetDefault(Constants.DefaultDababaseBufferSize)
             .WithDescription("Event collector buffer size");

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

        public static void Collect(string[] args)
        {
            Console.WriteLine("Saving mouse events to '{0}'", _dbPath);

            _collector = new DataCollector(_dbPath, _dbBatchSize);

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

        public static async Task Replay(string[] args)
        {
            Console.WriteLine("Replaying mouse events from '{0}'", _dbPath);

            _replayer = new DataReplayer(_dbPath);
            await _replayer.ReadEvents(_type);
            await _replayer.ReplayEvents();
        }

        #region Mouse stuff
        private static void mh_MouseEvent(object sender, MouseEvent me)
        {
            try
            {
                _collector.CollectMouseEvent(me);
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
                _collector.CollectKeyboardEvent(ke);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        #endregion
    }
}
