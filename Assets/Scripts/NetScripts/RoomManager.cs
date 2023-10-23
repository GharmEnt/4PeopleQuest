using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    public static RoomManager instance;

    [SerializeField] string[] playerNames;
    [SerializeField] GameObject[] spawnPoints;


    [SerializeField]string currentPlayerName;

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        spawnPoints = new GameObject[playerNames.Length]; 
    }

    private void OnEnable()
    {
        int random = Random.Range(0, playerNames.Length);
        currentPlayerName = playerNames[random];

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if(scene.buildIndex ==1)
        {
            spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

            int currSpawn = Mathf.RoundToInt(Random.Range(0, spawnPoints.Length));

            PhotonNetwork.Instantiate(currentPlayerName, spawnPoints[currSpawn].transform.position, Quaternion.identity);
        }
    }
}
