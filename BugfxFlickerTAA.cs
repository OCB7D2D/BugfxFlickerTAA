using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class BugfxFlickerTAA : IModApi
{

    private static string modname;

    public void InitMod(Mod mod)
    {
        Debug.Log("Loading TAA Flicker Patch: " + GetType().ToString());
        var harmony = new Harmony(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        modname = mod.ModInfo.Name?.ToString();
    }

    [HarmonyPatch(typeof(GameRenderManager))]
    [HarmonyPatch("SetAntialiasing")]
    public class GameRenderManager_SetAntialiasing
    {
        public static bool Prefix(int aaQuality, float sharpness,
            PostProcessLayer mainLayer, PostProcessLayer weaponLayer)
        {
            if (aaQuality > 3)
            {
                mainLayer.antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
                mainLayer.temporalAntialiasing.jitterSpread = 0.75f;
                mainLayer.temporalAntialiasing.stationaryBlending = 0.95f;
                mainLayer.temporalAntialiasing.motionBlending = 0.85f;
                mainLayer.temporalAntialiasing.sharpness = sharpness;
                weaponLayer.antialiasingMode = PostProcessLayer.Antialiasing.None;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(GameOptionsManager))]
    [HarmonyPatch("CalcTextureQualityMin")]
    public class GameOptionsManager_CalcTextureQualityMin
    {
        public static bool Prefix(ref int __result)
        {
            __result = 0; // SystemInfo.graphicsMemorySize < 3400 || SystemInfo.systemMemorySize < 4900 ? 1 : 0;
            return false;
        }
    }

    [HarmonyPatch(typeof(GameOptionsManager))]
    [HarmonyPatch("GetTextureFilter")]
    public class GameOptionsManager_GetTextureFilter
    {
        public static void Postfix(ref int __result)
        {
            // Ignore memory constraint and just use what is set by user
            __result = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxTexFilter);
        }
    }

}
