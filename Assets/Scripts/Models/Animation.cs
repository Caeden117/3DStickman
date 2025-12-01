using Newtonsoft.Json;
using UnityEngine;
using System.Collections.Generic;

namespace Stickman3D
{
    [JsonObject(MemberSerialization.OptOut)]
    public class Animation
    {
        /// <summary>
        /// Length of animation in seconds.
        /// </summary>
        public float Length { get; set; } = 1.0f;

        /// <summary>
        /// Map of object name to a Resource path, pointing to a prefab to instantiate.
        /// </summary>
        public Dictionary<string, string> ObjectMap { get; set; }

        /// <summary>
        /// Map of SceneNode path to the list of Keyframes affecting that object.
        /// </summary>
        public Dictionary<string, List<Keyframe>> KeyframeMap { get; set; }

        public bool InsertKeyframe(string path, in Keyframe keyframe)
        {
            if (!KeyframeMap.ContainsKey(path))
            {
                Debug.Log("Could not insert keyframe: path \'" + path + "\' does not exist!");
                return false;
            }

            var index = KeyframeMap[path].BinarySearch(keyframe);

            // Insert new or overwrite existing keyframe at the timestamp.
            if (index < 0)
            {
                index = ~index;
                KeyframeMap[path].Insert(index, keyframe);
            }
            else
            {
                KeyframeMap[path][index] = keyframe;
            }

            return true;
        }

        public bool RemoveKeyframe(string path, in Keyframe keyframe)
        {
            if (!KeyframeMap.ContainsKey(path))
            {
                Debug.Log("Could not remove keyframe: path \'" + path + "\' does not exist!");
                return false;
            }

            var index = KeyframeMap[path].BinarySearch(keyframe);

            if (index < 0)
            {
                Debug.Log("Could not remove keyframe: keyframe not found!");
                return false;
            }

            KeyframeMap[path].RemoveAt(index);
            return true;
        }

        public bool MoveKeyframe(string path, ref Keyframe keyframe, float newTime)
        {
            if (!RemoveKeyframe(path, keyframe))
            {
                return false;
            }

            keyframe.Time = newTime;

            return InsertKeyframe(path, keyframe);
        }
    }
}
