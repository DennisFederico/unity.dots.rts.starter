using Unity.Entities;

namespace rts.systems {
    
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.ThinClientSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(FixedStepSimulationSystemGroup))] //update after the physics system
    
    public partial class TargetingSystemGroup : ComponentSystemGroup {
        public TargetingSystemGroup() {
            SetRateManagerCreateAllocator(new RateUtils.FixedRateSimpleManager(1.5f));
        }
    }
}