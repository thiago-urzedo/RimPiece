using RimWorld;
using Verse;
using UnityEngine;

namespace RimPiece.Items
{
    public abstract class DevilFruit : ThingWithComps
    {
        public abstract FruitType Type { get; }

        protected override void PostIngested(Pawn ingester)
        {
            base.PostIngested(ingester);

            if (ingester == null || !ingester.RaceProps.Humanlike) return;

            if (IsAlreadyFruitUser(ingester))
            {
                TriggerGreedExplosion(ingester);
                return;
            }

            var extension = def.GetModExtension<Defs.DevilFruitExtension>();
            if (extension != null && extension.geneToGrant != null)
            {
                GrantGene(ingester, extension.geneToGrant);
            }
            else
            {
                Log.Error($"[RimPiece] Fruit {def.defName} is missing DevilFruitExtension or geneToGrant!");
            }

            ingester.health.AddHediff(HediffDef.Named("RimPieceCurseOfTheSea"));
            
            if (ingester.needs?.food != null) 
                ingester.needs.food.CurLevel = ingester.needs.food.MaxLevel;
            
            ingester.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDef.Named("RimPieceAteDevilFruit"));

            MoteMaker.ThrowText(ingester.DrawPos, ingester.Map, $"{Type} Power!", Color.cyan);
        }

        private void GrantGene(Pawn p, GeneDef gene)
        {
            if (p.genes != null)
            {
                p.genes.AddGene(gene, xenogene: true);
            }
        }

        private bool IsAlreadyFruitUser(Pawn p)
        {
            return p.health.hediffSet.HasHediff(HediffDef.Named("RimPieceCurseOfTheSea"));
        }

        private void TriggerGreedExplosion(Pawn p)
        {
            MoteMaker.ThrowText(p.DrawPos, p.Map, "GREED KILLS!", Color.red);
            GenExplosion.DoExplosion(p.Position, p.Map, 2f, DamageDefOf.Bomb, p, 50, 2f);
            if (!p.Dead) p.Kill(null, null);
        }
    }

    public enum FruitType { Paramecia, Zoan, Logia }
}