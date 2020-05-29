using System;

namespace OSCARGyroExporter {
  public static class ConsoleHelper {
    public static void PrintWarning(string text) {
      var previousColor = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine(text);
      Console.ForegroundColor = previousColor;
    }

    public static void PrintError(string text) {
      var previousColor = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine(text);
      Console.ForegroundColor = previousColor;
    }

    public static void PrintInfo(string text) {
      var previousColor = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine(text);
      Console.ForegroundColor = previousColor;
    }

    public static void PrintSuccess(string text) {
      var previousColor = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine(text);
      Console.ForegroundColor = previousColor;
    }
  }
}
