using EmergencyGhosts;
using Colossal;
using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using Game.Simulation;
using static Game.UI.Menu.AssetUploadPanelUISystem;

public class Mod : IMod
{
    public static ILog log = LogManager.GetLogger("EmergencyGhosts.Mod").SetShowsErrorsInUI(false);

    public static Setting m_Setting;

    public void OnLoad(UpdateSystem updateSystem)
    {
        log.Info((object)"OnLoad");
        ExecutableAsset asset = default(ExecutableAsset);
        if (GameManager.instance.modManager.TryGetExecutableAsset((IMod)(object)this, out asset))
        {
            log.Info((object)("Current mod asset at " + ((AssetData)asset).path));
        }
        m_Setting = new Setting((IMod)(object)this);
        ((ModSetting)m_Setting).RegisterInOptionsUI();
        GameManager.instance.localizationManager.AddSource("en-US", (IDictionarySource)(object)new LocaleEN(m_Setting));
        AssetDatabase.global.LoadSettings("EmergencyGhosts", (object)m_Setting, (object)new Setting((IMod)(object)this), false);
        updateSystem.UpdateBefore<EmergencyGhostsSystem, CarMoveSystem>((SystemUpdatePhase)12);
        log.Info((object)"EmergencyGhosts registered before CarMoveSystem");
    }

    public void OnDispose()
    {
        log.Info((object)"OnDispose");
        if (m_Setting != null)
        {
            ((ModSetting)m_Setting).UnregisterInOptionsUI();
            m_Setting = null;
        }
    }
}
