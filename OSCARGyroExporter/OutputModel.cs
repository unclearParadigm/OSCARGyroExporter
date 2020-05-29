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
        var date = inputModel.Time.ToString("yyyy-MM-dd");
        var timeOfDay = inputModel.Time.ToString("HH:mm:ss");
        var timestamp = new DateTimeOffset(inputModel.Time).ToUnixTimeSeconds();
        
        var orientation = (int)(90 - inputModel.Gfx * 90);
        var inclination = (int)(90 - inputModel.Gfy * 90);
        
        var model = new OutputModel(
          timestamp.ToString(),
          orientation.ToString(CultureInfo.InvariantCulture),
          inclination.ToString(CultureInfo.InvariantCulture),
          timeOfDay,
          date);

        return Result.Success(model);
      } catch (Exception exc) {
        return Result.Failure<OutputModel>(exc.Message);
      }
    }
  }
}
