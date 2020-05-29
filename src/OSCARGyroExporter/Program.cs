using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using CSharpFunctionalExtensions;

namespace OSCARGyroExporter {
  internal static class Program {
    private static readonly string DefaultInputPath = $"{Path.Join(Environment.CurrentDirectory, "in.csv")}";
    private static readonly string DefaultOutputPath = $"{Path.Join(Environment.CurrentDirectory, "out.csv")}";

    private static void Main(string[] args) {
      var inputPath = args.Any() && args.Length == 1 ? args[0] : DefaultInputPath;

      Console.Title = "OSCAR Gyro Exporter v1.0";
      Console.ForegroundColor = ConsoleColor.White;
      
      ConsoleHelper.PrintInfo("OSCAR Gyro Exporter v1.0                                         unclearParadigm");
      ConsoleHelper.PrintInfo("--------------------------------------------------------------------------------");
      ConsoleHelper.PrintInfo($"Eingabedatei: {inputPath}");
      ConsoleHelper.PrintInfo($"Ausgabedatei: {DefaultOutputPath}");
      ConsoleHelper.PrintInfo("--------------------------------------------------------------------------------");

      var referenceDateTime = Result.Failure<DateTime>("No value");
      while (referenceDateTime.IsFailure) {
        referenceDateTime = ReadReferenceDate();
        if (referenceDateTime.IsFailure) ConsoleHelper.PrintWarning(referenceDateTime.Error);
      }

      var operationResult = ReadFile(inputPath)
        .Bind(fileContent => MapToInputModels(fileContent, referenceDateTime.Value))
        .Bind(MapToOutputModel)
        .Bind(ConvertToOutputCsv)
        .Bind(WriteToFile);

      if (operationResult.IsSuccess)
        ConsoleHelper.PrintSuccess($"Konvertierung erfolgreich abgeschlossen. Die Datei befindet sich hier: {DefaultOutputPath}");

      if (operationResult.IsFailure)
        ConsoleHelper.PrintError(operationResult.Error);

      ConsoleHelper.PrintInfo("Beliebige Taste drücken um Applikation zu beenden...");
      Console.ReadKey();
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

    private static Result<List<InputModel>> MapToInputModels(string fileContent, DateTime referenceDateTime) {
      if (string.IsNullOrWhiteSpace(fileContent))
        return Result.Failure<List<InputModel>>("Die angegebene Datei ist leer und entspricht nicht dem Eingabeformat");

      var detectedFileEnding = fileContent.Contains("\r\n") ? "\r\n" : "\n";
      var lines = fileContent.Split(detectedFileEnding);

      var outputList = new List<Result<InputModel>>();

      foreach (var line in lines.Skip(1)) {
        var detectedColumnSeparatorChar = line.Contains(';') ? ';' : ',';
        var values = line.Split(detectedColumnSeparatorChar);
        var inputModelResult = InputModel.Create(values, referenceDateTime);

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

    private static Result WriteToFile(string outputCsv) {
      if (File.Exists(DefaultOutputPath)) {
        File.Delete(DefaultOutputPath);
      }

      try {
        File.Create(DefaultOutputPath).Close();
        using (var writer = new StreamWriter(DefaultOutputPath, false, Encoding.UTF8)) {
          writer.Write(outputCsv);
        }

        return Result.Success();
      } catch (Exception exc) {
        return Result.Failure(exc.Message);
      }
    }
  }
}
