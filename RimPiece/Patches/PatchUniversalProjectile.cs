using HarmonyLib;
using Verse;

namespace RimPiece.Patches
{
    [StaticConstructorOnStartup]
    public class PatchUniversalProjectile
    {
        static PatchUniversalProjectile()
        {
            var harmony = new Harmony("com.urzedo.rimpiece");
            
            var allProjectileTypes = typeof(Projectile).AllSubclassesNonAbstract();

            int count = 0;

            foreach (var type in allProjectileTypes)
            {
                var impactMethod = type.GetMethod("Impact", AccessTools.all);

                if (impactMethod != null && impactMethod.DeclaringType == type)
                {
                    try
                    {
                        harmony.Patch(
                            original: impactMethod,
                            prefix: new HarmonyMethod(typeof(FutureSightLogic), nameof(FutureSightLogic.Prefix))
                        );
                        count++;
                    }
                    catch (System.Exception e)
                    {
                        Log.Warning($"[Rim Piece] Failed to patch projectile {type.Name}: {e.Message}");
                    }
                }
            }
            
            Log.Message($"[Rim Piece] Future Sight initialized. Patched {count} projectile types (Standard & Modded).");
        }
    }
}