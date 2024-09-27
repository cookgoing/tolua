using UnityEngine;
using System.Collections;
using LuaInterface;

//两套协同勿交叉使用，类unity原生，大量使用效率低
public class TestCoroutine2 : LuaClient 
{
    string script =
    @"
        function CoExample()            
            WaitForSeconds(1)			--在 LuaCoroutine.cs中定义，看名字，就知道是干什么的。
            print('WaitForSeconds end time: '.. UnityEngine.Time.time)            
            WaitForFixedUpdate()
            print('WaitForFixedUpdate end frameCount: '..UnityEngine.Time.frameCount)
            WaitForEndOfFrame()
            print('WaitForEndOfFrame end frameCount: '..UnityEngine.Time.frameCount)
            Yield(null)
            print('yield null end frameCount: '..UnityEngine.Time.frameCount) -- todo: 为什么 yield null 跟 waitForEndOfFrame在同一帧; 而且这个结果跟我自己测试还不一样。个人觉得可能是Lua和C#通信导致滞后。
            Yield(0)
            print('yield(0) end frameCime: '..UnityEngine.Time.frameCount)
            local www = UnityEngine.WWW('http://www.baidu.com')
            Yield(www)
            print('yield(www) end time: '.. UnityEngine.Time.time)
            local s = tolua.tolstring(www.bytes)  -- tolstring 定义在ToLua.cs 中
            print(s:sub(1, 128))
            print('coroutine over')
        end

        function TestCo()            
            StartCoroutine(CoExample)    -- 在 LuaCoroutine.cs中定义，也是启动一个 协程，跟 coroutine.start 一样                               
        end

        local coDelay = nil

        function Delay()
	        local c = 1

	        while true do
		        WaitForSeconds(1) 
		        print('Count: '..c)
		        c = c + 1
	        end
        end

        function StartDelay()
	        coDelay = StartCoroutine(Delay)            
        end

        function StopDelay()
	        StopCoroutine(coDelay)
            coDelay = nil
        end
    ";

    protected override LuaFileUtils InitLoader()
    {
        return new LuaResLoader();
    }

    protected override void OnLoadFinished()
    {
        base.OnLoadFinished();

        luaState.DoString(script, "TestCoroutine2.cs");
        LuaFunction func = luaState.GetFunction("TestCo");
        func.Call();
        func.Dispose();
        func = null;
    }

    //屏蔽，例子不需要运行
    protected override void CallMain() { }

    bool beStart = false;
    string tips = null;

    void Start()
    {
#if UNITY_5 || UNITY_2017 || UNITY_2018
        Application.logMessageReceived += ShowTips;
#else
        Application.RegisterLogCallback(ShowTips);
#endif
    }

    void ShowTips(string msg, string stackTrace, LogType type)
    {
        tips += msg;
        tips += "\r\n";
    }

    new void OnApplicationQuit()
    {
#if UNITY_5 || UNITY_2017 || UNITY_2018
        Application.logMessageReceived -= ShowTips;
#else
        Application.RegisterLogCallback(null);
#endif
        base.OnApplicationQuit();
    }

    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width / 2 - 300, Screen.height / 2 - 200, 600, 400), tips);

        if (GUI.Button(new Rect(50, 50, 120, 45), "Start Counter"))
        {
            if (!beStart)
            {
                beStart = true;
                tips = "";
                LuaFunction func = luaState.GetFunction("StartDelay");
                func.Call();
                func.Dispose();
            }
        }
        else if (GUI.Button(new Rect(50, 150, 120, 45), "Stop Counter"))
        {
            if (beStart)
            {
                beStart = false;
                LuaFunction func = luaState.GetFunction("StopDelay");
                func.Call();
                func.Dispose();
            }
        }
    }
}
