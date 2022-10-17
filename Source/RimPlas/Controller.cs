using Mlie;
using UnityEngine;
using Verse;

namespace RimPlas;

public class Controller : Mod
{
    public static Settings Settings;
    public static string currentVersion;

    public Controller(ModContentPack content) : base(content)
    {
        Settings = GetSettings<Settings>();
        currentVersion =
            VersionFromManifest.GetVersionFromModMetaData(ModLister.GetActiveModWithIdentifier("Mlie.RimPlas"));
    }

    public override string SettingsCategory()
    {
        return "RimPlas.Name".Translate();
    }

    public override void DoSettingsWindowContents(Rect canvas)
    {
        Settings.DoWindowContents(canvas);
    }
}