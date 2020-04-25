using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.CommandLine;

namespace UnitTests
{
    public class SubClass
    {
        public int IntProperty { get; set; }
    }

    public class MainClass : RootObjectModel
    {
        public string TextProperty { get; set; }
        public SubClass Sub { get; set; } = new SubClass();

        protected override void DefineBindings( IObjectBinder objBinder )
        {
            var binder = (ObjectBinder<MainClass>) objBinder;

            binder.AddOption(mc => mc.Sub.IntProperty, "-i")
                .DefaultValue(-5);

            binder.AddOption( mc => mc.TextProperty, "-t" )
                .DefaultValue( "ralph" );
        }
    }
}
