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
            
            var levelBonus = (haki.ArmamentLevel + haki.ObservationLevel) * 10f;
            val += levelBonus;
            
            if (p.genes != null && p.genes.HasActiveGene(DefDatabase<GeneDef>.GetNamed("RimPieceConquerorsGene")))
            {
                val += 100f;
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
                text += $"Haki energy: +{totalLevels * 10}\n";
            }

            if (p.genes != null && p.genes.HasActiveGene(DefDatabase<GeneDef>.GetNamed("RimPieceConquerorsGene")))
            {
                text += "Conqueror's Gene: +100\n";
            }

            return text;
        }
    }
}