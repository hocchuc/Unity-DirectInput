using UnityEngine;
using DirectInputManager;
#if NWH_VEHICLE_PHYSICS_2
using NWH.VehiclePhysics2.Input;
#endif

namespace DirectInputManager.NWHIntegration
{
    /// <summary>
    /// Input Provider for NWH Vehicle Physics 2 using DirectInput.
    /// Default mappings are set for Logitech G29.
    /// </summary>
    public class NWHDirectInputProvider :
#if NWH_VEHICLE_PHYSICS_2
        VehicleInputProviderBase
#else
        MonoBehaviour
#endif
    {
        public string DeviceGUID;

        [Header("Axis Mappings (G29 Default)")]
        public int SteeringAxis = 0; 
        public bool InvertSteering = false;

        public int ThrottleAxis = 1; 
        public bool InvertThrottle = true;

        public int BrakeAxis = 5; 
        public bool InvertBrake = true;

        public int ClutchAxis = 2; // Defaulting to Slider0/Z
        public bool InvertClutch = true;

        public int HandbrakeAxis = -1; 
        public bool InvertHandbrake = false;

        [Header("Button Mappings")]
        public int ShiftUpButton = 4; // Right Paddle
        public int ShiftDownButton = 5; // Left Paddle
        public int EngineStartStopButton = 24; // Enter?
        
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

        // State holding
        private float _steering;
        private float _throttle;
        private float _brake;
        private float _clutch;
        private float _handbrake;
        
        private bool _shiftUp;
        private bool _shiftDown;
        private bool _engineStartStop;
        
        private bool _lowBeamLights;
        private bool _highBeamLights;
        private bool _hazardLights;
        private bool _extraLights;
        private bool _leftBlinker;
        private bool _rightBlinker;
        
        private bool _horn;
        private bool _flipOver;
        private bool _boost;
        private bool _cruiseControl;
        private bool _trailerAttachDetach;

        private bool _shiftIntoReverse;
        private bool _shiftIntoNeutral;
        private bool _shiftInto1;
        private bool _shiftInto2;
        private bool _shiftInto3;
        private bool _shiftInto4;
        private bool _shiftInto5;
        private bool _shiftInto6;
        private bool _shiftInto7;
        private bool _shiftInto8;


#if NWH_VEHICLE_PHYSICS_2
        public override float Steering => _steering;
        public override float Throttle => _throttle;
        public override float Brakes => _brake;
        public override float Clutch => _clutch;
        public override float Handbrake => _handbrake;
        
        public override bool ShiftUp => _shiftUp;
        public override bool ShiftDown => _shiftDown;
        public override bool EngineStartStop => _engineStartStop;

        public override bool LowBeamLights => _lowBeamLights;
        public override bool HighBeamLights => _highBeamLights;
        public override bool HazardLights => _hazardLights;
        public override bool ExtraLights => _extraLights;
        public override bool LeftBlinker => _leftBlinker;
        public override bool RightBlinker => _rightBlinker;

        public override bool Horn => _horn;
        public override bool FlipOver => _flipOver;
        public override bool Boost => _boost;
        public override bool CruiseControl => _cruiseControl;
        public override bool TrailerAttachDetach => _trailerAttachDetach;

        public override bool ShiftIntoReverse => _shiftIntoReverse;
        public override bool ShiftIntoNeutral => _shiftIntoNeutral;
        public override bool ShiftInto1 => _shiftInto1;
        public override bool ShiftInto2 => _shiftInto2;
        public override bool ShiftInto3 => _shiftInto3;
        public override bool ShiftInto4 => _shiftInto4;
        public override bool ShiftInto5 => _shiftInto5;
        public override bool ShiftInto6 => _shiftInto6;
        public override bool ShiftInto7 => _shiftInto7;
        public override bool ShiftInto8 => _shiftInto8;
#endif

        void Update()
        {
            if (string.IsNullOrEmpty(DeviceGUID)) return;
            if (!DIManager.IsDeviceActive(DeviceGUID)) return;

            FlatJoyState2 state = DIManager.GetDeviceState(DeviceGUID);

            // Axes
            _steering = MapAxis(state, SteeringAxis, -1f, 1f, InvertSteering, true);
            _throttle = MapAxis(state, ThrottleAxis, 0f, 1f, InvertThrottle, false);
            _brake = MapAxis(state, BrakeAxis, 0f, 1f, InvertBrake, false);
            _clutch = MapAxis(state, ClutchAxis, 0f, 1f, InvertClutch, false);
            _handbrake = HandbrakeAxis >= 0 ? MapAxis(state, HandbrakeAxis, 0f, 1f, InvertHandbrake, false) : 0f;

            // Buttons
            _shiftUp = IsButtonPressed(state, ShiftUpButton);
            _shiftDown = IsButtonPressed(state, ShiftDownButton);
            _engineStartStop = IsButtonPressed(state, EngineStartStopButton);

            _lowBeamLights = IsButtonPressed(state, LowBeamLightsButton);
            _highBeamLights = IsButtonPressed(state, HighBeamLightsButton);
            _hazardLights = IsButtonPressed(state, HazardLightsButton);
            _extraLights = IsButtonPressed(state, ExtraLightsButton);
            _leftBlinker = IsButtonPressed(state, LeftBlinkerButton);
            _rightBlinker = IsButtonPressed(state, RightBlinkerButton);

            _horn = IsButtonPressed(state, HornButton);
            _flipOver = IsButtonPressed(state, FlipOverButton);
            _boost = IsButtonPressed(state, BoostButton);
            _cruiseControl = IsButtonPressed(state, CruiseControlButton);
            _trailerAttachDetach = IsButtonPressed(state, TrailerAttachDetachButton);

            _shiftIntoReverse = IsButtonPressed(state, ShiftIntoReverseButton);
            _shiftIntoNeutral = IsButtonPressed(state, ShiftIntoNeutralButton);
            _shiftInto1 = IsButtonPressed(state, ShiftInto1Button);
            _shiftInto2 = IsButtonPressed(state, ShiftInto2Button);
            _shiftInto3 = IsButtonPressed(state, ShiftInto3Button);
            _shiftInto4 = IsButtonPressed(state, ShiftInto4Button);
            _shiftInto5 = IsButtonPressed(state, ShiftInto5Button);
            _shiftInto6 = IsButtonPressed(state, ShiftInto6Button);
            _shiftInto7 = IsButtonPressed(state, ShiftInto7Button);
            _shiftInto8 = IsButtonPressed(state, ShiftInto8Button);
        }

        private float MapAxis(FlatJoyState2 state, int axisIndex, float minOut, float maxOut, bool invert, bool isCentered)
        {
            int rawValue = GetAxisValue(state, axisIndex);
            float norm = rawValue / 65535f; // 0 to 1

            if (invert) norm = 1f - norm;

            if (isCentered)
            {
                return Mathf.Lerp(minOut, maxOut, norm);
            }
            else
            {
                return Mathf.Lerp(minOut, maxOut, norm);
            }
        }

        public void LoadProfile(DeviceMappingProfile profile)
        {
            if (profile == null) return;

            SteeringAxis = profile.SteeringAxis;
            InvertSteering = profile.InvertSteering;
            ThrottleAxis = profile.ThrottleAxis;
            InvertThrottle = profile.InvertThrottle;
            BrakeAxis = profile.BrakeAxis;
            InvertBrake = profile.InvertBrake;
            ClutchAxis = profile.ClutchAxis;
            InvertClutch = profile.InvertClutch;
            HandbrakeAxis = profile.HandbrakeAxis;
            InvertHandbrake = profile.InvertHandbrake;
            
            ShiftUpButton = profile.ShiftUpButton;
            ShiftDownButton = profile.ShiftDownButton;
            EngineStartStopButton = profile.EngineStartStopButton;
            
            LowBeamLightsButton = profile.LowBeamLightsButton;
            HighBeamLightsButton = profile.HighBeamLightsButton;
            HazardLightsButton = profile.HazardLightsButton;
            ExtraLightsButton = profile.ExtraLightsButton;
            LeftBlinkerButton = profile.LeftBlinkerButton;
            RightBlinkerButton = profile.RightBlinkerButton;
            
            HornButton = profile.HornButton;
            FlipOverButton = profile.FlipOverButton;
            BoostButton = profile.BoostButton;
            CruiseControlButton = profile.CruiseControlButton;
            TrailerAttachDetachButton = profile.TrailerAttachDetachButton;
            
            ShiftIntoReverseButton = profile.ShiftIntoReverseButton;
            ShiftIntoNeutralButton = profile.ShiftIntoNeutralButton;
            ShiftInto1Button = profile.ShiftInto1Button;
            ShiftInto2Button = profile.ShiftInto2Button;
            ShiftInto3Button = profile.ShiftInto3Button;
            ShiftInto4Button = profile.ShiftInto4Button;
            ShiftInto5Button = profile.ShiftInto5Button;
            ShiftInto6Button = profile.ShiftInto6Button;
            ShiftInto7Button = profile.ShiftInto7Button;
            ShiftInto8Button = profile.ShiftInto8Button;

            Debug.Log($"[NWHDirectInputProvider] Loaded profile for matching device.");
        }

        private int GetAxisValue(FlatJoyState2 state, int index)
        {
            switch (index)
            {
                case 0: return state.lX;
                case 1: return state.lY;
                case 2: return state.lZ;
                case 3: return state.lRx;
                case 4: return state.lRy;
                case 5: return state.lRz;
                case 6: return state.lU;
                case 7: return state.lV;
                default: return 32767;
            }
        }

        private bool IsButtonPressed(FlatJoyState2 state, int index)
        {
            if (index < 0) return false;
            if (index < 64) return ((state.buttonsA >> index) & 1) == 1;
            return ((state.buttonsB >> (index - 64)) & 1) == 1;
        }
    }
}
