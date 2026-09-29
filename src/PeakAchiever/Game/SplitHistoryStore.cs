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
    private Guid? _runKey;

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
        // A mini run starts in a later biome (RunManager.JumpToMiniRunBiomeWhenReady), so its times
        // are not those of a climb.
        if (RunSettings.isMiniRun)
            return;
        Guid runId = RunKey();
        string[] lines = splits
            .Select((split, index) => (split, index))
            .Where(entry => entry.split.IsWhole && (runEnded || !entry.split.IsCurrent))
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

    /// <summary>Called when a run begins: its times go under a key of their own.</summary>
    public void StartRun() => _runKey = null;

    /// <summary>
    /// The key this run's times are kept under, fixed at its first record: the game's run id
    /// (RunManager.RunId, shared by the lobby and restored on a rejoin), or, when the game gave none,
    /// one made for the run. Fixed so a run never splits across two keys if the game's id comes late.
    /// </summary>
    private Guid RunKey()
    {
        if (_runKey is { } key)
            return key;
        Guid fromGame = RunManager.Instance.RunId;
        if (fromGame == Guid.Empty)
            _log.LogInfo("The game gave this run no id; its biome times are kept under one made for it.");
        _runKey = fromGame == Guid.Empty ? Guid.NewGuid() : fromGame;
        return _runKey.Value;
    }

    /// <summary>Erases every biome time of one ascent, in memory and in the file.</summary>
    /// <returns>False when the file could not be rewritten; the times are then gone only until a restart.</returns>
    public bool EraseAscent(int ascent)
    {
        History.RemoveAscent(ascent);
        try
        {
            File.WriteAllLines(_path, History.Splits.Select(split => split.ToLine()));
            return true;
        }
        catch (IOException e)
        {
            _log.LogError($"Could not erase ascent {ascent} from {_path}: {e.Message}");
            return false;
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
