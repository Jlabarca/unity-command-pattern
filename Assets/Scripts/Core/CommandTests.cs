using System.Collections;
using com.jlabarca.cpattern.Core.Commands;
using com.jlabarca.cpattern.Enums;
using UnityEngine;

namespace com.jlabarca.cpattern.Core
{
    public class CommandTests : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(nameof(TestRoutine));
        }

        private IEnumerator TestRoutine()
        {
            yield return new WaitForSeconds(1);
            foreach (var actor in FarmerManager.instance.farmers)
            {
                StartCoroutine(nameof(SomeCommands), actor);
            }
        }

        private IEnumerator SomeCommands(Actor actor)
        {
            //Debug.Log(actor+" "+actor.id);
            //CommandManager.Instance.AddCommand(new IntentionCommand(actor, Intention.TillGround));
            yield return new WaitForSeconds(2/Time.timeScale);
            CommandManager.instance.AddCommand(new IntentionCommand(actor, Intention.TillGround));
            yield return new WaitForSeconds(5/Time.timeScale);
            CommandManager.instance.AddCommand(new IntentionCommand(actor, Intention.TillGround));
            yield return new WaitForSeconds(5/Time.timeScale);
            CommandManager.instance.AddCommand(new IntentionCommand(actor, Intention.TillGround));
            // yield return new WaitForSeconds(2/Time.timeScale);
            // CommandManager.Instance.AddCommand(new IntentionCommand(actor, Intention.PlantSeeds));
            // yield return new WaitForSeconds(2/Time.timeScale);
            // CommandManager.Instance.AddCommand(new IntentionCommand(actor, Intention.SellPlants));
            // yield return new WaitForSeconds(2/Time.timeScale);
            // CommandManager.Instance.AddCommand(new IntentionCommand(actor, Intention.SmashRocks));
        }
    }
}
