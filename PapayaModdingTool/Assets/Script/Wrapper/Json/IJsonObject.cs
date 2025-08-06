using System.Collections.Generic;


namespace PapayaModdingTool.Assets.Script.Wrapper.Json
{
    public interface IJsonObject
    {
        public string GetString(string key);
        public int GetInt(string key);
        public uint GetUInt(string key);
        public long GetLong(string key);
        public float GetFloat(string key);
        public bool ContainsKey(string key);
        public IJsonObject GetObject(string key);
        public List<IJsonObject> GetArray(string key);
        public List<int> GetIntArray(string key);
        public List<uint> GetUIntArray(string key);
        public List<long> GetLongArray(string key);
        public List<string> GetStringArray(string key);
        public bool SetString(string key, string val);
        public bool SetArray(string key, List<IJsonObject> values);
        public bool SetObject(string key, IJsonObject value);
    }
}