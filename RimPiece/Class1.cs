using System.Reflection;
using Verse;
using HarmonyLib;

namespace RimPiece
{
    [StaticConstructorOnStartup]
    public class RimPiece
    {
        static RimPiece()
        {
            Log.Message("One Piece Mod: Initializing Grand Line...");

            // Create a Harmony instance
            var harmony = new Harmony("com.urzedo.onepiece");

            // This tells Harmony to find all patches in your code and apply them
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}