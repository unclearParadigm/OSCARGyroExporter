using System;
using System.Globalization;
using CSharpFunctionalExtensions;

namespace OSCARGyroExporter {
  public class OutputModel {
    public readonly string Timestamp;
    public readonly string Orientation;
    public readonly string Inclination;
    public readonly string TimeOfDay;
    public readonly string Date;

    private OutputModel(string timestamp, string orientation, string inclination, string timeOfDay, string date) {
      Timestamp = timestamp;
      Orientation = orientation;
      Inclination = inclination;
      TimeOfDay = timeOfDay;
      Date = date;
    }

    public static Result<OutputModel> Create(InputModel inputModel) {
      try {
        var timestamp = inputModel.Time.Subtract(new DateTime(2001, 1, 1)).TotalSeconds;
        var date = inputModel.Time.ToString("yyyy-MM-dd");
        var timeOfDay = inputModel.Time.ToString("HH:mm:ss");

        var orientation = inputModel.Gfx * 180;
        var inclination = inputModel.Gfy * 180;

        var tsString = timestamp
          .ToString(CultureInfo.InvariantCulture)
          .Replace(",", ".");

        var model = new OutputModel(
          tsString,
          orientation.ToString(CultureInfo.InvariantCulture).Replace(",","."),
          inclination.ToString(CultureInfo.InvariantCulture).Replace(",","."),
          timeOfDay,
          date);

        return Result.Success(model);
      } catch (Exception exc) {
        return Result.Failure<OutputModel>(exc.Message);
      }
    }
  }
}
