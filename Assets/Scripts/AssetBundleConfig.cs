using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

namespace ABBusinissConfig
{
    [System.Serializable]
    public class AssetBundleConfig
    {
        [XmlElement("ABList")]
        public List<ABBase> ABList { get; set; }
    }

    [System.Serializable]
    public class ABBase
    {
        [XmlAttribute("Path")]
        public string Path { get; set; }

        // 类似MD5码，用于唯一标识
        [XmlAttribute("Crc")]
        public int Crc { get; set; }

        [XmlAttribute("ABName")]
        public string ABName { get; set; }

        [XmlAttribute("AssetName")]
        public string AssetName { get; set; }

        // 依赖其他AB包，依赖加载
        [XmlElement("ABDepends")]
        public List<string> ABDepends { get; set; }
    }
}