using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Linq;

namespace J4JSoftware.CommandLine
{
    public interface IRootObjectModel
    {
        //IObjectBinder ObjectBinder { get; }
        //Command Command { get; }
        //ObjectModels ChildModels { get; }
        //ParseResult ParseResult { get; }
        //ParseStatus ParseStatus { get; }

        ParseStatus GetParseStatus();
        ParseResult GetParseResult();
        void AddChildCommand( Command childCommand );

        bool Initialize(string[] args, IConsole console = null);
        bool Initialize(string args, IConsole console = null);
        bool Initialize(CommandLineBuilder builder, string[] args, IConsole console = null);

        //int InvokeParseResult( IConsole console = null );
    }

    public interface IObjectModel : IRootObjectModel
    {
        string GetCommandName();
        void DefineBindings();
    }
}