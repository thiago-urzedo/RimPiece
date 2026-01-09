using UnityEngine;
using Verse;
using RimWorld;
using RimPiece.Components;

namespace RimPiece.UI
{
    public class ITabPawnHaki: ITab
    {
        private static readonly Vector2 WinSize = new Vector2(300f, 200f);

        public ITabPawnHaki()
        {
            this.size = WinSize;
            this.labelKey = "RimPieceTabHaki"; 
            this.tutorTag = "Haki";
        }

        private Pawn SelPawnForHaki
        {
            get
            {
                switch (SelObject)
                {
                    case Pawn p:
                        return p;
                    case Corpse c:
                        return c.InnerPawn;
                    default:
                        return null;
                }
            }
        }

        public override bool IsVisible
        {
            get
            {
                var pawn = this.SelPawnForHaki;

                if (pawn == null || pawn.story == null) return false;

                return pawn.story.traits.HasTrait(TraitDef.Named("RimPieceHakiUser"));
            }
        }

        protected override void FillTab()
        {
            var rect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
            GUI.BeginGroup(rect);
            
            var pawn = SelPawnForHaki;
            var hakiComp = pawn?.GetComp<CompHaki>(); 
            var hasConqGene = pawn.genes != null &&
                              pawn.genes.HasActiveGene(DefDatabase<GeneDef>.GetNamed("RimPieceConquerorsGene"));

            if (hakiComp == null)
            {
                Widgets.Label(rect, "This entity cannot use Haki.");
                GUI.EndGroup();
                return;
            }

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0, 0, rect.width, 30), "Haki Mastery");
            Text.Font = GameFont.Small;

            var currentY = 55f;
            
            DrawHakiBar(new Rect(0, currentY, rect.width, 24f), 
                $"Armament (Lv {hakiComp.ArmamentLevel})", 
                hakiComp.ArmamentXp, 
                hakiComp.ArmamentMaxXp, 
                Color.black,
                hakiComp.ArmamentLevel == hakiComp.GetMaxLevel());
            
            currentY += 50f;
            
            DrawHakiBar(new Rect(0, currentY, rect.width, 24f), 
                $"Observation (Lv {hakiComp.ObservationLevel})", 
                hakiComp.ObservationXp, 
                hakiComp.ObservationMaxXp, 
                Color.black,
                hakiComp.ObservationLevel == hakiComp.GetMaxLevel());
            
            if (hasConqGene)
            {
                currentY += 40f;
                var conquerorsStatus = hakiComp.HasConquerors ? "Awakened" : "Dormant";
                Widgets.Label(new Rect(0, currentY, rect.width, 24f), $"Conqueror's Haki: {conquerorsStatus}");
            }

            GUI.EndGroup();
        }

        private static void DrawHakiBar(Rect rect, string label, float cur, float max, Color barColor, bool reachMaxLevel)
        {
            Widgets.Label(new Rect(rect.x, rect.y - 20f, rect.width, 20f), label);
            Widgets.DrawBoxSolid(rect, new Color(0.2f, 0.2f, 0.2f));
            
            var fillPercent = cur / max;
            fillPercent = Mathf.Clamp01(fillPercent);
            
            var fillRect = rect;
            fillRect.width *= fillPercent;
            
            Widgets.DrawBoxSolid(fillRect, barColor);
            Widgets.DrawBox(rect); 
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, reachMaxLevel ? "MAX" : $"{cur:F0} / {max:F0}");
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}