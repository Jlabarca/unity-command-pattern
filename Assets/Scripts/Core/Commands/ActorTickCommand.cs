namespace com.jlabarca.cpattern.Core.Commands
{
    public class ActorTickCommand : ICommand
    {
        private Actor _actor;
        private float _smooth;

        public ActorTickCommand(Actor actor, float smooth)
        {
            _actor = actor;
            _smooth = smooth;
        }

        public void Execute()
        {
            _actor.Tick(_smooth);
        }
    }
}
