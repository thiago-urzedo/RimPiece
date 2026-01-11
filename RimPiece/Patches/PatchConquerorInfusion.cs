using HarmonyLib;
using RimWorld;
using Verse;
using RimPiece.Components;
using UnityEngine;

namespace RimPiece.Patches
{
    [HarmonyPatch(typeof(Thing), "TakeDamage")]
    public class PatchConquerorInfusion
    {
        public static void Postfix(Thing __instance, DamageInfo dinfo)
        {
            if (!(__instance is Pawn victim)) return;
            if (!(dinfo.Instigator is Pawn attacker)) return;
            
            if (victim.Dead || !victim.Spawned) return;
            if (victim.RaceProps.IsMechanoid || victim.RaceProps.Animal || victim.IsMutant || victim.IsEntity) return; 
            
            if (!attacker.health.hediffSet.HasHediff(HediffDef.Named("RimPieceConquerorInfusion"))) return;

            var attackerHaki = attacker.GetComp<CompHaki>();
            var victimHaki = victim.GetComp<CompHaki>();
            var severityToAdd = 0.15f;

            if (victimHaki != null)
            {
                if (victimHaki.IsConqueror) return;
                var victimWill = victimHaki.ArmamentLevel + victimHaki.ObservationLevel;

                if (victimWill > 8 && victimWill < 12)
                {
                    severityToAdd -= 0.05f;
                }
                else if (victimWill > 12 && victimWill < 16)
                {
                    severityToAdd -= 0.075f;
                }
                else if (victimWill > 16)
                {
                    severityToAdd -= 0.1f;
                }
            }
            
            if (attackerHaki != null)
            {
                var attackerWill = attackerHaki.ArmamentLevel + attackerHaki.ObservationLevel;
                if (attackerWill >= 24)
                {
                    severityToAdd += 0.1f; 
                }
            }

            var hediffDef = HediffDef.Named("RimPieceOverwhelmed");
            var existingHediff = victim.health.hediffSet.GetFirstHediffOfDef(hediffDef);

            if (existingHediff != null)
            {
                existingHediff.Severity += severityToAdd;
            }
            else
            {
                var newHediff = HediffMaker.MakeHediff(hediffDef, victim);
                newHediff.Severity = severityToAdd;
                victim.health.AddHediff(newHediff);
            }
        }
    }
}