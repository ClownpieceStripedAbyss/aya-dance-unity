using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.SDK3.Components;
using VRC.SDK3.Components.Video;
using JLChnToZ.VRC.VVMW.I18N;

namespace JLChnToZ.VRC.VVMW {
  [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
  [DisallowMultipleComponent]
  [AddComponentMenu("VizVid/Button Panel Handler")]
  [DefaultExecutionOrder(2)]
  public class ButtonPanelHandler : VizVidBehaviour {
    [Header("Main Reference")]
    [Locatable(
      InstaniatePrefabPath = "Packages/idv.jlchntoz.vvmw/VVMW (No Controls).prefab",
      InstaniatePrefabPosition = LocatableAttribute.InstaniatePrefabHierachyPosition.Before
    ), BindUdonSharpEvent] public FrontendHandler handler;
    
    [Header("Preview Text")]
    [SerializeField] Text previewText;

    [Header("Dial Numbers")]
    [BindEvent(nameof(Button.onClick), nameof(_ButtonClick1))]
    [SerializeField] Button button1;
    [BindEvent(nameof(Button.onClick), nameof(_ButtonClick2))]
    [SerializeField] Button button2;
    [BindEvent(nameof(Button.onClick), nameof(_ButtonClick3))]
    [SerializeField] Button button3;
    [BindEvent(nameof(Button.onClick), nameof(_ButtonClick4))]
    [SerializeField] Button button4;
    [BindEvent(nameof(Button.onClick), nameof(_ButtonClick5))]
    [SerializeField] Button button5;
    [BindEvent(nameof(Button.onClick), nameof(_ButtonClick6))]
    [SerializeField] Button button6;
    [BindEvent(nameof(Button.onClick), nameof(_ButtonClick7))]
    [SerializeField] Button button7;
    [BindEvent(nameof(Button.onClick), nameof(_ButtonClick8))]
    [SerializeField] Button button8;
    [BindEvent(nameof(Button.onClick), nameof(_ButtonClick9))]
    [SerializeField] Button button9;
    [BindEvent(nameof(Button.onClick), nameof(_ButtonClick0))]
    [SerializeField] Button button0;
    [BindEvent(nameof(Button.onClick), nameof(_ButtonClickX))]
    [SerializeField] Button buttonX;
    [BindEvent(nameof(Button.onClick), nameof(_ButtonClickY))]
    [SerializeField] Button buttonY;

    string input = "";

    public string Input {
        get => input;
        set {
          input = value;
        }
    }

    private void AppendNumber(int number) {
      if (handler == null) return;
      Input += number.ToString();

      int id = -1;
      if (Int32.TryParse(Input, out id)) {
        int offset = handler.SongIdToOffset(id);

        if (offset != -1) {
          var entryTitles = handler.PlayListEntryTitles;
          var title = entryTitles[offset];
          SetPreviewText($"{id}: {title}");
        } else {
          SetPreviewText($"{id}: Not found");
        }

      } else {
        SetPreviewText("");
        Input = "";
      }
    }
    
    public void _ButtonClickNumber(int number) {
      if (handler == null) return;
      Debug.Log($"Button clicked: {number}");
      AppendNumber(number);
    }

    public void _ButtonClickX() {
      if (handler == null) return;
      SetPreviewText("");
      Input = "";
    }

    public void _ButtonClickY() {
      if (handler == null) return;

      int id = -1;
      if (Int32.TryParse(Input, out id)) {
        handler.PlaySongById(id, null);
      }

      SetPreviewText("");
      Input = "";
    }

    private void SetPreviewText(string text) {
      if (previewText != null) previewText.text = text;
    }

    void OnEnable() {
      if (button0 != null) button0.interactable = true;
      if (button1 != null) button1.interactable = true;
      if (button2 != null) button2.interactable = true;
      if (button3 != null) button3.interactable = true;
      if (button4 != null) button4.interactable = true;
      if (button5 != null) button5.interactable = true;
      if (button6 != null) button6.interactable = true;
      if (button7 != null) button7.interactable = true;
      if (button8 != null) button8.interactable = true;
      if (button9 != null) button9.interactable = true;
      if (buttonX != null) buttonX.interactable = true;
      if (buttonY != null) buttonY.interactable = true;
      SetPreviewText("");
    }

    public void _ButtonClick1() => _ButtonClickNumber(1);
    public void _ButtonClick2() => _ButtonClickNumber(2);
    public void _ButtonClick3() => _ButtonClickNumber(3);
    public void _ButtonClick4() => _ButtonClickNumber(4);
    public void _ButtonClick5() => _ButtonClickNumber(5);
    public void _ButtonClick6() => _ButtonClickNumber(6);
    public void _ButtonClick7() => _ButtonClickNumber(7);
    public void _ButtonClick8() => _ButtonClickNumber(8);
    public void _ButtonClick9() => _ButtonClickNumber(9);
    public void _ButtonClick0() => _ButtonClickNumber(0);
  }
}
