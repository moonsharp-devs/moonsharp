using System.Security.Cryptography;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Text;

namespace UnityLua
{
    public class ScriptsLoader : MonoBehaviour
    {
        public static ScriptsLoader self;
        public string filePattern = "*_client.lua";
        private void Awake() {
            if (self == null) {
                self = this;
                DontDestroyOnLoad(this);
            } else {
                Destroy(this);
            }
        }
        // Start is called before the first frame update
        void Start() {
            foreach (string item in LoadFilePaths()) {
                Scripts.self.AddScript(item, File.ReadAllText(item));
            }
        }
        IEnumerable<string> LoadFilePaths() {
#if UNITY_EDITOR
            string filePath = Path.Combine(Application.dataPath, "Resources/LuaFiles");
#else
            string filePath = Path.Combine(Application.persistentDataPath, "Resources/LuaFiles");
#endif
            return Directory.EnumerateFiles(filePath, filePattern, SearchOption.AllDirectories);
        }
    }
}