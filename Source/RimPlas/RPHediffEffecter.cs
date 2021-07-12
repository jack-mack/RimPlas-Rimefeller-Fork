using Verse;

namespace RimPlas
{
    // Token: 0x0200001E RID: 30
    internal class RPHediffEffecter
    {
        // Token: 0x06000077 RID: 119 RVA: 0x00004568 File Offset: 0x00002768
        internal static void HediffEffect(HediffDef hediffdef, float SeverityToApply, Pawn pawn, BodyPartRecord part,
            out bool immune)
        {
            immune = false;
            if (pawn.RaceProps.IsMechanoid || hediffdef == null)
            {
                return;
            }

            if (!ImmuneTo(pawn, hediffdef))
            {
                if (pawn.health.WouldDieAfterAddingHediff(hediffdef, part, SeverityToApply))
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
                    hediff = hediffSet?.GetFirstHediffOfDef(hediffdef);
                }

                var hashediff = hediff;
                if (hashediff != null)
                {
                    hashediff.Severity += SeverityToApply;
                    return;
                }

                var addhediff = HediffMaker.MakeHediff(hediffdef, pawn, part);
                addhediff.Severity = SeverityToApply;
                pawn.health.AddHediff(addhediff, part);
            }
            else
            {
                immune = true;
            }
        }

        // Token: 0x06000078 RID: 120 RVA: 0x00004604 File Offset: 0x00002804
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

        // Token: 0x06000079 RID: 121 RVA: 0x00004674 File Offset: 0x00002874
        internal static bool HasHediff(Pawn pawn, HediffDef def)
        {
            var health = pawn.health;
            var HS = health?.hediffSet;
            return HS?.GetFirstHediffOfDef(def) != null;
        }
    }
}