using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using System.Collections.Generic;
using RimPiece.Components;

namespace RimPiece.Patches
{
    [HarmonyPatch(typeof(FloatMenuMakerMap), "GetOptions")]
    public class PatchFloatMenu
    {
        public static void Postfix(List<Pawn> selectedPawns, Vector3 clickPos, List<FloatMenuOption> __result)
        {
            if (__result == null || selectedPawns == null || selectedPawns.Count == 0) return;
            if (selectedPawns.Count > 1) return;
            
            var pawn = selectedPawns[0];
            if (pawn.Drafted) return;

            foreach (var target in GenUI.TargetsAt(clickPos, TargetingParameters.ForPawns(), true))
            {
                var student = target.Thing as Pawn;
                if (student == null || student == pawn) continue;
                
                if (!student.IsColonist) continue;
                
                var masterComp = pawn.GetComp<CompHaki>();
                if (masterComp == null) continue;
                
                if (masterComp.TicksUntilNextTrain() > 0)
                {
                    var waitTime = (masterComp.TicksUntilNextTrain() / 60000f).ToString("0.0");
                    __result.Add(new FloatMenuOption($"Teach Haki: Cooldown {waitTime} days", null));
                    continue;
                }
                
                if (!masterComp.CanTrainOthers()) 
                {
                    __result.Add(new FloatMenuOption("Teach Haki: Need Lv 16+ in one category", null));
                    continue; 
                }
                
                var studentComp = student.GetComp<CompHaki>(); 
                if (studentComp != null && studentComp.IsHakiUser && masterComp.ArmamentLevel <= studentComp.ArmamentLevel && masterComp.ObservationLevel <= studentComp.ObservationLevel)
                {
                    __result.Add(new FloatMenuOption("Teach Haki: Nothing to teach", null));
                    continue;
                }

                var label = $"Teach Haki to {student.LabelShort}";
                
                __result.Add(new FloatMenuOption(label, () =>
                {
                    var job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("RimPieceTeachHaki"), student);
                    job.count = 1;
                    pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                }));
            }
        }
    }
}