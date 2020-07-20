using UnityEngine;

namespace com.jlabarca.cpattern.Core
{
    public class Executor : MonoBehaviour
    {
        public State state;
        public float executionPeriod;
        private float _elapsed;
        void Start()
        {
            state = new State();
        }

        void Update()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed > executionPeriod)
            {
                state.ExecuteNextCommand();
            }
        }
    }
}
