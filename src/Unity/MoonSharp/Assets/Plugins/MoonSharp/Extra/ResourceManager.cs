using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using System.Threading;
using MoonSharp.Interpreter;

namespace UnityLua {
    public class ResourceManager : MonoBehaviour {
        public static ResourceManager self;
        public Dictionary<string, GameObject> GUIDToGameObjectPair = new Dictionary<string, GameObject>();
        public ResourceTable table;
        // Start is called before the first frame update
        void Awake() {
            if (self == null) {
                self = this;
                DontDestroyOnLoad(this);
            } else {
                Destroy(this);
            }
            table.Convert();
        }
        public void RegisterCommand(string name, DynValue callback) {
            if (callback.Type != DataType.Function) return;
            TaskManager.self.AddListenerFirstStay(name, callback);
        }
        public void TriggerCommand(string name, GameObject sender, EventArgs args) {
            TaskManager.self.Enqueue(TaskManager.self.TriggerEvent(name, sender, args));
        }
        public bool ObjectExists(string guid) {
            object is_exists = null;
            TaskManager.self.Enqueue(_ObjectExists(guid, (exists) => { is_exists = exists; }));
            while (is_exists == null) {
                Task.Delay(10);
            }
            return (bool)is_exists;
        }
        delegate void ExistsCallback(bool exists);
        IEnumerator _ObjectExists(string guid, ExistsCallback callback) {
            yield return new WaitForEndOfFrame();
            callback(GUIDToGameObjectPair.ContainsKey(guid));
        }
        public void CreateObject(string guid){
            TaskManager.self.Enqueue(_CreateObject(guid));
        }
        IEnumerator _CreateObject(string guid) {
            yield return new WaitForEndOfFrame();
            GUIDToGameObjectPair.TryGetValue(guid, out GameObject obj);
            if (obj) Instantiate(obj);
            else Debug.LogError("object didn't found");
        }
        public string LoadObject(string name){
            string guid = null;
            TaskManager.self.Enqueue(_loadObject(name, (_guid) =>{guid = _guid;}));
            while (guid == null) {
                Task.Delay(10);
            }
            return guid;
        }
        delegate void loadCallback(string guid);
        IEnumerator _loadObject(string name, loadCallback callback) {
            yield return new WaitForEndOfFrame();
            table.nameToGuidPair.TryGetValue(name, out string guid);
            if (guid == null) yield return null;
            Addressables.LoadAssetAsync<GameObject>(guid).Completed += AfterLoaded;
            callback(guid);
        }

        private void AfterLoaded(AsyncOperationHandle<GameObject> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded){
                GameObject obj = handle.Result;
                string guid = obj.GetComponent<GUID>().guid;
                GUIDToGameObjectPair.Add(guid, obj);
            } else {
                Debug.LogError("Asset didn't load");
            }
        }
    }
}