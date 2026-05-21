#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SimpleOfflineTTS.Editor
{
    [InitializeOnLoad]
    public static class RequireInference250EditorCheck
    {
        const string PkgName = "com.unity.ai.inference";
        static readonly Version Min = new Version(2, 3, 0);

        static RequireInference250EditorCheck()
        {
            // Delay ensures this runs after the editor finishes loading / domain reload.
            EditorApplication.delayCall += CheckOnce;
        }

        static void CheckOnce()
        {
            try
            {
                if (!TryGetInstalledVersion(PkgName, out Version installed, out string raw))
                {
                    Debug.LogError(
                        $"[SimpleOfflineTTS] Requires {PkgName} >= {Min}, but the package is not installed. " +
                        $"Open Window > Package Manager and install/upgrade Unity Inference Engine."
                    );
                    return;
                }

                if (installed < Min)
                {
                    Debug.LogError(
                        $"[SimpleOfflineTTS] Requires {PkgName} >= {Min}, but found {raw}. " +
                        $"Please update it in Package Manager."
                    );

                    // Also pop a dialog once.
                    EditorUtility.DisplayDialog(
                        "SimpleOfflineTTS - Dependency too old",
                        $"This asset requires Unity Inference Engine (com.unity.ai.inference) >= {Min}\n\nFound: {raw}\n\nPlease update via Package Manager.",
                        "OK");
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        static bool TryGetInstalledVersion(string pkgName, out Version version, out string raw)
        {
            var pkg = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages().FirstOrDefault(p => p.name == pkgName);

            if (pkg == null)
            {
                version = null;
                raw = null;
                return false;
            }

            raw = pkg.version;
            version = ParseSemVer(raw);
            return true;
        }

        static Version ParseSemVer(string v)
        {
            // "2.5.0", "2.5.0-pre.1", etc.
            string core = v.Split('-', '+')[0];

            // Defensive: pad missing parts if Unity ever returns "2.5"
            string[] parts = core.Split('.');
            int major = parts.Length > 0 ? int.Parse(parts[0]) : 0;
            int minor = parts.Length > 1 ? int.Parse(parts[1]) : 0;
            int patch = parts.Length > 2 ? int.Parse(parts[2]) : 0;

            return new Version(major, minor, patch);
        }
    }
}
#endif
