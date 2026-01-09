using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;
using RimPiece.Components;

namespace RimPiece.Patches
{
    public static class FutureSightLogic
    {
        public static bool Prefix(Projectile __instance, Thing hitThing)
        {
            if (__instance == null || __instance.Destroyed) return true;
            
            if (hitThing is Pawn pawn && !pawn.Dead)
            {
                var haki = pawn.GetComp<CompHaki>();
                var isObservationActive = pawn.health.hediffSet.HasHediff(HediffDef.Named("RimPieceObservationHaki"));
                
                if (__instance.def.projectile.explosionRadius > 0f && haki.ObservationLevel < 12) return true;
                
                if (haki != null && isObservationActive)
                {
                    if (Rand.Value < haki.GetFutureSightChance())
                    {
                        MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "Future Sight!", Color.cyan);
                        
                        var threat = __instance.DamageAmount;
                        if (threat < 0) threat = 0;
                        
                        var xpToGain = threat * 1.2f; 
                        
                        haki.GainObservationXp(xpToGain);

                        __instance.Destroy();
                        return false; 
                    }
                }
            }
            return true;
        }
    }
}