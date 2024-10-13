using UnityEngine;
using System.Collections.Generic;
using LuaInterface;
using System;
using System.Reflection;


public class TestReflection : LuaClient
{
    string script =
@"    
    require 'tolua.reflection'          -- LuaReflection 里面注册了这个模块
    tolua.loadassembly('Assembly-CSharp')        -- LuaReflection 里面注册了这个方法
    local BindingFlags = require 'System.Reflection.BindingFlags' -- BindingFlags.lua

    function DoClick()
        print('do click')        
    end 

    function Test()  
        local t = typeof('TestExport')        -- ToLua.cs中有注册 tolua.typeof; 但是这里的typeof是typeof.lua中的方法。这个TestExport在TestOverload中有实现，并且它生成了Lua桥梁
        local func = tolua.getmethod(t, 'TestReflection')       --  getmethod 也是  LuaReflection 注册的  
        func:Call()        
        func:Destroy()
        func = nil
        
        local objs = {Vector3.one, Vector3.zero}
        local array = tolua.toarray(objs, typeof(Vector3)) -- toarray 是 Tolua.cs中注册的
        local obj = tolua.createinstance(t, array) -- LuaReflection 注册的
        --local constructor = tolua.getconstructor(t, typeof(Vector3):MakeArrayType()) -- 这个返回值 是 LuaConstructor 对应的Lua 桥梁
        --local obj = constructor:Call(array)        
        --constructor:Destroy()

        func = tolua.getmethod(t, 'Test', typeof('System.Int32'):MakeByRefType()) -- 这里的 typeof('System.Int32')，最终返回的是C#中的Tpye 所对应的Lua 桥梁。而后面的 MakeByRefType 表示的是，Test(out int i) 这个方法。tolua.getmethod 返回的是 LuaMethod 所表示的 lua 桥梁
        local r, o = func:Call(obj, 123)
        print(r..':'..o)
        func:Destroy()

        local property = tolua.getproperty(t, 'Number') -- tolua.getproperty 返回的是 LuaProperty 所对应的 lua 桥梁
        local num = property:Get(obj, null) -- LuaProperty 里面的 Get 方法，内部还是使用到了 PropertyInfo.GetValue; 这个方法里面有很多个重载，所以这里传入了一个null参数表示调用的是其中一个方法
        print('object Number: '..num)
        property:Set(obj, 456, null) -- Set 方法 跟 Get 方法同理
        num = property:Get(obj, null)
        property:Destroy()
        print('object Number: '..num)

        local field = tolua.getfield(t, 'field')  -- 这里的 getfiled 返回的是 LuaField 对应的 lua 桥梁
        num = field:Get(obj)
        print('object field: '.. num)
        field:Set(obj, 2048)
        num = field:Get(obj)
        field:Destroy()
        print('object field: '.. num)       
        
        field = tolua.getfield(t, 'OnClick')
        local onClick = field:Get(obj)        
        onClick = onClick + DoClick        -- 这里一开始惊了我，onClick不是普通的变量，是一个委托，所以才有一个 + 的步骤。至于委托相关的东西，可以去看 TestDelegate.cs
        field:Set(obj, onClick)        -- 这里很细节，已经做过实验，委托不像引用类型。比如有两个委托变量 a, b。 b = a; b += newAction, 这个新的事件只会添加到 b中，而不会添加到 a 中。
        local click = field:Get(obj)
        click:DynamicInvoke()
        field:Destroy()
        click:Destroy()
    end  
";

    string tips = null;

    protected override LuaFileUtils InitLoader()
    {
#if UNITY_4_6 || UNITY_4_7
        Application.RegisterLogCallback(ShowTips);        
#else
        Application.logMessageReceived += ShowTips;
#endif
        return new LuaResLoader();
    }

    //屏蔽，例子不需要运行
    protected override void CallMain() { }

    void TestAction()
    {
        Debugger.Log("Test Action");
    }

    protected override void OnLoadFinished()
    {        
        base.OnLoadFinished();

        /*Type t = typeof(TestExport);
        MethodInfo md = t.GetMethod("TestReflection");
        md.Invoke(null, null);

        Vector3[] array = new Vector3[] { Vector3.zero, Vector3.one };
        object obj = Activator.CreateInstance(t, array);
        md = t.GetMethod("Test", new Type[] { typeof(int).MakeByRefType() });
        object o = 123;
        object[] args = new object[] { o };
        object ret = md.Invoke(obj, args);
        Debugger.Log(ret + " : " + args[0]);

        PropertyInfo p = t.GetProperty("Number");
        int num = (int)p.GetValue(obj, null);
        Debugger.Log("object Number: {0}", num);
        p.SetValue(obj, 456, null);
        num = (int)p.GetValue(obj, null);
        Debugger.Log("object Number: {0}", num);

        FieldInfo f = t.GetField("field");
        num = (int)f.GetValue(obj);
        Debugger.Log("object field: {0}", num);
        f.SetValue(obj, 2048);
        num = (int)f.GetValue(obj);
        Debugger.Log("object field: {0}", num);*/

        luaState.CheckTop();
        luaState.DoString(script, "TestReflection.cs");
        LuaFunction func = luaState.GetFunction("Test");
        func.Call();
        func.Dispose();
        func = null;
    }

    void ShowTips(string msg, string stackTrace, LogType type)
    {
        tips += msg;
        tips += "\r\n";
    }

    new void OnApplicationQuit()
    {
#if UNITY_4_6 || UNITY_4_7
        Application.RegisterLogCallback(ShowTips);        
#else
        Application.logMessageReceived -= ShowTips;
#endif
        Destroy();
    }

    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width / 2 - 250, Screen.height / 2 - 150, 500, 300), tips);       
    }
}
