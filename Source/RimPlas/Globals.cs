using RimWorld;
using Verse;
using Verse.Sound;

namespace RimPlas
{
    // Token: 0x02000014 RID: 20
    internal static class Globals
    {
        // Token: 0x04000023 RID: 35
        internal static string StunHed = "HED_RPSecStun";

        // Token: 0x04000024 RID: 36
        internal static float StunSev = 1f;

        // Token: 0x06000046 RID: 70 RVA: 0x00003550 File Offset: 0x00001750
        internal static void HediffEffect(Pawn pawn, HediffDef stunHediffDef, float SeverityToApply)
        {
            if (pawn.RaceProps.IsMechanoid || SeverityToApply == 0f || stunHediffDef == null ||
                ImmuneTo(pawn, stunHediffDef))
            {
                return;
            }

            var health = pawn.health;
            Hediff hediff;
            if (health == null)
            {
                hediff = null;
            }
            else
            {
                var hediffSet = health.hediffSet;
                hediff = hediffSet?.GetFirstHediffOfDef(stunHediffDef);
            }

            var hashediff = hediff;
            if (hashediff != null)
            {
                hashediff.Severity += SeverityToApply;
                return;
            }

            var addhediff = HediffMaker.MakeHediff(stunHediffDef, pawn);
            addhediff.Severity = SeverityToApply;
            pawn.health.AddHediff(addhediff);
        }

        // Token: 0x06000047 RID: 71 RVA: 0x000035D8 File Offset: 0x000017D8
        internal static bool ImmuneTo(Pawn pawn, HediffDef def)
        {
            var hediffs = pawn.health.hediffSet.hediffs;
            foreach (var hediff in hediffs)
            {
                var curStage = hediff.CurStage;
                if (curStage?.makeImmuneTo == null)
                {
                    continue;
                }

                foreach (var hediffDef in curStage.makeImmuneTo)
                {
                    if (hediffDef == def)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // Token: 0x06000048 RID: 72 RVA: 0x00003647 File Offset: 0x00001847
        internal static void RemoveMental(Pawn pawn)
        {
            if (!pawn.InMentalState)
            {
                return;
            }

            var mentalState = pawn.MentalState;

            mentalState?.RecoverFromState();
        }

        // Token: 0x06000049 RID: 73 RVA: 0x00003664 File Offset: 0x00001864
        public static void DoSecSpecialEffects(Thing t, float radius)
        {
            FleckMaker.Static(t.Position, t.Map, FleckDefOf.ExplosionFlash, radius * 6f);
            for (var f = 1; f <= 4; f++)
            {
                FleckMaker.ThrowSmoke(t.Position.ToVector3Shifted() + Gen.RandomHorizontalVector(radius * 0.7f), t.Map,
                    radius * 0.6f);
            }

            var StunSound = DefDatabase<SoundDef>.GetNamed("Explosion_EMP", false);
            SoundInfo StunInfo = new TargetInfo(t.Position, t.Map);
            StunSound?.PlayOneShot(StunInfo);
        }
    }
}