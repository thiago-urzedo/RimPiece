using Verse;
using RimWorld;
using UnityEngine;
using System.Collections.Generic;
using TaranMagicFramework;
using AbilityDef = TaranMagicFramework.AbilityDef;

namespace RimPiece.Components
{
    public class CompHaki: ThingComp
    {
        private const int MaxLevel = 20;
        private const float LevelScalingMultiplier = 3f;
        
        private float _armamentXp;
        private int _armamentLevel;
        private float _observationXp;
        private int _observationLevel;
        private bool _hasConquerors;

        public int ArmamentLevel => _armamentLevel;
        public float ArmamentXp => _armamentXp;
        public float ArmamentMaxXp => Mathf.Min((_armamentLevel + 1) * 100f, 3000f);
        public int ObservationLevel => _observationLevel;
        public float ObservationXp => _observationXp;
        public float ObservationMaxXp => Mathf.Min((_observationLevel + 1) * 100f, 3000f);

        public bool HasConquerors => _hasConquerors;

        public int GetMaxLevel()
        {
            return MaxLevel;
        }

        public void GainArmamentXp(float rawInput)
        {
            if (rawInput <= 0 || _armamentLevel >= MaxLevel) return;
            
            var effectiveInput = Mathf.Clamp(rawInput, 0f, 100f);
            var levelPenalty = 1f - (_armamentLevel / 25f);
            var finalXp = effectiveInput * levelPenalty *  LevelScalingMultiplier;
            
            AddArmamentXp(finalXp);
        }

        private void AddArmamentXp(float amount)
        {
            if (amount <= 0) return;

            _armamentXp += amount;
    
            while (_armamentXp >= ArmamentMaxXp && _armamentLevel < MaxLevel)
            {
                _armamentXp -= ArmamentMaxXp;
                _armamentLevel++;
        
                if (parent is Pawn p && p.Faction != null && p.Faction.IsPlayer)
                {
                    Messages.Message(
                        $"{p.LabelShort} reached Armament Haki level {_armamentLevel}!",
                        p,
                        MessageTypeDefOf.PositiveEvent,
                        historical: false
                    );
                }
                
            }

            if (_armamentLevel >= MaxLevel) _armamentXp = 0;
        }

        public float GetArmamentAPBonus()
        {
            return 1f + (_armamentLevel * 0.03f);
        }

        public float GetArmamentDamageFactor()
        {
            return 1f + (_armamentLevel * 0.03f);
        }
        
        public float GetArmamentFlatDamage()
        {
            return 1f + (_armamentLevel * 0.7f);
        }

        public float GetIncomingDamageFactor()
        {
            return 1f - (_armamentLevel * 0.08f);
        }
        
        public bool TriggerRyuoChance()
        {
            if (_armamentLevel < 14) return false;
            
            var chance = Mathf.Pow((_armamentLevel - 14) / 6f, 0.2f) * 0.4f;
            return Rand.Value < chance;
        }
        
        public void GainObservationXp(float rawInput)
        {
            if (_observationLevel >= MaxLevel || rawInput <= 0) return;

            var effectiveInput = Mathf.Clamp(rawInput, 0f, 100f);
            var levelPenalty = 1f - (_observationLevel / 25f);
            var finalXp = effectiveInput * levelPenalty *  LevelScalingMultiplier;

            AddObservationXp(finalXp);
        }

        private void AddObservationXp(float amount)
        {
            if (amount <= 0) return;
            
            _observationXp += amount;

            while (_observationXp >= ObservationMaxXp && _observationLevel < MaxLevel)
            {
                _observationXp -= ObservationMaxXp;
                _observationLevel++;
                
                if (parent is Pawn p && p.Faction != null && p.Faction.IsPlayer)
                {
                    Messages.Message(
                        $"{p.LabelShort} reached Observation Haki level {_observationLevel}!",
                        p,
                        MessageTypeDefOf.PositiveEvent,
                        historical: false
                    );
                }
            }
            
            if (_observationLevel >= MaxLevel) _observationXp = 0;
        }
        
        public float GetFutureSightChance()
        {
            return _observationLevel * 0.025f;
        }

        public float GetDodgeBonus()
        {
            if (_observationLevel < 1) return 0f;
            return (_observationLevel / (float)MaxLevel) * 0.60f;
        }

        public float GetAimDelayFactor()
        {
            if (_observationLevel < 1) return 1f;
            return 1f - ((_observationLevel / (float)MaxLevel) * 0.40f);
        }
        
        public void UnlockConquerors()
        {
            _hasConquerors = true;
        }
        
        private void CheckAndGrantAbilities(Pawn p)
        {
            if (p.story == null || !p.story.traits.HasTrait(TraitDef.Named("RimPieceHakiUser"))) return;
            
            var compAbilities = p.GetComp<CompAbilities>();
            if (compAbilities == null) return;

            var hakiClassDef = DefDatabase<AbilityClassDef>.GetNamed("RimPieceHakiClass");
            var hakiClass = compAbilities.GetAbilityClass(hakiClassDef);

            if (hakiClass == null)
            {
                hakiClass = compAbilities.CreateAbilityClass(hakiClassDef, true);
                Messages.Message($"{p.LabelShort} awakened their Haki!", p, MessageTypeDefOf.PositiveEvent);
            }
            else if (!hakiClass.Unlocked)
            {
                hakiClass.Unlocked = true;
            }

            var armAbility = DefDatabase<AbilityDef>.GetNamed("RimPieceArmamentToggle");
            if (!hakiClass.Learned(armAbility))
            {
                hakiClass.LearnAbility(armAbility, false, 0); 
            }

            var obsAbility = DefDatabase<AbilityDef>.GetNamed("RimPieceObservationToggle");
            if (!hakiClass.Learned(obsAbility))
            {
                hakiClass.LearnAbility(obsAbility, false, 0);
            }
        }
        
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var g in base.CompGetGizmosExtra()) yield return g;

            if (DebugSettings.godMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "+100 Armament XP",
                    defaultDesc = "Instantly adds 100 XP to Armament Haki.",
                    icon = TexCommand.Attack,
                    action = () => 
                    {
                        AddArmamentXp(100f); 
                    }
                };
                
                yield return new Command_Action
                {
                    defaultLabel = "+1000 Armament XP",
                    defaultDesc = "Instantly adds 1000 XP to Armament Haki.",
                    icon = TexCommand.Attack,
                    action = () => 
                    {
                        AddArmamentXp(1000f); 
                    }
                };

                yield return new Command_Action
                {
                    defaultLabel = "+100 Observation XP",
                    defaultDesc = "Instantly adds 100 XP to Observation Haki.",
                    icon = TexCommand.Attack, 
                    action = () => 
                    {
                        AddObservationXp(100f);
                    }
                };
                
                yield return new Command_Action
                {
                    defaultLabel = "+1000 Observation XP",
                    defaultDesc = "Instantly adds 1000 XP to Observation Haki.",
                    icon = TexCommand.Attack, 
                    action = () => 
                    {
                        AddObservationXp(1000f);
                    }
                };
                
                yield return new Command_Action
                {
                    defaultLabel = "Reset Haki",
                    defaultDesc = "Resets all levels to 0.",
                    icon = TexCommand.ForbidOn,
                    action = () => 
                    {
                        _armamentLevel = 0;
                        _armamentXp = 0;
                        _observationLevel = 0;
                        _observationXp = 0;
                        // EnsureHediff();
                        Messages.Message("Haki levels reset.", MessageTypeDefOf.TaskCompletion, false);
                    }
                };
            }
        }
        
        public override void CompTick()
        {
            base.CompTick();

            // 1 second = 60 ticks
            // Gain little observation xp every second of combat, if engaging an enemy
            // TODO - review this in the future
            if (parent is Pawn p && p.Spawned && p.IsHashIntervalTick(60))
            {

                var isHakiUser = p.story != null && p.story.traits.HasTrait(TraitDef.Named("RimPieceHakiUser"));
                if (isHakiUser)
                {
                    CheckAndGrantAbilities(p);
                    
                    var isObservationActive = p.health.hediffSet.HasHediff(HediffDef.Named("RimPieceObservationHaki"));
                    if (isObservationActive && p.Drafted && p.TargetCurrentlyAimingAt != null)
                    {
                        GainObservationXp(1f); 
                    }
                }
            }
        }
        
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref _armamentXp, "armamentXP", 0f);
            Scribe_Values.Look(ref _armamentLevel, "armamentLevel", 0);
            Scribe_Values.Look(ref _observationXp, "observationXP", 0f);
            Scribe_Values.Look(ref _observationLevel, "observationLevel", 0);
            Scribe_Values.Look(ref _hasConquerors, "hasConquerors", false);
        }
    }
}