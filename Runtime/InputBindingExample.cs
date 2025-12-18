using UnityEngine;
using DirectInputManager;
using System.Collections.Generic;
using System;

namespace DirectInputManager.Examples
{
    public class InputBindingExample : MonoBehaviour
    {
        [Serializable]
        public class InputAction
        {
            public string ActionName;
            public string DisplayName;
            
            // Binding Data
            public string DeviceGuid;
            public string DeviceProductName;
            public InputType InputType;
            public int InputIndex; // Button index or Axis index/offset
            public bool IsBound;
            public bool Invert; // Added for pedals

            public string GetBindingString()
            {
                if (!IsBound) return "<None>";
                string inv = Invert ? " (Inv)" : "";
                return $"{DeviceProductName} - {InputType} {InputIndex}{inv}";
            }
        }

        public enum InputType
        {
            Button,
            Axis,
            POV
        }

        // Configuration
        public List<InputAction> Actions = new List<InputAction>
        {
            new InputAction { ActionName = "Steer", DisplayName = "Steering" },
            new InputAction { ActionName = "Throttle", DisplayName = "Throttle" },
            new InputAction { ActionName = "Brake", DisplayName = "Brake" },
            new InputAction { ActionName = "Clutch", DisplayName = "Clutch" },
            new InputAction { ActionName = "ShiftUp", DisplayName = "Shift Up" },
            new InputAction { ActionName = "ShiftDown", DisplayName = "Shift Down" }
        };

        // State
        private bool isBinding = false;
        private InputAction currentBindingAction = null;
        private Dictionary<string, FlatJoyState2> previousStates = new Dictionary<string, FlatJoyState2>();
        private const int AxisThreshold = 5000; // Sensitivity for axis detection

        void Start()
        {
            if (!DIManager.IsInitialized)
            {
                DIManager.Initialize();
            }
        }

        void Update()
        {
            if (isBinding)
            {
                DetectInput();
            }

            // Always update states for the next frame comparison
            UpdatePreviousStates();
        }

        void UpdatePreviousStates()
        {
            if (DIManager.ActiveDevices == null) return;

            foreach (var kvp in DIManager.ActiveDevices)
            {
                var guid = kvp.Key;
                var state = DIManager.GetDeviceState(guid);
                if (previousStates.ContainsKey(guid))
                {
                    previousStates[guid] = state;
                }
                else
                {
                    previousStates.Add(guid, state);
                }
            }
        }

        void DetectInput()
        {
            foreach (var kvp in DIManager.ActiveDevices)
            {
                string guid = kvp.Key;
                DeviceInfo info = kvp.Value.deviceInfo;
                FlatJoyState2 currentState = DIManager.GetDeviceState(guid);

                if (!previousStates.ContainsKey(guid)) continue;
                FlatJoyState2 prevState = previousStates[guid];

                // 1. Check Buttons (128 buttons total)
                for (int i = 0; i < 128; i++)
                {
                    if (IsButtonPressed(currentState, i) && !IsButtonPressed(prevState, i))
                    {
                        BindAction(currentBindingAction, guid, info.productName, InputType.Button, i);
                        return;
                    }
                }

                // 2. Check Axes
                CheckAxis(currentState.lX, prevState.lX, 0, guid, info);
                CheckAxis(currentState.lY, prevState.lY, 1, guid, info);
                CheckAxis(currentState.lZ, prevState.lZ, 2, guid, info); 
                CheckAxis(currentState.lRx, prevState.lRx, 3, guid, info);
                CheckAxis(currentState.lRy, prevState.lRy, 4, guid, info);
                CheckAxis(currentState.lRz, prevState.lRz, 5, guid, info); 
                CheckAxis(currentState.rglSlider[0], prevState.rglSlider[0], 6, guid, info); // Slider 0
                
                if (!isBinding) return; // Exit if bound inside CheckAxis
            }
        }

        void CheckAxis(int current, int prev, int axisIndex, string guid, DeviceInfo info)
        {
            if (!isBinding) return;
            int delta = Mathf.Abs(current - prev);
            if (delta > AxisThreshold)
            {
                BindAction(currentBindingAction, guid, info.productName, InputType.Axis, axisIndex);
            }
        }

        bool IsButtonPressed(FlatJoyState2 state, int buttonIndex)
        {
            if (buttonIndex < 64)
            {
                return ((state.buttonsA >> buttonIndex) & 1) == 1;
            }
            else
            {
                return ((state.buttonsB >> (buttonIndex - 64)) & 1) == 1;
            }
        }

        void BindAction(InputAction action, string guid, string deviceName, InputType type, int index)
        {
            action.DeviceGuid = guid;
            action.DeviceProductName = deviceName;
            action.InputType = type;
            action.InputIndex = index;
            action.IsBound = true;
            action.Invert = false; // Reset invert on new bind
            
            isBinding = false;
            currentBindingAction = null;
            Debug.Log($"Bound {action.ActionName} to {deviceName} {type} {index}");
        }

        public void ApplyG29Preset()
        {
            string guid = null;
            string prodName = "Unknown Device";
            
            // Find first connected device for now, or preferably a Driving device
            if (DIManager.ActiveDevices.Count > 0) 
            {
                 // Just grab the first one as a best guess for the demo
                 var enumerator = DIManager.ActiveDevices.GetEnumerator();
                 enumerator.MoveNext();
                 guid = enumerator.Current.Key;
                 prodName = enumerator.Current.Value.deviceInfo.productName;
            }
            else
            {
                Debug.LogWarning("No devices connected to apply preset to.");
                return;
            }

            // G29 Defaults
            // Steering: X Axis (0)
            SetBind("Steer", guid, prodName, InputType.Axis, 0, false);
            // Throttle: Y Axis (1) Inverted
            SetBind("Throttle", guid, prodName, InputType.Axis, 1, true);
            // Brake: Rz Axis (5) Inverted
            SetBind("Brake", guid, prodName, InputType.Axis, 5, true);
            // Clutch: Slider 0 (6) Inverted? Or Z (2)?
            // Trying Slider 0 (6) as it's common for G29 clutch. If not, Z (2).
            // Let's assume Slider 0 for now based on typical DInput G29 driver.
            SetBind("Clutch", guid, prodName, InputType.Axis, 6, true);
            
            // Shift Up: Button 12
            SetBind("ShiftUp", guid, prodName, InputType.Button, 12, false);
            // Shift Down: Button 13
            SetBind("ShiftDown", guid, prodName, InputType.Button, 13, false);

            Debug.Log("Applied G29 Preset.");
        }

        void SetBind(string name, string guid, string prod, InputType type, int index, bool inv)
        {
            var action = Actions.Find(a => a.ActionName == name);
            if(action != null)
            {
                action.DeviceGuid = guid;
                action.DeviceProductName = prod;
                action.InputType = type;
                action.InputIndex = index;
                action.IsBound = true;
                action.Invert = inv;
            }
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 450, 700));
            GUILayout.Box("Input Binding Manager");

            if (GUILayout.Button("Load G29 Preset"))
            {
                ApplyG29Preset();
            }
            
            GUILayout.Space(10);

            if (isBinding)
            {
                GUI.color = Color.yellow;
                GUILayout.Box($"PRESS ANY KEY FOR: {currentBindingAction.DisplayName}...");
                
                if (GUILayout.Button("Cancel"))
                {
                    isBinding = false;
                    currentBindingAction = null;
                }
                GUI.color = Color.white;
            }

            GUILayout.Space(10);

            foreach (var action in Actions)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(action.DisplayName, GUILayout.Width(80));
                GUILayout.Label(action.GetBindingString(), GUILayout.Width(200));
                
                if (!isBinding)
                {
                    if (GUILayout.Button("Bind", GUILayout.Width(50)))
                    {
                        currentBindingAction = action;
                        isBinding = true;
                    }
                    
                    if (action.InputType == InputType.Axis)
                    {
                        bool newInvert = GUILayout.Toggle(action.Invert, "Inv");
                        if(newInvert != action.Invert) action.Invert = newInvert;
                    }
                }
                else
                {
                    GUI.enabled = false;
                    GUILayout.Button("Bind", GUILayout.Width(50));
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndArea();
        }
    }
}
