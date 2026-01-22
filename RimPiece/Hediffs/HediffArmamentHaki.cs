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
                if (level < 2) return "Basic Hardening: User can slightly harden their skin to reduce pain.";
                if (level < 6) return "Hardened Armament: Attacks deal more damage to structures and armor.";
                if (level < 14) return "Spirit Vulcanization: Willpower compressed into a shell tougher than, turning user's body into a living weapon.";
                if (level < 18) return "Internal Destruction (Ryuo): Haki flows into the target, destroying them from inside.";
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
                    
                    sb.AppendLine($"Melee AP Bonus: +{hakiComp.GetArmamentAPBonus()}");
                    sb.AppendLine($"Melee Dmg Factor: x{hakiComp.GetArmamentDamageFactor():F2}");
                    
                    var flatDmg = hakiComp.GetArmamentFlatDamage();
                    if (flatDmg > 0) sb.AppendLine($"Melee Flat Dmg: +{flatDmg:F1}");

                    var reduction = 1f - hakiComp.GetIncomingDamageFactor();
                    var armourBonus = hakiComp.ArmamentLevel * 0.03f;
                    sb.AppendLine($"Damage Reduction: {reduction:P1}");
                    sb.AppendLine($"Armour Bonus: +{armourBonus:P0}");
                    

                    if (hakiComp.ArmamentLevel >= 70)
                    {
                        var ryuoChance = Mathf.Pow((hakiComp.ArmamentLevel - 14) / 6f, 0.2f) * 0.4f;
                        sb.AppendLine($"Ryuo (Armor Bypass): {ryuoChance:P1} chance");
                    }
                }

                return sb.ToString().TrimEnd();
            }
        }
        
        public override bool ShouldRemove => false;
    }
}