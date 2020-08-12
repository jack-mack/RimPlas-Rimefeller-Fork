using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimPlas
{
	// Token: 0x02000006 RID: 6
	public class Building_TempRegulator : Building_TempControl
	{
		// Token: 0x06000015 RID: 21 RVA: 0x000027F3 File Offset: 0x000009F3
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.UseBoost, "UseBoost", false, false);
		}

		// Token: 0x06000016 RID: 22 RVA: 0x0000280D File Offset: 0x00000A0D
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			IEnumerator<Gizmo> enumerator = null;
			if (base.Faction == Faction.OfPlayer)
			{
				yield return new Command_Toggle
				{
					icon = ContentFinder<Texture2D>.Get(this.commandTexture, true),
					defaultLabel = "RimPlas.Boost".Translate(),
					defaultDesc = "RimPlas.BoostDesc".Translate(),
					isActive = (() => this.UseBoost),
					toggleAction = delegate()
					{
						this.ToggleUseBoost(this.UseBoost);
					}
				};
			}
			yield break;
			yield break;
		}

		// Token: 0x06000017 RID: 23 RVA: 0x0000281D File Offset: 0x00000A1D
		public void ToggleUseBoost(bool flag)
		{
			this.UseBoost = !flag;
		}

		// Token: 0x06000018 RID: 24 RVA: 0x0000282C File Offset: 0x00000A2C
		public override void TickRare()
		{
			if (!this.compPowerTrader.PowerOn)
			{
				return;
			}
			CompProperties_Power props = this.compPowerTrader.Props;
			IntVec3 coolercell = base.Position + IntVec3.South.RotatedBy(base.Rotation);
			IntVec3 exhaustcell = base.Position + IntVec3.North.RotatedBy(base.Rotation);
			IntVec3 heatercell = coolercell;
			float ambientTemperature = heatercell.GetTemperature(base.Map);
			float targtemp = this.compTempControl.targetTemperature;
			bool powered = false;
			float temptolerance = 0.5f;
			float boost = 1f;
			if (this.UseBoost)
			{
				boost = 2f;
			}
			if (ambientTemperature < targtemp)
			{
				if (!heatercell.Impassable(base.Map))
				{
					float numheat = (ambientTemperature < 20f) ? 1f : ((ambientTemperature <= 120f) ? Mathf.InverseLerp(120f, 20f, ambientTemperature) : 0f);
					float energyLimit = this.compTempControl.Props.energyPerSecond * numheat * 4.16666651f * boost;
					float tempdelta = GenTemperature.ControlTemperatureTempChange(heatercell, base.Map, energyLimit, this.compTempControl.targetTemperature);
					if (!Mathf.Approximately(tempdelta, temptolerance))
					{
						heatercell.GetRoomGroup(base.Map).Temperature += tempdelta;
						powered = true;
					}
				}
			}
			else if (!exhaustcell.Impassable(base.Map) && !coolercell.Impassable(base.Map))
			{
				float tempexhaust = exhaustcell.GetTemperature(base.Map);
				float tempcool = coolercell.GetTemperature(base.Map);
				float tempdiff = tempexhaust - tempcool;
				if (tempexhaust - 40f > tempdiff)
				{
					tempdiff = tempexhaust - 40f;
				}
				float numcool = 1f - tempdiff * 0.0076923077f;
				if (numcool < 0f)
				{
					numcool = 0f;
				}
				float energyLimit = -1f * this.compTempControl.Props.energyPerSecond * numcool * 4.16666651f * boost;
				float tempdelta2 = GenTemperature.ControlTemperatureTempChange(coolercell, base.Map, energyLimit, this.compTempControl.targetTemperature);
				if (!Mathf.Approximately(tempdelta2, temptolerance))
				{
					coolercell.GetRoomGroup(base.Map).Temperature += tempdelta2;
					GenTemperature.PushHeat(exhaustcell, base.Map, (0f - energyLimit) * 1.25f);
					powered = true;
				}
			}
			if (powered)
			{
				this.compPowerTrader.PowerOutput = 0f - props.basePowerConsumption * boost;
			}
			else
			{
				this.compPowerTrader.PowerOutput = (0f - props.basePowerConsumption) * this.compTempControl.Props.lowPowerConsumptionFactor;
			}
			this.compTempControl.operatingAtHighPower = powered;
		}

		// Token: 0x0400000E RID: 14
		private const float HeatOutputMultiplier = 1.25f;

		// Token: 0x0400000F RID: 15
		private const float EfficiencyFalloffSpan = 100f;

		// Token: 0x04000010 RID: 16
		private const float EfficiencyLossPerDegreeDifference = 0.0076923077f;

		// Token: 0x04000011 RID: 17
		private bool UseBoost;

		// Token: 0x04000012 RID: 18
		[NoTranslate]
		private string commandTexture = "Things/Building/Temperature/UI/BoostPower";
	}
}
