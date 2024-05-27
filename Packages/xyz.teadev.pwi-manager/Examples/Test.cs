using System;
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.InputSystem;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon.Common;

public class Test : UdonSharpBehaviour
{
    private GameObject Cube;
    private bool init = false;
    private bool ded = false;
    public PWIManager pwiManager;
    private Vector3 lastPlayerPos = Vector3.zero;
    private Quaternion lastPlayerRot = Quaternion.identity;

    void Start()
    {
        this.Cube = this.gameObject;

        // If you don't want to pass in a gameobject with PWIManager in the editor, you can also just get the component at runtime like you would any other gameobject.
        // Though you can do this without GameObject.Find
        var manager = GameObject.Find("VRCXHelper").GetComponent<PWIManager>();
        Debug.Log("Got manager: " + manager.gameObject.name);

        SetCubeColor(Color.black);
        Cube.GetComponent<Renderer>().material.color = Color.black;
        Cube.GetComponent<Renderer>().material.SetFloat("_Metallic", 1.0f);

    }

    private void Init()
    {
        if (init) return;
        init = true;

        Debug.Log("Test Script doing Init");

        if (pwiManager.TryGetBool("bool_key", out var boolValue)) Debug.Log("bool_key: " + boolValue); else Debug.Log("bool_key not stored yet");
        if (pwiManager.TryGetInt("int_key", out var intValue)) Debug.Log("int_key: " + intValue); else Debug.Log("int_key not stored yet");
        if (pwiManager.TryGetFloat("float_key", out var floatValue)) Debug.Log("float_key: " + floatValue); else Debug.Log("float_key not stored yet");
        if (pwiManager.TryGetString("string_key", out var stringValue)) Debug.Log("string_key: " + stringValue); else Debug.Log("string_key not stored yet");
        if (pwiManager.TryGetVector3("vector3_key", out var vector3Value)) Debug.Log("vector3_key: " + vector3Value); else Debug.Log("vector3_key not stored yet");
        if (pwiManager.TryGetVector3("vector3_key2", out var vector3Value2)) Debug.Log("vector3_key2: " + vector3Value); else Debug.Log("vector3_key2 not stored yet");
        if (pwiManager.TryGetQuaternion("quaternion_key", out var quaternionValue)) Debug.Log("quaternion_key: " + quaternionValue); else Debug.Log("quaternion_key not stored yet");
        if (pwiManager.TryGetDataList("datalist_key", out var dataListValue)) Debug.Log($"datalist_key (Items: {dataListValue.Count}): " + dataListValue); else Debug.Log("datalist_key not stored yet");
        if (pwiManager.TryGetDataDictionary("datadictionary_key", out var dataDictionaryValue)) Debug.Log($"datadictionary_key (Keys: {dataDictionaryValue.Count}): " + dataDictionaryValue); else Debug.Log("datadictionary_key not stored yet");
        if (pwiManager.TryGetString("token_key", out var dataTokenValue)) Debug.Log("token_key: " + dataTokenValue); else Debug.Log("token_key not stored yet");
        if (pwiManager.TryGetInt("token_key2", out var dataTokenValue2)) Debug.Log("token_key: " + dataTokenValue); else Debug.Log("token_key not2 stored yet");

        DataList vectorList = new DataList();
        vectorList.Add(1);
        vectorList.Add(2);
        vectorList.Add(3);

        DataList list = new DataList();
        list.AddRange(vectorList);

        DataDictionary dictionary = new DataDictionary();
        dictionary.Add("key1", 1);
        dictionary.Add("key2", 2);
        dictionary.Add("key3", 3);

        pwiManager.StoreData("bool_key", true);
        pwiManager.StoreData("int_key", 123);
        pwiManager.StoreData("float_key", 123.456f);
        pwiManager.StoreData("string_key", "Hello World");
        pwiManager.StoreVector3("vector3_key", new Vector3(1, 2, 3));
        pwiManager.StoreData("vector3_key2", vectorList); // StoreVector3/2 is just a wrapper for StoreData that creates a DataList for you
        pwiManager.StoreQuaternion("quaternion_key", new Quaternion(1, 2, 3, 4));
        pwiManager.StoreData("datalist_key", list);
        pwiManager.StoreData("datadictionary_key", dictionary);
        pwiManager.StoreData("token_key", new DataToken("Hello World 2"));
        pwiManager.StoreData("token_key2", new DataToken(3453545));

        // sussy requests
        pwiManager.StoreData("dungeon-crawl", "'; DELETE FROM data WHERE world_id='wrld_12345' --");
        pwiManager.StoreData("stone-soup", "'; DELETE FROM data WHERE world_id='wrld_12345' --");
        pwiManager.DeleteData("play-it' OR 1=1;");

        if (pwiManager.TryGetVector3("cubepos", out Vector3 cubepos))
            SetCubePos(cubepos);

        if (pwiManager.TryGetVector3("playerpos", out Vector3 playerpos) && pwiManager.TryGetQuaternion("playerrot", out Quaternion playerrot))
            Networking.LocalPlayer.TeleportTo(playerpos, playerrot);

        // error checking.
        if (!pwiManager.TryGetBool("int_key", out var _1)) Debug.Log("int_key test OK");
        if (!pwiManager.TryGetInt("bool_key", out var _2)) Debug.Log("int_key test OK");
        if (!pwiManager.TryGetFloat("int_key", out var _3)) Debug.Log("int_key test OK");
        if (!pwiManager.TryGetString("int_key", out var _4)) Debug.Log("int_key test OK");
        if (!pwiManager.TryGetVector3("int_key", out var _5)) Debug.Log("int_key test OK");
        if (!pwiManager.TryGetQuaternion("int_key", out var _6)) Debug.Log("int_key test OK");
        if (!pwiManager.TryGetDataList("int_key", out var _7)) Debug.Log("int_key test OK");
        if (!pwiManager.TryGetDataDictionary("int_key", out var _8)) Debug.Log("int_key test OK");
        pwiManager.TryGetFloat("bool_key", out var _9);
        pwiManager.TryGetDouble("float_key", out var _10);
        if (pwiManager.TryGetInt("invalid_key", out var key))
        {
            Debug.Log("invalid_key test failed???: " + key);
        }
        else
        {
            Debug.Log("invalid_key test OK");
        }
        if (pwiManager.TryGetFloat("invalid_key", out var key2))
        {
            Debug.Log("invalid_key test failed???: " + key2);
        }
        else
        {
            Debug.Log("invalid_key test OK");
        }
        if (pwiManager.TryGetString("invalid_key", out var key3))
        {
            Debug.Log("invalid_key test failed???: " + key3);
        }
        else
        {
            Debug.Log("invalid_key test OK");
        }
        if (pwiManager.TryGetDataDictionary("invalid_key", out var key4))
        {
            Debug.Log("invalid_key test failed???: " + key4);
        }
        else
        {
            Debug.Log("invalid_key test OK");
        }
    }

    public override void OnDrop()
    {
        if (!pwiManager.IsReady())
        {
            return;
        }

        pwiManager.StoreVector3("cubepos", Cube.transform.position);
        SetCubeColor(Color.grey);
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (!pwiManager.IsReady())
        {
            return;
        }

        // local player obj is null when this is called, so we can just check that to see if this is us.
        if (Networking.LocalPlayer == null)
        {
            Debug.Log("Player leaving. Saving pos/rot to PWI.");
            // player obj is null when this is called.
            pwiManager.StoreVector3("playerpos", this.lastPlayerPos);
            pwiManager.StoreQuaternion("playerrot", this.lastPlayerRot);
            return;
        }
    }

    private void SetCubePos(Vector3 pos)
    {
        Cube.GetComponent<Rigidbody>().velocity = Vector3.zero;
        Cube.transform.rotation = Quaternion.identity;
        Cube.transform.position = pos;
        SetCubeColor(Color.green);
    }

    private void SetCubeColor(Color color)
    {
        Cube.GetComponent<Renderer>().material.color = color;
    }

    void Update()
    {
        if (!ded && pwiManager.VRCXUnavailable)
        {
            ded = true;
            SetCubeColor(Color.red);
            return;
        }

        if (!pwiManager.IsReady())
        {
            return;
        }

        // Cache player pos every 60 frames for onplayerleft event to store
        if (Time.frameCount % 60 == 0 && Utilities.IsValid(Networking.LocalPlayer))
        {
            this.lastPlayerPos = Networking.LocalPlayer.GetPosition();
            this.lastPlayerRot = Networking.LocalPlayer.GetRotation();
        }

        // Slowly change the color of the cube back to white if its not white
        if (Cube.GetComponent<Renderer>().material.color != Color.white)
        {
            SetCubeColor(Color.Lerp(Cube.GetComponent<Renderer>().material.color, Color.white, Time.deltaTime));
        }


        if (!init) Init();

        if (Input.GetKeyDown(KeyCode.L))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                Debug.Log("Manual Save Cube Position");
                pwiManager.StoreVector3("cubepos", Cube.transform.position);
                SetCubeColor(Color.grey);
            }
            else
            {
                if (pwiManager.TryGetVector3("cubepos", out Vector3 cubepos))
                {
                    Debug.Log("Manual Load Cube Position");
                    SetCubePos(cubepos);
                }
                else
                {
                    Debug.Log("Failed to manually load cube pos. Not stored yet?");
                    SetCubeColor(Color.red);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.P) && Input.GetKey(KeyCode.LeftShift))
        {
            pwiManager.DeleteAllData();
        }

        // this is just a test of how numbers are serialized/deserialized.
        // btw, all numbers are parsed as doubles.
        if (Input.GetKeyDown(KeyCode.O))
        {
            DataDictionary keyValuePairs
                = new DataDictionary();
            keyValuePairs.Add("bool", false);
            keyValuePairs.Add("float", new DataToken(5f));
            keyValuePairs.Add("int", new DataToken(5));
            keyValuePairs.Add("double", new DataToken(5d));
            keyValuePairs.Add("long", new DataToken(5L));
            DataDictionary keyValuePairs2
    = new DataDictionary();
            keyValuePairs2.Add("float", new DataToken(5f));
            keyValuePairs2.Add("int", new DataToken(5));
            keyValuePairs2.Add("double", new DataToken(5d));
            keyValuePairs2.Add("long", new DataToken(5L));
            keyValuePairs.Add("nestedDict", keyValuePairs2);
            DataList dataTokens = new DataList();
            dataTokens.Add(1);
            dataTokens.Add(2f);
            dataTokens.Add(2d);
            dataTokens.Add(2L);
            keyValuePairs.Add("nestedList", dataTokens);

            if(VRCJson.TrySerializeToJson(keyValuePairs, JsonExportType.Minify, out var json2))
            {
                Debug.Log(json2.String);
                if(VRCJson.TryDeserializeFromJson(json2.String, out var res))
                {
                    DataDictionary dict = res.DataDictionary;
                    for (int i = 0; i < dict.GetKeys().Count; i++)
                    {
                        DataToken key = dict.GetKeys()[i];
                        DataToken value = dict[key];
                        if (value.TokenType == TokenType.DataDictionary)
                        {
                            for (int j = 0; j < value.DataDictionary.GetKeys().Count; j++)
                            {
                                DataToken key2 = value.DataDictionary.GetKeys()[j];
                                DataToken value2 = dict[key2];
                                Debug.Log($"{key2.String} (from {key.String}) was {value2} {value2.TokenType}");
                            }
                        }
                        else if (value.TokenType == TokenType.DataList)
                        {
                            for (int j = 0; j < value.DataList.Count; j++)
                            {
                                DataToken item2 = value.DataList[j];
                                Debug.Log($"(from {key.String}) was {item2} {item2.TokenType}");
                            }
                        }
                        else
                        {
                            Debug.Log($"{key.String} was {value} {value.TokenType}");
                        }
                    }
                }
            }
        }
    }
}
