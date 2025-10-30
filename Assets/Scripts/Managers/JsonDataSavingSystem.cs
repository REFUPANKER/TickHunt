using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonDataSavingSystem : MonoBehaviour
{
    public void SaveData()
    {
        JsonUtility.ToJson("", true);
    }
}
