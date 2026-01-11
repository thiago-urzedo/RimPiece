using RimWorld;
using Verse;
using RimPiece.Components;

namespace RimPiece.Stats
{
    public class StatHakiEnergyGrowth : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!req.HasThing) return;

            var p = req.Thing as Pawn;
            if (p == null) return;

            var haki = p.GetComp<CompHaki>();
            if (haki == null) return;
            
            var levelBonus = (haki.ArmamentLevel + haki.ObservationLevel) * 20f;
            val += levelBonus;
            
            if (haki.IsConqueror)
            {
                val += 200f;
            }
        }
        
        public override string ExplanationPart(StatRequest req)
        {
            if (!req.HasThing) return null;

            var p = req.Thing as Pawn;
            if (p == null) return null;

            var haki = p.GetComp<CompHaki>();
            if (haki == null) return null;
            
            var text = "";
            
            var totalLevels = haki.ArmamentLevel + haki.ObservationLevel;
            if (totalLevels > 0)
            {
                text += $"Haki stamina: +{totalLevels * 10}\n";
            }

            if (haki.IsConqueror)
            {
                text += "Conqueror's Gene: +200\n";
            }

            return text;
        }
    }
}