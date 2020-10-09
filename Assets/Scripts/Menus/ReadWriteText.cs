using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class ReadWriteText : MonoBehaviour
{
    [Header("Default Parameters")]
    [HideInInspector]
    public float masterVolume = 10f, musicVolume = 10f, sfxVolume = 10f;

    void Awake()
    {
        if (!File.Exists(Application.dataPath + "/gameData.dat"))
        {
            masterVolume = 10f;
            musicVolume = 10f;
            sfxVolume = 10f;
            CreateFile();
        }
        else
            ReadFile();
    }

    void CreateFile()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.dataPath + "/gameData.dat");

        GameData data = new GameData
        {
            m_masterVolume = masterVolume,
            m_musicVolume = musicVolume,
            m_sfxVolume = sfxVolume
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

            masterVolume = data.m_masterVolume;
            musicVolume = data.m_musicVolume;
            sfxVolume = data.m_sfxVolume;
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
            m_masterVolume = masterVolume,
            m_musicVolume = musicVolume,
            m_sfxVolume = sfxVolume
        };

        bf.Serialize(file, data);
        //Debug.Log("File Overwrite with volume = " + masterVolume + " & override controls = " + overrideControls);
        file.Close();
    }

    [System.Serializable]
    public class GameData
    {
        public float m_masterVolume = 10f, m_musicVolume = 10f, m_sfxVolume = 10f;
    }
}
