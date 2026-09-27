using System.Reflection;
using HarmonyLib;

namespace PeakAchiever.Tests;

/// <summary>
/// Checks each hook still resolves to a method of the game assembly. MonoMod cannot detour on the
/// .NET 10 test host, so this calls the resolver PatchAll uses, without applying the patches.
/// </summary>
public class HarmonyPatchTargetsTests
{
    // HarmonyX 2.9.0: internal static MethodBase HarmonyLib.PatchTools.GetOriginalMethod(this HarmonyMethod)
    // (read with ilspycmd). Internal, hence the reflection.
    private static readonly MethodInfo ResolveOriginal = typeof(Harmony)
        .Assembly.GetType("HarmonyLib.PatchTools", throwOnError: true)!
        .GetMethod("GetOriginalMethod", BindingFlags.Static | BindingFlags.NonPublic, [typeof(HarmonyMethod)])!;

    public static TheoryData<string> PatchMethods()
    {
        var data = new TheoryData<string>();
        foreach (MethodInfo patch in PatchMethodInfos())
            data.Add($"{patch.DeclaringType!.Name}.{patch.Name}");
        return data;
    }

    [Theory]
    [MemberData(nameof(PatchMethods))]
    public void Patch_resolves_its_target_in_the_game_assembly(string patchName)
    {
        // given
        MethodInfo patch = PatchMethodInfos().Single(m => $"{m.DeclaringType!.Name}.{m.Name}" == patchName);
        HarmonyMethod target = HarmonyMethod.Merge(
            [.. HarmonyMethodExtensions.GetFromType(patch.DeclaringType!), .. HarmonyMethodExtensions.GetFromMethod(patch)]
        );
        // PatchAll defaults an unset method type to Normal (HarmonyX 2.9.0 PatchClassProcessor constructor).
        target.methodType ??= MethodType.Normal;

        // when
        var original = (MethodBase?)ResolveOriginal.Invoke(null, [target]);

        // then
        Assert.NotNull(original);
        Assert.Equal("Assembly-CSharp", original!.DeclaringType!.Assembly.GetName().Name);
    }

    [Fact]
    public void All_seven_hooks_are_discovered()
    {
        // when
        int count = PatchMethodInfos().Count();

        // then
        Assert.Equal(7, count);
    }

    private static IEnumerable<MethodInfo> PatchMethodInfos() =>
        typeof(Plugin)
            .Assembly.GetTypes()
            .Where(type => type.GetCustomAttributes<HarmonyPatch>().Any())
            .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
            .Where(method => method.GetCustomAttributes<HarmonyPatch>().Any());
}
