namespace Stickman3D
{
    /// <summary>
    /// Interface for any undo/redo command.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        void Do();

        /// <summary>
        /// Reverts the command.
        /// </summary>
        void Undo();
    }
}
