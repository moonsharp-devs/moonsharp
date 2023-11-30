using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;
using System;
public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Debug.LogError(MoonSharpFactorial());
        // Debug.LogError(EnumerableTest());
        TestGameObject();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private int Multiply(int a, int b){
        return a * b;
    }
    double MoonSharpFactorial()
    {
        string script = @"    
            -- defines a factorial function
            function fact (n)
                if (n == 0) then
                    return 1;
                else
                    return Multiply(n,fact(n - 1));
                end
            end";

        Script _script = new Script();
        _script.Globals["Number"] = 10;
        _script.Globals["Multiply"] = (Func<int, int, int>)Multiply;
        _script.DoString(script);
        DynValue res = _script.Call(_script.Globals["fact"], 15);
        return res.Number;
    }
    IEnumerable<int> GetNumbers(){
        for (int i = 0; i < 10; i++)
            yield return i;
    }
    void PrintLua(object value){
        Debug.LogError(value);
    }
    double EnumerableTest(){
        string scriptCode = @"
            total = 0;
            for i in GetNumbers() do
                total = total + i;
                PrintLua(total);
            end
            return total;
        ";
        Script script = new Script();
        script.Globals["GetNumbers"] = (Func<IEnumerable<int>>)GetNumbers;
        script.Globals["PrintLua"] = (Action<object>)PrintLua;
        DynValue res = script.DoString(scriptCode);
        return res.Number;
    }
    void TestGameObject(){
        GameObject myobj = new GameObject {
            name = "mamad"
        };
        myobj.transform.position = new Vector3(10, 20, 30);
        string scriptCode = @"
            PrintLua(myobj.name);
        ";
        Script script = new Script();
        UserData.RegisterType<GameObject>();
        // UserData.RegisterProxyType<GameObject>();
        script.Globals["PrintLua"] = (Action<object>)PrintLua;
        script.Globals["myobj"] = UserData.Create(myobj);
        script.DoString(scriptCode);
    }
}
