using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    public enum Button
    {
         Tutorial = 0,
         EnableDepth = 1,
         _3DPreview = 2,
         EnableMesh = 3,
         Effects = 4,
         Calibration = 5
    }
    public class ViveSR_Experience_IButton : MonoBehaviour
    {
        [SerializeField] Button ButtonType;
        public bool isOn = false;
        public bool disableWhenRotatedAway;
        public bool disabled;
        public bool allowToggle;

        public Color OriginalEmissionColor;
        public Color BrightColor;
        public Color DisableColor;

        public ViveSR_Experience_ISubMenu SubMenu;

        private void Awake()
        {
            OriginalEmissionColor = transform.GetChild(0).GetComponent<Renderer>().material.GetColor("_EmissionColor");
            if (disabled) transform.GetChild(0).GetComponent<Renderer>().material.SetColor("_EmissionColor", DisableColor);

            AwakeToDo();
        }
        protected virtual void AwakeToDo() { }

        private void Start()
        {
            StartToDo();
        }

        protected virtual void StartToDo() { }

        public void Action(bool isOn)
        {
            if (!disabled)
            {
                this.isOn = isOn;
                if (SubMenu == null)
                {
                    ViveSR_Experience.ButtonRenderers[(int)ButtonType].material.SetColor("_EmissionColor", isOn ? BrightColor : OriginalEmissionColor);
                }
                else
                {
                    SubMenu.enabled = isOn;
                    SubMenu.ToggleSubMenu(isOn);
                }
                ActionToDo();
            }
        }
        public virtual void ActionToDo() { }

        public virtual void ActOnRotator(bool isOn) 
        {
            Action(isOn);
        }

        protected void Update()
        {
            UpdateToDo();
        }
        protected virtual void UpdateToDo() { }

        public virtual void ForceExcuteButton(bool on)
        {
            Action(on);
        }

        public void EnableButton(bool on)
        {
            ViveSR_Experience.ButtonScripts[(int)ButtonType].disabled = !on;
            ViveSR_Experience.ButtonRenderers[(int)ButtonType].material.SetColor("_EmissionColor", on? ViveSR_Experience.ButtonScripts[(int)ButtonType].OriginalEmissionColor: ViveSR_Experience.ButtonScripts[(int)ButtonType].DisableColor);
        }
    }
}






