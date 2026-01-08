using Verse;
using RimWorld;
using UnityEngine;
using System.Collections.Generic;

namespace RimPiece.Components
{
    public class CompHaki: ThingComp
    {
        private const int MaxLevel = 100;
        private const float LevelScalingMultiplier = 3f;
        
        private float _armamentXp;
        private int _armamentLevel;
        private float _observationXp;
        private int _observationLevel;
        private bool _hasConquerors;

        public int ArmamentLevel => _armamentLevel;
        public float ArmamentXp => _armamentXp;
        public float ArmamentMaxXp => Mathf.Min((_armamentLevel + 1) * 25f, 3000f);
        public int ObservationLevel => _observationLevel;
        public float ObservationXp => _observationXp;
        public float ObservationMaxXp => Mathf.Min((_observationLevel + 1) * 25f, 3000f);

        public bool HasConquerors => _hasConquerors;

        public int GetMaxLevel()
        {
            return MaxLevel;
        }

        public void GainArmamentXp(float rawInput)
        {
            if (rawInput <= 0 || _armamentLevel >= MaxLevel) return;
            
            var effectiveInput = Mathf.Clamp(rawInput, 0f, 50f);
            var levelPenalty = 1f - (_armamentLevel / 120f);
            var finalXp = effectiveInput * levelPenalty *  LevelScalingMultiplier;
            
            AddArmamentXp(finalXp);
        }

        private void AddArmamentXp(float amount)
        {
            if (amount <= 0) return;

            _armamentXp += amount;
            var leveledUp = false;
    
            while (_armamentXp >= ArmamentMaxXp && _armamentLevel < MaxLevel)
            {
                _armamentXp -= ArmamentMaxXp;
                _armamentLevel++;
                leveledUp = true;
        
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
            if (leveledUp) EnsureHediff();
        }

        public float GetArmamentAPBonus()
        {
            if (_armamentLevel < 10) return 1f;

            var progress = _armamentLevel / 100f;
            return Mathf.Pow(progress, 1.4f) * 0.5f;
        }

        public float GetArmamentDamageFactor()
        {
            if (_armamentLevel < 10) return 1f;
            
            return 1f + (_armamentLevel / 100f);
        }
        
        public float GetArmamentFlatDamage()
        {
            if (_armamentLevel < 20) return 0f;
            return (_armamentLevel - 20) / 8f;
        }

        public float GetIncomingDamageFactor()
        {
            if (_armamentLevel < 10) return 1f;

            return Mathf.Max(0.4f, 1f - (_armamentLevel * 0.006f));
        }
        
        public bool TriggerRyuoChance()
        {
            if (_armamentLevel < 70) return false;
            
            var chance = Mathf.Pow((_armamentLevel - 70) / 30f, 0.2f) * 0.3f;
            return Rand.Value < chance;
        }
        
        public void GainObservationXp(float rawInput)
        {
            if (_observationLevel >= MaxLevel || rawInput <= 0) return;

            var effectiveInput = Mathf.Clamp(rawInput, 0f, 50f);
            var levelPenalty = 1f - (_observationLevel / 120f);
            var finalXp = effectiveInput * levelPenalty *  LevelScalingMultiplier;

            AddObservationXp(finalXp);
        }

        private void AddObservationXp(float amount)
        {
            if (amount <= 0) return;
            
            _observationXp += amount;
            var leveledUp = false;

            while (_observationXp >= ObservationMaxXp && _observationLevel < MaxLevel)
            {
                _observationXp -= ObservationMaxXp;
                _observationLevel++;
                leveledUp = true;
                
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
            if (leveledUp) EnsureHediff();
        }
        
        public float GetFutureSightChance()
        {
            if (_observationLevel < 10) return 0f;
            var progress = _observationLevel / 100f;
            return 0.05f + (Mathf.Pow(progress, 1.5f) * 0.55f);
        }

        public float GetDodgeBonus()
        {
            if (_observationLevel < 10) return 0f;
            return (_observationLevel / 100f) * 0.50f;
        }

        public float GetAimDelayFactor()
        {
            if (_observationLevel < 10) return 1f;
            return 1f - ((_observationLevel / 100f) * 0.40f);
        }
        
        public void UnlockConquerors()
        {
            _hasConquerors = true;
        }
        
        private void EnsureHediff()
        {
            if (!(parent is Pawn p) || p.Dead) return;

            var shouldHaveArmHediff = _armamentLevel > 0;
            var armHakiHediff = p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("RimPieceArmamentHaki"));
            if (shouldHaveArmHediff && armHakiHediff == null)
            {
                p.health.AddHediff(HediffDef.Named("RimPieceArmamentHaki"));
            }
            else if (!shouldHaveArmHediff && armHakiHediff != null)
            {
                p.health.RemoveHediff(armHakiHediff);
            }
            
            var shouldHaveObsHediff = _observationLevel > 0;
            var obsHakiHeadiff = p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("RimPieceObservationHaki"));
            if (shouldHaveObsHediff && obsHakiHeadiff == null)
            {
                p.health.AddHediff(HediffDef.Named("RimPieceObservationHaki"));
            }
            else if (!shouldHaveObsHediff && obsHakiHeadiff != null)
            {
                p.health.RemoveHediff(obsHakiHeadiff);
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
                        EnsureHediff();
                        Messages.Message("Haki levels reset.", MessageTypeDefOf.TaskCompletion, false);
                    }
                };
            }
        }
        
        public override void CompTick()
        {
            base.CompTick();

            // 1 second = 60 ticks
            // Gain little observation xp every 10 seconds of combat, if engaging an enemy
            // TODO - review this in the future
            if (parent is Pawn p && p.IsHashIntervalTick(600))
            {
                if (p.Drafted && p.TargetCurrentlyAimingAt != null && _observationLevel < MaxLevel)
                {
                    GainObservationXp(10f); 
                }
            }
        }
        
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            EnsureHediff();
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