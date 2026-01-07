using HarmonyLib;
using Verse;
using RimWorld;
using RimPiece.Components;

namespace RimPiece.Patches
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), "PreApplyDamage")]
    public static class PatchDamage
    {
        public static void Prefix(Pawn_HealthTracker __instance, DamageInfo dinfo, Pawn ___pawn)
        {
            if (___pawn == null || !___pawn.RaceProps.Humanlike || ___pawn.Dead) return;
            if (dinfo.Def == DamageDefOf.SurgicalCut) return; 
            
            var hakiComp = ___pawn.GetComp<CompHaki>();
            hakiComp?.AddArmamentXp(dinfo.Amount);
        }
    }
}