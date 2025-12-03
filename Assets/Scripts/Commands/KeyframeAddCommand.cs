namespace Stickman3D
{
    public class KeyframeAddCommand : ICommand
    {
        private readonly Animation animation;
        private readonly KeyframeEditorController keyframeEditorController;
        private readonly string keyframePath;
        private readonly Keyframe newKeyframe;

        public KeyframeAddCommand(Animation animation, KeyframeEditorController keyframeEditorController, string keyframePath, Keyframe newKeyframe)
        {
            this.animation = animation;
            this.keyframeEditorController = keyframeEditorController;
            this.keyframePath = keyframePath;
            this.newKeyframe = newKeyframe;
        }

        public void Do() => animation.InsertKeyframe(keyframePath, newKeyframe);

        public void Undo()
        {
            animation.RemoveKeyframe(keyframePath, newKeyframe);

            // Clear the current keyframe in the editor if it was the one that was just removed
            if (keyframeEditorController.GetCurrentPath() == keyframePath && keyframeEditorController.GetCurrentKeyframe() == newKeyframe)
            {
                keyframeEditorController.SetCurrent(null, default);
            }
        }
    }
}
