using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using CommandLine;
using CSharpFunctionalExtensions;

namespace OSCARGyroExporter {
  internal static class Program {
    private const string Version = "v2.2";
    private static void Main(string[] args) {
      Console.Title = $"OSCAR Gyro Exporter {Version}";
      Console.ForegroundColor = ConsoleColor.White;
          
      ConsoleHelper.PrintInfo($"OSCAR Gyro Exporter {Version}                                         unclearParadigm");
      ConsoleHelper.PrintInfo("--------------------------------------------------------------------------------");
      
      Parser.Default
        .ParseArguments<ManualCmdArguments, AutomaticCmdArguments>(args)
        .WithParsed<ManualCmdArguments>(ManualMode)
        .WithParsed<AutomaticCmdArguments>(AutomaticMode);
    }

    private static void ManualMode(ManualCmdArguments arg) {
      var referenceDate = AutoDetectReferenceDate(arg.InputFile)
        .ToResult("Automatische Erkennung des Referenzdatum fehlgeschlagen")
        .OnFailureCompensate(r => {
          var referenceDateTime = Result.Failure<DateTime>("No value");
          while (referenceDateTime.IsFailure) {
            referenceDateTime = ReadReferenceDate();
            if (referenceDateTime.IsFailure) ConsoleHelper.PrintWarning(referenceDateTime.Error);
          }
          
          return referenceDateTime;
        });

      var operationResult = ReadFile(arg.InputFile)
        .Bind(fileContent => MapToInputModels(fileContent, referenceDate.Value.AddMinutes(arg.TimezoneOffset), arg.SampleRate))
        .Bind(MapToOutputModel)
        .Bind(ConvertToOutputCsv)
        .Bind(fileContent => WriteToFile(fileContent, arg.OutputFile));

      if (operationResult.IsSuccess)
        ConsoleHelper.PrintSuccess($"Konvertierung erfolgreich abgeschlossen. Die Datei befindet sich hier: {arg.OutputFile}");

      if (operationResult.IsFailure)
        ConsoleHelper.PrintError(operationResult.Error);
      
      ConsoleHelper.PrintInfo("Beliebige Taste drücken um Applikation zu beenden...");
      Console.ReadKey();
    }

    private static void AutomaticMode(AutomaticCmdArguments arg) {
      if (!Directory.Exists(arg.InputDirectory)) {
        ConsoleHelper.PrintError("Das Eingabeverzeichnis existiert nicht");
        return;
      }

      if (!Directory.Exists(arg.OutputDirectory))
        Directory.CreateDirectory(arg.OutputDirectory);
      
      foreach (var f in arg.GetFilesInInputDirectory()) {
        var referenceDateTime = AutoDetectReferenceDate(Path.GetFileName(f));
        if(referenceDateTime.HasNoValue) continue;
        var outputFilepath = Path.Join(arg.OutputDirectory, Path.GetFileName(f));
        
        var operationResult = ReadFile(f)
          .Bind(fileContent => MapToInputModels(fileContent, referenceDateTime.Value.AddMinutes(arg.TimezoneOffset), arg.SampleRate))
          .Bind(MapToOutputModel)
          .Bind(ConvertToOutputCsv)
          .Bind(fileContent => WriteToFile(fileContent, outputFilepath));
        
        if (operationResult.IsSuccess)
          ConsoleHelper.PrintSuccess($"{outputFilepath} -> OK");
        
        if (operationResult.IsFailure)
          ConsoleHelper.PrintError(operationResult.Error);
      }
    }

    private static Maybe<DateTime> AutoDetectReferenceDate(string filename) {
      if(string.IsNullOrEmpty(filename))
        return Maybe<DateTime>.None;
      if (filename.Length < 10)
        return Maybe<DateTime>.None;

      try {
        var substring = filename.Substring(0, 10);
        var parsed = DateTime.ParseExact(substring, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        return Maybe<DateTime>.From(parsed);
      } catch {
        return Maybe<DateTime>.None;
      }
    }

    private static Result<DateTime> ReadReferenceDate() {
      try {
        Console.Write("Eingabe des Referenzdatum (im Format: JJJJ-MM-DD): ");
        var referenceDateString = Console.ReadLine();
        var parsedReferenceDate = DateTime.ParseExact(referenceDateString, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        return Result.Ok(parsedReferenceDate);
      } catch (Exception) {
        return Result.Failure<DateTime>("Die Eingabe ist ungültig! (gewünschtes Eingabe-Format: JJJJ-MM-DD)");
      }
    }

    private static Result<List<InputModel>> MapToInputModels(string fileContent, DateTime referenceDateTime, int sampleRate) {
      if (string.IsNullOrWhiteSpace(fileContent))
        return Result.Failure<List<InputModel>>("Die angegebene Datei ist leer und entspricht nicht dem Eingabeformat");

      var detectedFileEnding = fileContent.Contains("\r\n") ? "\r\n" : "\n";
      var lines = fileContent.Split(detectedFileEnding);

      var outputList = new List<Result<InputModel>>();
      foreach (var line in lines.Skip(1).Where((x, i) => i % sampleRate == 0)) {
        var detectedColumnSeparatorChar = line.Contains(';') ? ';' : ',';
        var values = line.Split(detectedColumnSeparatorChar);

        var inputModelResult = InputModel.Create(
          values, 
          referenceDateTime, 
          outputList.Where(c => c.IsSuccess).Select(c => c.Value).ToList());

        if (inputModelResult.IsFailure)
          ConsoleHelper.PrintWarning($"Fehlerhafte Zeile in Eingabedatei gefunden: '{inputModelResult.Error}'");
        


        outputList.Add(inputModelResult);
      }

      return outputList.Any(c => c.IsSuccess)
        ? Result.Success(outputList.Where(c => c.IsSuccess).Select(c => c.Value).ToList())
        : Result.Failure<List<InputModel>>(
          "Die Eingabedatei besitzt nicht das korrekte Format. Das Einlesen war nicht erfolgreich");
    }

    private static Result<List<OutputModel>> MapToOutputModel(List<InputModel> inputModels) {
      var allModels = new List<Result<OutputModel>>();
      foreach (var outputModel in inputModels.Select(OutputModel.Create)) {
        if (outputModel.IsFailure)
          ConsoleHelper.PrintWarning(outputModel.Error);
        allModels.Add(outputModel);
      }

      return Result.Success(allModels.Where(c => c.IsSuccess).Select(c => c.Value).ToList());
    }

    private static Result<string> ConvertToOutputCsv(List<OutputModel> outputModels) {
      var sB = new StringBuilder();
      sB
        .Append("Timestamp,Orientation,Inclination,Time_of_day,Date")
        .Append("\n");
      
      foreach (var outputModel in outputModels) {
        sB
          .Append(outputModel.Timestamp).Append(",")
          .Append(outputModel.Orientation).Append(",")
          .Append(outputModel.Inclination).Append(",")
          .Append(outputModel.TimeOfDay).Append(",")
          .Append(outputModel.Date).Append("\n");
      }
      
      return Result.Success(sB.ToString());
    }

    private static Result<string> ReadFile(string filePath) {
      if (!File.Exists(filePath))
        return Result.Failure<string>($"Die angegebene Eingabedatei existiert nicht. Pfad: '{filePath}'");

      try {
        using (var fileStream = new StreamReader(filePath)) {
          var fileContent = fileStream.ReadToEnd();
          return Result.Success(fileContent);
        }
      } catch (Exception exc) {
        return Result.Failure<string>(exc.Message);
      }
    }

    private static Result WriteToFile(string outputCsv, string outputFilepath) {
      if (File.Exists(outputFilepath)) {
        File.Delete(outputFilepath);
      }

      try {
        File.Create(outputFilepath).Close();
        using (var writer = new StreamWriter(outputFilepath, false, Encoding.UTF8)) {
          writer.Write(outputCsv);
        }

        return Result.Success();
      } catch (Exception exc) {
        return Result.Failure(exc.Message);
      }
    }
  }
}
