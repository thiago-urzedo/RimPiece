using Verse;
using System.Text;
using RimPiece.Components;

namespace RimPiece.Hediffs
{
    public class HediffConquerorsHaki : HediffWithComps
    {
        public override UnityEngine.Color LabelColor => UnityEngine.Color.cyan;
        
        public override string Description => "Unlimited Power: The user coats their attacks with Conqueror's Haki. Massive damage boost.";

        public override string TipStringExtra
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append(base.TipStringExtra);

                var p = this.pawn;
                var hakiComp = p?.GetComp<CompHaki>();

                if (hakiComp != null && hakiComp.ObservationLevel >= 16 && hakiComp.ArmamentLevel >= 16)
                {
                    sb.AppendLine("--- Coating Stats ---");
                    
                    sb.AppendLine($"Melee AP Bonus: +50%");
                    sb.AppendLine($"Melee Dmg: x1.5");
                    sb.AppendLine($"Stun Chance: 10%");
                }
                return sb.ToString().TrimEnd();
            }
        }
        
        public override bool ShouldRemove => false;
    }
}