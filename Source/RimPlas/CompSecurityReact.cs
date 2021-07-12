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
        public CompProperties_SecurityReact Props => (CompProperties_SecurityReact) props;

        // Token: 0x0600003C RID: 60 RVA: 0x00003274 File Offset: 0x00001474
        public override void CompTick()
        {
            if (Find.TickManager.TicksGame % 60 != 0 || IsDisabled(parent))
            {
                return;
            }

            var target = MentalDetect(parent, Props.radius);
            if (target == null)
            {
                return;
            }

            var thingWithComps = parent;
            var radius = Props.radius;
            var target2 = target;
            var compPropertiesSecurityReact = Props;
            DoStunner(thingWithComps, radius, target2, compPropertiesSecurityReact?.SecurityHediff,
                Props.SecuritySeverity);
        }

        // Token: 0x0600003D RID: 61 RVA: 0x000032EC File Offset: 0x000014EC
        public static void DoStunner(ThingWithComps securityThing, float radius, Thing target, string SecHed,
            float SecSev)
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

            var SeverityToApply = Globals.StunSev;
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
            var DetectThings = GenRadial
                .RadialDistinctThingsAround(securityThing.Position, securityThing.Map, radius, true).ToList();
            if (DetectThings.Count <= 0)
            {
                return null;
            }

            foreach (var mentalDetect in DetectThings)
            {
                if (mentalDetect is not Pawn || (mentalDetect as Pawn).IsBurning() || (mentalDetect as Pawn).Dead ||
                    (mentalDetect as Pawn).Downed || !(mentalDetect as Pawn).Awake())
                {
                    continue;
                }

                if ((mentalDetect as Pawn).IsPrisoner &&
                    PrisonBreakUtility.IsPrisonBreaking(mentalDetect as Pawn))
                {
                    return mentalDetect;
                }

                if ((mentalDetect as Pawn).IsColonist && (mentalDetect as Pawn).InMentalState)
                {
                    return mentalDetect;
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

            var compFlickable = checkThing.TryGetComp<CompFlickable>();
            if (compFlickable != null && !compFlickable.SwitchIsOn)
            {
                return true;
            }

            var compPowerTrader = checkThing.TryGetComp<CompPowerTrader>();
            if (compPowerTrader != null && !compPowerTrader.PowerOn)
            {
                return true;
            }

            var compRefuelable = checkThing.TryGetComp<CompRefuelable>();
            return compRefuelable != null && !compRefuelable.HasFuel;
        }
    }
}