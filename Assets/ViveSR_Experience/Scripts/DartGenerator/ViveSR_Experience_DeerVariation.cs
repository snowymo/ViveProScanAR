using UnityEngine;
namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_DeerVariation : MonoBehaviour
    {
        ViveSR_Experience_DeerColorMgr deerMgr;
        // Use this for initialization
        private void Awake()
        {
            deerMgr = ViveSR_Experience_DeerColorMgr.instance;
        }

        void Start()
        {
            GetComponent<Renderer>().material = deerMgr.deerMaterials[Random.Range(0, deerMgr.deerMaterials.Count - 1)];
            int scale = Random.Range(0, deerMgr.deerScale.Count);
            transform.localScale = new Vector3(deerMgr.deerScale[scale], deerMgr.deerScale[scale], deerMgr.deerScale[scale]);
        }
    }
}