using UnityEngine;
using Valve.VR.InteractionSystem;
namespace Vive.Plugin.SR.Experience
{

    public class ViveSR_Experience_DartThrowGenerator : ViveSR_Experience_IDartGenerator
    {

        public override void TriggerPress()
        {
            GenerateDart();
            InstantiatedDarts.Add(currentGameObj);

            isHolding = true;
        }
       
        protected override void TriggerHold()
        {
            currentGameObj.transform.position = ViveSR_Experience.AttachPoint.transform.position;
        }

        public override void TriggerRelease()
        {
            ViveSR_Experience.targetHandScript.DetachObject(currentGameObj);

            currentGameObj.transform.parent = null;
            isHolding = false;
        }
        protected override void GenerateDart()
        {
            currentGameObj = Instantiate(dart_prefabs[currentDartPrefeb], ViveSR_Experience.AttachPoint.transform);
            if (currentDartPrefeb == 1)                     //1=deer
            {
                currentGameObj.transform.LookAt(ViveSR_Experience.PlayerHeadCollision.transform);
            }
            currentGameObj.GetComponent<ViveSR_Experience_Dart>().dartGeneratorMgr = dartGeneratorMgr;
            //attach obj without trigger(SteamVR standard) so Velocity Estimator can work right.
            ViveSR_Experience.targetHandScript.AttachObject(currentGameObj, currentGameObj.GetComponent<Throwable>().attachmentFlags, currentGameObj.GetComponent<Throwable>().attachmentPoint);
        }
    }
}