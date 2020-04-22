# J4JCommandLine
These libraries build on [command-line-api](https://github.com/dotnet/command-line-api) and add 
some useful features.

### OptionExtras
This library provides some fluent extensions for configuring `Option` objects. For example, to add
a description you can do this:
```
var option = new Option<int>( new string[] {"-x", "--someIntOption"});

option.Description("some integer option");
```
They can also be chained together:
```
var option = new Option<int>( new string[] {"-x", "--someIntOption"});

option.Description("some integer option")
    .Name("optionName")
    .DefaultValue(10);
```
There are also a number of validators available. Some are only for types that support 
`IComparable<T>`. For example, to require a value to be greater than 0:
```
var option = new Option<int>( new string[] {"-x", "--someIntOption"});

option.Description("some integer option")
    .Name("optionName")
    .DefaultValue(10)
    .Validator(OptionInRange<int>.GreaterThan(0));
```
Other validators (not involving a value being inside or outside a range) are created this way:
```
var option = new Option<string>( new string[] {"-x", "--someTextOption"});

option.Description("some text option")
    .Name("optionName")
    .DefaultValue("none")
    .Validator(new OptionInSet<string>("none", "some", "whatever");
```

### ObjectBinder
This library provides middleware you can add to the `System.CommandLine` pipeline to bind
the output of the pipeline to an object of your choice. Executing the pipeline against 
command line text will either configure the target object or display error messages and help
text on the console. If configuration doesn't succeed or help was requested by the user the 
target object will be marked as such so the calling program will be able to abort its operation.

**Important: as of 4/21/2020 the library does not yet support `System.CommandLine` directives.
Any provided directives will be stripped out of the command line text and ignored.**

You use the library by deriving your configuration object from `RootObjectModel`, specifying
the bindings between command line options and configuration properties by overriding the
protected virtual method `DefineBindings()` (ignore the references to the `Beta` property; 
we'll discuss those in the section below on complex bound properties):

```
public class SimulationContext : RootObjectModel
{
    public SimulationContext( IJ4JLoggerFactory loggerFactory )
        : base("Investment Simulator")
    {
        Betas = new BetaDistribution( this, loggerFactory );

        ChildModels.Add( Betas );
    }

    public int Years { get; set; }
    public int Investments { get; set; }
    public int Simulations { get; set; }
    public double MeanMarketReturn { get; set; }
    public double StdDevMarketReturn { get; set; }
    public BetaDistribution Betas { get; }

    protected override void DefineBindings( IObjectBinder objBinder )
    {
        base.DefineBindings( objBinder );

        var binder = (ObjectBinder<SimulationContext>) objBinder;

        binder.AddOption(sc => sc.Years, "-y", "--years")
            .Description("years to simulate")
            .DefaultValue(10)
            .Validator(OptionInRange<int>.GreaterThanEqual(1));

        binder.AddOption(sc => sc.Investments, "-i", "--investments")
            .Description("investments to simulate")
            .DefaultValue(5)
            .Validator(OptionInRange<int>.GreaterThanEqual(1));

        binder.AddOption(sc => sc.Simulations, "-s", "--simulations")
            .Description("simulations to run")
            .DefaultValue(10)
            .Validator(OptionInRange<int>.GreaterThanEqual(1));

        binder.AddOption(sc => sc.MeanMarketReturn, "-r", "--meanReturn")
            .Description("mean annual rate of return for the total market")
            .DefaultValue(0.1)
            .Validator(OptionInRange<double>.GreaterThan(0.0));

        binder.AddOption(sc => sc.StdDevMarketReturn, "-d", "--stdDevReturn")
            .Description("standard deviation of total market annual rate of return")
            .DefaultValue(0.2)
            .Validator(OptionInRange<double>.GreaterThan(0.0));
    }
}
```
The bindings take advantage of a fluent API contained in the library. To apply it you
have to convert `objBinder` to an `ObjectBinder<T>`, where `T` is the type of the object
you're binding to. The cast is guaranteed to work because that's how `objBinder` is created
in the code.

Processing the command line text is done like this:
```
class Program
{
    private static IServiceProvider _services;
    private static IJ4JLogger _logger;
    private static SimulationContext _context;
    private static Simulator _simulator;

    static void Main( string[] args )
    {
        _services = ConfigureServices();

        var loggerFactory = _services.GetRequiredService<IJ4JLoggerFactory>();
        _logger = loggerFactory.CreateLogger( typeof(Program) );

        _context = _services.GetRequiredService<SimulationContext>();
            
        if( !_context.Initialize( args ) )
        {
            Environment.ExitCode = 1;
            return;
        }
        
        // remainder of program
```
Leaving aside the specific details of the example, all of the configuration parsing action
takes place via the call `_context.Initialize(args)`. If either help is requested or an error
was encountered it returns false. If parsing was successful it returns true.
### Complex Bound Properties
Because of what I think may stem from how `System.CommandLine` does property binding complex 
bound properties (i.e., properties that are themselves objects, like `Beta`) have to be handled
a little counter-intuitively.

The approach takes advantage of `System.CommandLine` subcommands. You configure them like this:
```
public class BetaDistribution : ObjectModel
{
    private Beta _betaDist;

    public BetaDistribution( SimulationContext simContext )
        : base( "beta", simContext )
    {
    }

    public double Alpha { get; set; }
    public double Beta { get; set; }
    public double Minimum { get; set; } = -1.0;
    public double Maximum { get; set; } = 1.0;

    public Beta Distribution => _betaDist ??= new Beta( Alpha, Beta );

    protected override void DefineBindings( IObjectBinder objBinder )
    {
        base.DefineBindings( objBinder );

        var binder = (ObjectBinder<BetaDistribution>) objBinder;

        binder.AddOption(b => b.Alpha, "-a")
            .Description("alpha parameter for beta distribution of investment betas")
            .DefaultValue(1.0)
            .Validator(OptionInRange<double>.GreaterThan(0.0));

        binder.AddOption(b => b.Beta, "-b")
            .Description("beta parameter for beta distribution of investment betas")
            .DefaultValue(2.0)
            .Validator(OptionInRange<double>.GreaterThan(0.0));

        binder.AddOption(b=> b.Maximum, "-x")
            .Description("maximum investment beta")
            .DefaultValue(2.0);

        binder.AddOption(b => b.Minimum, "-m")
            .Description("minimum investment beta")
            .DefaultValue(-2.0);
    }
}
```
This is identical to the way a `RootObjectModel`-derived class is defined, except that you
derive from `ObjectModel` and you have to specify both the subcommand name (*beta*, in this 
example) and the parent command that the new subcommand belongs to (*simContext*, in this
example). The parent command must be derived from either `RootObjectModel` or `ObjectModel`.

The command line text might then look like this:
>-y 50 -r 0.3 -d 0.5 beta -b 1 -a 1 -m -0.2 -x 0.3

Admittedly it'd be nicer if you didn't have to include the subcommand text in the command line
arguments. Hopefully I'll figure out a way to do that.
