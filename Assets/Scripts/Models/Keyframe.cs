using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Stickman3D
{
    [JsonObject(MemberSerialization.OptOut)]
    public struct Keyframe : IComparable<Keyframe>, IEquatable<Keyframe>
    {
        /// <summary>
        /// Keyframe time, in seconds.
        /// </summary>
        public float Time { get; set; }

        /// <summary>
        /// Transformation matrix of the object at this keyframe, in local space (relative to its parent - *NOT* localToWorldMatrix).
        /// </summary>
        public Matrix4x4 Transform { get; set; }

        // Implements IEquatable<Keyframe>
        public readonly bool Equals(Keyframe other) => CompareTo(other) == 0;

        // Implements IComparable<Keyframe>
        // Compares keyframes based on their Time property. Used for sorting and enforcing uniqueness (Times must be unique)
        public readonly int CompareTo(Keyframe other) => Time.CompareTo(other.Time);
    }
}
