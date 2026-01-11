using HarmonyLib;
using RimWorld;
using Verse;
using RimPiece.Components;

namespace RimPiece.Patches
{
    [HarmonyPatch(typeof(Thing), "TakeDamage")]
    public class PatchWeaponDurability
    {
        public static bool Prefix(Thing __instance, DamageInfo dinfo)
        {
            if (!(__instance.ParentHolder is Pawn_EquipmentTracker tracker)) return true;

            var pawn = tracker.pawn;
            if (pawn == null) return true;

            var hediffSet = pawn.health.hediffSet;
            
            var armamentActive = hediffSet.HasHediff(HediffDef.Named("RimPieceArmamentHaki"));
            var infusionActive = hediffSet.HasHediff(HediffDef.Named("RimPieceConquerorInfusion"));

            return !armamentActive && !infusionActive;
        }
    }
}