using System;
using HarmonyLib;

namespace PvPAlwaysOn
{
    [HarmonyPatch]
    public class PvPPatcher
    {
        [HarmonyPatch(typeof(Player), nameof(Player.Update))]
        [HarmonyPostfix]
        private static void Postfix(Player __instance)
        {
            if (!ZNetScene.instance) return;
            if (Game.m_instance && !Player.m_localPlayer) return;

            if (!PvPAlwaysPlugin._forcePvP.Value) return;
            try
            {
                if (!InventoryGui.instance) return;
                PvPEnforcer(InventoryGui.instance);
            }
            catch (Exception exception)
            {
                PvPAlwaysPlugin.PvPAlwaysLogger.LogError($"There was an error in setting the PvP {exception}");
            }
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateCharacterStats))]
        [HarmonyPostfix]
        private static void Postfix(InventoryGui __instance)
        {
            try
            {
                if (PvPAlwaysPlugin._forcePvP != null && (!__instance || !PvPAlwaysPlugin._forcePvP.Value)) return;
                PvPEnforcer(__instance);
            }
            catch (Exception exception)
            {
                PvPAlwaysPlugin.PvPAlwaysLogger.LogError($"There was an error in setting the PvP {exception}");
            }
        }

        private static void PvPEnforcer(InventoryGui invGUI)
        {
            if (!Player.m_localPlayer) return;
            if (PvPAlwaysPlugin._forcePvP is not { Value: true }) return;
            Player.m_localPlayer.m_pvp = PvPAlwaysPlugin._forcePvP.Value;
            Player.m_localPlayer.SetPVP(PvPAlwaysPlugin._forcePvP.Value);
            InventoryGui.instance.m_pvp.isOn = PvPAlwaysPlugin._forcePvP.Value;
            invGUI.m_pvp.interactable = false;
        }
    }
}