using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimPlas
{
	// Token: 0x02000014 RID: 20
	internal static class Globals
	{
		// Token: 0x06000046 RID: 70 RVA: 0x00003550 File Offset: 0x00001750
		internal static void HediffEffect(Pawn pawn, HediffDef stunHediffDef, float SeverityToApply)
		{
			if (!pawn.RaceProps.IsMechanoid && SeverityToApply != 0f && stunHediffDef != null && !Globals.ImmuneTo(pawn, stunHediffDef))
			{
				Pawn_HealthTracker health = pawn.health;
				Hediff hediff;
				if (health == null)
				{
					hediff = null;
				}
				else
				{
					HediffSet hediffSet = health.hediffSet;
					hediff = ((hediffSet != null) ? hediffSet.GetFirstHediffOfDef(stunHediffDef, false) : null);
				}
				Hediff hashediff = hediff;
				if (hashediff != null)
				{
					hashediff.Severity += SeverityToApply;
					return;
				}
				Hediff addhediff = HediffMaker.MakeHediff(stunHediffDef, pawn, null);
				addhediff.Severity = SeverityToApply;
				pawn.health.AddHediff(addhediff, null, null, null);
			}
		}

		// Token: 0x06000047 RID: 71 RVA: 0x000035D8 File Offset: 0x000017D8
		internal static bool ImmuneTo(Pawn pawn, HediffDef def)
		{
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				HediffStage curStage = hediffs[i].CurStage;
				if (curStage != null && curStage.makeImmuneTo != null)
				{
					for (int j = 0; j < curStage.makeImmuneTo.Count; j++)
					{
						if (curStage.makeImmuneTo[j] == def)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		// Token: 0x06000048 RID: 72 RVA: 0x00003647 File Offset: 0x00001847
		internal static void RemoveMental(Pawn pawn)
		{
			if (pawn.InMentalState)
			{
				MentalState mentalState = pawn.MentalState;
				if (mentalState == null)
				{
					return;
				}
				mentalState.RecoverFromState();
			}
		}

		// Token: 0x06000049 RID: 73 RVA: 0x00003664 File Offset: 0x00001864
		public static void DoSecSpecialEffects(Thing t, float radius)
		{
			MoteMaker.MakeStaticMote(t.Position, t.Map, ThingDefOf.Mote_ExplosionFlash, radius * 6f);
			for (int f = 1; f <= 4; f++)
			{
				MoteMaker.ThrowSmoke(t.Position.ToVector3Shifted() + Gen.RandomHorizontalVector(radius * 0.7f), t.Map, radius * 0.6f);
			}
			SoundDef StunSound = DefDatabase<SoundDef>.GetNamed("Explosion_EMP", false);
			SoundInfo StunInfo = new TargetInfo(t.Position, t.Map, false);
			if (StunSound != null)
			{
				StunSound.PlayOneShot(StunInfo);
			}
		}

		// Token: 0x04000023 RID: 35
		internal static string StunHed = "HED_RPSecStun";

		// Token: 0x04000024 RID: 36
		internal static float StunSev = 1f;
	}
}
