using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;
using RimPiece.Components;

namespace RimPiece.Patches
{
    [HarmonyPatch(typeof(Building_Trap), "CheckSpring")]
    public class PatchTrapAvoidance
    {
        public static bool Prefix(Building_Trap __instance, Pawn p)
        {
            if (p == null || !p.Spawned || p.Dead) return true;
            
            if (!p.health.hediffSet.HasHediff(HediffDef.Named("RimPieceObservationHaki"))) 
            {
                return true;
            }

            var hakiComp = p.GetComp<CompHaki>();
            if (hakiComp == null) return true;

            var chance = hakiComp.GetTrapAvoidance();
            if (chance > 0.95f) chance = 0.95f;

            if (Rand.Value < chance)
            {
                MoteMaker.ThrowText(p.DrawPos, p.Map, "Future Sight", Color.cyan);
                hakiComp.GainObservationXp(15f);

                return false; 
            }
            return true;
        }
    }
}