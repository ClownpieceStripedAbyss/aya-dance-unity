﻿#if UDONSHARP
using UdonSharp;

using UnityEngine.UI;

using VRC.SDKBase;

namespace AudioLink
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class GlobalToggle : UdonSharpBehaviour
    {
        [UdonSynced]
        private bool syncedValue;
        private bool deserializing;
        private Toggle toggle;
        private VRCPlayerApi localPlayer;

        private void Start()
        {
            toggle = transform.GetComponent<Toggle>();
            localPlayer = Networking.LocalPlayer;
            syncedValue = toggle.isOn;
            deserializing = false;

            if (Networking.IsOwner(gameObject))
                RequestSerialization();
        }

        public override void OnDeserialization()
        {
            deserializing = true;
            toggle.isOn = syncedValue;
            deserializing = false;
        }

        public void ToggleUpdate()
        {
            if (toggle == null)
                return;
            if (deserializing)
                return;
            if (!Networking.IsOwner(gameObject))
                Networking.SetOwner(localPlayer, gameObject);

            syncedValue = toggle.isOn;
            RequestSerialization();
        }
    }
}
#endif
