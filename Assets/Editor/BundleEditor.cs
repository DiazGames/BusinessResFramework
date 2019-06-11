using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ABBusinissConfig
{
    public class BundleEditor
    {
        private static string m_BundleTargetPath = Application.streamingAssetsPath;
        private static string ABCONFIGPATH = "Assets/Editor/ABConfig.asset";
        // Key是AB包名，Value是路径，所有文件夹AB包dic
        private static Dictionary<string, string> m_AllFileDir = new Dictionary<string, string>();
        // 过滤的List
        private static List<string> m_AllFileAB = new List<string>();
        // 单个Prefab的AB包
        private static Dictionary<string, List<string>> m_AllPrefabDir = new Dictionary<string, List<string>>();
        // 储存所有有效路径
        private static List<string> m_ConfigFil = new List<string>();


        [MenuItem("Tools/打包")]
        public static void Build()
        {
            m_ConfigFil.Clear();
            m_AllFileDir.Clear();
            m_AllFileAB.Clear();
            m_AllPrefabDir.Clear();
            ABConfig aBConfig = AssetDatabase.LoadAssetAtPath<ABConfig>(ABCONFIGPATH);

            foreach (ABConfig.FileDirABName fileDir in aBConfig.m_AllFileDirAB)
            {
                if (m_AllFileDir.ContainsKey(fileDir.ABName))
                {
                    Debug.Log("AB包配置名字重复，请检查！");
                }
                else
                {
                    m_AllFileDir.Add(fileDir.ABName, fileDir.Path);
                    m_AllFileAB.Add(fileDir.Path);
                    m_ConfigFil.Add(fileDir.Path);
                }
            }

            string[] allPrefabStr = AssetDatabase.FindAssets("t:Prefab", aBConfig.m_AllPrefabPath.ToArray());

            for (int i = 0; i < allPrefabStr.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(allPrefabStr[i]);
                EditorUtility.DisplayProgressBar("查找Prefab", "Prefab" + path, i * 1.0f / allPrefabStr.Length);
                m_ConfigFil.Add(path);
                if (!ContainAlFileAB(path))
                {
                    // 加载Prefab
                    GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    // 找到Prefab的依赖项
                    string[] allDepend = AssetDatabase.GetDependencies(path);
                    List<string> allDependPath = new List<string>();
                    // 循环依赖项
                    for (int j = 0; j < allDepend.Length; j++)
                    {
                        //Debug.Log(allDepend[j]);
                        // 查看Prefab的依赖项是否已经被打过包，把未打包的依赖项提取出来
                        if (!ContainAlFileAB(allDepend[j]) && !allDepend[j].EndsWith(".cs", System.StringComparison.Ordinal))
                        {
                            allDependPath.Add(allDepend[j]);
                            // 加载到过滤List中，用于以后判断过滤
                            m_AllFileAB.Add(allDepend[j]);
                        }
                    }
                    if (m_AllPrefabDir.ContainsKey(obj.name))
                    {
                        Debug.Log("存在相同名字的Prefab! 名字：" + obj.name);
                    }
                    else
                    {
                        m_AllPrefabDir.Add(obj.name, allDependPath);
                    }
                }

            }

            foreach (string name in m_AllFileDir.Keys)
            {
                SetABName(name, m_AllFileDir[name]);
            }
            foreach (string name in m_AllPrefabDir.Keys)
            {
                SetABName(name, m_AllPrefabDir[name]);
            }

            BuildAssetBundle();

            string[] oldABNames = AssetDatabase.GetAllAssetBundleNames();
            for (int i = 0; i < oldABNames.Length; i++)
            {
                AssetDatabase.RemoveAssetBundleName(oldABNames[i], true);
                EditorUtility.DisplayProgressBar("清除AB包名", "名字：" + oldABNames[i], i * 1.0f / oldABNames.Length);
            }

            // 刷新
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();

        }

        /// <summary>
        /// 判断是否包含已打过包的路径资源,用来做AB包冗余剔除
        /// </summary>
        /// <returns><c>true</c>, if al file ab was contained, <c>false</c> otherwise.</returns>
        /// <param name="path">Path.</param>
        static bool ContainAlFileAB(string path)
        {
            for (int i = 0; i < m_AllFileAB.Count; i++)
            {
                if (path == m_AllFileAB[i] || 
                    (path.Contains(m_AllFileAB[i]) 
                        && (path.Replace(m_AllFileAB[i], ""))[0] == '/'))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 设置AB包名
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="path">Path.</param>
        static void SetABName(string name, string path)
        {
            AssetImporter assetImporter = AssetImporter.GetAtPath(path);
            if (assetImporter == null)
            {
                Debug.Log("不存在此路径文件：" + path);
            }
            else
            {
                assetImporter.assetBundleName = name;
            }
        }

        static void SetABName(string name, List<string> paths)
        {
            for (int i = 0; i < paths.Count; i++)
            {
                SetABName(name, paths[i]);
            }
        }

        /// <summary>
        /// 打包
        /// </summary>
        static void BuildAssetBundle()
        {
            string[] allBundles = AssetDatabase.GetAllAssetBundleNames();
            // key为全路径，value为包名
            Dictionary<string, string> resPathDic = new Dictionary<string, string>();
            for (int i = 0; i < allBundles.Length; i++)
            {
                string[] allBundlePath = AssetDatabase.GetAssetPathsFromAssetBundle(allBundles[i]);
                for (int j = 0; j < allBundlePath.Length; j++)
                {
                    if (allBundlePath[j].EndsWith(".cs", System.StringComparison.Ordinal))
                    {
                        continue;
                    }
                    Debug.Log("此AB包：" + allBundles[i] + " 下面包含的资源文件路径：" + allBundlePath[j]);
                    if (ValidPath(allBundlePath[j]))
                    {
                        resPathDic.Add(allBundlePath[j], allBundles[i]);
                    }
                }
            }

            DeleteNouseAB();
            // 生成AB包配置表
            WriteData(resPathDic);

            BuildPipeline.BuildAssetBundles(m_BundleTargetPath, BuildAssetBundleOptions.ChunkBasedCompression,
                EditorUserBuildSettings.activeBuildTarget);
        }

        static void WriteData(Dictionary<string, string> resPathDic)
        {
            AssetBundleConfig config = new AssetBundleConfig();
            config.ABList = new List<ABBase>();
            foreach (string path in resPathDic.Keys)
            {
                ABBase aBBase = new ABBase();
                aBBase.Path = path;
                aBBase.Crc = CRC32.GetCRC32(path);
                aBBase.ABName = resPathDic[path];
                aBBase.AssetName = path.Remove(0, path.LastIndexOf("/", System.StringComparison.Ordinal) + 1);
                aBBase.ABDepends = new List<string>();
                string[] resDependence = AssetDatabase.GetDependencies(path);
                for (int i = 0; i < resDependence.Length; i++)
                {
                    string tempPath = resDependence[i];
                    if (tempPath == path || path.EndsWith(".cs", System.StringComparison.Ordinal))
                    {
                        continue;
                    }
                    string abName = "";
                    if (resPathDic.TryGetValue(tempPath, out abName))
                    {
                        if (abName == resPathDic[path])
                        {
                            continue;
                        }
                        if (!aBBase.ABDepends.Contains(abName))
                        {
                            aBBase.ABDepends.Add(abName);
                        }
                    }
                }
                config.ABList.Add(aBBase);
            }
            // 写入XML
            string xmlPath = "Assets/GameData/Data/ABData/AssetbundleConfig.xml";
            if (File.Exists(xmlPath))
            {
                File.Delete(xmlPath);
            }

            FileStream fs = new FileStream(xmlPath, FileMode.Create,
                FileAccess.ReadWrite, FileShare.ReadWrite);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
            XmlSerializer xs = new XmlSerializer(config.GetType());
            xs.Serialize(sw, config);
            sw.Close();
            fs.Close();
            // 写入二进制文件
            foreach (ABBase abBase in config.ABList)
            {
                abBase.Path = "";
            }
            string bytePath = "Assets/GameData/Data/ABData/AssetbundleConfig.bytes";
            FileStream fs2 = new FileStream(bytePath ,
                FileMode.Create, 
                FileAccess.ReadWrite,
                FileShare.ReadWrite);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs2, config);
            fs2.Close();

        }

        /// <summary>
        /// 删除无用的AB包
        /// </summary>
        static void DeleteNouseAB()
        {
            string[] allBundles = AssetDatabase.GetAllAssetBundleNames();
            DirectoryInfo direction = new DirectoryInfo(m_BundleTargetPath);
            FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                if (ContainABName(files[i].Name, allBundles) || files[i].Name.EndsWith(".meta", System.StringComparison.Ordinal))
                {
                    continue;
                }
                else
                {
                    Debug.Log("此AB包已经被删除或者改名了：" + files[i].Name);
                    if (File.Exists(files[i].FullName))
                    {
                        File.Delete(files[i].FullName);
                    }
                }
            }
        }

        /// <summary>
        /// 遍历文件夹里的文件名与设置的所有AB包进行检查判断
        /// </summary>
        /// <returns><c>true</c>, if ABN ame was contained, <c>false</c> otherwise.</returns>
        /// <param name="name">Name.</param>
        /// <param name="strs">Strs.</param>
        static bool ContainABName(string name, string[] strs)
        {
            for (int i = 0; i < strs.Length; i++)
            {
                if (name == strs[i])
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 是否有效路径
        /// </summary>
        /// <returns><c>true</c>, if path was valided, <c>false</c> otherwise.</returns>
        /// <param name="path">Path.</param>
        static bool ValidPath(string path)
        {
            for (int i = 0; i < m_ConfigFil.Count; i++)
            {
                if (path.Contains(m_ConfigFil[i]))
                {
                    return true;
                }
            }
            return false;
        }
    }
}