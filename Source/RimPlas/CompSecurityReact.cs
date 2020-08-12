using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RimPlas
{
	// Token: 0x02000011 RID: 17
	public class CompSecurityReact : ThingComp
	{
		// Token: 0x1700000A RID: 10
		// (get) Token: 0x0600003B RID: 59 RVA: 0x00003265 File Offset: 0x00001465
		public CompProperties_SecurityReact Props
		{
			get
			{
				return (CompProperties_SecurityReact)this.props;
			}
		}

		// Token: 0x0600003C RID: 60 RVA: 0x00003274 File Offset: 0x00001474
		public override void CompTick()
		{
			if (Find.TickManager.TicksGame % 60 == 0 && !CompSecurityReact.IsDisabled(this.parent))
			{
				Thing target = CompSecurityReact.MentalDetect(this.parent, this.Props.radius);
				if (target != null)
				{
					ThingWithComps parent = this.parent;
					float radius = this.Props.radius;
					Thing target2 = target;
					CompProperties_SecurityReact props = this.Props;
					CompSecurityReact.DoStunner(parent, radius, target2, (props != null) ? props.SecurityHediff : null, this.Props.SecuritySeverity);
				}
			}
		}

		// Token: 0x0600003D RID: 61 RVA: 0x000032EC File Offset: 0x000014EC
		public static void DoStunner(ThingWithComps securityThing, float radius, Thing target, string SecHed, float SecSev)
		{
			HediffDef secHediffDef;
			if (SecHed != null)
			{
				secHediffDef = DefDatabase<HediffDef>.GetNamed(SecHed, false);
				if (secHediffDef == null)
				{
					secHediffDef = DefDatabase<HediffDef>.GetNamed(Globals.StunHed, false);
				}
			}
			else
			{
				secHediffDef = DefDatabase<HediffDef>.GetNamed(Globals.StunHed, false);
			}
			float SeverityToApply = Globals.StunSev;
			if (SecSev > 0f)
			{
				SeverityToApply = SecSev;
			}
			Globals.RemoveMental(target as Pawn);
			Globals.HediffEffect(target as Pawn, secHediffDef, SeverityToApply);
			Globals.DoSecSpecialEffects(securityThing, radius);
		}

		// Token: 0x0600003E RID: 62 RVA: 0x00003354 File Offset: 0x00001554
		public static Thing MentalDetect(ThingWithComps securityThing, float radius)
		{
			List<Thing> DetectThings = GenRadial.RadialDistinctThingsAround(securityThing.Position, securityThing.Map, radius, true).ToList<Thing>();
			if (DetectThings.Count > 0)
			{
				for (int i = 0; i < DetectThings.Count; i++)
				{
					if (DetectThings[i] is Pawn && !(DetectThings[i] as Pawn).IsBurning() && !(DetectThings[i] as Pawn).Dead && !(DetectThings[i] as Pawn).Downed && (DetectThings[i] as Pawn).Awake())
					{
						if ((DetectThings[i] as Pawn).IsPrisoner && PrisonBreakUtility.IsPrisonBreaking(DetectThings[i] as Pawn))
						{
							return DetectThings[i];
						}
						if ((DetectThings[i] as Pawn).IsColonist && (DetectThings[i] as Pawn).InMentalState)
						{
							return DetectThings[i];
						}
					}
				}
			}
			return null;
		}

		// Token: 0x0600003F RID: 63 RVA: 0x00003460 File Offset: 0x00001660
		public static bool IsDisabled(ThingWithComps checkThing)
		{
			if (checkThing.IsBrokenDown())
			{
				return true;
			}
			CompFlickable compFlickable = checkThing.TryGetComp<CompFlickable>();
			if (compFlickable != null && !compFlickable.SwitchIsOn)
			{
				return true;
			}
			CompPowerTrader compPowerTrader = checkThing.TryGetComp<CompPowerTrader>();
			if (compPowerTrader != null && !compPowerTrader.PowerOn)
			{
				return true;
			}
			CompRefuelable compRefuelable = checkThing.TryGetComp<CompRefuelable>();
			return compRefuelable != null && !compRefuelable.HasFuel;
		}
	}
}
