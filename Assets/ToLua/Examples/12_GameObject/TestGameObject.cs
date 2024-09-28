using UnityEngine;
using System.Collections;
using LuaInterface;

public class TestGameObject: MonoBehaviour
{
    private string script =
        @"                                                
            local Color = UnityEngine.Color 				--这个Color不是桥梁，它是tolua自定义的脚本：Color.lua
            local GameObject = UnityEngine.GameObject
            local ParticleSystem = UnityEngine.ParticleSystem 

            function OnComplete()
                print('OnComplete CallBack')
            end                       
            
            local go = GameObject('go')
            go:AddComponent(typeof(ParticleSystem))
            local node = go.transform
            node.position = Vector3.one                  
            print('gameObject is: '..tostring(go))            -- UnityEngine_GameObjectWrap 重写了 __tostring     
            --go.transform:DOPath({Vector3.zero, Vector3.one * 10}, 1, DG.Tweening.PathType.Linear, DG.Tweening.PathMode.Full3D, 10, nil)
            --go.transform:DORotate(Vector3(0,0,360), 2, DG.Tweening.RotateMode.FastBeyond360):OnComplete(OnComplete)            
            GameObject.Destroy(go, 2)                  
            go.name = '123'
            --print('delay destroy gameobject is: '..go.name)                                           
        ";

    LuaState lua = null;

    void Start()
    {
#if UNITY_5 || UNITY_2017 || UNITY_2018
        Application.logMessageReceived += ShowTips;
#else
        Application.RegisterLogCallback(ShowTips);
#endif
        lua = new LuaState();
        lua.LogGC = true;
        lua.Start();
        LuaBinder.Bind(lua);
        lua.DoString(script, "TestGameObject.cs");
    }

    void Update()
    {
        lua.CheckTop();
        lua.Collect();        
    }

    string tips = "";
        
    void ShowTips(string msg, string stackTrace, LogType type)
    {
        tips += msg;
        tips += "\r\n";
    }

    void OnApplicationQuit()
    {        
        lua.Dispose();
        lua = null;
#if UNITY_5 || UNITY_2017 || UNITY_2018
        Application.logMessageReceived -= ShowTips;
#else
        Application.RegisterLogCallback(null);
#endif    
    }

    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width / 2 - 300, Screen.height / 2 - 300, 600, 600), tips);
    }
}
