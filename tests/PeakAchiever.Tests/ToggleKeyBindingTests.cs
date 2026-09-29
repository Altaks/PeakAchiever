using PeakAchiever.Controls;

namespace PeakAchiever.Tests;

public class ToggleKeyBindingTests
{
    [Theory]
    [InlineData("<Keyboard>/f6", "<Keyboard>/f6")]
    [InlineData("<Keyboard>/numpad1", "<Keyboard>/numpad1")]
    public void An_input_system_path_is_kept(string stored, string expected)
    {
        // when
        string? path = ToggleKeyBinding.FromConfig(stored);

        // then
        Assert.Equal(expected, path);
    }

    [Theory]
    [InlineData("F6", "<Keyboard>/f6")]
    [InlineData("F12", "<Keyboard>/f12")]
    [InlineData("J", "<Keyboard>/j")]
    [InlineData("Alpha3", "<Keyboard>/3")]
    [InlineData("Keypad7", "<Keyboard>/numpad7")]
    [InlineData("Return", "<Keyboard>/enter")]
    [InlineData("PageUp", "<Keyboard>/pageUp")]
    public void A_key_saved_by_an_older_version_converts(string stored, string expected)
    {
        // when
        string? path = ToggleKeyBinding.FromConfig(stored);

        // then
        Assert.Equal(expected, path);
    }

    [Theory]
    [InlineData("")]
    [InlineData("F6 + LeftShift")]
    [InlineData("Mouse3")]
    [InlineData("NotAKey")]
    public void A_value_that_cannot_convert_is_rejected(string stored)
    {
        // when
        string? path = ToggleKeyBinding.FromConfig(stored);

        // then
        Assert.Null(path);
    }
}
