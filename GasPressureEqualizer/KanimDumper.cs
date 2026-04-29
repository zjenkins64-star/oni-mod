using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using UnityEngine;

namespace GasPressureEqualizer
{
    // Hooks KAnimFile.Initialize to capture the TextAsset bytes the moment they're
    // passed in, before ONI parses + drops the references. Run once per machine —
    // creates a .dumped marker so subsequent loads no-op. Remove or comment out
    // the [HarmonyPatch] once you've confirmed the files are extracted.
    // FinalizeLoading is the method that nulls animFile/buildFile after parsing.
    // Prefix-patch it to grab the bytes via animBytes/buildBytes properties while
    // the references are still alive.
    [HarmonyPatch(typeof(KAnimFile), nameof(KAnimFile.FinalizeLoading))]
    public static class KAnimFile_FinalizeLoading_DumpPatch
    {
        // Source kanim → target folder/file prefix. Add more entries to dump
        // additional kanims for editing.
        private static readonly System.Collections.Generic.Dictionary<string, string> Targets =
            new System.Collections.Generic.Dictionary<string, string>
            {
                { "utilities_gas",    "equalizer_duct" },
                { "ventgas",          "equalizer_vent" },
                { "utilitygasbridge", "equalizer_bridge" },
            };

        private const string ROOT = @"c:\Users\zjenk\source\repos\GasPressureEqualizer\GasPressureEqualizer\anim\assets";

        public static void Prefix(KAnimFile __instance)
        {
            if (__instance == null) return;
            string n = __instance.name ?? string.Empty;

            // FinalizeLoading is called with names like "ventgas_kanim" — strip
            // the suffix before looking up in our base-named target table.
            string baseName = n.EndsWith("_kanim") ? n.Substring(0, n.Length - "_kanim".Length) : n;
            if (!Targets.TryGetValue(baseName, out string targetName)) return;

            string outDir = Path.Combine(ROOT, targetName);
            string markerPath = Path.Combine(outDir, ".dumped");
            string mainPng = Path.Combine(outDir, $"{targetName}_0.png");
            // Belt-and-suspenders: skip if EITHER the marker or the main PNG exists.
            // Prevents accidentally overwriting hand-edited assets.
            if (File.Exists(markerPath) || File.Exists(mainPng)) return;

            try
            {
                Directory.CreateDirectory(outDir);
                byte[] anim = __instance.animBytes;
                byte[] build = __instance.buildBytes;
                var textures = __instance.textureList;

                if (anim != null)
                {
                    File.WriteAllBytes(Path.Combine(outDir, $"{targetName}_anim.bytes"), anim);
                    Debug.Log($"[GPE] Dumped {n}_anim ({anim.Length} bytes)");
                }
                if (build != null)
                {
                    File.WriteAllBytes(Path.Combine(outDir, $"{targetName}_build.bytes"), build);
                    Debug.Log($"[GPE] Dumped {n}_build ({build.Length} bytes)");
                }

                if (textures != null)
                {
                    for (int i = 0; i < textures.Count; i++)
                    {
                        var src = textures[i];
                        if (src == null) continue;
                        byte[] png = EncodePng(src);
                        string filename = i == 0 ? $"{targetName}_0.png" : $"{targetName}_{i}.png";
                        File.WriteAllBytes(Path.Combine(outDir, filename), png);
                        Debug.Log($"[GPE] Dumped {src.name} ({src.width}x{src.height})");
                    }
                }

                File.WriteAllText(markerPath, $"Dumped {n} at {System.DateTime.UtcNow:o}\n");
                Debug.Log($"[GPE] Kanim dump complete: {outDir}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GPE] Kanim dump failed for {n}: {ex}");
            }
        }

        private static byte[] EncodePng(Texture2D src)
        {
            var rt = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(src, rt);
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var readable = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
            readable.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0);
            readable.Apply();
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            byte[] png = readable.EncodeToPNG();
            UnityEngine.Object.Destroy(readable);
            return png;
        }
    }
}
