using System;
using System.Globalization;
using CSharpFunctionalExtensions;

namespace OSCARGyroExporter {
  public class InputModel {
    public readonly DateTime Time;
    public readonly decimal Gfx;
    public readonly decimal Gfy;
    public readonly decimal Gfz;

    private InputModel(DateTime time, decimal gfx, decimal gfy, decimal gfz) {
      Time = time;
      Gfx = gfx;
      Gfy = gfy;
      Gfz = gfz;
    }

    public static Result<InputModel> Create(string[] args, DateTime referenceDateTime) {
      try {
        var time = DateTime.ParseExact(args[0], "HH:mm:ss:ffff", CultureInfo.InvariantCulture);
        var correctedTime = referenceDateTime
          .AddHours(time.Hour)
          .AddMinutes(time.Minute)
          .AddSeconds(time.Second)
          .AddMilliseconds(time.Millisecond);

        var gfx = decimal.Parse(args[1].Replace(",", "."), CultureInfo.InvariantCulture);
        var gfy = decimal.Parse(args[2].Replace(",", "."), CultureInfo.InvariantCulture);
        var gfz = decimal.Parse(args[3].Replace(",", "."), CultureInfo.InvariantCulture);

        return Result.Success(new InputModel(correctedTime, gfx, gfy, gfz));
      } catch (Exception) {
        return Result.Failure<InputModel>($"Konnte '{string.Join(",", args)}' nicht interpretieren");
      }
    }
  }
}
