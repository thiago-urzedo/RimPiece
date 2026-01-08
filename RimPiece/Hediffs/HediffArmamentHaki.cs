using Verse;
using System.Text;
using RimPiece.Components;
using UnityEngine;

namespace RimPiece.Hediffs
{
    public class HediffArmamentHaki : HediffWithComps
    {
        public override UnityEngine.Color LabelColor => UnityEngine.Color.cyan;
        
        public override string Description
        {
            get
            {
                var p = this.pawn;
                var hakiComp = p?.GetComp<CompHaki>();
                if (hakiComp == null) return base.Description;

                var level = hakiComp.ArmamentLevel;
                if (level < 10) return "Basic Hardening: User can slightly harden their skin to reduce pain.";
                if (level < 30) return "Hardened Armament: Attacks deal more damage to structures and armor.";
                if (level < 70) return "Spirit Vulcanization: Willpower compressed into a shell tougher than, turning user's body into a living weapon.";
                if (level < 90) return "Internal Destruction (Ryuo): Haki flows into the target, destroying them from inside.";
                return "Advanced Ryuo: Mastery of Armament Haki, allowing the user to bypass physical defenses entirely.";
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

                if (hakiComp != null && hakiComp.ArmamentLevel > 0)
                {
                    sb.AppendLine("--- Armament Stats ---");
                    
                    sb.AppendLine($"Melee AP Bonus: +{hakiComp.GetArmamentAPBonus():P0}");
                    sb.AppendLine($"Melee Dmg Factor: x{hakiComp.GetArmamentDamageFactor():F2}");
                    
                    var flatDmg = hakiComp.GetArmamentFlatDamage();
                    if (flatDmg > 0) sb.AppendLine($"Melee Flat Dmg: +{flatDmg:F1}");

                    float reduction = 1f - hakiComp.GetIncomingDamageFactor();
                    sb.AppendLine($"Damage Reduction: {reduction:P1}");

                    if (hakiComp.ArmamentLevel >= 70)
                    {
                        var ryuoChance = Mathf.Pow((hakiComp.ArmamentLevel - 70) / 30f, 1.2f) * 0.30f;
                        sb.AppendLine($"Ryuo (Armor Bypass): {ryuoChance:P1} chance");
                    }
                }

                return sb.ToString().TrimEnd();
            }
        }
        
        public override bool ShouldRemove => false;
    }
}