// ReSharper disable UnusedAutoPropertyAccessor.Global
using CommandLine;

namespace OSCARGyroExporter {
  // ReSharper disable once ClassNeverInstantiated.Global
  public class CmdArguments {
    [Option('a', "automated", Default = false, Required = false, HelpText = "Optimiert die Programmausführung für die Verwendung in Scripts")]
    public bool HeadlessMode { get; set; }
    
    [Option('t', "timezoneoffset", Default = 0, Required = false, HelpText = "Fügt einen statischen Offset in Minuten zum Referenzdatum")]
    public int StaticTimezoneOffset { get; set; }
    
    [Option('i', "intputpath", Default = null, Required = false, HelpText = "Der Pfad zur Eingabedatei (*.csv)")]
    public string InputPath { get; set; }
  }
}
