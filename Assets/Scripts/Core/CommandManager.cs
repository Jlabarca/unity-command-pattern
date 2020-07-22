using System.Collections.Generic;
using com.jlabarca.cpattern.Core.Commands;
using UnityEngine;

namespace com.jlabarca.cpattern.Core
{
    public class CommandManager : MonoBehaviour
    {
        internal static CommandManager Instance;
        private int _index;

        public State state;
        public float executionPeriod;
        private float _elapsed;

        void Awake(){

            if (Instance == null){
                Instance = this;
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
            _elapsed += Time.deltaTime;

            if (!(_elapsed > executionPeriod)) return;

            state.ExecuteNextCommand();
            _elapsed = 0;
        }

        public void AddCommand(ICommand command)
        {
            Debug.Log("AddCommand "+command.GetType().Name);
            state.commands.Add(command);
        }
    }
}
