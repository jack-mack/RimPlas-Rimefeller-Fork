using RimWorld;
using Verse;

namespace RimPlas
{
    // Token: 0x0200000E RID: 14
    public class CompRPHospBed : ThingComp
    {
        // Token: 0x17000008 RID: 8
        // (get) Token: 0x06000030 RID: 48 RVA: 0x00003024 File Offset: 0x00001224
        public CompProperties_RPHospBed Props => (CompProperties_RPHospBed) props;

        // Token: 0x06000031 RID: 49 RVA: 0x00003034 File Offset: 0x00001234
        public override void CompTick()
        {
            base.CompTick();
            Thing bed = parent;
            var CPT = bed.TryGetComp<CompPowerTrader>();
            if (CPT == null || !CPT.PowerOn || bed.IsBrokenDown())
            {
                return;
            }

            var Occupant = OccupiedBy(bed);
            if (Occupant == null)
            {
                return;
            }

            if (Controller.Settings.AllowPainCtrlHB && Props.PainCtrlTicks > 0 &&
                bed.IsHashIntervalTick(Props.PainCtrlTicks))
            {
                var patient = OccupiedByPawnInPain(Occupant);
                if (patient != null)
                {
                    var PainCtrl = HediffDef.Named(Props.PainCtrlHediff);
                    var PainCtrlSev = Props.PainCtrlSev;
                    if (!RPHediffEffecter.HasHediff(patient, PainCtrl))
                    {
                        RPHediffEffecter.HediffEffect(PainCtrl, PainCtrlSev, patient, null, out _);
                    }
                }
            }

            if (!bed.IsHashIntervalTick(625))
            {
                return;
            }

            if (Controller.Settings.AllowMentalCtrlHB)
            {
                var patient2 = OccupiedByPawnWithMentalState(Occupant);
                patient2?.mindState.mentalStateHandler.Reset();
            }

            if (!Controller.Settings.AllowRecCtrlHB)
            {
                return;
            }

            var patient3 = OccupiedByPawnWithRec(Occupant);
            if (patient3 != null)
            {
                patient3.needs.joy.CurLevel += Props.RecPerHour / 4f;
            }
        }

        // Token: 0x06000032 RID: 50 RVA: 0x0000316A File Offset: 0x0000136A

        // Token: 0x06000033 RID: 51 RVA: 0x00003174 File Offset: 0x00001374
        private Pawn OccupiedBy(Thing bed)
        {
            Pawn p = null;
            if (bed != null && bed.Spawned && bed is Building_Bed buildingBed)
            {
                p = buildingBed.GetCurOccupant(0);
            }

            return p;
        }

        // Token: 0x06000034 RID: 52 RVA: 0x000031A4 File Offset: 0x000013A4
        private Pawn OccupiedByPawnWithMentalState(Pawn p)
        {
            if (p != null && !p.mindState.mentalStateHandler.InMentalState)
            {
                p = null;
            }

            return p;
        }

        // Token: 0x06000035 RID: 53 RVA: 0x000031BF File Offset: 0x000013BF
        private Pawn OccupiedByPawnWithRec(Pawn p)
        {
            if (p == null)
            {
                return null;
            }

            var needs = p.needs;

            if (needs?.joy != null)
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

            return p;
        }

        // Token: 0x06000036 RID: 54 RVA: 0x000031FE File Offset: 0x000013FE
        private Pawn OccupiedByPawnInPain(Pawn p)
        {
            if (p != null && p.health.hediffSet.PainTotal <= 0.1f)
            {
                p = null;
            }

            return p;
        }
    }
}