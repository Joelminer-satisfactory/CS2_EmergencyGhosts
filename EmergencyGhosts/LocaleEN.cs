using System.Collections.Generic;
using EmergencyGhosts;
using Colossal;
using Game.Modding;

public class LocaleEN : IDictionarySource
{
    private readonly Setting m_Setting;

    public LocaleEN(Setting setting)
    {
        m_Setting = setting;
    }

    public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
    {
        return new Dictionary<string, string>
        {
            {
                ((ModSetting)m_Setting).GetSettingsLocaleID(),
                "Emergency Ghosts"
            },
            {
                ((ModSetting)m_Setting).GetOptionTabLocaleID("Main"),
                "Main"
            },
            {
                ((ModSetting)m_Setting).GetOptionTabLocaleID("Tweaks"),
                "Tweaks"
            },
            {
                ((ModSetting)m_Setting).GetOptionGroupLocaleID("Toggle"),
                "Settings"
            },
            {
                ((ModSetting)m_Setting).GetOptionLabelLocaleID("Enabled"),
                "Enable Emergency vehicle ghosting"
            },
            {
                ((ModSetting)m_Setting).GetOptionDescLocaleID("Enabled"),
                "When enabled, emergency vehicles will ignore other vehicles and drive through them. Traffic signals, pedestrians, and trains are still respected."
            },
            {
                ((ModSetting)m_Setting).GetOptionLabelLocaleID("EmergencyOnly"),
                "Only while responding to emergencies"
            },
            {
                ((ModSetting)m_Setting).GetOptionDescLocaleID("EmergencyOnly"),
                "When enabled, emergency vehicles will only ghost when actively responding to an emergency, or transporting a patient."
            },
            {
                ((ModSetting)m_Setting).GetOptionLabelLocaleID("SpeedMultiplier"),
                "Speed Multiplier"
            },
            {
                ((ModSetting)m_Setting).GetOptionDescLocaleID("SpeedMultiplier"),
                "The percentage of max speed to use when ghosting through traffic. Minimum value is 1%. To stop vehicles ghosting completely, use the general setting"
            },
            {
                ((ModSetting)m_Setting).GetOptionLabelLocaleID("ResetButton"),
                "Reset to Defaults"
            },
            {
                ((ModSetting)m_Setting).GetOptionDescLocaleID("ResetButton"),
                "Resets all settings to their default values."
            },
            {
                ((ModSetting)m_Setting).GetOptionWarningLocaleID("ResetButton"),
                "Are you sure you want to reset all settings to their default values?"
            },
            {
                ((ModSetting)m_Setting).GetOptionLabelLocaleID("versionInfo"),
                "Version 1.0.5dev3"
            }

        };
    }

    public void Unload()
    {
    }
}