using System;
using System.Collections.Generic;
using com.jlabarca.cpattern.Core.Commands;

namespace com.jlabarca.cpattern.Core
{
    [Serializable]
    public struct State
    {
        public List<ICommand> commands;
        public int index;

        public void ExecuteNextCommand()
        {
            if (index >= commands.Count) return;
            //Debug.Log("Executing "+commands[index].GetType().Name);
            commands[index].Execute();
            index++;
        }
    }
}
