using Unity.Entities;
using UnityEngine;

namespace rts.systems {
    
    [DisableAutoCreation]
    [UpdateInGroup(typeof(MyVariableSystemGroup), OrderLast = true)]
    public partial struct VariableRateSystem : ISystem {
    
        private int _ticks;
    
        public void OnUpdate(ref SystemState state) {
            _ticks++;
            
            var currentGroup = state.World.GetExistingSystemManaged<MyVariableSystemGroup>();
            // Change every 5 ticks
            if (_ticks % 5 == 0) {
                currentGroup.RateInSeconds += 1f;
            }
            Debug.Log($"ticks: {_ticks} - rate: {currentGroup.RateInSeconds} - elapsed: {SystemAPI.Time.ElapsedTime} - delta: {SystemAPI.Time.DeltaTime}");
        }
    }
}