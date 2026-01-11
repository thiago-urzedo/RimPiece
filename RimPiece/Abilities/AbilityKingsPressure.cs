using RimWorld;
using Verse;
using System.Collections.Generic;
using UnityEngine;
using Ability = TaranMagicFramework.Ability;
using RimPiece.Components;

namespace RimPiece.Abilities
{
    public class AbilityKingsPressure : Ability
    {
        public override void Start(bool consumeEnergy = true)
        {
            base.Start(consumeEnergy);

            var radius = this.AbilityTier.effectRadius;
            var victims = new List<Pawn>();
            
            foreach (var t in GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, radius, true))
            {
                if (t is Pawn p && p != this.pawn && !p.Dead && p.Spawned && !p.RaceProps.IsMechanoid)
                {
                    victims.Add(p);
                }
            }

            // TODO - Find a new effect for CoC (EMP blast not working)
            // MoteMaker.MakeStaticMote(pawn.Position, pawn.Map, ThingDef.Named("Mote_BlastEMP"), 10f);
            
            foreach (var victim in victims)
            {
                ApplyPressure(victim);
            }
        }

        private void ApplyPressure(Pawn victim)
        {
            var attackerHaki = pawn.GetComp<CompHaki>();
            var attackerPower = (attackerHaki != null) ? (attackerHaki.ArmamentLevel + attackerHaki.ObservationLevel) : 0;

            var defense = 0;
            var victimHaki = victim.GetComp<CompHaki>();
            var victimSameFaction = victim.Faction != null && victim.Faction.IsPlayer;
            
            if (victimHaki != null && victimHaki.IsHakiUser)
            {
                defense = victimHaki.ArmamentLevel + victimHaki.ObservationLevel;
            }

            var diff = attackerPower - defense;
            var stunChance = 0.10f + (diff * 0.05f);

            if (victimSameFaction) stunChance *= 0.5f;
            
            if (Rand.Value < stunChance)
            {
                var stunPower = (15f + (diff * 1f)) / 2f;
                if (victimSameFaction) stunPower *= 0.6f;
                
                var dinfo = new DamageInfo(DamageDefOf.Stun, stunPower, 0, -1, pawn);
                victim.TakeDamage(dinfo);
                
                MoteMaker.ThrowText(victim.DrawPos, victim.Map, "Overwhelmed!", Color.red);
            }
            else
            {
                MoteMaker.ThrowText(victim.DrawPos, victim.Map, "Resisted", Color.white);
            }
        }
    }
}