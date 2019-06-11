using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ABBusinissConfig
{
    public class ResourceTest : MonoBehaviour
    {
        //public GameObject m_Prefab;

        // Start is called before the first frame update
        void Start()
        {
            //GameObject obj0 = Instantiate(m_Prefab);
            //GameObject obj1 = Instantiate(Resources.Load("Attack") as GameObject);
            //AssetBundle assetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/attack");
            //GameObject obj2 = Instantiate(assetBundle.LoadAsset<GameObject>("attack"));
            //GameObject obj3 = Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GameData/Prefabs/Attack.prefab"));

            TestLoadAB();
        }

        void TestLoadAB()
        {
            AssetBundle configAB = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/assetbundleconfig");
            TextAsset textAsset = configAB.LoadAsset<TextAsset>("AssetbundleConfig");
            MemoryStream ms = new MemoryStream(textAsset.bytes);
            BinaryFormatter bf = new BinaryFormatter();
            AssetBundleConfig test = (AssetBundleConfig)bf.Deserialize(ms);
            ms.Close();
            string path = "Assets/GameData/Prefabs/Attack.prefab";
            int crc = CRC32.GetCRC32(path);
            ABBase aBBase = null;
            for (int i = 0; i < test.ABList.Count; i++)
            {
                if (test.ABList[i].Crc == crc)
                {
                    aBBase = test.ABList[i];
                }
            }

            for (int i = 0; i < aBBase.ABDepends.Count; i++)
            {
                AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + aBBase.ABDepends[i]);
            }
            AssetBundle assetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + aBBase.ABName);
            GameObject obj2 = Instantiate(assetBundle.LoadAsset<GameObject>(aBBase.AssetName));
        }

        //#region 类序列化XML
        ///// <summary>
        ///// 数据模型
        ///// </summary>
        //void SeriaLizeTest()
        //{
        //    SerializeTest sTest = new SerializeTest
        //    {
        //        Id = 1,
        //        Name = "测试",
        //        List = new List<int>()
        //    };
        //    sTest.List.Add(2);
        //    sTest.List.Add(3);

        //    DeSerializeTest();
        //}

        ///// <summary>
        ///// Xml 序列化 类序列化Xml
        ///// </summary>
        ///// <param name="test">Test.</param>
        //void XmlSerialize(SerializeTest test)
        //{
        //    // 打开文件流
        //    FileStream fileStream = new FileStream(Application.dataPath + "/test.xml", FileMode.Create,
        //        FileAccess.ReadWrite, FileShare.ReadWrite);
        //    // 写入流
        //    StreamWriter sw = new StreamWriter(fileStream, System.Text.Encoding.UTF8);
        //    XmlSerializer xml = new XmlSerializer(test.GetType());
        //    xml.Serialize(sw, test);

        //    // 关闭
        //    sw.Close();
        //    fileStream.Close();
        //}
        //#endregion

        //#region XML反序列化类
        ///// <summary>
        ///// 反序列化得到的数据
        ///// </summary>
        //void DeSerializeTest()
        //{
        //    SerializeTest test = XmlDeSerialize();
        //    Debug.Log(test.Id + " " + test.Name);
        //    foreach (int item in test.List)
        //    {
        //        Debug.Log(item);
        //    }
        //}

        ///// <summary>
        ///// Xml反序列化 Xml文件反序列化类 
        ///// </summary>
        //SerializeTest XmlDeSerialize()
        //{
        //    // 打开文件流
        //    FileStream fileStream = new FileStream(Application.dataPath + "/test.xml", FileMode.Open,
        //        FileAccess.ReadWrite, FileShare.ReadWrite);
        //    XmlSerializer xs = new XmlSerializer(typeof(SerializeTest));
        //    SerializeTest test = (SerializeTest)xs.Deserialize(fileStream);
        //    fileStream.Close();
        //    return test;
        //}
        //#endregion

        //#region 类序列化二进制文件
        ///// <summary>
        ///// 类数据模型
        ///// </summary>
        //void BinarySeriaLizeTest()
        //{
        //    SerializeTest sTest = new SerializeTest
        //    {
        //        Id = 9,
        //        Name = "测试类序列化二进制文件",
        //        List = new List<int>()
        //    };
        //    sTest.List.Add(10);
        //    sTest.List.Add(98);

        //    BinarySerialize(sTest);
        //}
        ///// <summary>
        ///// 类序列化二进制文件
        ///// </summary>
        ///// <param name="test">Test.</param>
        //void BinarySerialize(SerializeTest test)
        //{
        //    FileStream fs = new FileStream(Application.dataPath + "/test.bytes", 
        //        FileMode.Create, 
        //        FileAccess.ReadWrite,
        //        FileShare.ReadWrite);
        //    BinaryFormatter bf = new BinaryFormatter();
        //    bf.Serialize(fs, test);
        //    fs.Close();
        //}
        //#endregion

        //#region 二进制文件反序列化类
        ///// <summary>
        ///// 二进制文件反序列化类数据
        ///// </summary>
        //void BinaryDeserializeTest()
        //{
        //    SerializeTest test = BinaryDeserialize();
        //    Debug.Log(test.Id + " " + test.Name);
        //    foreach (int item in test.List)
        //    {
        //        Debug.Log(item);
        //    }
        //}

        ///// <summary>
        ///// 二进制文件反序列化类
        ///// </summary>
        ///// <returns>The deserialize.</returns>
        //SerializeTest BinaryDeserialize()
        //{
        //    TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/test.bytes");
        //    MemoryStream ms = new MemoryStream(textAsset.bytes);
        //    BinaryFormatter bf = new BinaryFormatter();
        //    SerializeTest test = (SerializeTest)bf.Deserialize(ms);
        //    ms.Close();
        //    return test;
        //}
        //#endregion

        //#region Unity Asset 序列化
        ///// <summary>
        ///// Unity Asset 序列化
        ///// </summary>
        //void ReadTestAssets()
        //{
        //    AssetsSeriaLize assets = UnityEditor.AssetDatabase.LoadAssetAtPath<AssetsSeriaLize>("Assets/Scripts/TestAsset.asset");
        //    Debug.Log(assets.Id + "" + assets.Name);
        //    foreach (string item in assets.TestList)
        //    {
        //        Debug.Log(item);
        //    }
        //}
        //#endregion
    }
}