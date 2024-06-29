using System;
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

public class ReceiptClient : UdonSharpBehaviour
{
    public VRCUrl defaultReceiptApiUrlPrefix = new VRCUrl("https://aya-dance.kiva.moe/r/ad-");

    [NonSerialized]
    private VRCUrl getReceiptUrl = VRCUrl.Empty;

    public VRCUrl ApiUrl {
        get => getReceiptUrl;
        set {
            getReceiptUrl = value;
        }
    }
    
    [NonSerialized]
    private UdonSharpBehaviour eventListener = null;
    [NonSerialized]
    private string eventListenerPrefix = "_OnReceiptClient";

    public UdonSharpBehaviour EventListener {
        set {
            eventListener = value;
        }
    }
    public string EventListenerPrefix {
        set {
            eventListenerPrefix = value;
        }
    }
    private void SendReceiptUpdatedEvent() {
        if (eventListener != null) {
            eventListener.SendCustomEvent($"{eventListenerPrefix}Updated");
        }
    }
    private void SendReceiptConnectedEvent() {
        if (eventListener != null) {
            eventListener.SendCustomEvent($"{eventListenerPrefix}Connected");
        }
    }
    private void SendReceiptDisconnectedEvent() {
        if (eventListener != null) {
            eventListener.SendCustomEvent($"{eventListenerPrefix}Disconnected");
        }
    }

    // https://github.com/ClownpieceStripedAbyss/pypy-cdn/blob/9c29aee7f8f57c101254038e3a148589ea912271/src/cdn/receipt.rs#L17
    // #[derive(Debug, Clone, Serialize, Deserialize)]
    // pub struct Receipt {
    //     pub receipt_id: ReceiptId,
    //     pub room_id: RoomId,
    //     pub target: UserId,
    //     pub created_at: i64,
    //     pub expires_at: i64,
    //     pub song_id: Option<SongId>,
    //     pub song_url: Option<String>,
    //     pub sender: Option<UserId>,
    //     pub message: Option<String>,
    // }
    [NonSerialized]
    private DataList cachedReceipts = new DataList();     // of type Vec<Receipt as DataDictionary as DataToken>
    [NonSerialized]
    private DataList acceptedReceiptIds = new DataList(); // of type Vec<ReceiptId as String as DataToken>

    public DataList GetReceipts() {
        return cachedReceipts;
    }

    public bool ReceiptIsAlreadyAccepted(DataDictionary receipt) {
        var id = ReceiptReceiptId(receipt);
        if (id == null) {
            // actually this should never happen
            return false;
        }
        return acceptedReceiptIds.Contains(id);
    }
    
    public void AcceptReceipt(DataDictionary receipt) {
        Debug.Log($"Accepted receipt {ReceiptToString(receipt)}");
        var id = ReceiptReceiptId(receipt);
        acceptedReceiptIds.Add(id);
    }

    public static DataDictionary ReceiptAtIndex(DataList receipts, int index) {
        if (index < 0 || index >= receipts.Count) {
            return null;
        }
        return receipts[index].DataDictionary;
    }

    public static string ReceiptReceiptId(DataDictionary receipt) {
        DataToken tok;
        if (receipt.TryGetValue("receipt_id", out tok) && tok.TokenType == TokenType.String) {
            return tok.String;
        }
        return null;
    }

    public static string ReceiptRoomId(DataDictionary receipt) {
        DataToken tok;
        if (receipt.TryGetValue("room_id", out tok) && tok.TokenType == TokenType.String) {
            return tok.String;
        }
        return null;
    }

    public static string ReceiptTarget(DataDictionary receipt) {
        DataToken tok;
        if (receipt.TryGetValue("target", out tok) && tok.TokenType == TokenType.String) {
            return tok.String;
        }
        return null;
    }

    public static long ReceiptCreatedAt(DataDictionary receipt) {
        DataToken tok;
        if (receipt.TryGetValue("created_at", out tok) && tok.TokenType == TokenType.Double) {
            return (long) tok.Number;
        }
        return 0;
    }

    public static long ReceiptExpiresAt(DataDictionary receipt) {
        DataToken tok;
        if (receipt.TryGetValue("expires_at", out tok) && tok.TokenType == TokenType.Double) {
            return (long) tok.Number;
        }
        return 0;
    }

    public static int ReceiptSongId(DataDictionary receipt) {
        DataToken tok;
        if (receipt.TryGetValue("song_id", out tok) && tok.TokenType == TokenType.Double) {
            return (int) tok.Number;
        }
        return -1;
    }

    public static string ReceiptSongUrl(DataDictionary receipt) {
        DataToken tok;
        if (receipt.TryGetValue("song_url", out tok) && tok.TokenType == TokenType.String) {
            return tok.String;
        }
        return null;
    }

    public static string ReceiptSender(DataDictionary receipt) {
        DataToken tok;
        if (receipt.TryGetValue("sender", out tok) && tok.TokenType == TokenType.String) {
            return tok.String;
        }
        return null;
    }

    public static string ReceiptMessage(DataDictionary receipt) {
        DataToken tok;
        if (receipt.TryGetValue("message", out tok) && tok.TokenType == TokenType.String) {
            return tok.String;
        }
        return null;
    }

    public static string ReceiptToString(DataDictionary receipt) {
        return $"Receipt {ReceiptReceiptId(receipt)} for {ReceiptTarget(receipt)} in room {ReceiptRoomId(receipt)} created at {ReceiptCreatedAt(receipt)} expires at {ReceiptExpiresAt(receipt)} song {ReceiptSongId(receipt)} {ReceiptSongUrl(receipt)} from {ReceiptSender(receipt)} message {ReceiptMessage(receipt)}";
    }

    [NonSerialized]
    private bool receiptClientEnabled = false;
    [NonSerialized]
    private bool receiptClientConnected = false;

    public bool ReceiptClientEnabled {
        get => receiptClientEnabled;
        set {
            if (value == receiptClientEnabled) {
                return;
            }
            if (value) {
                SendCustomEventDelayedSeconds(nameof(FetchReceipt), 1.0f);
            }
            receiptClientEnabled = value;
            receiptClientConnected = false;
        }
    }
    public bool ReceiptClientConnected => receiptClientConnected;

    bool ParseReceiptJson(string json) {
        DataToken tok;
        DataToken[] receipts;
        
        if (VRCJson.TryDeserializeFromJson(json, out tok) && tok.TokenType == TokenType.DataList) {
            receipts = tok.DataList.ToArray();
        } else {
            Debug.Log("Failed to deserialize receipt JSON as a DataList.");
            return false;
        }

        var newReceipts = new DataList();
        foreach (var receiptTok in receipts) {
            if (receiptTok.TokenType != TokenType.DataDictionary) {
                Debug.Log("Skipping non data dictionary object");
                continue;
            }
            var receipt = receiptTok.DataDictionary;
            var id = ReceiptReceiptId(receipt);
            if (id == null) {
                Debug.Log("Skipping receipt without receipt_id");
                continue;
            }

            // NOTE: we should always keep all receipts locally,
            // because we cannot tell the server that we have accepted it
            // due to VRC restrictions.
            // if (acceptedReceiptIds.Contains(id)) {
            //     Debug.Log($"Skipping already accepted receipt {id}");
            //     continue;
            // }
            newReceipts.Add((DataDictionary) receipt);
        }

        // Only update the cachedReceipts if we have new receipts
        var updated = false;
        if (cachedReceipts.Count != newReceipts.Count) {
            updated = true;
        } else {
            // NOTE: receipts returned by server are always sorted, so
            // it is safe to compare them by index
            for (int i = 0; i < cachedReceipts.Count; i++) {
                if (ReceiptReceiptId(ReceiptAtIndex(cachedReceipts, i)) != ReceiptReceiptId(ReceiptAtIndex(newReceipts, i))) {
                    updated = true;
                    break;
                }
            }
        }
        
        if (updated) {
            cachedReceipts = newReceipts;
            // Now clean the acceptedReceiptIds list: if the uuid is not used
            // by the server anymore, we should remove it from the local list
            // to avoid potential uuid clashes.
            for (int i = 0; i < acceptedReceiptIds.Count; i++) {
                // cachedReceipts.NoneMatch(receipt => receipt["receipt_id"].String == acceptedReceiptIds[i].String)
                bool noneMatch = true;
                for (int j = 0; j < cachedReceipts.Count; j++) {
                    var cachedId = ReceiptReceiptId(ReceiptAtIndex(cachedReceipts, j));
                    if (cachedId != null && cachedId == acceptedReceiptIds[i].String) {
                        noneMatch = false;
                        break;
                    }
                }
                // noneMatch, imples this uuid is not in the cachedReceipts, free it
                if (noneMatch) {
                    Debug.Log($"Removing accepted receipt {acceptedReceiptIds[i].String}");
                    acceptedReceiptIds.RemoveAt(i);
                    i--;
                }
            }
        }

        return updated;
    }

    public void FetchReceipt() {
        // race condition: if user disabled the client while we are waiting for the next fetch.
        if (!ReceiptClientEnabled) return;
        if (getReceiptUrl == null || getReceiptUrl == VRCUrl.Empty || string.IsNullOrEmpty(getReceiptUrl.Get())) {
            Debug.Log("FetchReceipt: receipt api url is empty, skipping");
            return;
        }
        Debug.Log($"Fetching receipt from {getReceiptUrl.Get()}");
        VRCStringDownloader.LoadUrl(getReceiptUrl, (IUdonEventReceiver) this);
    }

    public override void OnStringLoadSuccess(IVRCStringDownload result) {
        // race condition: if user disabled the client while the request is in flight.
        if (!ReceiptClientEnabled) return;

        if (!receiptClientConnected) {
            receiptClientConnected = true;
            SendReceiptConnectedEvent();
        }

        string resultAsUTF8 = result.Result;
        Debug.Log($"FetchReceipt: UTF8: {resultAsUTF8}");
        var updated = ParseReceiptJson(resultAsUTF8);
        Debug.Log($"FetchReceipt: Updated = {updated}");

        // race condition: if user disabled the client while we are processing the result.
        if (updated && ReceiptClientEnabled) {
            SendReceiptUpdatedEvent();
        }
        // race condition: if user disabled the client while we are processing the result.
        if (ReceiptClientEnabled) {
            SendCustomEventDelayedSeconds(nameof(FetchReceipt), 15.0f);
        }
    }

    public override void OnStringLoadError(IVRCStringDownload result) {
        // race condition: if user disabled the client while the request is in flight.
        if (!ReceiptClientEnabled) return;

        if (receiptClientConnected) {
            receiptClientConnected = false;
            SendReceiptDisconnectedEvent();
        }

        Debug.LogError($"FetchReceipt: Error loading string: {result.ErrorCode} - {result.Error}");

        // race condition: if user disabled the client while we are processing the result.
        if (ReceiptClientEnabled) {
            SendCustomEventDelayedSeconds(nameof(FetchReceipt), 15.0f);
        }
    }

    void Start() {
        Debug.Log("ReceiptClient loaded");
    }
}
