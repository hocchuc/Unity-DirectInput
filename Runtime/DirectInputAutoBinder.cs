using UnityEngine;
using DirectInputManager;
using System.Linq;

namespace DirectInputManager.AutoBind
{
    /// <summary>
    /// Automatically detects and binds Driving devices (Steering Wheels) when they are connected.
    /// Add this component to a GameObject in your scene to enable auto-binding.
    /// </summary>
    public class DirectInputAutoBinder : MonoBehaviour
    {
        // DirectInput device types
        // Ref: https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ee416625(v=vs.85)
        private const uint DI8DEVTYPE_JOYSTICK = 20;
        private const uint DI8DEVTYPE_GAMEPAD = 21;
        private const uint DI8DEVTYPE_DRIVING = 22;

        void Start()
        {
            // Ensure DIManager is ready
            if (!DIManager.IsInitialized)
            {
                if (!DIManager.Initialize())
                {
                    Debug.LogError("[DirectInputAutoBinder] Failed to initialize DirectInput Manager.");
                    return;
                }
            }

            // Subscribe to device events
            DIManager.OnDeviceAdded += OnDeviceAdded;

            // Perform an initial scan
            ScanAndBind();
        }

        void OnDestroy()
        {
            // Unsubscribe to avoid memory leaks or calling on destroyed object
            DIManager.OnDeviceAdded -= OnDeviceAdded;
        }

        private async void ScanAndBind()
        {
            // Ensure we have the latest list of devices
            await DIManager.EnumerateDevicesAsync();

            // Iterate through all found devices
            if (DIManager.Devices != null)
            {
                foreach (var device in DIManager.Devices)
                {
                    CheckAndBindDevice(device);
                }
            }
        }

        private void OnDeviceAdded(DeviceInfo device)
        {
            CheckAndBindDevice(device);
        }

        private void CheckAndBindDevice(DeviceInfo device)
        {
            // The least significant byte of the device type specifies the main type.
            // 0x14 (20) = DI8DEVTYPE_JOYSTICK
            // 0x15 (21) = DI8DEVTYPE_GAMEPAD
            // 0x16 (22) = DI8DEVTYPE_DRIVING
            uint mainType = device.deviceType & 0xFF;

            if (mainType == DI8DEVTYPE_DRIVING || 
                mainType == DI8DEVTYPE_JOYSTICK || 
                mainType == DI8DEVTYPE_GAMEPAD)
            {
                // Check if already active to avoid spamming or re-attaching
                if (!DIManager.IsDeviceActive(device.guidInstance))
                {
                    Debug.Log($"[DirectInputAutoBinder] Detected Compatible Device ({GetDeviceTypeName(mainType)}): '{device.productName}'. Attempting to attach...");
                    
                    if (DIManager.Attach(device.guidInstance))
                    {
                        Debug.Log($"[DirectInputAutoBinder] Successfully attached to '{device.productName}'!");
                        BindToNWHProvider(device.guidInstance);
                    }
                    else
                    {
                        Debug.LogError($"[DirectInputAutoBinder] Failed to attach to '{device.productName}'.");
                    }
                }
            }
        }

        private string GetDeviceTypeName(uint type)
        {
            switch (type)
            {
                case DI8DEVTYPE_DRIVING: return "Driving/Wheel";
                case DI8DEVTYPE_JOYSTICK: return "Joystick";
                case DI8DEVTYPE_GAMEPAD: return "Gamepad";
                default: return "Unknown";
            }
        }

        public System.Collections.Generic.List<DirectInputManager.NWHIntegration.DeviceMappingProfile> SupportedProfiles;

        private void BindToNWHProvider(string guid)
        {
            // Look for the provider in the scene or on this object
            var provider = FindObjectOfType<DirectInputManager.NWHIntegration.NWHDirectInputProvider>();
            
            if (provider != null)
            {
                provider.DeviceGUID = guid;
                
                // Try to find a matching profile
                // We need the product name, so let's retrieve device info from DIManager
                if (DIManager.ActiveDevices.TryGetValue(guid, out var activeDevice))
                {
                   string pName = activeDevice.deviceInfo.productName;
                   Debug.Log($"[DirectInputAutoBinder] Assigned Device {guid} ({pName}) to NWHDirectInputProvider.");

                   if (SupportedProfiles != null)
                   {
                       foreach (var profile in SupportedProfiles)
                       {
                           if (!string.IsNullOrEmpty(profile.ProductNameSubstring) && 
                               pName.IndexOf(profile.ProductNameSubstring, System.StringComparison.OrdinalIgnoreCase) >= 0)
                           {
                               Debug.Log($"[DirectInputAutoBinder] Found matching profile '{profile.name}' for device '{pName}'. Applying settings.");
                               provider.LoadProfile(profile);
                               break;
                           }
                       }
                   }
                }
                else
                {
                    Debug.Log($"[DirectInputAutoBinder] Assigned Device {guid} to NWHDirectInputProvider.");
                }
            }
            else
            {
                // Optional: Check if we should search all vehicles
                /* 
                var providers = FindObjectsOfType<DirectInputManager.NWHIntegration.NWHDirectInputProvider>();
                foreach (var p in providers) { p.DeviceGUID = guid; }
                */
            }
        }
    }
}
