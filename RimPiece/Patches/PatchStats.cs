using HarmonyLib;
using Verse;
using RimWorld;
using RimPiece.Components;

namespace RimPiece.Patches
{
    [HarmonyPatch(typeof(StatWorker), "GetValueUnfinalized", new[] { typeof(StatRequest), typeof(bool) })]
    public static class PatchStats
    {
        private static readonly AccessTools.FieldRef<StatWorker, StatDef> GetStatDef = 
            AccessTools.FieldRefAccess<StatWorker, StatDef>("stat");
        
        public static void Postfix(StatWorker __instance, StatRequest req, ref float __result)
        {
            if (req.Thing is Pawn p)
            {
                var haki = p.GetComp<CompHaki>();
                if (haki == null || haki.ObservationLevel < 1) return;

                var currentStat = GetStatDef(__instance);
                
                if (currentStat == StatDefOf.MeleeDodgeChance)
                {
                    __result += haki.GetDodgeBonus();
                }
                else if (currentStat == StatDefOf.AimingDelayFactor)
                {
                    __result *= haki.GetAimDelayFactor();
                }
            }
        }
        
    }
}