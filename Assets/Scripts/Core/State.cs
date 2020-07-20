using System.Collections.Generic;
using com.jlabarca.cpattern.Core.Commands;

namespace com.jlabarca.cpattern.Core
{
    public struct State
    {
        public List<ICommand> commands;
        public int index;

        public void ExecuteNextCommand()
        {
            if (commands.Count >= index) return;
            commands[index].Execute();
            index++;
        }
    }
}
