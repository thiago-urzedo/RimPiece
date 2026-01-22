using RimWorld;
using Verse;
using UnityEngine;

namespace RimPiece.Hediffs
{
    public class HediffKibiDango : HediffWithComps
    {
        public Pawn master;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref master, "master");
        }

        public override void Tick()
        {
            base.Tick();
            if (pawn.IsHashIntervalTick(250))
            {
                if (master != null && master.Dead)
                {
                    TriggerBerserk();
                }
            }
        }

        private void TriggerBerserk()
        {
            pawn.health.RemoveHediff(this);
            pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
            Messages.Message($"{pawn.LabelShort} went berserk after {master.LabelShort}'s death!", pawn, MessageTypeDefOf.NegativeEvent);
        }

        public override void PostRemoved()
        {
            base.PostRemoved();

            if (pawn.Dead || pawn.InMentalState) return;

            if (Rand.Value < 0.5f)
            {
                Messages.Message($"{pawn.LabelShort} has grown attached to {master.LabelShort} and is now permanently tamed!", pawn, MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                pawn.SetFaction(null);
                if (!master.Dead)
                {
                    Messages.Message($"{pawn.LabelShort} broke free from Kibi dango control and returned to the wild.", pawn, MessageTypeDefOf.NeutralEvent);
                }
            }
        }
    }
}