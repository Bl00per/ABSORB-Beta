using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class ReadWriteText : MonoBehaviour
{
    [Header("Default Parameters")]
    [HideInInspector]
    public float volume = 100f;
    [HideInInspector]
    public bool overrideControls = false;

    void CreateFile()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.dataPath + "/gameData.dat");

        GameData data = new GameData
        {
            mVolume = volume,
            mOverrideControls = overrideControls
        };

        bf.Serialize(file, data);
        Debug.Log("File Created");
        file.Close();
    }

    void ReadFile()
    {
        if (new FileInfo(Application.dataPath + "/gameData.dat").Length == 0)
        {
            Debug.Log("File is empty @ " + Application.dataPath + "/gameData.dat");
            CreateFile();
        }
        else if (File.Exists(Application.dataPath + "/gameData.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.dataPath + "/gameData.dat", FileMode.Open);
            GameData data = (GameData)bf.Deserialize(file);

            volume = data.mVolume;
            overrideControls = data.mOverrideControls;
            //Debug.Log("File Read");
            file.Close();
        }
    }

    public void OverwriteData()
    {
        BinaryFormatter bf = new BinaryFormatter();

        FileStream file = File.Create(Application.dataPath + "/gameData.dat");
        GameData data = new GameData
        {
            mVolume = volume,
            mOverrideControls = overrideControls
        };

        bf.Serialize(file, data);
        Debug.Log("File Overwrite with volume = " + volume + " & override controls = " + overrideControls);
        file.Close();
    }

    // Start is called before the first frame update
    void Awake()
    {
        if (!File.Exists(Application.dataPath + "/gameData.dat"))
        {
            volume = 100;
            overrideControls = false;
            CreateFile();
        }
        else
            ReadFile();
    }

    [System.Serializable]
    public class GameData
    {
        public float mVolume = 100;
        public bool mOverrideControls = false;
    }
}
