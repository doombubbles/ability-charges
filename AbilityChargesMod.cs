using System;
using System.Collections.Generic;
using System.Linq;
using MelonLoader;
using BTD_Mod_Helper;
using AbilityCharges;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;

[assembly: MelonInfo(typeof(AbilityChargesMod), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace AbilityCharges;

public class AbilityChargesMod : BloonsTD6Mod
{
    // Global dictionary storing charge counts for ability's by their ObjectId
    public static readonly Dictionary<ObjectId, int> Charges = new();

    public static readonly ModSettingInt MaxCharges = new(10)
    {
        description = "The maximum number of charges that each ability can accumulate. " +
                      "1 is the same as default behavior.",
        slider = true,
        min = 1,
        max = 100,
        icon = VanillaSprites.MoabEliminatorUpgradeIcon
    };

    public static readonly ModSettingInt MaxChargesForPassives = new(10)
    {
        description = "The maximum number of charges that passive abilities like Bomb Blitz can accumulate. " +
                      "Note that you'll have no way of knowing how many charges you have at any given time",
        slider = true,
        min = 1,
        max = 100,
        icon = VanillaSprites.BombBlitzUpgradeIcon
    };

    /// <summary>
    /// Make sure there's no accidental cross-pollination of charge counts between games
    /// </summary>
    public override void OnGameObjectsReset()
    {
        FlushCharges();
    }

    public static void FlushCharges()
    {
        if (InGame.instance == null || InGame.instance.bridge == null)
        {
            Charges.Clear();
            return;
        }

        try
        {
            var objectIds = InGame.instance.bridge
                .GetAllAbilities(true)
                .ToList()
                .Select(simulation => simulation.ability.Id)
                .ToHashSet();

            foreach (var ability in Charges.Keys.ToList().Where(id => !objectIds.Contains(id)))
            {
                Charges.Remove(ability);
            }
        }
        catch (Exception)
        {
            Charges.Clear();
        }
    }
}