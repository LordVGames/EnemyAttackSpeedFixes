using BepInEx;
using RoR2;

namespace GolemClapAttackSpeedFix
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "LordVGames";
        public const string PluginName = "GolemClapAttackSpeedFix";
        public const string PluginVersion = "1.0.0";
        public void Awake()
        {
            Log.Init(Logger);

            IL.EntityStates.GolemMonster.ClapState.OnEnter += Main.ClapState_OnEnter;
            IL.EntityStates.GolemMonster.ClapState.FixedUpdate += Main.ClapState_FixedUpdate;
            IL.EntityStates.GolemMonster.ClapState.OnExit += Main.ClapState_OnExit;
        }
    }
}