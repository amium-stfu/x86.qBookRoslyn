using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qbook.ScintillaEditor
{
    internal static class Snippets
    {

        public static string NewPageCode(string name) => newPage(name);
        private static string newPage(string name)
        {
            return $@"namespace Definition{name} {{ //<CodeStart>
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using QB;

    public class qPage
    {{
        //common fields/properties/methods/classes/types go here

        public void Initialize()
        {{
            //initialization code goes here

        }}

        public void Run()
        {{
            //run/work code goes here

        }}

        public void Destroy()
        {{
            //destroy/cleanup code goes here
        }}
    }}
    //<CodeEnd>
}}
";
        }
        public static string NewSubCode(oPage page,string name) => newSubCode(page,name);
        private static string newSubCode(oPage page,string name)
        {
            return $@"namespace Definition{page.Name} {{
    //<CodeStart>
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using QB;

    public class {name}()
    {{

    }}
    //<CodeEnd>
}}
";
        }
    }
}
