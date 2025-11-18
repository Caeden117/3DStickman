using Newtonsoft.Json;
using System.Collections.Generic;

namespace Stickman3D
{
    [JsonObject(MemberSerialization.OptOut)]
    public class Animation
    {
        /// <summary>
        /// Map of SceneNode path to a Resource path, pointing to a prefab to instantiate.
        /// </summary>
        public Dictionary<string, string> ObjectMap { get; set; }

        /// <summary>
        /// Map of SceneNode path to the list of Keyframes affecting that object.
        /// </summary>
        public Dictionary<string, List<Keyframe>> KeyframeMap { get; set; }
    }
}
