using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Logging;
using PeakAchiever.Tracking;

namespace PeakAchiever.Game;

/// <summary>
/// The biomes finished in past runs, one line each in a file beside the config, so the Speed Climber
/// ETA carries over from one game session to the next.
/// </summary>
internal sealed class SplitHistoryStore
{
    private const string FileName = "Altaks.PeakAchiever.splits.csv";

    private readonly string _path;
    private readonly ManualLogSource _log;
    private bool _warnedNoRunId;

    public SplitHistoryStore(string configDirectory, ManualLogSource log)
    {
        _path = Path.Combine(configDirectory, FileName);
        _log = log;
        History = new SplitHistory(Load());
    }

    public SplitHistory History { get; }

    /// <summary>
    /// Records each biome of this run finished and not yet recorded; the current one too when
    /// <paramref name="runEnded"/>, since reaching the summit ends it.
    /// </summary>
    public void RecordFinished(IReadOnlyList<BiomeSplit> splits, bool runEnded)
    {
        Guid runId = RunManager.Instance.RunId;
        // A mini run starts in a later biome (RunManager.JumpToMiniRunBiomeWhenReady), so its times
        // are not those of a climb.
        if (RunSettings.isMiniRun)
            return;
        if (runId == Guid.Empty)
        {
            if (!_warnedNoRunId)
                _log.LogWarning("This run has no id; its biome times are not recorded for the ETA.");
            _warnedNoRunId = true;
            return;
        }
        // A scout who joined mid-run saw only the end of their first biome.
        int firstWhole = RunFactsReader.JoinedMidRun ? 1 : 0;
        string[] lines = splits
            .Select((split, index) => (split, index))
            .Where(entry => entry.index >= firstWhole && (runEnded || !entry.split.IsCurrent))
            .Select(entry => new PastSplit(runId, Ascents.currentAscent, entry.index, entry.split.Biome, entry.split.Seconds))
            .Where(History.Add)
            .Select(split => split.ToLine())
            .ToArray();
        if (lines.Length == 0)
            return;
        try
        {
            File.AppendAllLines(_path, lines);
        }
        catch (IOException e)
        {
            _log.LogError($"Could not save the biome times to {_path}: {e.Message}");
        }
    }

    private IEnumerable<PastSplit> Load()
    {
        if (!File.Exists(_path))
            yield break;
        string[] lines;
        try
        {
            lines = File.ReadAllLines(_path);
        }
        catch (IOException e)
        {
            _log.LogError($"Could not read the biome times from {_path}, the ETA starts empty: {e.Message}");
            yield break;
        }
        foreach (string line in lines)
        {
            if (PastSplit.TryParse(line, out PastSplit split))
                yield return split;
            else
                _log.LogWarning($"Ignoring a malformed line in {_path}: '{line}'");
        }
    }
}
