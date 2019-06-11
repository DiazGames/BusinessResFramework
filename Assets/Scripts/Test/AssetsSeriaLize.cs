using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 右键菜单栏-生成一个空的Asset文件，在Unity编辑器中填充数据
//[CreateAssetMenu(fileName = "TestAsset", menuName = "CreatAssets", order = 0)]

public class AssetsSeriaLize : ScriptableObject
{
    public int Id;
    public string Name;
    public List<string> TestList;
}
