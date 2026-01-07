using Verse;
using RimWorld;

namespace RimPiece.Components
{
    public class CompHaki: ThingComp
    {
        private float _armamentXp;
        private int _armamentLevel;
        private float _observationXp;
        private int _observationLevel;
        private bool _hasConquerors;

        public int ArmamentLevel => _armamentLevel;
        public float ArmamentXp => _armamentXp;
        public float ArmamentMaxXp => (_armamentLevel + 1) * 1000f;
        public int ObservationLevel => _observationLevel;
        public float ObservationXp => _observationXp;
        public float ObservationMaxXp => (_observationLevel + 1) * 1000f;

        public bool HasConquerors => _hasConquerors;

        public void AddArmamentXp(float amount)
        {
            if (amount <= 0) return;

            _armamentXp += amount;
    
            if (_armamentXp >= ArmamentMaxXp)
            {
                _armamentXp -= ArmamentMaxXp;
                _armamentLevel++;
        
                if (parent is Pawn p)
                {
                    Messages.Message(
                        $"{p.LabelShort} reached Armament Haki level {_armamentLevel}!",
                        p,
                        MessageTypeDefOf.PositiveEvent,
                        historical: false
                    );
                }
            }
        }

        public void AddObservationXp(float amount)
        {
            if (amount <= 0) return;
            
            _observationXp += amount;

            if (_observationXp >= ObservationMaxXp)
            {
                _observationXp -= ObservationMaxXp;
                _observationLevel++;
                
                if (parent is Pawn p)
                {
                    Messages.Message(
                        $"{p.LabelShort} reached Observation Haki level {_armamentLevel}!",
                        p,
                        MessageTypeDefOf.PositiveEvent,
                        historical: false
                    );
                }
            }
        }
        
        public void UnlockConquerors()
        {
            _hasConquerors = true;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref _armamentXp, "armamentXP", 0f);
            Scribe_Values.Look(ref _armamentLevel, "armamentLevel", 0);
            Scribe_Values.Look(ref _observationXp, "observationXP", 0f);
            Scribe_Values.Look(ref _observationLevel, "observationLevel", 0);
            Scribe_Values.Look(ref _hasConquerors, "hasConquerors", false);
        }

        // TODO - Remove latter
        // debug values
        public override string CompInspectStringExtra()
        {
            if (!Prefs.DevMode) return null;
            
            var conquerorsState = _hasConquerors ? "Awakened" : "Dormant";
            return $"[Debug] Armament: Lv{_armamentLevel} | Observation: Lv{_observationLevel} | Conquerors: {conquerorsState}";
        }
    }
}