using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UnityLua {
    [CreateAssetMenu(fileName = "Resource tables", menuName = "Scriptables/ResourceTable", order =1)]
    public class ResourceTable : ScriptableObject
    {
        public Dictionary<string, string> nameToGuidPair = new Dictionary<string, string>();
        public ResourceList[] resourceTable = new ResourceList[0];
        public void Convert(){
            foreach (ResourceList item in resourceTable)
            {
                nameToGuidPair.Add(item.key, item.value);
            }
        }
    }

    [Serializable]
    public class ResourceList {
        public string key;
        public string value;
        public ResourceList() {}
        public ResourceList(string _key, string _value) {
            key = _key;
            value = _value;
        }
    }
}