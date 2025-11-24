using System.Collections.Generic;
using UnityEngine;

namespace Stickman3D
{
    /// <summary>
    /// Core controller for the timeline editor.
    /// </summary>
    public class TimelineEditorController : MonoBehaviour
    {
        [SerializeField]
        private TimelineController timelineController;

        public SceneNode SelectedObj
        {
            get => selectedObj;
            set
            {
                selectedObj = value;
                selectedObjPath = selectedObj != null
                    ? timelineController.GetPathForNode(selectedObj)
                    : null;
                selectedObjKeyframes = selectedObj != null
                    ? timelineController.GetKeyframesForPath(selectedObjPath)
                    : null;
            }
        }

        private SceneNode selectedObj;
        private string selectedObjPath;
        private List<Keyframe> selectedObjKeyframes;

        public bool InsertKeyframe(Keyframe keyframe)
        {
            if (selectedObjKeyframes == null)
            {
                Debug.LogError("No object selected or selected object has no keyframe list.");
                return false;
            }

            var insertIndex = selectedObjKeyframes.BinarySearch(keyframe);

            if (insertIndex < 0)
            {
                // Keyframe does not already exist at this time, insert it
                insertIndex = ~insertIndex;
                selectedObjKeyframes.Insert(insertIndex, keyframe);
                return true;
            }
            else
            {
                // Keyframe already exists. Because duplicates arent allowed, overwrite
                selectedObjKeyframes[insertIndex] = keyframe;
                return true;
            }
        }

        public bool RemoveKeyframe(Keyframe keyframe)
        {
            if (selectedObjKeyframes == null)
            {
                Debug.LogError("No object selected or selected object has no keyframe list.");
                return false;
            }

            var removeIndex = selectedObjKeyframes.BinarySearch(keyframe);
            if (removeIndex >= 0)
            {
                // Keyframe found, remove it
                selectedObjKeyframes.RemoveAt(removeIndex);
                return true;
            }
            else
            {
                // Keyframe not found
                return false;
            }
        }
    }
}
