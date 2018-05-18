using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    public enum DartPlacementMode
    {
        Throwable,
        Raycast,
        Portal
    }

    public class ViveSR_Experience_DartGeneratorMgr : MonoBehaviour
    {
        [SerializeField] bool AllowSwitchingTool = true;
        [SerializeField] bool AutoEnable = true;

        public DartPlacementMode dartPlacementMode;
        public List<ViveSR_Experience_IDartGenerator> dartGenerators;

        [HideInInspector] public float coolDownTime = 0.2f, tempTime;

        private void Awake()
        {
            dartGenerators.Add(GetComponent<ViveSR_Experience_DartThrowGenerator>());
            
            if(GetComponent<ViveSR_Experience_DartRaycastGenerator>())
                dartGenerators.Add(GetComponent<ViveSR_Experience_DartRaycastGenerator>());
            //  dartGenerators.Add(GetComponent <ViveSR_Experience_DartPortalGenerator>());

            if(AutoEnable) dartGenerators[(int)dartPlacementMode].enabled = true;
        }

        public void SwitchPlacementMode()
        {
            if(AllowSwitchingTool)
            {
                ViveSR_Experience_IDartGenerator oldDartGenerator = dartGenerators[(int)(dartPlacementMode)];
                oldDartGenerator.TriggerRelease();

                GameObject lastObj = oldDartGenerator.InstantiatedDarts[oldDartGenerator.InstantiatedDarts.Count - 1];
                if (!lastObj.name.Contains("Drawer"))
                    Destroy(lastObj);
                else lastObj.GetComponent<ViveSR_Experience_IPortalDrawer>().FinishDrawing();                         
                dartGenerators[(int)(dartPlacementMode)].enabled = false;

                //switch to the other DartGenerator
                dartPlacementMode = (DartPlacementMode)(((int)dartPlacementMode + 1) % dartGenerators.Count);

                ViveSR_Experience_IDartGenerator newDartGenerator = dartGenerators[(int)dartPlacementMode];
                newDartGenerator.enabled = true;
                newDartGenerator.TriggerPress();
            }
        }

        public void DestroyObjs() 
        {
            foreach (ViveSR_Experience_IDartGenerator dartGenerator in dartGenerators)
            {
                dartGenerator.DestroyObjs();
            }
        }     
    }
}