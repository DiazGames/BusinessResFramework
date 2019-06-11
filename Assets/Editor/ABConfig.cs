using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ABBusinissConfig
{
    // 右键菜单栏-生成一个空的Asset文件，在Unity编辑器中填充数据
    [CreateAssetMenu(fileName = "ABConfig", menuName = "CreatABConfig", order = 0)]
    public class ABConfig : ScriptableObject
    {
        // 单个文件所在文件夹路径，会遍历文件夹下所有Prefab，所有Prefab的名字不能重复，必须保证名字的唯一性。
        public List<string> m_AllPrefabPath = new List<string>();
        public List<FileDirABName> m_AllFileDirAB = new List<FileDirABName>();

        [System.Serializable]
        public struct FileDirABName
        {
            public string ABName;
            public string Path;
        }
    }
}