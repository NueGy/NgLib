using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.TESTS.MODELS
{
    [DemoAttribute("test", "democlass for test")]
    public class DemoClass
    {
        [DemoAttribute("id", "test on Field")]
        public int Id =0;

        [DemoAttribute("info", "test on private Field")]
        private string _info= null;

        [DemoAttribute("name", "test on Property")]
        public string Name { get; set; }


        public DateTime Date { get; set; }

        public bool Enabled { get; set; }

        public long LongValue { get; set; }



        [DemoAttribute("calculatemethod", "test on Method")]
        public string Calculate()
        {
            return "Hello " + this.Name;
        }

    }
}
