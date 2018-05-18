using UnityEngine;
using Valve.VR.InteractionSystem;
namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Dart : MonoBehaviour
    {
        [SerializeField] Throwable throwable;
        public ViveSR_Experience_DartGeneratorMgr dartGeneratorMgr;

        void Update()
        {
            if (transform.position.y < -1) Destroy(gameObject);
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.name.Contains("Model_cld") && !dartGeneratorMgr.dartGenerators[(int)dartGeneratorMgr.dartPlacementMode].isHolding)
            {    
                if (throwable != null) Destroy(throwable);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name.Contains("PlaneMeshCollider") && !dartGeneratorMgr.dartGenerators[(int)dartGeneratorMgr.dartPlacementMode].isHolding)
            {
                if (gameObject.name.Contains("Dart_dart"))
                {
                    Rigidbody rigid = GetComponent<Rigidbody>();
                    rigid.useGravity = false;
                    rigid.isKinematic = true;
                    if(throwable != null) Destroy(throwable);
                }
            }
        }
    }
}
