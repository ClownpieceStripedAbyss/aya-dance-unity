﻿using System;
using UnityEngine;
using UnityEngine.UI;
using UdonSharp;

namespace JLChnToZ.VRC.VVMW {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [RequireComponent(typeof(ScrollRect))]
    [BindEvent(typeof(ScrollRect), nameof(ScrollRect.onValueChanged), nameof(_OnScroll))]
    [AddComponentMenu("VizVid/Components/Pooled Scroll View")]
    [DefaultExecutionOrder(2)]
    public class PooledScrollView : UdonSharpEventSender {
        ScrollRect scrollRect;
        [FieldChangeCallback(nameof(EventPrefix))]
        [SerializeField] string eventPrefix = "_On";
        [SerializeField] GameObject template;
        [FieldChangeCallback(nameof(SelectedIndex))]
        [SerializeField] int selectedIndex = -1;
        [NonSerialized] public int lastClickedIndex;
        [NonSerialized] public int lastDeletedIndex;
        [NonSerialized] public int lastSecondaryIndex;
        [NonSerialized] public int lastInteractIndex;
        ListEntry[] entries;
        [FieldChangeCallback(nameof(EntryNames))]
        string[] entryNames;
        [FieldChangeCallback(nameof(EntryCanDelete))]
        bool[] entryCanDelete;
        [FieldChangeCallback(nameof(EntryCanSecondary))]
        bool[] entryCanSecondary;
        bool hasInit;
        [FieldChangeCallback(nameof(CanDelete))]
        bool canDelete = true;
        [FieldChangeCallback(nameof(CanInteract))]
        bool canInteract = true;
        [FieldChangeCallback(nameof(CanSecondary))]
        bool canSecondary = true;
        public bool autoSelect = true;
        int offset, count;
        RectTransform viewportRect, contentRect, templateRect;
        Vector2 prevAnchorPosition;
        float entriesPerViewport;
        string entryClickEventName = "_OnEntryClick";
        string entryDeleteEventName = "_OnEntryDelete";
        string entrySecondaryEventName = "_OnEntrySecondary";
        string scrollEventName = "_OnScroll";

        public string EventPrefix {
            get => eventPrefix;
            set {
                eventPrefix = value;
                entryClickEventName = eventPrefix + "EntryClick";
                entryDeleteEventName = eventPrefix + "EntryDelete";
                entrySecondaryEventName = eventPrefix + "EntrySecondary";
                scrollEventName = eventPrefix + "Scroll";
            }
        }
        public int SelectedIndex {
            get => selectedIndex;
            set {
                selectedIndex = value;
                if (hasInit && gameObject.activeInHierarchy) {
                    UpdateEntryState();
                    ScrollToSelected();
                }
            }
        }

        public string[] EntryNames {
            get {
                if (entryNames == null) return null;
                if (offset == 0 && count == entryNames.Length)
                    return entryNames;
                var result = new string[count];
                Array.Copy(entryNames, offset, result, 0, count);
                return result;
            }
            set {
                entryNames = value;
                offset = 0;
                count = entryNames != null ? entryNames.Length : 0;
                if (hasInit && gameObject.activeInHierarchy)
                    UpdateEntryState();
            }
        }

        public bool[] EntryCanDelete {
            get {
                if (entryCanDelete == null) return null;
                var result = new bool[entryCanDelete.Length];
                Array.Copy(entryCanDelete, 0, result, 0, entryCanDelete.Length);
                return result;
            }
            set {
                entryCanDelete = value;
                if (hasInit && gameObject.activeInHierarchy) {
                    for (var i = 0; i < entries.Length; i++) {
                        entries[i].HasDelete = entryCanDelete != null && i < entryCanDelete.Length ? entryCanDelete[i] : canDelete;
                    }
                }
            }
        }

        public bool[] EntryCanSecondary {
            get {
                if (entryCanSecondary == null) return null;
                var result = new bool[entryCanSecondary.Length];
                Array.Copy(entryCanSecondary, 0, result, 0, entryCanSecondary.Length);
                return result;
            }
            set {
                entryCanSecondary = value;
                if (hasInit && gameObject.activeInHierarchy) {
                    for (var i = 0; i < entries.Length; i++) {
                        entries[i].HasSecondary = entryCanSecondary != null && i < entryCanSecondary.Length ? entryCanSecondary[i] : canSecondary;
                    }
                }
            }
        }

        public bool CanDelete {
            get => canDelete;
            set {
                canDelete = value;
                if (hasInit && gameObject.activeInHierarchy)
                    foreach (var entry in entries)
                        entry.HasDelete = value;
            }
        }

        public bool CanSecondary {
            get => canSecondary;
            set {
                canSecondary = value;
                if (hasInit && gameObject.activeInHierarchy)
                    foreach (var entry in entries)
                        entry.HasSecondary = value;
            }
        }

        public bool CanInteract {
            get => canInteract;
            set {
                canInteract = value;
                if (hasInit && gameObject.activeInHierarchy)
                    foreach (var entry in entries)
                        entry.Unlocked = value;
            }
        }

        public float ScrollPosition {
            get {
                if (scrollRect == null) return 0F;
                return scrollRect.normalizedPosition.y;
            }
            set {
                if (scrollRect == null) return;
                var normalizedPosition = scrollRect.normalizedPosition;
                normalizedPosition.y = value;
                scrollRect.normalizedPosition = normalizedPosition;
            }
        }

        void OnEnable() {
            if (!hasInit) {
                if (template == null) {
                    var listEntry = GetComponentInChildren<ListEntry>(true);
                    if (listEntry == null) {
                        Debug.LogError("No template found in PooledScrollView", this);
                        return;
                    }
                    template = listEntry.gameObject;
                }
                scrollRect = GetComponent<ScrollRect>();
                viewportRect = scrollRect.viewport;
                contentRect = scrollRect.content;
                templateRect = template.GetComponent<RectTransform>();
                var templateHeight = templateRect.rect.height;
                var viewportHeight = viewportRect.rect.height;
                entriesPerViewport = viewportHeight / templateHeight;
                var entryCount = Mathf.CeilToInt(entriesPerViewport) + 1;
                entries = new ListEntry[entryCount];
                for (var i = 0; i < entryCount; i++) {
                    var instance = Instantiate(template);
                    instance.transform.SetParent(contentRect, false);
                    var entry = instance.GetComponent<ListEntry>();
                    entry.asPooledEntry = true;
                    entry.indexAsUserData = true;
                    entry.callbackTarget = this;
                    entry.callbackEventName = nameof(_OnEntryClick);
                    entry.deleteEventName = nameof(_OnEntryDelete);
                    entry.secondaryEventName = nameof(_OnEntrySecondary);
                    entry.callbackVariableName = nameof(lastInteractIndex);
                    entry.entryOffset = i;
                    entry.HasDelete = canDelete;
                    entry.HasSecondary = canSecondary;
                    entry.Unlocked = canInteract;
                    entry.spawnedEntryCount = entryCount;
                    entry._OnParentScroll();
                    entries[i] = entry;
                }
                template.gameObject.SetActive(false);
                EventPrefix = eventPrefix;
                hasInit = true;
                if (entryNames != null) UpdateEntryState();
            } else {
                if (entryNames != null) UpdateEntryState();
                foreach (var entry in entries) {
                    entry.HasDelete = canDelete;
                    entry.HasSecondary = canSecondary;
                    entry.Unlocked = canInteract;
                }
            }
            ScrollToSelected();
        }

        void UpdateEntryState() {
            var size = contentRect.sizeDelta;
            size.y = count * templateRect.rect.height;
            contentRect.sizeDelta = size;
            for (var i = 0; i < entries.Length; i++) {
                var entry = entries[i];
                entry.pooledEntryNames = entryNames;
                entry.selectedEntryIndex = selectedIndex;
                entry.pooledEntryOffset = offset;
                entry.pooledEntryCount = count;
                entry._UpdatePositionAndContent();
            }
        }

        public void SetEntries(string[] entries, int offset, int count) {
            entryNames = entries;
            this.offset = offset;
            this.count = count;
            if (hasInit && gameObject.activeInHierarchy)
                UpdateEntryState();
        }

        public void SetIndexWithoutScroll(int index) {
            selectedIndex = index;
            if (hasInit && gameObject.activeInHierarchy)
                UpdateEntryState();
        }

        public void ScrollToSelected() => ScrollTo(selectedIndex);

        public void ScrollTo(int index) {
            if (!hasInit) return;
            var normalizedPosition = scrollRect.normalizedPosition;
            normalizedPosition.y = count > entriesPerViewport ? Mathf.Clamp01(index / (count - entriesPerViewport)) : 0;
            scrollRect.normalizedPosition = normalizedPosition;
        }

        public void _OnEntryClick() {
            lastClickedIndex = lastInteractIndex;
            if (autoSelect) SetIndexWithoutScroll(lastClickedIndex);
            SendEvent(entryClickEventName);
        }

        public void _OnEntryDelete() {
            lastDeletedIndex = lastInteractIndex;
            SendEvent(entryDeleteEventName);
        }

        public void _OnEntrySecondary() {
            lastSecondaryIndex = lastInteractIndex;
            SendEvent(entrySecondaryEventName);
        }

        public void _OnScroll() {
            if (hasInit) {
                // It jitters in some cases, so we need to filter it.
                var newAnchorPosition = contentRect.anchoredPosition;
                if (Vector2.Distance(prevAnchorPosition, newAnchorPosition) < 0.005F) return;
                prevAnchorPosition = newAnchorPosition;
                foreach (var entry in entries)
                    entry._OnParentScroll();
            }
            SendEvent(scrollEventName);
        }
    }
}