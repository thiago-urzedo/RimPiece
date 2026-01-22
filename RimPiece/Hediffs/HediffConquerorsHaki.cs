using Verse;
using System.Text;
using RimPiece.Components;

namespace RimPiece.Hediffs
{
    public class HediffConquerorsHaki : HediffWithComps
    {
        public override UnityEngine.Color LabelColor => UnityEngine.Color.cyan;
        
        public override string Description => "Conqueror's Power: The user coats their attacks with Conqueror's Haki. Massive damage boost.";

        public override string TipStringExtra
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append(base.TipStringExtra);

                var p = this.pawn;
                var hakiComp = p?.GetComp<CompHaki>();

                if (hakiComp != null && hakiComp.ObservationLevel + hakiComp.ArmamentLevel >= 30)
                {
                    var armourBonus = hakiComp.ArmamentLevel * 0.05f;
                    
                    sb.AppendLine("--- Coating Stats ---");
                    
                    sb.AppendLine($"Melee AP Bonus: +50%");
                    sb.AppendLine($"Melee Dmg: x1.5");
                    sb.AppendLine($"Stun Chance: 10%");
                    sb.AppendLine($"Armour Bonus: +{armourBonus:P0}");
                }
                return sb.ToString().TrimEnd();
            }
        }
        
        public override bool ShouldRemove => false;
    }
}