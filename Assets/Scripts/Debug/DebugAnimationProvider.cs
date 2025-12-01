using UnityEngine;

namespace Stickman3D
{
    public class DebugAnimationProvider : MonoBehaviour
    {
        private static readonly Animation debugAnimation = new()
        {
            ObjectMap = new()
            {
                { "Cube", "Cube" }
            },
            KeyframeMap = new()
            {
                {
                    "Cube", new()
                    {
                        new()
                        {
                            Time = 0,
                            Transform = Matrix4x4.identity,
                        },
                        new()
                        {
                            Time = 1,
                            Transform = Matrix4x4.TRS(2 * Vector3.up, Quaternion.identity, Vector3.one)
                        },
                        new()
                        {
                            Time = 2,
                            Transform = Matrix4x4.TRS(2 * Vector2.up, Quaternion.Euler(0, 0, 90), Vector3.one)
                        },
                        new()
                        {
                            Time = 3,
                            Transform = Matrix4x4.TRS(new(0, 2, 3), Quaternion.Euler(0, 0, 90), Vector3.one)
                        },
                        new()
                        {
                            Time = 4,
                            Transform = Matrix4x4.TRS(new(0, 2, 3), Quaternion.Euler(0, 0, 90), 2f * Vector3.one)
                        },
                        new()
                        {
                            Time = 5,
                            Transform = Matrix4x4.identity
                        }
                    }
                },
                {
                    "Animation Camera", new()
                    {
                        new()
                        {
                            Time = 0,
                            Transform = Matrix4x4.TRS(new(3, 3, -4), Quaternion.Euler(30, -30, 0), Vector3.one)
                        },
                        new()
                        {
                            Time = 5,
                            Transform = Matrix4x4.TRS(new(4, 3, 3), Quaternion.Euler(30, -120, 0), Vector3.one)
                        }
                    }
                },
                {
                    "Point Light", new()
                }
            }
        };

        private TimelineController timeline;

        private void Start()
        {
            debugAnimation.ResetLength();

            if (TryGetComponent(out timeline))
            {
                timeline.LoadAnimation(debugAnimation);
                //timeline.IsPlaying = true;
            }
        }
    }
}
