using BepInEx;
using RoR2;

namespace EnemyAttackSpeedFixes
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public static PluginInfo PluginInfo { get; private set; }
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "LordVGames";
        public const string PluginName = "EnemyAttackSpeedFixes";
        public const string PluginVersion = "1.2.0";
        public void Awake()
        {
            PluginInfo = Info;
            Log.Init(Logger);
            //ModAssets.Init();

            Main.StoneGolem.SetupILHooks();
            Main.Mithrix.SetupILHooks();
            Main.StoneTitan.SetupILHooks();
        }
    }
}