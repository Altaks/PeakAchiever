using System.IO;
using System.Linq;
using BepInEx.Logging;
using TMPro;
using UnityEngine;

namespace PeakAchiever.Hud;

/// <summary>
/// Colours, fonts and sprites of the tracker, resolved once. Values come from the signed-off mockup;
/// fonts and the check mark are the game's own assets, the cross and pin ship with the mod.
/// </summary>
internal sealed class HudStyle
{
    public static readonly Color Ink = Rgb(0xF6, 0xEF, 0xE3);
    public static readonly Color InkSoft = Rgb(0xD9, 0xCF, 0xBF);
    public static readonly Color InkMuted = Rgb(0xA8, 0x9C, 0x8A);
    public static readonly Color CardBackground = new(24f / 255, 19f / 255, 15f / 255, 0.74f);
    public static readonly Color MarkBackground = Rgb(0x18, 0x13, 0x0F);
    public static readonly Color BarTrack = new(Ink.r, Ink.g, Ink.b, 0.16f);
    public static readonly Color ProgressFill = Rgb(0xF2, 0xC1, 0x4E);
    public static readonly Color Achieved = Rgb(0x8F, 0xD4, 0x6A);
    public static readonly Color Unattainable = Rgb(0xFF, 0x7A, 0x66);
    public static readonly Color LockedIconTint = new(0.45f, 0.45f, 0.45f, 1f);
    public static readonly Color PinMarkerInk = Rgb(0x1D, 0x18, 0x13);

    // TMP font assets shipped in the game's resources.assets (names read with UnityPy, game v2.4.c).
    private const string DisplayFontName = "DarumaDropOne-Regular SDF";
    private const string BodyFontName = "Montserrat-Medium SDF";
    private const string StrongFontName = "Montserrat-ExtraBold SDF";
    // The check mark sprite the game ships in sharedassets1.assets (108x95, white, tinted at runtime).
    private const string GameCheckSpriteName = "Check";

    private const int RoundedTextureSize = 32;
    private const int RoundedCornerRadius = 12;
    private const int CircleTextureSize = 64;

    // Mod assets are embedded under this logical name prefix (see the csproj EmbeddedResource item).
    private const string EmbeddedAssetPrefix = "PeakAchiever.Assets.";

    /// <summary>
    /// Build it once the game's UI has loaded: before that, its fonts and sprites are not in memory.
    /// </summary>
    public HudStyle(ManualLogSource log)
    {
        TMP_FontAsset[] fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        DisplayFont = FindFont(fonts, DisplayFontName, log);
        BodyFont = FindFont(fonts, BodyFontName, log);
        StrongFont = FindFont(fonts, StrongFontName, log);
        RoundedRect = CreateRoundedRect();
        Circle = CreateCircle();
        Cross = LoadEmbeddedSprite("Cross.png", log);
        Pin = LoadEmbeddedSprite("Pin.png", log);
        Warning = LoadEmbeddedSprite("Warning.png", log);
        Sprite? gameCheck = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(s => s.name == GameCheckSpriteName);
        if (gameCheck == null)
            log.LogError($"Game sprite '{GameCheckSpriteName}' not found; earned badges show a plain dot instead.");
        Check = gameCheck ?? Circle;
    }

    public TMP_FontAsset DisplayFont { get; }
    public TMP_FontAsset BodyFont { get; }
    public TMP_FontAsset StrongFont { get; }
    public Sprite Check { get; }
    public Sprite Cross { get; }
    public Sprite Pin { get; }
    public Sprite Warning { get; }

    /// <summary>A 9-sliced rounded rectangle for card backgrounds and bars.</summary>
    public Sprite RoundedRect { get; }

    public Sprite Circle { get; }

    private static TMP_FontAsset FindFont(TMP_FontAsset[] fonts, string name, ManualLogSource log)
    {
        TMP_FontAsset? font = fonts.FirstOrDefault(f => f.name == name);
        if (font != null)
            return font;
        log.LogError($"Game font '{name}' not found; falling back to the TextMeshPro default font.");
        return TMP_Settings.defaultFontAsset;
    }

    private static Sprite LoadEmbeddedSprite(string fileName, ManualLogSource log)
    {
        string resourceName = EmbeddedAssetPrefix + fileName;
        using Stream? stream = typeof(HudStyle).Assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new FileNotFoundException($"Embedded asset '{resourceName}' is missing from the mod assembly.");
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false) { name = fileName };
        if (!texture.LoadImage(buffer.ToArray()))
            log.LogError($"Embedded asset '{resourceName}' is not a readable PNG.");
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private static Sprite CreateRoundedRect()
    {
        Texture2D texture = CreateMask(RoundedTextureSize, (x, y) => InsideRoundedRect(x, y, RoundedTextureSize, RoundedCornerRadius));
        var border = new Vector4(RoundedCornerRadius, RoundedCornerRadius, RoundedCornerRadius, RoundedCornerRadius);
        return Sprite.Create(
            texture,
            new Rect(0, 0, RoundedTextureSize, RoundedTextureSize),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            border
        );
    }

    private static Sprite CreateCircle()
    {
        float radius = CircleTextureSize / 2f;
        Texture2D texture = CreateMask(
            CircleTextureSize,
            (x, y) => Coverage(Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(radius, radius)), radius)
        );
        return Sprite.Create(texture, new Rect(0, 0, CircleTextureSize, CircleTextureSize), new Vector2(0.5f, 0.5f));
    }

    private static Texture2D CreateMask(int size, System.Func<int, int, float> alphaAt)
    {
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
        };
        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
            pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alphaAt(x, y) * 255));
        texture.SetPixels32(pixels);
        texture.Apply();
        return texture;
    }

    private static float InsideRoundedRect(int x, int y, int size, int radius)
    {
        float cx = Mathf.Clamp(x + 0.5f, radius, size - radius);
        float cy = Mathf.Clamp(y + 0.5f, radius, size - radius);
        return Coverage(Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy)), radius);
    }

    /// <summary>One pixel of anti-aliasing at the shape edge.</summary>
    private static float Coverage(float distanceFromCentre, float radius) =>
        Mathf.Clamp01(radius - distanceFromCentre + 0.5f);

    private static Color Rgb(byte r, byte g, byte b) => new Color32(r, g, b, 255);
}
