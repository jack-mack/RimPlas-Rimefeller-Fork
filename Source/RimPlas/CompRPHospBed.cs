using System;
using RimWorld;
using Verse;

namespace RimPlas
{
	// Token: 0x0200000E RID: 14
	public class CompRPHospBed : ThingComp
	{
		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000030 RID: 48 RVA: 0x00003024 File Offset: 0x00001224
		public CompProperties_RPHospBed Props
		{
			get
			{
				return (CompProperties_RPHospBed)this.props;
			}
		}

		// Token: 0x06000031 RID: 49 RVA: 0x00003034 File Offset: 0x00001234
		public override void CompTick()
		{
			base.CompTick();
			Thing bed = this.parent;
			CompPowerTrader CPT = bed.TryGetComp<CompPowerTrader>();
			if (CPT != null && CPT.PowerOn && !bed.IsBrokenDown())
			{
				Pawn Occupant = this.OccupiedBy(bed);
				if (Occupant != null)
				{
					if (Controller.Settings.AllowPainCtrlHB && this.Props.PainCtrlTicks > 0 && bed.IsHashIntervalTick(this.Props.PainCtrlTicks))
					{
						Pawn patient = this.OccupiedByPawnInPain(bed, Occupant);
						if (patient != null)
						{
							HediffDef PainCtrl = HediffDef.Named(this.Props.PainCtrlHediff);
							float PainCtrlSev = this.Props.PainCtrlSev;
							if (!RPHediffEffecter.HasHediff(patient, PainCtrl))
							{
								bool immune;
								RPHediffEffecter.HediffEffect(PainCtrl, PainCtrlSev, patient, null, out immune);
							}
						}
					}
					if (bed.IsHashIntervalTick(625))
					{
						if (Controller.Settings.AllowMentalCtrlHB)
						{
							Pawn patient2 = this.OccupiedByPawnWithMentalState(bed, Occupant);
							if (patient2 != null)
							{
								patient2.mindState.mentalStateHandler.Reset();
							}
						}
						if (Controller.Settings.AllowRecCtrlHB)
						{
							Pawn patient3 = this.OccupiedByPawnWithRec(bed, Occupant);
							if (patient3 != null)
							{
								patient3.needs.joy.CurLevel += this.Props.RecPerHour / 4f;
							}
						}
					}
				}
			}
		}

		// Token: 0x06000032 RID: 50 RVA: 0x0000316A File Offset: 0x0000136A
		public override void CompTickRare()
		{
			base.CompTickRare();
		}

		// Token: 0x06000033 RID: 51 RVA: 0x00003174 File Offset: 0x00001374
		private Pawn OccupiedBy(Thing bed)
		{
			Pawn p = null;
			if (bed != null && bed.Spawned && bed is Building_Bed)
			{
				p = (bed as Building_Bed).GetCurOccupant(0);
			}
			return p;
		}

		// Token: 0x06000034 RID: 52 RVA: 0x000031A4 File Offset: 0x000013A4
		private Pawn OccupiedByPawnWithMentalState(Thing bed, Pawn p)
		{
			if (p != null && !p.mindState.mentalStateHandler.InMentalState)
			{
				p = null;
			}
			return p;
		}

		// Token: 0x06000035 RID: 53 RVA: 0x000031BF File Offset: 0x000013BF
		private Pawn OccupiedByPawnWithRec(Thing bed, Pawn p)
		{
			if (p != null)
			{
				bool flag;
				if (p == null)
				{
					flag = (null != null);
				}
				else
				{
					Pawn_NeedsTracker needs = p.needs;
					flag = (((needs != null) ? needs.joy : null) != null);
				}
				if (flag)
				{
					if (p.needs.joy.CurLevel >= 0.75f)
					{
						p = null;
					}
				}
				else
				{
					p = null;
				}
			}
			return p;
		}

		// Token: 0x06000036 RID: 54 RVA: 0x000031FE File Offset: 0x000013FE
		private Pawn OccupiedByPawnInPain(Thing bed, Pawn p)
		{
			if (p != null && p.health.hediffSet.PainTotal <= 0.1f)
			{
				p = null;
			}
			return p;
		}
	}
}
