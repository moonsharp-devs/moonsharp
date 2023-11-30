using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using System.Threading;
using UnityLua;
using MoonSharp.Interpreter;

public class TestDispatcher : MonoBehaviour
{
    // Start is called before the first frame update
    public delegate void Callback(object value);
    void Start()
    {
        // Task.Run(() =>
        // {
        //     Debug.LogError("Start the Task");
        //     TaskManager.self.Enqueue(UnityObject((object value)=>{
        //         Debug.LogError((string)value);
        //     }));
        // });
        // Task.Run(() =>
        // {
        //     Debug.LogError("Start From Task");
        //     // StartCoroutine(RegisterEvents("name", OnEvent));
        //     TaskManager.self.Enqueue(RegisterEvents("name",OnEvent));
        //     // TaskManager.self.AddListener(OnEvent);
        //     Task.Delay(500);

        //     // MyEventArgs args = new MyEventArgs {
        //     //     message = "Triggered from Task"
        //     // };
        //     EventArgs args = new EventArgs();
        //     // TaskManager.self.TriggerEvent(this, args);

        //     // TaskManager.self.Enqueue(TaskManager.self.TriggerEvent(this, args));
        //     Debug.LogError("End From Task");
        // });
        Task.Run(() =>
        {
            // TaskManager.self.AddListenerFirstStay("hello",OnEventList);
            // TaskManager.self.AddListenerFirstStay("hello",OnEventList1);

            // EventArgs args = new EventArgs();
            // StartCoroutine(TaskManager.self.TriggerEvent("hello", this, args));
            // TaskManager.self.Enqueue(TaskManager.self.TriggerEvent("hello", this, args));
        });
        // TaskManager.self.AddListener("hello",OnEventList);

        // EventArgs args = new EventArgs();
        // StartCoroutine(TaskManager.self.TriggerEvent("hello", this, args));

    }
    public IEnumerator RegisterEvents(string name, Action<DynValue> function){
        TaskManager.self.AddListener(name, function);
        yield return new WaitForEndOfFrame();
    }
    public void OnEventList(object sender, EventArgs args) {
        Debug.LogError(sender);
    }
    public void OnEventList1(object sender, EventArgs args) {
        Debug.LogError(args);
    }
    public void OnEvent(DynValue function) {
        Debug.LogError(name);
    }
    public IEnumerator UnityObject(Callback callback) {
        Debug.LogError("Start IEnum");
        yield return new WaitForSeconds(5);
        callback("hello from callback");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
public class MyEventArgs : EventArgs
{
    public string message;
}
