using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class SubPropertyBindingTestCase
    {
        [ TestMethod ]
        public void Bind_to_subproperty()
        {
            var mainClass = new MainClass();

            mainClass.Initialize("-t sometext -i 27", new TestConsole() );

            mainClass.TextProperty
                .Should()
                .BeEquivalentTo( "sometext" );

            mainClass.Sub
                .IntProperty
                .Should()
                .Be( 27 );
        }
    }
}
