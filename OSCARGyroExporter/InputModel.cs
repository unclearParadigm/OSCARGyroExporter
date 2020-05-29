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

        var gfx = Convert.ToDecimal(args[1].Replace(",", "."));
        var gfy = Convert.ToDecimal(args[2].Replace(",", "."));
        var gfz = Convert.ToDecimal(args[3].Replace(",", "."));

        return Result.Success(new InputModel(correctedTime, gfx, gfy, gfz));
      } catch (Exception) {
        return Result.Failure<InputModel>($"Konnte '{string.Join(",", args)}' nicht interpretieren");
      }
    }
  }
}
