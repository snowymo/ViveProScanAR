using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_DartCollision : MonoBehaviour
    {
        [SerializeField] Rigidbody rigid;

        private void OnTriggerEnter(Collider other)
        {
            if (ViveSR_Experience.targetHandScript.AttachedObjects.Count == 1)
            {
                if (other.gameObject.name.Contains("PlaneMeshCollider"))
                {            
                    FreezeDart();
                }
                else if (other.gameObject.name.Contains("ViveDeer"))
                {
                    gameObject.transform.parent = other.gameObject.transform;

                    FreezeDart();
                }
            }
        }

        void FreezeDart()
        {
            rigid.isKinematic = true;
            Destroy(this);
        }
    }
}