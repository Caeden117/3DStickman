using System.Collections.Generic;
using UnityEngine;

namespace Stickman3D
{
    public class HistoryController : MonoBehaviour
    {
        private readonly List<ICommand> commandList = new();
        private int currentCommandIndex = -1;

        /// <summary>
        /// Adds a command into history and executes it.
        /// </summary>
        public void ExecuteCommand(ICommand command)
        {
            // If we are not at the end of the command list, remove all commands after the current index
            if (currentCommandIndex < commandList.Count - 1)
            {
                commandList.RemoveRange(currentCommandIndex + 1, commandList.Count - currentCommandIndex - 1);
            }

            // Execute the new command and add it to the list
            command.Do();
            commandList.Add(command);
            currentCommandIndex++;
        }

        /// <summary>
        /// Undoes the last executed command.
        /// </summary>
        public void Undo()
        {
            if (currentCommandIndex >= 0)
            {
                commandList[currentCommandIndex].Undo();
                currentCommandIndex--;
            }
        }

        /// <summary>
        /// Redoes the last undone command.
        /// </summary>
        public void Redo()
        {
            if (currentCommandIndex < commandList.Count - 1)
            {
                currentCommandIndex++;
                commandList[currentCommandIndex].Do();
            }
        }

        /// <summary>
        /// Clears command history. Used when switching between animations.
        /// </summary>
        public void Clear()
        {
            commandList.Clear();
            currentCommandIndex = -1;
        }

        private void Update()
        {
            // Early return - control key must be held down
            if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
            {
                return;
            }

            // Getting shift state because some programs support Ctrl+Shift+Z/Y
            var shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            // Z+Y key states, using GetKeyDown to avoid multiple triggers on hold
            var zHeld = Input.GetKeyDown(KeyCode.Z);
            var yHeld = Input.GetKeyDown(KeyCode.Y);

            // Supporting both Ctrl+Z / Ctrl+Shift+Y for Undo
            if ((!shiftHeld && zHeld) || (shiftHeld && yHeld))
            {
                Undo();
            }
            // Supporting both Ctrl+Y / Ctrl+Shift+Z for Redo
            else if ((!shiftHeld && yHeld) || (shiftHeld && zHeld))
            {
                Redo();
            }
        }
    }
}
