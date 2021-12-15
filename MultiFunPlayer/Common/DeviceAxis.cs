using MultiFunPlayer.Settings.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace MultiFunPlayer.Common;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
[TypeConverter(typeof(DeviceAxisTypeConverter))]
public sealed class DeviceAxis
{
    private int _id;

    [JsonProperty] public string Name { get; init; }
    [JsonProperty] public float DefaultValue { get; init; }
    [JsonProperty] public string FriendlyName { get; init; }
    [JsonProperty] public IEnumerable<string> FunscriptNames { get; init; }

    public override string ToString() => Name;
    public override int GetHashCode() => _id;

    [OnDeserialized]
    internal void OnDeserializedMethod(StreamingContext context) => _id = _count++;

    private static int _count;
    private static int _outputMaximum;
    private static string _outputFormat;
    private static Dictionary<string, DeviceAxis> _axes;
    public static IReadOnlyCollection<DeviceAxis> All => _axes.Values;

    public static DeviceAxis Parse(string name) => _axes.GetValueOrDefault(name, null);
    public static IEnumerable<DeviceAxis> Parse(params string[] names) => names.Select(n => Parse(n));

    public static bool TryParse(string name, out DeviceAxis axis)
    {
        axis = Parse(name);
        return axis != null;
    }

    public static string ToString(DeviceAxis axis, float value) => $"{axis}{string.Format(_outputFormat, value * _outputMaximum)}";
    public static string ToString(DeviceAxis axis, float value, int interval) => $"{ToString(axis, value)}I{interval}";


// ISSUE: (no external bug tracker) Issue w/ Tempest firmware as of December 2021 where sometimes the last character is dropped
// FIX: Add " A90" before \n this ensures the last character can be safely discarded as long as the user isn't using axis A9
// FUTURE: remove this fix when no longer necessary for the firmware
public static string ToString(IEnumerable<KeyValuePair<DeviceAxis, float>> values, int interval)
        => $"{values.Aggregate(string.Empty, (s, x) => $"{s} {ToString(x.Key, x.Value, interval)}")} A90\n".TrimStart();

    public static bool IsDirty(float value, float lastValue)
        => float.IsFinite(value) && (!float.IsFinite(lastValue) || MathF.Abs(lastValue - value) * 999 >= 1);

    public static void LoadSettings(JObject settings, JsonSerializer serializer)
    {
        if (!settings.TryGetValue<List<DeviceAxis>>("Axes", serializer, out var axes)
         || !settings.TryGetValue<int>("OutputPrecision", serializer, out var precision))
            throw new JsonReaderException("Unable to read device settings");

        _outputMaximum = (int)(MathF.Pow(10, precision) - 1);
        _outputFormat = $"{{0:{new string('0', precision)}}}";
        _axes = axes.ToDictionary(a => a.Name, a => a);
    }
}
