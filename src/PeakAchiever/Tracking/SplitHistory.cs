using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PeakAchiever.Tracking;

/// <summary>A biome finished in a past run: which run, at which ascent, its place in the run, and how long it took.</summary>
internal readonly record struct PastSplit(Guid RunId, int Ascent, int Index, Biome.BiomeType Biome, float Seconds)
{
    private const char FieldSeparator = ',';
    private const int FieldCount = 5;

    public string ToLine() =>
        string.Join(
            FieldSeparator.ToString(),
            RunId.ToString(),
            Ascent.ToString(CultureInfo.InvariantCulture),
            Index.ToString(CultureInfo.InvariantCulture),
            Biome.ToString(),
            Seconds.ToString("R", CultureInfo.InvariantCulture)
        );

    public static bool TryParse(string line, out PastSplit split)
    {
        split = default;
        string[] fields = line.Split(FieldSeparator);
        if (
            fields.Length != FieldCount
            || !Guid.TryParse(fields[0], out Guid runId)
            || !int.TryParse(fields[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int ascent)
            || !int.TryParse(fields[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int index)
            || !Enum.TryParse(fields[3], out Biome.BiomeType biome)
            || !Enum.IsDefined(typeof(Biome.BiomeType), biome)
            || !float.TryParse(fields[4], NumberStyles.Float, CultureInfo.InvariantCulture, out float seconds)
        )
            return false;
        split = new PastSplit(runId, ascent, index, biome, seconds);
        return true;
    }
}

/// <summary>Every biome finished in past runs, one entry per run and place in the run.</summary>
internal sealed class SplitHistory
{
    private readonly Dictionary<(Guid RunId, int Index), PastSplit> _splits = [];

    public SplitHistory(IEnumerable<PastSplit> splits)
    {
        foreach (PastSplit split in splits)
            Add(split);
    }

    /// <returns>False when that run's split at that place is already known, as after a rejoin.</returns>
    public bool Add(PastSplit split)
    {
        if (_splits.ContainsKey((split.RunId, split.Index)))
            return false;
        _splits[(split.RunId, split.Index)] = split;
        return true;
    }

    /// <summary>The median time of each biome finished at this ascent.</summary>
    public IReadOnlyDictionary<Biome.BiomeType, float> MediansAt(int ascent) =>
        _splits
            .Values.Where(split => split.Ascent == ascent)
            .GroupBy(split => split.Biome)
            .ToDictionary(biome => biome.Key, biome => Median(biome.Select(split => split.Seconds).ToArray()));

    private static float Median(float[] values)
    {
        Array.Sort(values);
        int middle = values.Length / 2;
        return values.Length % 2 == 1 ? values[middle] : (values[middle - 1] + values[middle]) / 2f;
    }
}
