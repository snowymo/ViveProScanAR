using UnityEngine;
using System.Collections.Generic;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Tutorial_TouchpadTexts : MonoBehaviour
    {
        public List<ViveSR_Experience_Tutorial_Line> buttonTexts;

        public string GetDefaultText()
        {
            return buttonTexts[0].text;
        }
    }
}
