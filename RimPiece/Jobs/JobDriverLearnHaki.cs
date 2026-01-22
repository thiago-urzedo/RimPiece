using Verse;
using Verse.AI;
using System.Collections.Generic;

namespace RimPiece.Jobs
{
    public class JobDriverLearnHaki: JobDriver
    {
        private Pawn Master => TargetA.Thing as Pawn;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var listen = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Never,
                tickAction = () =>
                {
                    pawn.rotationTracker.FaceTarget(Master);

                    if (Master == null || 
                        Master.CurJobDef != DefDatabase<JobDef>.GetNamed("RimPieceTeachHaki") || 
                        Master.Position.DistanceTo(pawn.Position) > 4f)
                    {
                        EndJobWith(JobCondition.Succeeded);
                    }
                }
            };

            yield return listen;
        }
    }
}