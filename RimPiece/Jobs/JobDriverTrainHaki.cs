using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;
using RimPiece.Components;
using UnityEngine;

namespace RimPiece.Jobs
{
    public class JobDriverTrainHaki : JobDriver
    {
        private Pawn Student => TargetA.Thing as Pawn;
        private const int DurationTicks = 5000;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Student, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            var training = new Toil
            {
                initAction = () =>
                {
                    var learnJob = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("RimPieceLearnHaki"), pawn);
                    
                    Student.jobs.StopAll();
                    Student.pather.StopDead();
                    Student.rotationTracker.FaceTarget(pawn);
                    Student.jobs.StartJob(learnJob, JobCondition.InterruptForced);
                },
                tickAction = () =>
                {
                    pawn.rotationTracker.FaceTarget(Student);
                    Student.rotationTracker.FaceTarget(pawn);

                    if (pawn.IsHashIntervalTick(600))
                    {
                        FleckMaker.ThrowMicroSparks(Student.DrawPos, Map);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = DurationTicks
            };

            training.WithProgressBarToilDelay(TargetIndex.A);
            training.AddFailCondition(() => Student.Dead || !Student.Spawned || Student.Position.DistanceTo(pawn.Position) > 3f);

            yield return training;

            yield return new Toil
            {
                initAction = ApplyTrainingResults,
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }

        private void ApplyTrainingResults()
        {
            var masterComp = pawn.GetComp<CompHaki>();
            var studentComp = Student.GetComp<CompHaki>();

            if (masterComp == null || studentComp == null) return;

            if (!studentComp.IsHakiUser)
            {
                Student.story.traits.GainTrait(new Trait(TraitDef.Named("RimPieceHakiUser")));
                studentComp.UpdateCachedVariables(Student);
                studentComp.CompTick(); 
                
                Messages.Message($"{Student.LabelShort} has awakened Haki under {pawn.LabelShort}'s guidance!", Student, MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                var levelDiffArm = Mathf.Max(0, masterComp.ArmamentLevel - studentComp.ArmamentLevel);
                var levelDiffObs = Mathf.Max(0, masterComp.ObservationLevel - studentComp.ObservationLevel);

                var xpGainArm = levelDiffArm * 800f;
                var xpGainObs = levelDiffObs * 800f;
                if (xpGainArm > 0) studentComp.GainArmamentXp(xpGainArm);
                if (xpGainObs > 0) studentComp.GainObservationXp(xpGainObs);

                Messages.Message($"{pawn.LabelShort} taught Haki to {Student.LabelShort}.", Student, MessageTypeDefOf.PositiveEvent);
            }

            masterComp.Notify_TrainingCompleted();
        }
    }
}