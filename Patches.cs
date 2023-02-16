
using System.Linq;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors.Abilities;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu;
using Il2CppSystem.Collections.Generic;

namespace AbilityCharges;

/// <summary>
/// If it's got 1 or more charges, it should think there's no cooldown happening
/// </summary>
[HarmonyPatch(typeof(Ability), nameof(Ability.IsReady))]
internal static class Ability_IsReady
{
    [HarmonyPrefix]
    private static void Prefix(Ability __instance, ref float __state)
    {
        __state = __instance.CooldownRemaining;
        if (__instance.GetCharges() >= 1)
        {
            __instance.CooldownRemaining = 0;
        }
    }

    [HarmonyPostfix]
    private static void Postfix(Ability __instance, ref float __state)
    {
        __instance.CooldownRemaining = __state;
    }
}

/// <summary>
/// Allow it to be activated based on charge count rather than cooldown
/// </summary>
[HarmonyPatch(typeof(Ability), nameof(Ability.Activate))]
internal static class Ability_Activate
{
    [HarmonyPrefix]
    private static void Prefix(Ability __instance, ref float __state)
    {
        __state = __instance.CooldownRemaining;
        if (__instance.GetCharges() >= 1)
        {
            __instance.CooldownRemaining = 0;
        }
    }

    [HarmonyPostfix]
    private static void Postfix(Ability __instance, ref float __state)
    {
        // If the cooldown was set back to above 0 when a charge is available, it was successfully used
        if (__instance.GetCharges() >= 1 && __instance.CooldownRemaining > 0)
        {
            var shouldAllowCooldown = __instance.GetCharges() == __instance.GetMaxCharges();
            __instance.DecrementCharges();

            // Let the full cooldown begin again if we're using it from max charges
            if (shouldAllowCooldown) return;
        }

        __instance.CooldownRemaining = __state;
    }
}

/// <summary>
/// Increment the charges whenever the cooldown hits 0
/// </summary>
[HarmonyPatch(typeof(Ability), nameof(Ability.Process))]
internal static class Ability_Process
{
    [HarmonyPostfix]
    private static void Postfix(Ability __instance)
    {
        // The cooldown is not going, and the charges are less than max
        if (__instance.CooldownRemaining == 0 && __instance.GetCharges() < __instance.GetMaxCharges())
        {
            __instance.IncrementCharges();
            __instance.CooldownRemaining = __instance.abilityModel.cooldownFrames;
        }
    }
}

/// <summary>
/// Make refreshing the cooldown simply give it a whole additional charge
/// </summary>
[HarmonyPatch(typeof(Ability), nameof(Ability.RefreshCooldown))]
internal static class Ability_RefreshCooldown
{
    [HarmonyPrefix]
    private static bool Prefix(Ability __instance)
    {
        __instance.IncrementCharges();
        return false;
    }
}

/// <summary>
/// Make clearing the cooldown simply give it a whole additional charge
/// </summary>
[HarmonyPatch(typeof(Ability), nameof(Ability.ClearCooldown))]
internal static class Ability_ClearCooldown
{
    [HarmonyPrefix]
    private static bool Prefix(Ability __instance, ref float __state)
    {
        __state = __instance.CooldownRemaining;
        __instance.SetCharges(__instance.GetCharges() + 1);
        return true;
    }

    [HarmonyPostfix]
    private static void Postfix(Ability __instance, ref float __state)
    {
        __instance.CooldownRemaining = __state;
    }
}

/// <summary>
/// Allow the charges to be properly saved in the metadata
/// </summary>
[HarmonyPatch(typeof(Ability), nameof(Ability.GetSaveMetaData))]
internal static class Ability_GetSaveMetaData
{
    [HarmonyPostfix]
    private static void Postfix(Ability __instance, Dictionary<string, string> metaData)
    {
        metaData[__instance.SaveKey()] = __instance.GetCharges().ToString();
    }
}

/// <summary>
/// Allow the charges to be properly set from the save metadata
/// </summary>
[HarmonyPatch(typeof(Ability), nameof(Ability.SetSaveMetaData))]
internal static class Ability_SetSaveMetaData
{
    [HarmonyPostfix]
    private static void Postfix(Ability __instance, Dictionary<string, string> metaData)
    {
        if (metaData.ContainsKey(__instance.SaveKey()) && int.TryParse(metaData[__instance.SaveKey()], out var charges))
        {
            __instance.SetCharges(charges);
        }
        else
        {
            __instance.SetCharges(__instance.CooldownRemaining > 0 ? 0 : 1);
        }
    }
}

/// <summary>
/// Allow it to visually show multiple charges from the same ability
/// </summary>
[HarmonyPatch(typeof(StackedAbilityButton), nameof(StackedAbilityButton.GetReadyCount))]
internal static class StackedAbilityButton_GetReadyCount
{
    [HarmonyPrefix]
    private static bool Prefix(StackedAbilityButton __instance, ref int __result)
    {
        __result = __instance.abilities.ToList().Sum(a2s => a2s.ability.GetCharges());
        return false;
    }
}