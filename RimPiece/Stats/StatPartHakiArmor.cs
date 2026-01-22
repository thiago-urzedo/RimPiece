using RimWorld;
using Verse;
using RimPiece.Components;

namespace RimPiece.Stats
{
    public class StatPartHakiArmor : StatPart
    {
        private const float ArmorPerLevel = 0.03f; 
        private const float CoCArmorPerLevel = 0.05f;

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!req.HasThing) return;

            var p = req.Thing as Pawn;
            if (p == null) return;

            var haki = p.GetComp<CompHaki>();
            if (haki == null) return;
            
            if (p.health.hediffSet.HasHediff(HediffDef.Named("RimPieceArmamentHaki")))
            {
                var bonus = haki.ArmamentLevel * ArmorPerLevel;
                val += bonus; 
            }
            
            if (p.health.hediffSet.HasHediff(HediffDef.Named("RimPieceConquerorInfusion")))
            {
                var bonus = (haki.ArmamentLevel * 0.75f) * CoCArmorPerLevel;
                val += bonus; 
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
            
            if (p.health.hediffSet.HasHediff(HediffDef.Named("RimPieceArmamentHaki")))
            {
                var bonus = haki.ArmamentLevel * ArmorPerLevel;
                text += $"Armament Haki (Lv {haki.ArmamentLevel}): +{bonus.ToStringPercent()}\n";
            }
            
            if (p.health.hediffSet.HasHediff(HediffDef.Named("RimPieceConquerorInfusion")))
            {
                var bonus = (haki.ArmamentLevel * 0.75f) * CoCArmorPerLevel;
                text += $"Conqueror's Haki: +{bonus.ToStringPercent()}\n";
            }
            
            return text;
        }
    }
}