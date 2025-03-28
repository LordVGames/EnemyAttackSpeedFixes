using System.IO;
using UnityEngine;

namespace EnemyAttackSpeedFixes
{
    // NOT CURRENTLY USED. MAYBE IN THE FUTURE
    public static class ModAssets
    {
        public static AssetBundle AssetBundle;
        public const string BundleName = "animgolemdiff";

        public static string AssetBundlePath
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(Plugin.PluginInfo.Location), BundleName);
            }
        }

        public static void Init()
        {
            AssetBundle = AssetBundle.LoadFromFile(AssetBundlePath);
        }
    }
}