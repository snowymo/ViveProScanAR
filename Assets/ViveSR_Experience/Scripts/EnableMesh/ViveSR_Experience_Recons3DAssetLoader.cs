using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Recons3DAssetLoader : MonoBehaviour
    {
        public bool isMeshReady;
        public bool isColliderReady;
        public MeshRenderer[] meshRnds;
        public MeshRenderer[] cldRnds;
        private void LoadMeshDoneCallBack(GameObject go)
        {
            meshRnds = go.GetComponentsInChildren<MeshRenderer>();
            int numRnds = meshRnds.Length;
            for (int id = 0; id < numRnds; ++id)
            {
                meshRnds[id].sharedMaterial.shader = Shader.Find("ViveSR/MeshCuller, Shadowed, Stencil");
                meshRnds[id].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            isMeshReady = true;
        }

        private void LoadColliderDoneCallBack(GameObject go)
        {           
            if (ViveSR_StaticColliderPool.ProcessDataAndGenColliderInfo(go) == true)
            {
                ViveSR_StaticColliderPool cldPool = go.AddComponent<ViveSR_StaticColliderPool>();
                Rigidbody rigid = go.AddComponent<Rigidbody>();
                rigid.isKinematic = true;
                rigid.useGravity = false;

                cldPool.OrganizeHierarchy();

                cldRnds = go.GetComponentsInChildren<MeshRenderer>(true);
            }
            isColliderReady = true;
        }

        public GameObject LoadMeshObj(string path)
        {
            isMeshReady = false;
            return OBJLoader.LoadOBJFile(path, LoadMeshDoneCallBack);
        }

        public GameObject LoadColliderObj(string path)
        {
            isColliderReady = false;
            return OBJLoader.LoadOBJFile(path, LoadColliderDoneCallBack);
        }   
    }
}