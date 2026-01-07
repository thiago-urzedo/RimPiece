using System;
using System.Reflection;
using System.Collections.Generic;
using Verse;
using HarmonyLib;
using RimPiece.UI;

namespace RimPiece
{
    [StaticConstructorOnStartup]
    public class RimPiece
    {
        static RimPiece()
        {
            Log.Message("[Rim Piece]: Entering the Grand Line...");

            var harmony = new Harmony("com.urzedo.onepiece");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            InjectHakiTab();
        }
        
        private static void InjectHakiTab()
        {
            var humanDef = DefDatabase<ThingDef>.GetNamed("Human");

            if (humanDef != null)
            {
                if (humanDef.inspectorTabs == null)
                {
                    humanDef.inspectorTabs = new List<System.Type>();
                }
                humanDef.inspectorTabs.Add(typeof(ITab_Pawn_Haki));
                
                if (humanDef.inspectorTabsResolved == null)
                {
                    humanDef.inspectorTabsResolved = new List<InspectTabBase>();
                }
                var tabInstance = (InspectTabBase)Activator.CreateInstance(typeof(ITab_Pawn_Haki));
                humanDef.inspectorTabsResolved.Add(tabInstance);

                Log.Message("[Rim Piece] Haki Tab successfully injected into Human Def.");
            }
            else
            {
                Log.Error("[Rim Piece] Critical Error: Could not find 'Human' definition!");
            }
        }
    }
}