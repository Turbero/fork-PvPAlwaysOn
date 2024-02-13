using System;
using HarmonyLib;
using PvPAlwaysOn.Util;
using WhereYouAt.Compatibility.WardIsLove;

namespace PvPAlwaysOn;

[HarmonyPatch]
static class PlayerUpdatePatch
{
    private static bool _insideWard;

    [HarmonyPatch(typeof(Player), nameof(Player.Update))]
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateCharacterStats))]
    [HarmonyPostfix]
    static void EnforcePvP(Player __instance)
    {
        if (!ZNetScene.instance) return;
        if (Game.instance && !Player.m_localPlayer) return;

        if (PvPAlwaysPlugin.ForcePvP.Value == PvPAlwaysPlugin.Toggle.Off) return;
        try
        {
            if (!InventoryGui.instance) return;
            _insideWard = WardIsLovePlugin.IsLoaded()
                ? WardMonoscript.InsideWard(Player.m_localPlayer.transform.position)
                : PrivateArea.InsideFactionArea(Player.m_localPlayer.transform.position, Character.Faction.Players);
            bool isInsideTerritory = Marketplace_API.IsInstalled() && Marketplace_API.IsPointInsideTerritoryWithFlag(Player.m_localPlayer.transform.position, Marketplace_API.TerritoryFlags.PveOnly, out _, out _, out _);
            if (isInsideTerritory) return;
            if (_insideWard && PvPAlwaysPlugin.OffInWards.Value == PvPAlwaysPlugin.Toggle.On) return;

            Functions.PvPEnforcer(InventoryGui.instance);
        }
        catch (Exception exception)
        {
            PvPAlwaysPlugin.PvPAlwaysLogger.LogError($"There was an error in setting the PvP {exception}");
        }
    }
}