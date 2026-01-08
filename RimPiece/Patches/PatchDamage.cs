using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;
using RimPiece.Components;

namespace RimPiece.Patches
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), "PreApplyDamage")]
    public static class PatchDamage
    {
        public static void Prefix(Pawn_HealthTracker __instance, ref DamageInfo dinfo, Pawn ___pawn)
        {
            if (dinfo.Def == DamageDefOf.SurgicalCut) return; 
            
            var pawn = ___pawn;
            
            // Taking damage
            if (pawn != null && !pawn.Dead)
            {
                var pawnHaki = pawn.GetComp<CompHaki>();
                if (pawnHaki != null)
                {
                    var xpToGain = dinfo.Amount * 0.8f;
                    pawnHaki.GainArmamentXp(xpToGain);
                    pawnHaki.GainObservationXp(xpToGain * 0.4f);
                    
                    var reduction = pawnHaki.GetIncomingDamageFactor();
                    var newAmount = dinfo.Amount * reduction;
                    dinfo.SetAmount(newAmount);
                    
                    if (pawnHaki.ObservationLevel >= 50 && dinfo.HitPart != null)
                    {
                        bool isVital = dinfo.HitPart.def.tags.Contains(BodyPartTagDefOf.ConsciousnessSource) || 
                                       dinfo.HitPart.def.tags.Contains(BodyPartTagDefOf.BloodPumpingSource);
                        
                        if (isVital)
                        {
                            if (Rand.Value < 0.5f)
                            {
                                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "Vital Avoided!", Color.cyan);
                                pawnHaki.GainObservationXp(xpToGain * 2f);
                            
                                var torso = pawn.RaceProps.body.corePart;
                            
                                dinfo = new DamageInfo(
                                    dinfo.Def, dinfo.Amount, dinfo.ArmorPenetrationInt, 
                                    dinfo.Angle, dinfo.Instigator, 
                                    torso,
                                    dinfo.Weapon, dinfo.Category, dinfo.IntendedTarget
                                );
                            }
                        }
                    }
                }
            }
            
            // Dealing damage
            if (dinfo.Instigator is Pawn attacker && !attacker.Dead)
            {
                if (attacker == pawn) return;
                
                var attackerHaki = attacker.GetComp<CompHaki>();
                if (attackerHaki != null)
                {
                    float xpToGain = dinfo.Amount * 0.6f;
                    // Bonus XP if attack is melee
                    // TODO - review this in the future
                    if (!dinfo.Def.isRanged)
                    {
                        xpToGain *= 1.25f;
                    }
                    attackerHaki.GainArmamentXp(xpToGain);

                    var baseAP = dinfo.ArmorPenetrationInt;
                    var addedAP = attackerHaki.GetArmamentAPBonus();
                    if (dinfo.Def.isRanged) addedAP *= 0.5f;
                    var finalAP = baseAP + addedAP;
                    
                    var dmgFactor = attackerHaki.GetArmamentDamageFactor();
                    var flatDmg = 0f;

                    if (!dinfo.Def.isRanged)
                    {
                        flatDmg = attackerHaki.GetArmamentFlatDamage();
                    }
                    
                    var finalAmount = (dinfo.Amount * dmgFactor) +  flatDmg;
                    
                    dinfo = new DamageInfo(
                        dinfo.Def,
                        finalAmount,
                        finalAP,
                        dinfo.Angle,
                        dinfo.Instigator,
                        dinfo.HitPart,
                        dinfo.Weapon,
                        dinfo.Category,
                        dinfo.IntendedTarget,
                        dinfo.InstigatorGuilty,
                        dinfo.SpawnFilth,
                        dinfo.WeaponQuality,
                        dinfo.CheckForJobOverride,
                        dinfo.PreventCascade
                    );

                    if (attackerHaki.TriggerRyuoChance())
                    {
                        dinfo.SetIgnoreArmor(true);
                    }
                }
            }
        }
    }
}