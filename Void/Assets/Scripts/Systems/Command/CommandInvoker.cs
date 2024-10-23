using System.Collections.Generic;

public class CommandInvoker
{
    public readonly Stack<ICommand> CommandHistory = new Stack<ICommand>();
    public readonly Stack<ICommand> RedoStack = new Stack<ICommand>();

    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        CommandHistory.Push(command);
        RedoStack.Clear();
    }

    public void Undo()
    {
        if (CommandHistory.Count > 0)
        {
            ICommand command = CommandHistory.Pop();
            command.Undo();
            RedoStack.Push(command);
        }
    }

    public void Redo()
    {
        if (RedoStack.Count > 0)
        {
            ICommand command = RedoStack.Pop();
            command.Execute();
            CommandHistory.Push(command);
        }
    }
}