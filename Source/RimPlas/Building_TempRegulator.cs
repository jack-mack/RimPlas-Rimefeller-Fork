using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimPlas
{
    // Token: 0x02000006 RID: 6
    public class Building_TempRegulator : Building_TempControl
    {
        // Token: 0x0400000E RID: 14
        private const float HeatOutputMultiplier = 1.25f;

        // Token: 0x0400000F RID: 15
        private const float EfficiencyFalloffSpan = 100f;

        // Token: 0x04000010 RID: 16
        private const float EfficiencyLossPerDegreeDifference = 0.0076923077f;

        // Token: 0x04000012 RID: 18
        [NoTranslate] private readonly string commandTexture = "Things/Building/Temperature/UI/BoostPower";

        // Token: 0x04000011 RID: 17
        private bool UseBoost;

        // Token: 0x06000015 RID: 21 RVA: 0x000027F3 File Offset: 0x000009F3
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref UseBoost, "UseBoost");
        }

        // Token: 0x06000016 RID: 22 RVA: 0x0000280D File Offset: 0x00000A0D
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (Faction == Faction.OfPlayer)
            {
                yield return new Command_Toggle
                {
                    icon = ContentFinder<Texture2D>.Get(commandTexture),
                    defaultLabel = "RimPlas.Boost".Translate(),
                    defaultDesc = "RimPlas.BoostDesc".Translate(),
                    isActive = () => UseBoost,
                    toggleAction = delegate { ToggleUseBoost(UseBoost); }
                };
            }
        }

        // Token: 0x06000017 RID: 23 RVA: 0x0000281D File Offset: 0x00000A1D
        public void ToggleUseBoost(bool flag)
        {
            UseBoost = !flag;
        }

        // Token: 0x06000018 RID: 24 RVA: 0x0000282C File Offset: 0x00000A2C
        public override void TickRare()
        {
            if (!compPowerTrader.PowerOn)
            {
                return;
            }

            var props = compPowerTrader.Props;
            var coolercell = Position + IntVec3.South.RotatedBy(Rotation);
            var exhaustcell = Position + IntVec3.North.RotatedBy(Rotation);
            var heatercell = coolercell;
            var ambientTemperature = heatercell.GetTemperature(Map);
            var targtemp = compTempControl.targetTemperature;
            var powered = false;
            var temptolerance = 0.5f;
            var boost = 1f;
            if (UseBoost)
            {
                boost = 2f;
            }

            if (ambientTemperature < targtemp)
            {
                if (!heatercell.Impassable(Map))
                {
                    var numheat = ambientTemperature < 20f ? 1f :
                        ambientTemperature <= 120f ? Mathf.InverseLerp(120f, 20f, ambientTemperature) : 0f;
                    var energyLimit = compTempControl.Props.energyPerSecond * numheat * 4.16666651f * boost;
                    var tempdelta = GenTemperature.ControlTemperatureTempChange(heatercell, Map, energyLimit,
                        compTempControl.targetTemperature);
                    if (!Mathf.Approximately(tempdelta, temptolerance))
                    {
                        heatercell.GetRoomOrAdjacent(Map).Temperature += tempdelta;
                        powered = true;
                    }
                }
            }
            else if (!exhaustcell.Impassable(Map) && !coolercell.Impassable(Map))
            {
                var tempexhaust = exhaustcell.GetTemperature(Map);
                var tempcool = coolercell.GetTemperature(Map);
                var tempdiff = tempexhaust - tempcool;
                if (tempexhaust - 40f > tempdiff)
                {
                    tempdiff = tempexhaust - 40f;
                }

                var numcool = 1f - (tempdiff * 0.0076923077f);
                if (numcool < 0f)
                {
                    numcool = 0f;
                }

                var energyLimit = -1f * compTempControl.Props.energyPerSecond * numcool * 4.16666651f * boost;
                var tempdelta2 = GenTemperature.ControlTemperatureTempChange(coolercell, Map, energyLimit,
                    compTempControl.targetTemperature);
                if (!Mathf.Approximately(tempdelta2, temptolerance))
                {
                    coolercell.GetRoomOrAdjacent(Map).Temperature += tempdelta2;
                    GenTemperature.PushHeat(exhaustcell, Map, (0f - energyLimit) * 1.25f);
                    powered = true;
                }
            }

            if (powered)
            {
                compPowerTrader.PowerOutput = 0f - (props.basePowerConsumption * boost);
            }
            else
            {
                compPowerTrader.PowerOutput =
                    (0f - props.basePowerConsumption) * compTempControl.Props.lowPowerConsumptionFactor;
            }

            compTempControl.operatingAtHighPower = powered;
        }
    }
}