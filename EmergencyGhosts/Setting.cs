using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using Game.UI;

namespace EmergencyGhosts
{
    [FileLocation("EmergencyGhosts")]
    [SettingsUIGroupOrder(new string[] { "Main Toggle", "Main Slider", "Main Button", "Tweaks Toggle", "Tweaks Slider", "Tweaks Button" })]
    [SettingsUITabOrder(new string[] { "Main", "Tweaks" })]
    public class Setting : ModSetting
    {
        public Setting(IMod mod) : base(mod)
        {
        }

        public const string kSection = "Main";

        public const string kToggleGroup = "Toggle";

        public const string kSliderGroup = "Slider";

        public const string kButtonGroup = "Button";
        
        public const string kInfoGroup = "Info";

        [SettingsUISection("Main", "Toggle")]
        public bool Enabled { get; set; } = true;

        [SettingsUISection("Tweaks", "Toggle")]
        public bool EmergencyOnly { get; set; } = true;

        [SettingsUISection("Tweaks", "Slider")]
        [SettingsUISlider(min = 1f, max = 100f, step = 1f, scalarMultiplier = 1f, unit = Unit.kPercentage)]
        public float SpeedMultiplier { get; set; } = 50f;

        [SettingsUISection("Main", "Button")]
        [SettingsUIButton]
        [SettingsUIConfirmation]
        public bool ResetButton
        {
            set
            {
                SetDefaults();
            }
        }

        [SettingsUISection("Main", "Info")]
        [SettingsUIMultilineText]
        public string versionInfo => "dev";
        public override void SetDefaults()
        {
            Enabled = true;
            EmergencyOnly = true;
            SpeedMultiplier = 50f;
        }
    }
}