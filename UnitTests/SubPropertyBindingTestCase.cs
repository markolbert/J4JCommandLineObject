using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class SubPropertyBindingTestCase
    {
        [TestMethod]
        public void Bind_to_subproperty()
        {
            var mainOption = new Option<string>("-t");
            var subOption = new Option<int>("-i");

            var modelBinder = new ModelBinder<MainClass>();
            modelBinder.BindMemberFromValue(mc => mc.TextProperty, mainOption);
            modelBinder.BindMemberFromValue(mc => mc.Sub.IntProperty, subOption);

            var mainClass = new MainClass();

            var cmdLine = "-t sometext -i 27";
            var args = CommandLineStringSplitter.Instance
                .Split( cmdLine )
                .ToList();

            var builder = new CommandLineBuilder()
                .UseDefaults()
                .UseMiddleware( ic =>
                {
                    modelBinder.UpdateInstance(mainClass, ic.BindingContext);
                } );

            var parser = builder.Build();
            var parseResults = parser.Parse( args );
        }
    }
}
