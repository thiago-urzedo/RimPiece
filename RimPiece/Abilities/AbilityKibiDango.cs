using RimPiece.Hediffs;
using RimWorld;
using Verse;
using UnityEngine;
using Ability = TaranMagicFramework.Ability;

namespace RimPiece.Abilities
{
    public class AbilityKibiDango : Ability
    {
        public override bool CanHitTarget(LocalTargetInfo target)
        {
            return (target.Pawn != null && target.Pawn.RaceProps.Animal && target.Pawn.Faction != Faction.OfPlayer);
        }

        public override void Start(bool consumeEnergy = true)
        {
            base.Start(consumeEnergy);
            
            var targetAnimal = this.curTarget.Pawn;
            if (targetAnimal == null) return;

            var power = targetAnimal.kindDef.combatPower;
            var durationDays = 20f * (100f / power);
            durationDays = Mathf.Clamp(durationDays, 1f, 60f);
            
            InteractionWorker_RecruitAttempt.DoRecruit(this.pawn, targetAnimal, out var label, out var letter);

            if (targetAnimal.training != null && targetAnimal.playerSettings != null)
            {
                var canLearnObedience = targetAnimal.training.CanAssignToTrain(TrainableDefOf.Obedience).Accepted;
                if (canLearnObedience)
                {
                    targetAnimal.training.SetWantedRecursive(TrainableDefOf.Obedience, true);
                    targetAnimal.training.Train(TrainableDefOf.Obedience, this.pawn, true);
                    
                    if (targetAnimal.training.CanAssignToTrain(TrainableDefOf.Release).Accepted)
                    {
                        targetAnimal.training.SetWantedRecursive(TrainableDefOf.Release, true);
                        targetAnimal.training.Train(TrainableDefOf.Release, this.pawn, true);
                    }

                    if (targetAnimal.training.HasLearned(TrainableDefOf.Obedience))
                    {
                        targetAnimal.playerSettings.Master = this.pawn;
                        targetAnimal.playerSettings.followDrafted = true;
                    }
                }
            }
            
            var hediffDef = HediffDef.Named("RimPieceKibiDangoControl");
            var hediff = targetAnimal.health.AddHediff(hediffDef);
            
            if (hediff is HediffKibiDango kibiHediff)
            {
                kibiHediff.master = this.pawn;
            }

            var comp = hediff.TryGetComp<HediffComp_Disappears>();
            if (comp != null)
            {
                comp.ticksToDisappear = Mathf.RoundToInt(durationDays * 60000f);
            }

            MoteMaker.ThrowText(targetAnimal.DrawPos, targetAnimal.Map, $"{durationDays:0.0} Days Control", Color.cyan);
            FleckMaker.ThrowMicroSparks(targetAnimal.DrawPos, targetAnimal.Map);
        }
    }
}