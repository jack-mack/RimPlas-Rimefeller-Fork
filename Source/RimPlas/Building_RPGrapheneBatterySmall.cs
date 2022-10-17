using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimPlas;

[StaticConstructorOnStartup]
public class Building_RPGrapheneBatterySmall : Building
{
    private const float MinEnergyToExplode = 500f;

    private const float EnergyToLoseWhenExplode = 400f;

    private const float ExplodeChancePerDamage = 0.05f;

    private static readonly Vector2 BarSize = new Vector2(0.65f, 0.2f);

    private static readonly Material BatteryBarFilledMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.9f, 0.85f, 0.2f));

    private static readonly Material BatteryBarUnfilledMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));

    private int ticksToExplode;

    private Sustainer wickSustainer;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ticksToExplode, "ticksToExplode");
    }

    public override void Draw()
    {
        base.Draw();
        var comp = GetComp<CompPowerBattery>();
        var r = default(GenDraw.FillableBarRequest);
        r.center = DrawPos + (Vector3.up * 0.1f);
        r.size = BarSize;
        r.fillPercent = comp.StoredEnergy / comp.Props.storedEnergyMax;
        r.filledMat = BatteryBarFilledMat;
        r.unfilledMat = BatteryBarUnfilledMat;
        r.margin = 0.15f;
        var rotation = Rotation;
        rotation.Rotate(RotationDirection.Clockwise);
        r.rotation = rotation;
        GenDraw.DrawFillableBar(r);
        if (ticksToExplode > 0 && Spawned)
        {
            Map.overlayDrawer.DrawOverlay(this, OverlayTypes.BurningWick);
        }
    }

    public override void Tick()
    {
        base.Tick();
        if (ticksToExplode <= 0)
        {
            return;
        }

        if (wickSustainer == null)
        {
            StartWickSustainer();
        }
        else
        {
            wickSustainer.Maintain();
        }

        ticksToExplode--;
        if (ticksToExplode != 0)
        {
            return;
        }

        var randomCell = this.OccupiedRect().RandomCell;
        var radius = Rand.Range(0.5f, 1f) * 3f;
        GenExplosion.DoExplosion(randomCell, Map, radius, DamageDefOf.Flame, null);
        GetComp<CompPowerBattery>().DrawPower(400f);
    }

    public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
    {
        base.PostApplyDamage(dinfo, totalDamageDealt);
        if (Destroyed || ticksToExplode != 0 || dinfo.Def != DamageDefOf.Flame || !(Rand.Value < 0.05f) ||
            !(GetComp<CompPowerBattery>().StoredEnergy > 500f))
        {
            return;
        }

        ticksToExplode = Rand.Range(70, 150);
        StartWickSustainer();
    }

    private void StartWickSustainer()
    {
        var info = SoundInfo.InMap(this, MaintenanceType.PerTick);
        wickSustainer = SoundDefOf.HissSmall.TrySpawnSustainer(info);
    }
}