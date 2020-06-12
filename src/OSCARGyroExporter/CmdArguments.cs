// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.IO;
using CommandLine;

namespace OSCARGyroExporter {
  [Verb("manual", true, HelpText="Manueller Modus")]
  public class ManualCmdArguments {
    [Option('i', "input", Default="./in.csv", Required = false, HelpText = "Eingabedatei (*.csv)")]
    public string InputFile { get; set; }
    
    [Option('o', "output", Default="./out.csv", Required = false, HelpText = "Ausgabedatei (*.csv)")]
    public string OutputFile { get; set; }
    
    [Option('t', "timezoneoffset", Default=0, Required = false, HelpText = "Statischer Offset welcher zum/vom Referenzdatum addiert/subtrahiert wird")]
    public int TimezoneOffset { get; set; }

    [Option('s', "samplerate", Default=1, Required = false, HelpText = "Abtastrate der Eingabedatei. 1=jeder Messpunkt, 2=jeder zweite Messpunkt, 3=..., 4...")]
    public int SampleRate { get; set; }
  }
  
  // ReSharper disable once ClassNeverInstantiated.Global
  [Verb("automatic", false, HelpText="Automatischer Modus (für Steuerung aus Scripts)")]
  public class AutomaticCmdArguments {
    [Option('i', "input", Default="./in/", Required = false, HelpText = "Eingabeverzeichnis")]
    public string InputDirectory { get; set; }
    
    [Option('o', "output", Default="./out/", Required = false, HelpText = "Ausgabeverzeichnis")]
    public string OutputDirectory { get; set; }
    
    [Option('t', "timezoneoffset", Default=0, Required = false, HelpText = "Statischer Offset welcher zum/vom Referenzdatum addiert/subtrahiert wird")]
    public int TimezoneOffset { get; set; }
    
    [Option('s', "samplerate", Default=1, Required = false, HelpText = "Abtastrate der Eingabedatei. 1=jeder Messpunkt, 2=jeder zweite Messpunkt, 3=..., 4...")]
    public int SampleRate { get; set; }

    public string[] GetFilesInInputDirectory() => Directory.GetFiles(InputDirectory, "*.csv");
  }
}
