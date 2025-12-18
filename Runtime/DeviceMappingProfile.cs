using UnityEngine;

namespace DirectInputManager.NWHIntegration
{
    [CreateAssetMenu(fileName = "New Device Profile", menuName = "DirectInput/Device Profile")]
    public class DeviceMappingProfile : ScriptableObject
    {
        [Tooltip("Substring of the product name to match this profile (e.g. 'G29', 'T300')")]
        public string ProductNameSubstring;

        [Header("Axis Mappings")]
        public int SteeringAxis = 0; 
        public bool InvertSteering = false;

        public int ThrottleAxis = 1; 
        public bool InvertThrottle = true;

        public int BrakeAxis = 5; 
        public bool InvertBrake = true;

        public int ClutchAxis = 2; 
        public bool InvertClutch = true;

        public int HandbrakeAxis = -1; 
        public bool InvertHandbrake = false;

        [Header("Button Mappings")]
        public int ShiftUpButton = 4;
        public int ShiftDownButton = 5;
        public int EngineStartStopButton = 24;
        
        [Header("Lights")]
        public int LowBeamLightsButton = -1;
        public int HighBeamLightsButton = -1;
        public int HazardLightsButton = -1;
        public int ExtraLightsButton = -1;
        public int LeftBlinkerButton = -1;
        public int RightBlinkerButton = -1;

        [Header("Modules")]
        public int HornButton = -1;
        public int FlipOverButton = -1;
        public int BoostButton = -1;
        public int CruiseControlButton = -1;
        public int TrailerAttachDetachButton = -1;

        [Header("H-Shifter")]
        public int ShiftIntoReverseButton = -1;
        public int ShiftIntoNeutralButton = -1;
        public int ShiftInto1Button = -1;
        public int ShiftInto2Button = -1;
        public int ShiftInto3Button = -1;
        public int ShiftInto4Button = -1;
        public int ShiftInto5Button = -1;
        public int ShiftInto6Button = -1;
        public int ShiftInto7Button = -1;
        public int ShiftInto8Button = -1;
    }
}
