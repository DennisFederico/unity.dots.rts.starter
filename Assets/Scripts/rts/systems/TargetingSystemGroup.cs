using Unity.Entities;
using Unity.Transforms;

namespace rts.systems {
    
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.ThinClientSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TransformSystemGroup))] //update after the physics system
    
    public partial class TargetingSystemGroup : ComponentSystemGroup {
        public TargetingSystemGroup() {
            SetRateManagerCreateAllocator(new RateUtils.FixedRateCatchUpManager(1.5f));
        }
    }
}