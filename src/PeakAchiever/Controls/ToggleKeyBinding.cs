using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PeakAchiever.Controls;

/// <summary>Reads the tracker key from the config, where it is an Input System binding path.</summary>
internal static class ToggleKeyBinding
{
    public const string DefaultPath = "<Keyboard>/f6";

    private const string PathPrefix = "<";
    private const string KeyboardPrefix = "<Keyboard>/";

    // Up to 0.1.0 the key was a BepInEx KeyboardShortcut, written as a UnityEngine.KeyCode name.
    private static readonly Regex Letter = new("^[A-Z]$");
    private static readonly Regex FunctionKey = new("^F([1-9]|1[0-5])$");
    private static readonly Regex Digit = new("^Alpha([0-9])$");
    private static readonly Regex KeypadDigit = new("^Keypad([0-9])$");

    // KeyCode names whose Input System keyboard control is named otherwise.
    private static readonly Dictionary<string, string> NamedKeys = new()
    {
        ["Return"] = "enter",
        ["Space"] = "space",
        ["Tab"] = "tab",
        ["Backspace"] = "backspace",
        ["Insert"] = "insert",
        ["Delete"] = "delete",
        ["Home"] = "home",
        ["End"] = "end",
        ["PageUp"] = "pageUp",
        ["PageDown"] = "pageDown",
        ["BackQuote"] = "backquote",
    };

    /// <returns>The binding path, or null for a value it cannot read (a key with modifiers among them).</returns>
    public static string? FromConfig(string stored)
    {
        string value = stored.Trim();
        if (value.StartsWith(PathPrefix, System.StringComparison.Ordinal))
            return value;
        if (Letter.IsMatch(value) || FunctionKey.IsMatch(value))
            return KeyboardPrefix + value.ToLowerInvariant();
        if (Digit.Match(value) is { Success: true } digit)
            return KeyboardPrefix + digit.Groups[1].Value;
        if (KeypadDigit.Match(value) is { Success: true } keypad)
            return KeyboardPrefix + "numpad" + keypad.Groups[1].Value;
        return NamedKeys.TryGetValue(value, out string control) ? KeyboardPrefix + control : null;
    }
}
