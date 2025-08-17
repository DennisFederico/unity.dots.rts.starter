using System.Collections.Generic;
using UnityEngine;

namespace rts.scriptable {
    [CreateAssetMenu()]
    public class ResourceTypeListSO : ScriptableObject {
        public List<ResourceTypeSO> resourceTypeSOList;
    }
}