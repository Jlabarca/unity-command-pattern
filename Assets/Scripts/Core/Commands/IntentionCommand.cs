using com.jlabarca.cpattern.Enums;

namespace com.jlabarca.cpattern.Core.Commands
{
    public class IntentionCommand : ICommand
    {
        private Actor _actor;
        private Intention _intention;

        public IntentionCommand(Actor actor, Intention intention)
        {
            _actor = actor;
            _intention = intention;
        }

        public void Execute()
        {
            _actor.SetIntention(_intention);
        }
    }

    class IntentionCommandImpl : IntentionCommand
    {
    }
}
