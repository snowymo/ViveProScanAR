using System.Collections.Generic;
using UnityEngine;

public class ViveSR_Experience_DeerColorMgr : MonoBehaviour {

    //store materials
    public List<Material> deerMaterials;
    [SerializeField] Texture deerTexture;
    [SerializeField, Range(1, 20)] int deerHueVariation = 5;

    //store scale
    [SerializeField] public List<float> deerScale;

    private static ViveSR_Experience_DeerColorMgr _instance;
    public static ViveSR_Experience_DeerColorMgr instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ViveSR_Experience_DeerColorMgr>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        for (int i = 0; i < deerHueVariation + 1; i++)
        {
            Material newMat = new Material(Shader.Find("ViveSR_Experience/viveDeerShader"));
            newMat.SetFloat("_Hue", Random.Range(0f, 1f));
            newMat.mainTexture = deerTexture;
            deerMaterials.Add(newMat);
        }
    }                 
}
