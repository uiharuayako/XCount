using Dalamud.Game.ClientState.Objects.SubKinds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace XCount
{
    static class StaticUtil
    {
        public static bool EnableAlertChat=false;
        public static string ReplaceStrings(string inputString, string[] originalStrings, string[] replacementStrings)
        {
            if (originalStrings.Length != replacementStrings.Length)
            {
                throw new ArgumentException("The number of original strings must match the number of replacement strings.");
            }

            for (int i = 0; i < originalStrings.Length; i++)
            {
                inputString = inputString.Replace(originalStrings[i], replacementStrings[i]);
            }

            return inputString;
        }

        public static float DistanceToPlayer(PlayerCharacter obj)
        {
            if (obj == null) return 0;
            Vector3 objPosition = new(obj.Position.X, obj.Position.Y, obj.Position.Z);
            Vector3 selfPosition = new(XCPlugin.ClientState.LocalPlayer.Position.X, XCPlugin.ClientState.LocalPlayer.Position.Y, XCPlugin.ClientState.LocalPlayer.Position.Z);
            return Math.Max(0, Vector3.Distance(objPosition, selfPosition) - obj.HitboxRadius - XCPlugin.ClientState.LocalPlayer.HitboxRadius);
        }

    }
}
