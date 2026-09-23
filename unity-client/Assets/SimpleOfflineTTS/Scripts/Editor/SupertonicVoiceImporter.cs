using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace SimpleOfflineTTS.Editor
{
    [ScriptedImporter(1, "bin")]
    public sealed class SupertonicVoiceBinaryImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            byte[] data = File.ReadAllBytes(ctx.assetPath);

            SupertonicVoiceBinary voice = ScriptableObject.CreateInstance<SupertonicVoiceBinary>();
            voice.name = Path.GetFileNameWithoutExtension(ctx.assetPath);
            voice.SetBytes(data);

            ctx.AddObjectToAsset("SupertonicVoice", voice);
            ctx.SetMainObject(voice);
        }
    }
}
