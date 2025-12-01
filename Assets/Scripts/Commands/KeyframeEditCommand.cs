namespace Stickman3D
{
    public class KeyframeEditCommand : ICommand
    {
        private readonly Animation animation;
        private readonly KeyframeEditorController keyframeEditorController;
        private readonly string keyframePath;
        private readonly Keyframe oldKeyframe;
        private readonly Keyframe newKeyframe;

        public KeyframeEditCommand(Animation animation, KeyframeEditorController keyframeEditorController, string keyframePath, Keyframe oldKeyframe, Keyframe newKeyframe)
        {
            this.animation = animation;
            this.keyframeEditorController = keyframeEditorController;
            this.keyframePath = keyframePath;
            this.oldKeyframe = oldKeyframe;
            this.newKeyframe = newKeyframe;
        }

        public void Do()
        {
            animation.RemoveKeyframe(keyframePath, oldKeyframe);
            animation.InsertKeyframe(keyframePath, newKeyframe);

            if (keyframeEditorController.GetCurrentPath() == keyframePath && keyframeEditorController.GetCurrentKeyframe() == oldKeyframe)
            {
                keyframeEditorController.SetCurrent(keyframePath, newKeyframe);
            }
        }

        public void Undo()
        {
            animation.RemoveKeyframe(keyframePath, newKeyframe);
            animation.InsertKeyframe(keyframePath, oldKeyframe);

            if (keyframeEditorController.GetCurrentPath() == keyframePath && keyframeEditorController.GetCurrentKeyframe() == newKeyframe)
            {
                keyframeEditorController.SetCurrent(keyframePath, oldKeyframe);
            }
        }
    }
}
