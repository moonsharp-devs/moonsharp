using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;
using MoonSharp.Interpreter;
namespace UnityLua
{
    public class TaskManager : MonoBehaviour
    {
        
        private static readonly Queue<Action> queue = new Queue<Action>();
        public Lookup<string, DynValue> commandRegistery;
        static event Action<DynValue> eventHandler;
        public static Dictionary<string, DynValue> eventList = new Dictionary<string, DynValue>();
        public static TaskManager self = null;
        public void Enqueue(IEnumerator action) {
            lock (queue) {
                queue.Enqueue (() => {
                    StartCoroutine (action);
                });
            }
        }
        public void AddListener(string name, Action<DynValue> handler) {
            eventHandler += handler;
        }
        public void AddListenerLastStay(string name, DynValue callback) {
            eventList.Remove(name);
            eventList.Add(name, callback);
        }
        public void AddListenerFirstStay(string name, DynValue callback) {
            bool exists = eventList.ContainsKey(name);
            if (exists == false) eventList.Add(name, callback);
        }
        public void RemoveListener(string name) {
            eventList.Remove(name);
        }
        public IEnumerator TriggerEvent(string name, GameObject sender, EventArgs args) {
            eventList.TryGetValue(name, out DynValue _calBack);
            _calBack?.Function.Call(sender, args);
            yield return new WaitForEndOfFrame();
        }
        public IEnumerator TriggerEvent(DynValue function) {
            eventHandler?.Invoke(function);
            yield return new WaitForEndOfFrame();
        }
        public void Enqueue(Action action) {
            lock (queue) {
                queue.Enqueue (() => {
                    ActionWrapper(action);
                });
            }
        }
        public Task EnqueueAsync(Action action)
        {
            var tcs = new TaskCompletionSource<bool>();

            void WrappedAction() {
                try 
                {
                    action();
                    tcs.TrySetResult(true);
                } catch (Exception ex) 
                {
                    tcs.TrySetException(ex);
                }
            }

            Enqueue(ActionWrapper(WrappedAction));
            return tcs.Task;
        }
        IEnumerator ActionWrapper(Action action)
        {
            action();
            yield return null;
        }
        void Awake() {
            if (self == null) {
                self = this;
                DontDestroyOnLoad(gameObject);
            } else {
                Destroy(this);
            }
        }

        // Update is called once per frame
        void Update() {
            lock (queue) {
                while (queue.Count > 0)
                {
                    queue.Dequeue().Invoke();
                }
            }
        }
    }
}