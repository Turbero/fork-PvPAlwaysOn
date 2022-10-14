namespace PvPAlwaysOn.Util;

public class Functions
{
    internal static void PvPEnforcer(InventoryGui invGUI)
    {
        if (!Player.m_localPlayer) return;
        if (PvPAlwaysPlugin.ForcePvP is not { Value: PvPAlwaysPlugin.Toggle.On }) return;
        Player.m_localPlayer.m_pvp = PvPAlwaysPlugin.ForcePvP.Value == PvPAlwaysPlugin.Toggle.On;
        Player.m_localPlayer.SetPVP(PvPAlwaysPlugin.ForcePvP.Value == PvPAlwaysPlugin.Toggle.On);
        InventoryGui.instance.m_pvp.isOn = PvPAlwaysPlugin.ForcePvP.Value == PvPAlwaysPlugin.Toggle.On;
        invGUI.m_pvp.interactable = false;
    }
}