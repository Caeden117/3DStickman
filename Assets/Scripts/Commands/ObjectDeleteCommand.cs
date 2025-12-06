using System.Collections.Generic;

namespace Stickman3D
{
    public class ObjectDeleteCommand : ICommand
    {
        private readonly TimelineController timelineController;
        private readonly string objectName;
        private readonly string resourcePath;
        private readonly List<Keyframe> keyframes;

        public ObjectDeleteCommand(TimelineController timelineController, string objectName, string resourcePath, List<Keyframe> keyframes)
        {
            this.timelineController = timelineController;
            this.objectName = objectName;
            this.resourcePath = resourcePath;
            this.keyframes = keyframes;
        }

        public void Do() => timelineController.DeleteObject(objectName);

        public void Undo()
        {
            timelineController.CreateObject(objectName, resourcePath);
            timelineController.LoadedAnimation.KeyframeMap[objectName] = keyframes;
        }
    }
}
