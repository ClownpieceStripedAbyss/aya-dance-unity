using System;
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

/// <summary>
/// This class is a basic, drag-n-drop implementation of VRCX 'PWI' World API (wow acronyms are fun) for UdonSharp.
/// This can be dropped onto any active object in the scene and will automatically initialize itself and cache the data stored for the current world, if any.
/// <br/> To store data, simply get this component and call StoreData(key, value) with the key and value you want to store. 
/// <br/> Credits to Nekromateion for the initial implementation of this class.
/// </summary>
public class PWIManager : UdonSharpBehaviour
{
    private VRCUrl urlGetAll = new VRCUrl("http://127.0.0.1:22500/vrcx/data/getall"); // URL to retrieve all world data from WorldDB

    [NonSerialized]
    private DataDictionary cachedTokens = new DataDictionary();
    [NonSerialized]
    private string connectionKey = null;
    /// <summary>
    /// False if initialization is not yet done. Do not use any of the methods until this is True.
    /// </summary>
    [NonSerialized]
    public bool Initialized = false;
    /// <summary>
    /// True if VRCX is not running, leaving PWI unusable. Do not use any of the methods if this is true.
    /// </summary>
    [NonSerialized]
    public bool VRCXUnavailable = false;
    [NonSerialized]
    private int initAttempts = 0;
    [Tooltip("The max amount of times to attempt to initialize a connection to VRCX before giving up.")]
    public readonly int MaxInitAttempts = 5;

    /// <summary>
    /// Returns whether the PWIManager has been initialized and VRCX is available.
    /// </summary>
    /// <returns>True if PWIManager has been initialized and VRCX is available, otherwise false.</returns>
    public bool IsReady()
    {
        return Initialized && !VRCXUnavailable;
    }

    /// <summary>
    /// Parses the response from a VRCX API request and returns the data string if successful.
    /// </summary>
    /// <param name="response">The response string from the API request.</param>
    /// <returns>The data string from the API response if successful, otherwise null.</returns>
    private string ParseVRCXApiResponse(string response)
    {
        if (string.IsNullOrEmpty(response))
        {
            Debug.LogWarning("VRCX API: Response was null.");
            return null;
        }

        if (VRCJson.TryDeserializeFromJson(response, out DataToken result))
        {
            if (result.TokenType == TokenType.DataDictionary)
            {
                DataDictionary data = (DataDictionary)result;

                // Check if error exists
                if (data.TryGetValue("error", out var errorToken) && !errorToken.IsNull)
                {
                    Debug.LogWarning($"VRCX API: Response contained error: {errorToken}");
                    return null;
                }

                // Check if ok is present and true
                if (data.TryGetValue("ok", out var okToken) && !okToken.Boolean)
                {
                    Debug.LogWarning($"VRCX API: Response was not ok but contained no error??? Response: {response}");
                    return null;
                }

                // Check if connection key exists and store it
                // This is sent with *almost* every response, but not all of them, so check if it's null
                if (connectionKey == null && data.TryGetValue("connectionKey", out var connectionKeyToken) && !connectionKeyToken.IsNull)
                {
                    connectionKey = connectionKeyToken.String;
                    Debug.Log("VRCX API: Connection key received: " + connectionKey);
                }

                // Check if data exists and return it
                if (data.TryGetValue("data", out var dataToken) && !dataToken.IsNull)
                {
                    return dataToken.String;
                }
                else
                {
                    Debug.LogWarning($"VRCX API: Response did not contain data. Response: {response}");
                }
            }
            else
            {
                Debug.LogWarning($"VRCX API: Response returned valid json but wasn't a dictionary??? TokenType: " + result.TokenType.ToString());
            }
        }
        else
        {
            Debug.LogWarning($"VRCX API: Failed to parse response. Response: {response}. Result: {result.ToString()}");
        }

        return null;
    }

    /// <summary>
    /// Retrieve a sbyte from VRCX using the provided key.
    /// </summary>
    /// <param name="key">The key to store the data under.</param>
    /// <returns>True if the item was found and False if the item was not found.</returns>
    public bool TryGetSByte(string key, out sbyte value)
    {
        var res = cachedTokens.TryGetValue(key, out var data);
        if (res)
            res = sbyte.TryParse(data.ToString(), out value);
        else
            value = default(sbyte);
        return res;
    }

    /// <summary>
    /// Retrieve a byte from VRCX using the provided key.
    /// </summary>
    /// <param name="key">The key to store the data under.</param>
    /// <returns>True if the item was found and False if the item was not found.</returns>
    public bool TryGetByte(string key, out byte value)
    {
        var res = cachedTokens.TryGetValue(key, out var data);
        if (res)
            res = byte.TryParse(data.ToString(), out value);
        else
            value = default(byte);
        return res;
    }

    /// <summary>
    /// Retrieve a short from VRCX using the provided key.
    /// </summary>
    /// <param name="key">The key to store the data under.</param>
    /// <returns>True if the item was found and False if the item was not found.</returns>
    public bool TryGetShort(string key, out short value)
    {
        var res = cachedTokens.TryGetValue(key, out var data);
        if (res)
            res = short.TryParse(data.ToString(), out value);
        else
            value = default(short);
        return res;
    }

    /// <summary>
    /// Retrieve a ushort from VRCX using the provided key.
    /// </summary>
    /// <param name="key">The key to store the data under.</param>
    /// <returns>True if the item was found and False if the item was not found.</returns>
    public bool TryGetUShort(string key, out ushort value)
    {
        var res = cachedTokens.TryGetValue(key, out var data);
        if (res)
            res = ushort.TryParse(data.ToString(), out value);
        else
            value = default(ushort);
        return res;
    }

    /// <summary>
    /// Retrieve a long from VRCX using the provided key.
    /// </summary>
    /// <param name="key">The key to store the data under.</param>
    /// <returns>True if the item was found and False if the item was not found.</returns>
    public bool TryGetLong(string key, out long value)
    {
        var res = cachedTokens.TryGetValue(key, out var data);
        if (res)
            res = long.TryParse(data.ToString(), out value);
        else
            value = default(long);
        return res;
    }

    /// <summary>
    /// Retrieve a ulong from VRCX using the provided key.
    /// </summary>
    /// <param name="key">The key to store the data under.</param>
    /// <returns>True if the item was found and False if the item was not found.</returns>
    public bool TryGetULong(string key, out ulong value)
    {
        var res = cachedTokens.TryGetValue(key, out var data);
        if (res)
            res = ulong.TryParse(data.ToString(), out value);
        else
            value = default(ulong);
        return res;
    }

    /// <summary>
    /// Retrieve a int from VRCX using the provided key.
    /// </summary>
    /// <param name="key">The key to store the data under.</param>
    /// <returns>True if the item was found and False if the item was not found.</returns>
    public bool TryGetInt(string key, out int value)
    {
        var res = cachedTokens.TryGetValue(key, out var data);
        if (res)
            res = int.TryParse(data.ToString(), out value);
        else
            value = default(int);
        return res;
    }

    /// <summary>
    /// Retrieve a uint from VRCX using the provided key.
    /// </summary>
    /// <param name="key">The key to store the data under.</param>
    /// <returns>True if the item was found and False if the item was not found.</returns>
    public bool TryGetUInt(string key, out uint value)
    {
        var res = cachedTokens.TryGetValue(key, out var data);
        if (res)
            res = uint.TryParse(data.ToString(), out value);
        else
            value = default(uint);
        return res;
    }

    /// <summary>
    /// Retrieve a double from VRCX using the provided key.
    /// </summary>
    /// <param name="key">The key to store the data under.</param>
    /// <returns>True if the item was found and False if the item was not found.</returns>
    public bool TryGetDouble(string key, out double value)
    {
        var res = cachedTokens.TryGetValue(key, out var data);
        if (res)
            res = double.TryParse(data.ToString(), out value);
        else
            value = default(double);
        return res;
    }

    /// <summary>
    /// Retrieve a float from VRCX using the provided key.
    /// </summary>
    /// <param name="key">The key to store the data under.</param>
    /// <returns>True if the item was found and False if the item was not found.</returns>
    public bool TryGetFloat(string key, out float value)
    {
        var res = cachedTokens.TryGetValue(key, out var data);
        if (res)
            res = float.TryParse(data.ToString(), out value);
        else
            value = default(float);
        return res;
    }

    /// <summary>
    /// Retrieve a boolean value from VRCX using the provided key.
    /// </summary>
    /// <param name="key">The key to store the data under.</param>
    /// <param name="value">The boolean value retrieved from VRCX.</param>
    /// <returns>True if the item was found; False if the item was not found.</returns>
    public bool TryGetBool(string key, out bool value)
    {
        var res = cachedTokens.TryGetValue(key, out var data);
        if (res)
            res = bool.TryParse(data.ToString(), out value);
        else
            value = false;
        return res;
    }

    /// <summary>
    /// Retrieve a DataList (JSON array representation) from VRCX using the provided key.
    /// </summary>
    /// <param name="key">The key to store the data under.</param>
    /// <param name="value">The DataList value retrieved from VRCX.</param>
    /// <returns>True if the item was found and deserialized; False if the item was not found or failed to deserialize as a token.</returns>
    public bool TryGetDataList(string key, out DataList value)
    {
        var res = cachedTokens.TryGetValue(key, out var data);
        if (res)
        {
            if (VRCJson.TryDeserializeFromJson(data.String, out DataToken token))
            {
                if (token.TokenType == TokenType.DataList)
                {
                    // We could cache this result but since DataToken stores a reference to the object, if the user modifies the object then the cached result would be invalid when they next retrieve it.
                    // We could reconstruct it every time but I feel like that would just be even worse than this
                    value = token.DataList;
                    res = true;
                }
            }
        }

        value = null;
        res = false;
        return res;
    }

    /// <summary>
    /// Retrieve a DataDictionary (JSON object representation) from VRCX using the provided key.
    /// </summary>
    /// <param name="key">The key to store the data under.</param>
    /// <param name="value">The DataDictionary value retrieved from VRCX.</param>
    /// <returns>True if the item was found and deserialized; False if the item was not found or failed to deserialize as a token.</returns>
    public bool TryGetDataDictionary(string key, out DataDictionary value)
    {

        var res = cachedTokens.TryGetValue(key, out var data);
        if (res)
        {
            if (VRCJson.TryDeserializeFromJson(data.String, out DataToken token))
            {
                if (token.TokenType == TokenType.DataDictionary)
                {
                    // We could cache this result but since DataToken stores a reference to the object, if the user modifies the object then the cached result would be invalid when they next retrieve it.
                    value = token.DataDictionary;
                    res = true;
                }
            }
        }

        value = null;
        res = false;
        return res;
    }

    /// <summary>
    /// Retrieve a Vector3 from VRCX using the provided key.
    /// This function actually just looks for a DataList with 3 items in it and tries to parse them as floats.
    /// </summary>
    /// <param name="key">The key to store the data under.</param>
    /// <param name="value">The Vector3 value retrieved from VRCX.</param>
    /// <returns>True if the item was found and False if the item was not found.</returns>
    public bool TryGetVector3(string key, out Vector3 value)
    {
        var res = cachedTokens.TryGetValue(key, out var data);
        if (res)
        {
            if (VRCJson.TryDeserializeFromJson(data.String, out DataToken token))
            {
                if (token.TokenType == TokenType.DataList)
                {
                    var dataList = token.DataList;

                    if (dataList.Count == 3)
                    {
                        res = float.TryParse(dataList[0].ToString(), out var x);
                        res = float.TryParse(dataList[1].ToString(), out var y);
                        res = float.TryParse(dataList[2].ToString(), out var z);
                        value = new Vector3(x, y, z);

                        return res;
                    }
                }
            }
        }

        value = Vector3.zero;
        res = false;
        return res;
    }

    /// <summary>
    /// Retrieve a Vector2 from VRCX using the provided key.
    /// This function actually just looks for a DataList with 2 items in it and tries to parse them as floats.
    /// </summary>
    /// <param name="key">The key to store the data under.</param>
    /// <param name="value">The Vector2 value retrieved from VRCX.</param>
    /// <returns>True if the item was found and parsed correctly and False if the item was not found or datalist found did not contain 2 entries.</returns>
    public bool TryGetVector2(string key, out Vector2 value)
    {
        var res = cachedTokens.TryGetValue(key, out var data);
        if (res)
        {
            if (VRCJson.TryDeserializeFromJson(data.String, out DataToken token))
            {
                if (token.TokenType == TokenType.DataList)
                {
                    var dataList = token.DataList;
                    if (dataList.Count == 2)
                    {
                        res = float.TryParse(dataList[0].ToString(), out var x);
                        res = float.TryParse(dataList[1].ToString(), out var y);
                        value = new Vector2(x, y);
                        return res;
                    }
                }
            }
        }

        value = Vector2.zero;
        res = false;
        return res;
    }

    /// <summary>
    /// Retrieve a Quaternion from VRCX using the provided key.
    /// This function actually just looks for a DataList with 4 items in it and tries to parse them as floats.
    /// </summary>
    /// <param name="key">The key to store the data under.</param>
    /// <param name="value">The Quaternion value retrieved from VRCX.</param>
    /// <returns>True if the item was found and parsed correctly and False if the item was not found or datalist found did not contain 4 entries.</returns>
    public bool TryGetQuaternion(string key, out Quaternion value)
    {
        var res = cachedTokens.TryGetValue(key, out var data);
        if (res)
        {
            if (VRCJson.TryDeserializeFromJson(data.String, out DataToken token))
            {
                if (token.TokenType == TokenType.DataList)
                {
                    var dataList = token.DataList;
                    if (dataList.Count == 4)
                    {
                        res = float.TryParse(dataList[0].ToString(), out var x);
                        res = float.TryParse(dataList[1].ToString(), out var y);
                        res = float.TryParse(dataList[2].ToString(), out var z);
                        res = float.TryParse(dataList[3].ToString(), out var w);
                        value = new Quaternion(x, y, z, w);
                        return res;
                    }
                }
            }
        }

        value = Quaternion.identity;
        res = false;
        return res;
    }

    /// <summary>
    /// Retrieve a string from VRCX using the provided key.
    /// </summary>
    /// <param name="key">The key to store the data under.</param>
    /// <param name="value">The string value retrieved from VRCX.</param>
    /// <returns>True if the item was found and False if the item was not found.</returns>
    public bool TryGetString(string key, out string value)
    {
        var res = cachedTokens.TryGetValue(key, out var data);
        if (res)
            value = data.String;
        else
            value = null;

        return res;
    }

    /// <summary>
    /// Retrieve data from VRCX using the provided key. This function will always return a DataToken with a type of String. This is for if you want to do your own parsing for data types.
    /// </summary>
    /// <param name="key">The key to store the data under.</param>
    /// <returns>True if the item was found and False if the item was not found.</returns>
    public bool TryGetData(string key, out DataToken value)
    {
        return cachedTokens.TryGetValue(key, out value);
    }

    /// <summary>
    /// Gets all currently cached keys. You can use this to iterate over all keys and retrieve the values.
    /// Use after /getall to have a full list of all entries for your world. (or another, if you have permission)
    /// </summary>
    /// <returns>A Datalist containing the keys.</returns>
    public DataList GetKeys()
    {
        return cachedTokens.GetKeys();
    }

    public string ConvertDataTokenToString(DataToken token)
    {
        if (token.IsNumber)
        {
            return token.ToString();
        }

        switch (token.TokenType)
        {
            case TokenType.String:
                return token.String;
            case TokenType.Boolean:
                return token.Boolean.ToString();
            case TokenType.Reference:
                return token.Reference.ToString();
            case TokenType.DataList:
                if (VRCJson.TrySerializeToJson(token.DataList, JsonExportType.Minify, out DataToken json))
                    return json.ToString();
                else
                    Debug.Log("Failed to convert DataList to string");
                return null;
            case TokenType.DataDictionary:
                if (VRCJson.TrySerializeToJson(token.DataDictionary, JsonExportType.Minify, out DataToken json2))
                    return json2.ToString();
                else
                    Debug.Log("Failed to convert DataDictionary to string");
                return null;
            case TokenType.Null:
                return null;
            default:
                return null;
        }
    }

    /// <summary>
    /// This method is responsible for storing data in VRCX. It takes a key-value pair, where the value is an instance of DataToken.
    /// </summary>
    /// <remarks>
    /// Be aware that the DataToken provided is always transformed into a string before it's cached and sent to VRCX. 
    /// This means that if you retrieve the data you've stored by using TryGetData, you will obtain a new DataToken object. 
    /// This new object will be <c>TokenType.String</c> and will contain the string representation of the stored data. It is important to understand that you won't receive/store a reference to your original DataToken.
    /// <br/> This function supports the storing of any data type supported by <c>DataToken.</c>
    /// </remarks>
    /// <param name="key">The unique identifier used to save and retrieve the data.</param>
    /// <param name="value">The DataToken object containing the data to be stored.</param>
    /// <example>
    /// Here is how you can use this method:
    /// <code>
    /// StoreData("YourKey1", 1);
    /// StoreData("YourKey2", "test");
    /// StoreData("YourKey3", false);
    /// StoreData("YourKey4", new DataToken("a str token"));
    /// StoreData("YourKey5", new DataList());
    /// </code>
    /// </example>
    public void StoreData(string key, DataToken value)
    {
        string valueStr = ConvertDataTokenToString(value);
        cachedTokens[key] = new DataToken(valueStr);

        DataDictionary dict = new DataDictionary();
        dict.Add("requestType", "store");
        dict.Add("connectionKey", connectionKey);
        dict.Add("key", key);
        dict.Add("value", valueStr);

        if (VRCJson.TrySerializeToJson(dict, JsonExportType.Minify, out DataToken json))
        {
            Debug.Log("[VRCX-World] " + json);
        }
        else
        {
            Debug.LogWarning($"VRCX API: Failed to serialize VRCX store request ({json}): {dict.ToString()}");
            return;
        }
    }

    /// <summary>
    /// Stores a Vector3 value in VRCX using the provided key.
    /// </summary>
    /// <param name="key">The key to store the data under.</param>
    /// <param name="value">The Vector3 value to store in VRCX.</param>
    public void StoreVector3(string key, Vector3 value)
    {
        DataList list = new DataList();
        list.Add(value.x);
        list.Add(value.y);
        list.Add(value.z);

        StoreData(key, new DataToken(list));
    }

    /// <summary>
    /// Stores a Vector2 value in VRCX using the provided key.
    /// </summary>
    /// <param name="key">The key to store the data under.</param>
    /// <param name="value">The Vector2 value to store in VRCX.</param>
    public void StoreVector2(string key, Vector2 value)
    {
        DataList list = new DataList();
        list.Add(value.x);
        list.Add(value.y);

        StoreData(key, new DataToken(list));
    }

    /// <summary>
    /// Stores a Quaternion value in VRCX using the provided key.
    /// </summary>
    /// <param name="key">The key to store the data under.</param>
    /// <param name="value">The Quaternion value to store in VRCX.</param>
    public void StoreQuaternion(string key, Quaternion value)
    {
        DataList list = new DataList();
        list.Add(value.x);
        list.Add(value.y);
        list.Add(value.z);
        list.Add(value.w);
        StoreData(key, new DataToken(list));
    }

    /// <summary>
    /// This method is responsible for deleting data in VRCX.
    /// If the key is not found, nothing happens. No error is thrown.
    /// </summary>
    /// <param name="key">The key of the row to delete</param>
    public void DeleteData(string key)
    {
        if (cachedTokens.ContainsKey(key)) 
        {
            cachedTokens.Remove(key);
        }
        
        DataDictionary dict = new DataDictionary();
        dict.Add("requestType", "delete");
        dict.Add("connectionKey", connectionKey);
        dict.Add("key", key);

        if (VRCJson.TrySerializeToJson(dict, JsonExportType.Minify, out DataToken json))
        {
            Debug.Log("[VRCX-World] " + json);
        }
        else
        {
            Debug.LogWarning($"VRCX API: Failed to serialize VRCX delete request ({json}): {dict.ToString()}");
            return;
        }
    }

    /// <summary>
    /// This method deletes all data belonging to the current world in VRCX and the local cache.
    /// </summary>
    public void DeleteAllData()
    {
        cachedTokens.Clear();
        DataDictionary dict = new DataDictionary();
        dict.Add("requestType", "delete-all");
        dict.Add("connectionKey", connectionKey);

        if (VRCJson.TrySerializeToJson(dict, JsonExportType.Minify, out DataToken json))
        {
            Debug.Log("[VRCX-World] " + json);
        }
        else
        {
            Debug.LogWarning($"VRCX API: Failed to serialize VRCX delete-all request ({json}): {dict.ToString()}");
            return;
        }
    }

    /// <summary>
    /// Resets the cache by loading the URL for getting all data from the VRCX database again.
    /// </summary>
    public void ResetCache()
    {
        VRCStringDownloader.LoadUrl(urlGetAll, (IUdonEventReceiver)this);
    }

    /// <summary>
    /// Sets whether or not the external reading of this world's data by other worlds is enabled
    /// </summary>
    /// <param name="state">Whether external reading will be enabled</param>
    public void SetExternalReading(bool state)
    {
        DataDictionary dict = new DataDictionary();
        dict.Add("requestType", "set-setting");
        dict.Add("connectionKey", connectionKey);
        dict.Add("key", "externalReads");
        dict.Add("value", state.ToString());

        if (VRCJson.TrySerializeToJson(dict, JsonExportType.Minify, out DataToken json))
        {
            Debug.Log("[VRCX-World] " + json);
        }
        else
        {
            Debug.LogWarning($"VRCX API: Failed to serialize VRCX set-setting request ({json}): {dict.ToString()}");
            return;
        }
    }

    public void Start()
    {
#if !UNITY_EDITOR
        VRCStringDownloader.LoadUrl(urlGetAll, (IUdonEventReceiver)this);
#endif
    }

    public override void OnStringLoadSuccess(IVRCStringDownload result)
    {
        //Debug.Log($"StringLoad '{result.Url}' Success: {result.Result}");

        var url = result.Url;

        // /vrcx/data/getall
        if (url.Equals(urlGetAll))
        {
            // Parse the JSON response. Returns the string value of 'data'
            // This will also set our connectionKey
            string data = ParseVRCXApiResponse(result.Result);

            if (!string.IsNullOrEmpty(data) && VRCJson.TryDeserializeFromJson(data, out var dataToken))
                cachedTokens = dataToken.DataDictionary; // Override the cache with the new data

            Initialized = true;
            Debug.Log("VRCX API: PWI Manager got all stored world data. Existing keys: " + cachedTokens.Count);
        }
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        Debug.LogError($"VRCX API: StringLoad '{result.Url}' Fail {result.ErrorCode} - '{result.Error}': {result.Result}");

        var url = result.Url;

        // If we can't connect to VRCX, mark it as unavailable
        if (!Initialized && result.ErrorCode == 0 && result.Error.Contains("Cannot connect"))
        {
            Debug.LogError("VRCX API: VRCX is not running.");
            VRCXUnavailable = true;
            return;
        }

        // If we're trying to initialize through /getall and we get a 500 or 503 error, retry up to MaxInitAttempts times
        if (url.Equals(urlGetAll) && (result.ErrorCode == 500 || result.ErrorCode == 503) && initAttempts <= MaxInitAttempts)
        {
            initAttempts++;
            Debug.LogError($"VRCX API: Failed to initialize through /getall. Retry #{initAttempts}...");
            VRCStringDownloader.LoadUrl(urlGetAll, (IUdonEventReceiver)this);
        }

        // If we've reached the max attempts, mark VRCX as unavailable, but only if this is the first time we've used /getall 
        if (initAttempts >= MaxInitAttempts && !Initialized)
        {
            Debug.LogError("VRCX API: Failed to initialize through /getall. Max attempts reached. Marking VRCX as unavailable");
            VRCXUnavailable = true;
        }
    }
}