using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    public class SubClass
    {
        public int IntProperty { get; set; }
    }

    public class MainClass
    {
        public string TextProperty { get; set; }
        public SubClass Sub { get; set; } = new SubClass();
    }
}
