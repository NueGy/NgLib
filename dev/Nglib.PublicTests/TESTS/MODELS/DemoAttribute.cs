using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.TESTS.MODELS
{
    public class DemoAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DemoAttribute(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }
    }
}
