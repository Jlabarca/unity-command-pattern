using System;
using System.Collections.Generic;
using com.jlabarca.cpattern.Core.Commands;
using UnityEngine;

namespace com.jlabarca.cpattern.Core
{
    public class CommandManager : MonoBehaviour
    {
        internal static CommandManager instance;
        private int _index;
        private float _elapsed;

        public State state;
        public float executionPeriod;
        public List<MonoBehaviour> tickables;

        public int CommandsCount => state.commands.Count;

        void Awake(){

            if (instance == null){
                instance = this;
                DontDestroyOnLoad(gameObject);
            } else {
                Destroy(this);
            }
        }

        private void Start()
        {
            state = new State
            {
                commands = new List<ICommand>()
            };
        }

        private void FixedUpdate()
        {
            if (executionPeriod > 0)
            {
                _elapsed += Time.timeScale * Time.deltaTime;

                if (!(_elapsed > executionPeriod)) return;
            }

            state.ExecuteNextCommand();
            _elapsed = 0;

            foreach (var tickable in tickables)
            {
                tickable.SendMessage("Tick");
            }
        }

        public void AddCommand(ICommand command)
        {
            //Debug.Log("AddCommand "+command.GetType().Name);
            state.commands.Add(command);
        }
    }
}
