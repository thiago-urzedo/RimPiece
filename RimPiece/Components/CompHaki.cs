using Verse;
using RimWorld;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TaranMagicFramework;
using AbilityDef = TaranMagicFramework.AbilityDef;

namespace RimPiece.Components
{
    public class CompHaki: ThingComp
    {
        private const int MaxLevel = 20;
        private const float LevelScalingMultiplier = 2f;

        private const float ArmamentDrain = 1.0f;
        private const float ObservationDrain = 1.0f;
        private const float CoCInfusionDrain = 2.5f;
        private const float PassiveRegen = 2f;
        private const int TrainingCooldownTicks = 420000;
        
        private float _armamentXp;
        private int _armamentLevel;
        private float _observationXp;
        private int _observationLevel;
        private bool _levelsInitialized = false;
        private int _lastTrainingTick = -999999;
        
        private static TraitDef _hakiUserTraitDef;
        private static TraitDef HakiUserTrait
        {
            get
            {
                if (_hakiUserTraitDef == null)
                {
                    _hakiUserTraitDef = TraitDef.Named("RimPieceHakiUser");
                }

                return _hakiUserTraitDef;
            }
        }
        
        private static GeneDef _conqGeneDef;
        private static GeneDef ConqGeneDef
        {
            get
            {
                if (_conqGeneDef == null)
                {
                    _conqGeneDef = DefDatabase<GeneDef>.GetNamed("RimPieceConquerorsGene");
                }
                return _conqGeneDef;
            }
        }

        public int ArmamentLevel => _armamentLevel;
        public float ArmamentXp => _armamentXp;
        public float ArmamentMaxXp => Mathf.Min((_armamentLevel + 1) * 100f, 3000f);
        public int ObservationLevel => _observationLevel;
        public float ObservationXp => _observationXp;
        public float ObservationMaxXp => Mathf.Min((_observationLevel + 1) * 100f, 3000f);

        public bool IsHakiUser { get; private set; }
        public bool IsConqueror { get; private set; }

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

        public float GetTrapAvoidance()
        {
            return 0.10f + (_observationLevel * 0.04f);
        }
        
        public bool CanTrainOthers()
        {
            if (ArmamentLevel < 16 && ObservationLevel < 16) return false;
            return (Find.TickManager.TicksGame - _lastTrainingTick) >= TrainingCooldownTicks;
        }

        public int TicksUntilNextTrain()
        {
            return TrainingCooldownTicks - (Find.TickManager.TicksGame - _lastTrainingTick);
        }

        public void Notify_TrainingCompleted()
        {
            _lastTrainingTick = Find.TickManager.TicksGame;
        }
        
        private void CheckAndGrantAbilities(Pawn p)
        {
            if (!IsHakiUser) return;
            
            var compAbilities = p.GetComp<CompAbilities>();
            if (compAbilities == null) return;

            var hakiClassDef = DefDatabase<AbilityClassDef>.GetNamed("RimPieceHakiClass");
            var hakiClass = compAbilities.GetAbilityClass(hakiClassDef);

            if (hakiClass == null)
            {
                hakiClass = compAbilities.CreateAbilityClass(hakiClassDef, true);
                if (p.Faction != null && p.Faction.IsPlayer)
                {
                    Messages.Message($"{p.LabelShort} awakened their Haki!", p, MessageTypeDefOf.PositiveEvent);
                }
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
            
            if (IsConqueror)
            {
                var conqTree = DefDatabase<AbilityTreeDef>.GetNamed("RimPieceConquerorHakiTree");
                if (!hakiClass.TreeUnlocked(conqTree))
                {
                    hakiClass.UnlockTree(conqTree);
                }
                
                if (_armamentLevel + _observationLevel >= 12)
                {
                    var blast = DefDatabase<AbilityDef>.GetNamed("RimPieceConquerorBlast");
                    if (!hakiClass.Learned(blast))
                    {
                        hakiClass.LearnAbility(blast, false, 0);
                        if (p.Faction != null && p.Faction.IsPlayer)
                        {
                            Messages.Message($"{p.LabelShort} has awakened Conqueror's Haki!", p, MessageTypeDefOf.PositiveEvent);
                        }
                    }
                }

                if (_armamentLevel + _observationLevel >= 30)
                {
                    var infusion = DefDatabase<AbilityDef>.GetNamed("RimPieceConquerorInfusion");
                    if (!hakiClass.Learned(infusion))
                    {
                        hakiClass.LearnAbility(infusion, false, 0);
                        if (p.Faction != null && p.Faction.IsPlayer)
                        {
                            Messages.Message($"{p.LabelShort} mastered Conqueror's Infusion!", p, MessageTypeDefOf.PositiveEvent);
                        }
                    }
                }
            }
        }
        
        private void ManageEnergy(Pawn p)
        {
            
            var compAbilities = p.GetComp<CompAbilities>();
            if (compAbilities == null) return;

            var resourceDef = DefDatabase<AbilityResourceDef>.GetNamed("RimPieceHakiResource");
            var resource = compAbilities.GetAbilityResource(resourceDef);
            if (resource == null) return;
            
            var hediffSet = p.health.hediffSet;
            var armActive = hediffSet.HasHediff(HediffDef.Named("RimPieceArmamentHaki"));
            var obsActive = hediffSet.HasHediff(HediffDef.Named("RimPieceObservationHaki"));
            var infActive = hediffSet.HasHediff(HediffDef.Named("RimPieceConquerorInfusion"));
            var isExhausted = hediffSet.HasHediff(HediffDef.Named("RimPieceHakiExhaustion"));
            
            if (armActive || obsActive || infActive)
            {
                if (!p.Drafted) ForceDisableAbilities(p, compAbilities);
                
                var totalDrain = 0f;
                if (armActive) totalDrain += ArmamentDrain;
                if (obsActive) totalDrain += ObservationDrain;
                if (infActive) totalDrain += CoCInfusionDrain;

                if (resource.energy >= totalDrain)
                {
                    resource.energy -= totalDrain;
                }
                else
                {
                    resource.energy = 0;
                    ForceDisableAbilities(p, compAbilities);
                    if (!isExhausted)
                    {
                        var exhaustion = HediffMaker.MakeHediff(HediffDef.Named("RimPieceHakiExhaustion"), p);
                        hediffSet.AddDirect(exhaustion);
                    }
                    if (p.Faction != null && p.Faction.IsPlayer)
                    {
                        Messages.Message($"{p.LabelShort} ran out of Haki!", p, MessageTypeDefOf.NegativeEvent);
                    }
                }
            }
            else if (resource.energy < resource.MaxEnergy)
            {
                resource.energy += PassiveRegen;
                if (resource.energy > resource.MaxEnergy)
                {
                    resource.energy = resource.MaxEnergy;
                }
            }
        }

        private void ForceDisableAbilities(Pawn p, CompAbilities compAbilities)
        {
            DisableToggle(compAbilities, "RimPieceArmamentToggle");
            DisableToggle(compAbilities, "RimPieceObservationToggle");
            DisableToggle(compAbilities, "RimPieceConquerorInfusion");
        }

        private void DisableToggle(CompAbilities comp, string abilityDefName)
        {
            var abilityDef = DefDatabase<AbilityDef>.GetNamed(abilityDefName, false);
            if (abilityDef == null) return;

            foreach (var ability in comp.AllLearnedAbilities.Where(ability => ability.def == abilityDef && ability.Active))
            {
                ability.End();
                break;
            }
        }
        
        private void InitializeHakiData(Pawn p)
        {
            if (p == null || p.story == null) return;
            
            _levelsInitialized = true;

            if (p.genes != null)
            {
                var cocGeneDef = DefDatabase<GeneDef>.GetNamed("RimPieceConquerorsGene");
                
                if (cocGeneDef != null && !p.genes.HasActiveGene((cocGeneDef)))
                {
                    if (Rand.Value < 0.03f)
                    {
                        p.genes.AddGene(cocGeneDef, true);
                    }
                }
            }

            if (IsConqueror)
            {
                if (!IsHakiUser)
                {
                    p.story.traits.GainTrait(new Trait(TraitDef.Named("RimPieceHakiUser")));
                }
            }

            if (IsHakiUser)
            {
                var power = p.kindDef.combatPower;
                
                var armamentPotentialLevel = Mathf.RoundToInt(power / 15f);
                armamentPotentialLevel += Rand.Range(-3, 3);
                armamentPotentialLevel = Mathf.Clamp(armamentPotentialLevel, 0, MaxLevel);
                
                var observationPotentialLevel = Mathf.RoundToInt(power / 15f);
                observationPotentialLevel += Rand.Range(-3, 3);
                observationPotentialLevel = Mathf.Clamp(observationPotentialLevel, 0, MaxLevel);

                _armamentLevel = armamentPotentialLevel;
                _observationLevel = observationPotentialLevel;
                
                _armamentXp = Rand.Range(0, ArmamentMaxXp);
                _observationXp = Rand.Range(0, ObservationMaxXp);
            }
        }
        
        public void UpdateCachedVariables(Pawn p)
        {
            IsConqueror = p.genes != null && ConqGeneDef != null && p.genes.HasActiveGene(ConqGeneDef);
            IsHakiUser = p.story != null && HakiUserTrait != null && p.story.traits.HasTrait(HakiUserTrait);
        }
        
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            
            if (parent is Pawn p)
            {
                UpdateCachedVariables(p); 
                
                if (!respawningAfterLoad && !_levelsInitialized && p.Spawned) 
                {
                    InitializeHakiData(p);
                }
            }
        }
        
        public override void CompTick()
        {
            base.CompTick();

            // 1 second = 60 ticks
            if (parent is Pawn p && p.Spawned && p.IsHashIntervalTick(60))
            {
                if (IsHakiUser)
                {
                    CheckAndGrantAbilities(p);
                    ManageEnergy(p);
                    
                    // Gain little observation xp every second of combat, if engaging an enemy
                    // TODO - review this in the future
                    var isObservationActive = p.health.hediffSet.HasHediff(HediffDef.Named("RimPieceObservationHaki"));
                    if (isObservationActive && p.Drafted && p.TargetCurrentlyAimingAt != null)
                    {
                        GainObservationXp(1f); 
                    }
                    
                    // Gain little observation xp every second of meditation or hunting
                    if (p.CurJob != null && (p.CurJob.def == JobDefOf.Meditate || p.CurJob.def == JobDefOf.Hunt) && !p.pather.Moving)
                    {
                        GainObservationXp(1f);
                    }
                }
            }
        }
        
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var g in base.CompGetGizmosExtra()) yield return g;

            if (DebugSettings.godMode && IsHakiUser)
            {
                yield return new Command_Action
                {
                    defaultLabel = "[DEV] +100 Armament XP",
                    defaultDesc = "Instantly adds 100 XP to Armament Haki.",
                    icon = TexCommand.Attack,
                    action = () => 
                    {
                        AddArmamentXp(100f); 
                    }
                };
                
                yield return new Command_Action
                {
                    defaultLabel = "[DEV] +1000 Armament XP",
                    defaultDesc = "Instantly adds 1000 XP to Armament Haki.",
                    icon = TexCommand.Attack,
                    action = () => 
                    {
                        AddArmamentXp(1000f); 
                    }
                };

                yield return new Command_Action
                {
                    defaultLabel = "[DEV] +100 Observation XP",
                    defaultDesc = "Instantly adds 100 XP to Observation Haki.",
                    icon = TexCommand.Attack, 
                    action = () => 
                    {
                        AddObservationXp(100f);
                    }
                };
                
                yield return new Command_Action
                {
                    defaultLabel = "[DEV] +1000 Observation XP",
                    defaultDesc = "Instantly adds 1000 XP to Observation Haki.",
                    icon = TexCommand.Attack, 
                    action = () => 
                    {
                        AddObservationXp(1000f);
                    }
                };
                
                yield return new Command_Action
                {
                    defaultLabel = "[DEV] Reset Haki",
                    defaultDesc = "Resets all levels to 0.",
                    icon = TexCommand.Attack,
                    action = () => 
                    {
                        _armamentLevel = 0;
                        _armamentXp = 0;
                        _observationLevel = 0;
                        _observationXp = 0;
                        Messages.Message("Haki levels reset.", MessageTypeDefOf.TaskCompletion, false);
                    }
                };
            }
        }
        
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref _armamentXp, "armamentXP", 0f);
            Scribe_Values.Look(ref _armamentLevel, "armamentLevel", 0);
            Scribe_Values.Look(ref _observationXp, "observationXP", 0f);
            Scribe_Values.Look(ref _observationLevel, "observationLevel", 0);
            Scribe_Values.Look(ref _levelsInitialized, "levelsInitialized", false);
            Scribe_Values.Look(ref _lastTrainingTick, "lastTrainingTick", -999999);
        }
    }
}