using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputSimulator.Model
{
    public class Screenshot
    {
        public Screenshot()
        {

        }

        public Screenshot(string path, int testCase)
        {
            Path = path;
            TestCase = testCase;
        }

        public string Path { get; set; }

        [PrimaryKey]
        public int TestCase { get; set; }
    }
}
