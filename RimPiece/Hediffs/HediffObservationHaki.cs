using Verse;
using RimWorld;
using System.Text;
using UnityEngine;
using RimPiece.Components;

namespace RimPiece.Hediffs
{
    public class HediffObservationHaki : HediffWithComps
    {
        public override UnityEngine.Color LabelColor => UnityEngine.Color.cyan;
        
        public override string Description
        {
            get
            {
                var p = this.pawn;
                var hakiComp = p?.GetComp<CompHaki>();
                if (hakiComp == null) return base.Description;

                var level = hakiComp.ObservationLevel;
                if (level < 10) return "Something feels off: The user has heightened awareness.";
                if (level < 30) return "Heightened Senses: The user feels threats before they happen.";
                if (level < 70) return "Predictive Movement: The user reads intent, dodging attacks with ease.";
                if (level < 90) return "Future Glimpses: The user can see moments into the future.";
                return "Future Sight: The user sees seconds ahead, rewriting reality to avoid harm.";
            }
        }
        
        public override string TipStringExtra
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append(base.TipStringExtra);

                var p = this.pawn;
                var hakiComp = p?.GetComp<CompHaki>();

                if (hakiComp != null && hakiComp.ObservationLevel > 0)
                {
                    sb.AppendLine("--- Observation Stats ---");
                    
                    sb.AppendLine($"Future Sight (Nullify Dmg): {hakiComp.GetFutureSightChance():P0}");
                    sb.AppendLine($"Melee Dodge Bonus: +{hakiComp.GetDodgeBonus():P0}");
                    sb.AppendLine($"Aim Time: x{hakiComp.GetAimDelayFactor():F2}");
                    
                    if (hakiComp.ObservationLevel >= 50)
                        sb.AppendLine("Vitals Protection: Active (50% chance)");
                }
                return sb.ToString().TrimEnd();
            }
        }
        
        public override bool ShouldRemove => false;
    }
}