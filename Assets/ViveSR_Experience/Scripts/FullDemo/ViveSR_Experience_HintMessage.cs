using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Vive.Plugin.SR.Experience
{
    public enum hintType
    {
        onController = 0,
        onHeadSet = 1
    }
    public class ViveSR_Experience_HintMessage : MonoBehaviour
    {         
        private static ViveSR_Experience_HintMessage _instance;
        public static ViveSR_Experience_HintMessage instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ViveSR_Experience_HintMessage>();
                }
                return _instance;
            }
        }
        [SerializeField] List<Text> hintTxts;

        IEnumerator CurrentCoroutine;

        public void SetHintMessage(hintType hintType, string txt, bool autoFadeOff, float waitTime = 3f)
        {
            hintTxts[(int)hintType].text = txt;
            if (CurrentCoroutine != null) StopCoroutine(CurrentCoroutine);
            if (autoFadeOff) HintTextFadeOff(hintType);
        }

        public void HintTextFadeOff(hintType hintType, float waitTime = 3f)
        {
            hintTxts[(int)hintType].color = new Color(hintTxts[(int)hintType].color.r, hintTxts[(int)hintType].color.g, hintTxts[(int)hintType].color.b, 1);
            CurrentCoroutine = FadeOff(hintType, waitTime);
            StartCoroutine(CurrentCoroutine);
        }

        IEnumerator FadeOff(hintType hintType, float waitTime = 3f)
        {
            yield return new WaitForSeconds(waitTime);
            
            while (hintTxts[(int)hintType].color.a >= 0)
            {
                hintTxts[(int)hintType].color -= new Color(0f, 0f, 0f, 2f * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }

            CurrentCoroutine = null;
            hintTxts[(int)hintType].text = "";
            hintTxts[(int)hintType].color = new Color(hintTxts[(int)hintType].color.r, hintTxts[(int)hintType].color.g, hintTxts[(int)hintType].color.b, 1);
        }        
    }
}