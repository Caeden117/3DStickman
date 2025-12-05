using UnityEngine;

namespace Stickman3D
{
    public class ActivateObjectOnTrigger : MonoBehaviour
    {
        [SerializeField] private GameObject[] objects;

        private int activeTriggers = 0;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer != gameObject.layer) return;

            activeTriggers++;

            if (activeTriggers > 1) return;

            foreach (var obj in objects)
            {
                obj.SetActive(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer != gameObject.layer) return;

            activeTriggers--;

            if (activeTriggers > 0) return;

            foreach (var obj in objects)
            {
                obj.SetActive(false);
            }
        }
    }
}
