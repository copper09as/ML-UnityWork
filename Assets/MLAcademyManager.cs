using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class MLAcademyManager : MonoBehaviour
{
    void Awake()
    {
        // 这一行会强制创建并初始化 Academy 单例
        var _ = Academy.Instance;
        Debug.Log("✅ ML-Agents Academy Initialized.");
    }

}
