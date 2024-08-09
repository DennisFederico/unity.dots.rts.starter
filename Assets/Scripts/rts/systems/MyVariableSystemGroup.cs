using Unity.Entities;

namespace rts.systems {

    [DisableAutoCreation]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial class MyVariableSystemGroup : ComponentSystemGroup {

        private float _rateInSeconds = 2f;

        public float RateInSeconds {
            get => _rateInSeconds;
            set {
                _rateInSeconds = value;
                ChangeUpdateRate(value);
            }
        }

        public MyVariableSystemGroup() {
            SetRateManagerCreateAllocator(new RateUtils.FixedRateCatchUpManager(RateInSeconds));
        }

        private void ChangeUpdateRate(float seconds) {
            RateManager.Timestep = seconds; 
        }
    }
}