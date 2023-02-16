using System;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors.Abilities;

namespace AbilityCharges;

public static class Extensions
{
    public static int GetCharges(this Ability ability) =>
        AbilityChargesMod.Charges.TryGetValue(ability.Id, out var result) ? result : 0;

    public static void SetCharges(this Ability ability, int charges) =>
        AbilityChargesMod.Charges[ability.Id] = Math.Clamp(charges, 0, ability.GetMaxCharges());

    public static void IncrementCharges(this Ability ability, int value = 1) =>
        ability.SetCharges(ability.GetCharges() + value);

    public static void DecrementCharges(this Ability ability, int value = 1) =>
        ability.SetCharges(ability.GetCharges() - value);

    public static string SaveKey(this Ability ability) => ability.abilityModel.displayName + " Charges";

    public static int GetMaxCharges(this Ability ability) => ability.abilityModel.IsPassive
        ? AbilityChargesMod.MaxChargesForPassives
        : AbilityChargesMod.MaxCharges;
}