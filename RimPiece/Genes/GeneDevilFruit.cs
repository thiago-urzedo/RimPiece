using Verse;
using RimWorld;
using TaranMagicFramework;
using AbilityDef = TaranMagicFramework.AbilityDef;

namespace RimPiece.Genes
{
    public class GeneDevilFruit : Gene
    {
        public AbilityDef AbilityToGrant => def.GetModExtension<GeneExtensionTMF>()?.tmfAbility;

        public override void PostAdd()
        {
            base.PostAdd();
            AddAbility();
        }

        public override void PostRemove()
        {
            base.PostRemove();
            RemoveAbility();
        }

        private void AddAbility()
        {
            if (pawn == null || AbilityToGrant == null) return;

            var compAbilities = pawn.GetComp<CompAbilities>();
            if (compAbilities == null) return;

            var classDef = DefDatabase<AbilityClassDef>.GetNamed("RimPieceDevilFruitClass");
            var abilityClass = compAbilities.GetAbilityClass(classDef);

            if (abilityClass == null)
            {
                abilityClass = compAbilities.CreateAbilityClass(classDef, true);
            }
            
            if (!abilityClass.Unlocked)
            {
                abilityClass.Unlocked = true;
            }
            
            var treeDef = DefDatabase<AbilityTreeDef>.GetNamed("RimPieceDevilFruitTree");
            if (treeDef != null && !abilityClass.TreeUnlocked(treeDef))
            {
                abilityClass.UnlockTree(treeDef);
            }

            if (!abilityClass.Learned(AbilityToGrant))
            {
                abilityClass.LearnAbility(AbilityToGrant, false, 0);
                
                if (pawn.Faction != null && pawn.Faction.IsPlayer)
                {
                    Messages.Message($"{pawn.LabelShort} gained the power of the {AbilityToGrant.label}!", pawn, MessageTypeDefOf.PositiveEvent);
                }
            }
        }

        private void RemoveAbility()
        {
            // TODO - check how to remove the abilities properly
        }
    }
}