using UnityEngine;
using System.Collections;
using LuaInterface;
using System;
using System.Reflection;
using System.Text;

public class TestString : LuaClient
{
    string script =
@"           
    function Test()
        local str = System.String.New('男儿当自强')
        local index = str:IndexOfAny('儿自')
        print('and index is: '..index)
        local buffer = str:ToCharArray()				-- 这里的 string, 包括 buffer都是C#中的类型，不是Lua中的类型
        print('str type is: '..type(str)..' buffer[0] is ' .. buffer[0]) -- 注意：这里的buffer虽然在C#中是一个 char[], 但是lua和C#没有建立char的桥梁。所以buffer[0] 输出得不是 字符，而是 数字。
        local luastr = tolua.tolstring(buffer)			-- 这里的tolua.tolstring 在 Tolua.cs中被定义了，它可以把一个byte[] 转成 lua 中的字符串
        print('lua string is: '..luastr..' type is: '..type(luastr))
        luastr = tolua.tolstring(str)
        print('lua string is: '..luastr)                    
    end

	function TestString(strInCS)
		local sInLua = 'a123'

		print('strInCS: ', type(strInCS), 'sInLua: ', type(sInLua))
		--[[
			1. 从 C# 端 传递过来的 strInCS，会自动变成 lua的 string, 因为如果C#的string, 输出应该 userdata
			2. 造成这个现象的原因，应该是 LuaState.cs 中，StackTraits<string>.Init 方法
			3. 其他的类型，比如 AccessingEnum.cs 中的枚举，因为没有相关的 StackTraits，所以传递过去，还是C#中的枚举
		]]
	end
";

    protected override LuaFileUtils InitLoader()
    {
        return new LuaResLoader();
    }

    //屏蔽，例子不需要运行
    protected override void CallMain() { }

    protected override void OnLoadFinished()
    {
#if UNITY_5 || UNITY_2017 || UNITY_2018
        Application.logMessageReceived += ShowTips;
#else
        Application.RegisterLogCallback(ShowTips);
#endif  
        base.OnLoadFinished();
        luaState.DoString(script);
        LuaFunction func = luaState.GetFunction("Test");
        func.Call();
        func.Dispose();
        func = null;

		LuaFunction func2 = luaState.GetFunction("TestString");
        func2.Call("string in C#");
        func2.Dispose();
        func2 = null;
    }

    string tips;

    void ShowTips(string msg, string stackTrace, LogType type)
    {
        tips += msg;
        tips += "\r\n";
    }

    new void OnApplicationQuit()
    {
        base.OnApplicationQuit();

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
