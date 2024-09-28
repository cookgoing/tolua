using UnityEngine;
using System.Collections;
using LuaInterface;
using System;

public class TestOutArg : MonoBehaviour 
{            
    string script =
        @"                                
            local box = UnityEngine.BoxCollider
                                                                            
            function TestPick(ray)                                                                  
                local _layer = 2 ^ LayerMask.NameToLayer('Default')                
                local time = os.clock()                                                  
                local flag, hit = UnityEngine.Physics.Raycast(ray, nil, 5000, _layer)                -- UnityEngine_PhysicsWrap 中改写了 Raycast 的逻辑，让 RayCastHit不以out的形式返回，而是以函数返回值的方式返回                             
                --local flag, hit = UnityEngine.Physics.Raycast(ray, RaycastHit.out, 5000, _layer)                                
                                
                if flag then
                    print('pick from lua, point: '..tostring(hit.point))     --注意哦，这里的 RaycastHit 不是桥梁，而是tolua自定义的脚本：RayCastHit.lua                              
                end
            end
        ";

    LuaState state = null;
    LuaFunction func = null;
    string tips = string.Empty;

    void Start () 
    {
#if UNITY_5 || UNITY_2017 || UNITY_2018
        Application.logMessageReceived += ShowTips;
#else
        Application.RegisterLogCallback(ShowTips);
#endif
        state = new LuaState();
        LuaBinder.Bind(state);
        state.Start();
        state.DoString(script, "TestOutArg.cs");
        func = state.GetFunction("TestPick");        
	}

    void ShowTips(string msg, string stackTrace, LogType type)
    {
        tips += msg;
        tips += "\r\n";
    }

    void OnApplicationQuit()
    {
#if UNITY_5 || UNITY_2017 || UNITY_2018
        Application.logMessageReceived -= ShowTips;
#else
        Application.RegisterLogCallback(null);
#endif        
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(Screen.width / 2 - 300, Screen.height / 2 - 300, 600, 600), tips);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Camera camera = Camera.main;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);                  
            RaycastHit hit;
            bool flag = Physics.Raycast(ray, out hit, 5000, 1 << LayerMask.NameToLayer("Default"));            

            if (flag)
            {
                Debugger.Log("pick from c#, point: [{0}, {1}, {2}]", hit.point.x, hit.point.y, hit.point.z);
            }

            func.BeginPCall();
            func.Push(ray);
            func.PCall();
            func.EndPCall();
        }

        state.CheckTop();
        state.Collect();
    }

    void OnDestroy()
    {
        func.Dispose();
        func = null;

        state.Dispose();
        state = null;
    }
}
