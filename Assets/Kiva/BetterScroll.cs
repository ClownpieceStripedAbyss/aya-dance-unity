using UdonSharp;
using UnityEngine;
using UnityEngine.EventSystems;
using VRC.SDKBase;
using VRC.Udon;

public class BetterScroll : UdonSharpBehaviour
{
    private BoxCollider2D boxCollider;
    private bool isInUse = false;

    void Start() {
        boxCollider = GetComponent<BoxCollider2D>();
        Debug.Log($"boxCollider: {boxCollider}");
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player) {
        Debug.Log($"OnPlayerTriggerEnter: {player}");
        isInUse = true;
    }

    public override void OnPlayerTriggerStay(VRCPlayerApi player) {
        Debug.Log($"OnPlayerTriggerStay: {player}");
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player) {
        Debug.Log($"OnPlayerTriggerExit: {player}");
        isInUse = false;
    }

    // Moving the mouse up and down on Desktop, typically the right stick up and down on gamepad and VR controllers.
    public override void InputLookVertical(float value, VRC.Udon.Common.UdonInputEventArgs args) {
        Debug.Log($"InputLookVertical: {value}, isInUse: {isInUse}");
    }
}
