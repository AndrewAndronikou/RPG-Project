using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class OffScreenSpawner : MonoBehaviour
{
    [SerializeField] float spawnTime = 3f;
    [SerializeField] GameObject[] enemies;
    [SerializeField] int numberOfEnemies = 5;
    int i;

    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("Spawn", spawnTime, spawnTime);
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Spawn()
    {
        //Max Num of enemies in scene
      //  int nrEnemiesExist = GameObject.FindGameObjectsWithTag("Enemy").Length;
        int randEnemyIndex = Random.Range(0, enemies.Length);

        //follows the player
        transform.position = player.transform.position + Random.insideUnitSphere * 40;

        // if (nrEnemiesExist < numberOfEnemies || enemies.Length > 0)
        //  {
         Instantiate(enemies[randEnemyIndex], transform.position, Quaternion.identity);
            //Debug.Log("Spawned: " + nrEnemiesExist + " / " + numberOfEnemies );
           // nrEnemiesExist++;
        //}
    }
}
