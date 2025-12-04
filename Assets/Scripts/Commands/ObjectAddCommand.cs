namespace Stickman3D
{
    // This command is simplified a lot since objects cannot have keyframes at the moment of creation.
    // Therefore, we can assume that the keyframe list will always be empty.
    public class ObjectAddCommand : ICommand
    {
        private readonly TimelineController timelineController;
        private readonly string objectName;
        private readonly string resourcePath;

        public ObjectAddCommand(TimelineController timelineController, string objectName, string resourcePath)
        {
            this.timelineController = timelineController;
            this.objectName = objectName;
            this.resourcePath = resourcePath;
        }

        public void Do()
        {
            // Create the object in the timeline controller and assign a default keyframe list
            timelineController.CreateObject(objectName, resourcePath);
            timelineController.LoadedAnimation.KeyframeMap[objectName] = new();
        }

        public void Undo() => timelineController.DeleteObject(objectName);
    }
}
