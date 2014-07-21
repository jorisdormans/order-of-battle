using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusketLine.Core
{
    public static class Settings
    {
        public static int MessageScore = 10001;

        public static bool Generals = false;
        public static bool GeneralsIncreaseOrder = true;
        public static bool OrderAffectsRate = false;
        public static bool OrderDissipates = true;
        public static bool HitEnemyEffect = true;
        public static bool Flee = true;
        public static bool VisualizeOrder = true;
        public static bool HoldCommand = true;
        public static bool FriendKilledEffect = true;
        public static bool ShotAtEffect = true;

        public static float FleeThreshold = -0.1f;
        public static float ShotAtEffectValue = -0.05f;
        public static float FriendKilledEffectValue = -0.3f;
        public static float FriendKilledEffectRadius = 40;
        public static float HitEnemyEffectValue = 0.5f;
        public static float HitEnemyEffectRadius = 40f;
        public static float OrderDissipation = 0.999f;

    }
}
