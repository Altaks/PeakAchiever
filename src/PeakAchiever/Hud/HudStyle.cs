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
    // A light rim that draws the bar's shape against any background.
    public static readonly Color BarRim = new(Ink.r, Ink.g, Ink.b, 0.4f);
    public static readonly Color ProgressFill = Rgb(0xF2, 0xC1, 0x4E);
    public static readonly Color Achieved = Rgb(0x8F, 0xD4, 0x6A);
    public static readonly Color Unattainable = Rgb(0xFF, 0x7A, 0x66);
    // Between the progress yellow and the cross red: a limit close by, not reached.
    public static readonly Color Caution = Rgb(0xFF, 0x9F, 0x43);
    public static readonly Color LockedIconTint = new(0.45f, 0.45f, 0.45f, 1f);
    public static readonly Color PinMarkerInk = Rgb(0x1D, 0x18, 0x13);
    // The thread of the card's seam, the warm tan of a scout patch's stitching (signed-off mockup).
    public static readonly Color Seam = new Color32(0xC9, 0xA8, 0x6B, 0xBF);
    // A chip's fill, so its outlined text reads over any scene.
    public static readonly Color ChipFill = new(MarkBackground.r, MarkBackground.g, MarkBackground.b, 0.6f);

    // TMP font assets shipped in the game's resources.assets (names read with UnityPy, game v2.4.c).
    private const string DisplayFontName = "DarumaDropOne-Regular SDF";
    private const string BodyFontName = "Montserrat-Medium SDF";
    private const string StrongFontName = "Montserrat-ExtraBold SDF";
    // The check mark sprite the game ships in sharedassets1.assets (108x95, white, tinted at runtime).
    private const string GameCheckSpriteName = "Check";

    private const int RoundedTextureSize = 32;
    // The seam: a dashed rounded outline, tiled along the edges. The edges' tiled middle (size minus both
    // borders) holds a whole number of dashes, so the tiles join without a gap.
    private const int SeamTextureSize = 32;
    private const int SeamCornerRadius = 12;
    private const float SeamThickness = 1.5f;
    private const int SeamDashPeriod = 8;
    private const int SeamDashOn = 5;
    private const int RoundedCornerRadius = 12;
    private const int CircleTextureSize = 64;
    private const int StarTextureSize = 64;
    private const int StarPoints = 5;
    // Inner radius of a regular five-point star, as a share of the outer one.
    private const float StarInnerRatio = 0.4f;
    private const int StarSupersampling = 4;
    // The tear masks: wider than tall like a card, the jagged line down the middle.
    private const int TearTextureWidth = 128;
    private const int TearTextureHeight = 64;
    // Where the line crosses each of its evenly spaced heights, off the middle, in texels:
    // an irregular zigzag, top to bottom.
    private static readonly float[] TearZigzag = [0f, 5f, -4f, 6f, -3f, 4f, -6f, 3f, -2f, 5f, 0f];

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
        SeamOutline = CreateSeam();
        Circle = CreateCircle();
        Pill = CreatePill(Circle.texture);
        Star = CreateStar();
        TearLeft = CreateTear(keepLeft: true);
        TearRight = CreateTear(keepLeft: false);
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

    /// <summary>The card's stitched seam: a dashed rounded outline, sliced, its edges meant to be tiled.</summary>
    public Sprite SeamOutline { get; }

    /// <summary>A 9-sliced rounded rectangle for card backgrounds and bars.</summary>
    public Sprite RoundedRect { get; }

    public Sprite Circle { get; }

    /// <summary>
    /// The circle cut in nine, its borders the whole radius: stretched, it keeps half-circle ends
    /// (see <see cref="UiFactory.AddPill"/>).
    /// </summary>
    public Sprite Pill { get; }

    /// <summary>The border of <see cref="Pill"/>, in texture pixels.</summary>
    public const float PillBorder = CircleTextureSize / 2f - 1f;

    /// <summary>Masks the left side of a card, up to a jagged tear down its middle.</summary>
    public Sprite TearLeft { get; }

    /// <summary>Masks the right side of a card, from the same tear as <see cref="TearLeft"/>.</summary>
    public Sprite TearRight { get; }

    /// <summary>A five-point star, drawn in code like <see cref="Circle"/>.</summary>
    public Sprite Star { get; }

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

    private static Sprite CreateSeam()
    {
        const float inset = SeamThickness;
        Texture2D texture = CreateMask(
            SeamTextureSize,
            (x, y) =>
            {
                float px = x + 0.5f;
                float py = y + 0.5f;
                // Distance to the rounded outline, inset so the line stays inside the texture.
                float cx = Mathf.Clamp(px, SeamCornerRadius, SeamTextureSize - SeamCornerRadius);
                float cy = Mathf.Clamp(py, SeamCornerRadius, SeamTextureSize - SeamCornerRadius);
                float fromLine = Mathf.Abs(Vector2.Distance(new Vector2(px, py), new Vector2(cx, cy)) - (SeamCornerRadius - inset));
                float line = Mathf.Clamp01(SeamThickness / 2f - fromLine + 0.5f);
                bool onCorner = px != cx && py != cy;
                // Along a straight edge, the dash pattern follows the coordinate running along it.
                float along = py == cy ? y : x;
                bool dash = onCorner || along % SeamDashPeriod < SeamDashOn;
                return dash ? line : 0f;
            }
        );
        var border = new Vector4(SeamCornerRadius, SeamCornerRadius, SeamCornerRadius, SeamCornerRadius);
        return Sprite.Create(texture, new Rect(0, 0, SeamTextureSize, SeamTextureSize), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
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

    // One texel is left between the borders, for the stretched middle.
    private static Sprite CreatePill(Texture2D circle) =>
        Sprite.Create(
            circle,
            new Rect(0, 0, CircleTextureSize, CircleTextureSize),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(PillBorder, PillBorder, PillBorder, PillBorder)
        );

    private static Sprite CreateStar()
    {
        Vector2[] outline = StarOutline(StarTextureSize / 2f);
        Texture2D texture = CreateMask(StarTextureSize, (x, y) => SupersampledCoverage(x, y, outline));
        return Sprite.Create(texture, new Rect(0, 0, StarTextureSize, StarTextureSize), new Vector2(0.5f, 0.5f));
    }

    // Ten points alternating outer and inner radius, the first pointing up.
    private static Vector2[] StarOutline(float radius)
    {
        var points = new Vector2[StarPoints * 2];
        for (int i = 0; i < points.Length; i++)
        {
            float angle = Mathf.PI / 2f + i * Mathf.PI / StarPoints;
            float distance = i % 2 == 0 ? radius : radius * StarInnerRatio;
            points[i] = new Vector2(radius + distance * Mathf.Cos(angle), radius + distance * Mathf.Sin(angle));
        }
        return points;
    }

    // The share of a pixel's sub-samples inside the outline, for a smooth edge.
    private static float SupersampledCoverage(int x, int y, Vector2[] outline)
    {
        int inside = 0;
        for (int sy = 0; sy < StarSupersampling; sy++)
        for (int sx = 0; sx < StarSupersampling; sx++)
        {
            var point = new Vector2(x + (sx + 0.5f) / StarSupersampling, y + (sy + 0.5f) / StarSupersampling);
            if (InsidePolygon(point, outline))
                inside++;
        }
        return (float)inside / (StarSupersampling * StarSupersampling);
    }

    // Even-odd ray casting.
    private static bool InsidePolygon(Vector2 point, Vector2[] polygon)
    {
        bool inside = false;
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            if ((polygon[i].y > point.y) != (polygon[j].y > point.y)
                && point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x)
                inside = !inside;
        }
        return inside;
    }

    private static Sprite CreateTear(bool keepLeft)
    {
        Texture2D texture = CreateMask(
            TearTextureWidth,
            TearTextureHeight,
            (x, y) =>
            {
                // One texel of anti-aliasing across the line.
                float left = Mathf.Clamp01(TearLineAt(y) - x);
                return keepLeft ? left : 1f - left;
            }
        );
        return Sprite.Create(texture, new Rect(0, 0, TearTextureWidth, TearTextureHeight), new Vector2(0.5f, 0.5f));
    }

    private static float TearLineAt(int y)
    {
        float along = (float)y / (TearTextureHeight - 1) * (TearZigzag.Length - 1);
        int below = Mathf.Min((int)along, TearZigzag.Length - 2);
        float offset = Mathf.Lerp(TearZigzag[below], TearZigzag[below + 1], along - below);
        return TearTextureWidth / 2f + offset;
    }

    private static Texture2D CreateMask(int size, System.Func<int, int, float> alphaAt) => CreateMask(size, size, alphaAt);

    private static Texture2D CreateMask(int width, int height, System.Func<int, int, float> alphaAt)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
        };
        var pixels = new Color32[width * height];
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
            pixels[y * width + x] = new Color32(255, 255, 255, (byte)(alphaAt(x, y) * 255));
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
