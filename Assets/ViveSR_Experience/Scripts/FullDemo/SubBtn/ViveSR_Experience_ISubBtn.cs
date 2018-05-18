using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    public abstract class ViveSR_Experience_ISubBtn : MonoBehaviour
    {
        [HideInInspector] public ViveSR_Experience_ISubMenu SubMenu;
        protected int ThisButtonTypeNum;

        public bool isOn;
        public bool disabled;
        public bool isToggleAllowed;
        public new Renderer renderer;

        private void Awake()
        {
            SubMenu = transform.parent.parent.GetComponent<ViveSR_Experience_ISubMenu>();
            AwakeToDo();
        }

        protected virtual void AwakeToDo() { }

        private void Start()
        {
            StartToDo();
        }
        protected virtual void StartToDo() { }

        public virtual void Execute()
        {
            if ((isOn && isToggleAllowed) || !isOn) isOn = !isOn;
            renderer.material.SetColor("_EmissionColor", isOn ? ViveSR_Experience.ButtonScripts[ViveSR_Experience.rotator.currentButtonNum].BrightColor : ViveSR_Experience.ButtonScripts[ViveSR_Experience.rotator.currentButtonNum].OriginalEmissionColor);
            ExecuteToDo();
        }
        public virtual void ExecuteToDo() {}

        public void EnableButton(bool on)
        {  
            SubMenu.subBtnScripts[ThisButtonTypeNum].disabled = !on;
            SubMenu.subBtnScripts[ThisButtonTypeNum].renderer.material.SetColor("_EmissionColor", on ? ViveSR_Experience.ButtonScripts[ViveSR_Experience.rotator.currentButtonNum].OriginalEmissionColor : ViveSR_Experience.ButtonScripts[ViveSR_Experience.rotator.currentButtonNum].DisableColor);
        }

        public void ForceExcute(bool on)
        {
            if (!disabled)
            {
                isOn = on;
                SubMenu.subBtnScripts[ThisButtonTypeNum].renderer.material.SetColor("_EmissionColor", on ? ViveSR_Experience.ButtonScripts[ViveSR_Experience.rotator.currentButtonNum].BrightColor : ViveSR_Experience.ButtonScripts[ViveSR_Experience.rotator.currentButtonNum].OriginalEmissionColor);
                ExecuteToDo();
            }
        }
        private void Update()
        {
            UpdateToDo();
        }
        protected virtual void UpdateToDo() { }
    }
}