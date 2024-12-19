using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance = null; // Singleton

    // Lista de enemigos en la escena
    public List<GameObject> enemies = new List<GameObject>();

    int id = 0;

    // Método para añadir un enemigo
    public void AddEnemy(GameObject enemy)
    {
        if (!enemies.Contains(enemy))
        {
            enemies.Add(enemy);
            enemy.GetComponent<EnemiesIA>().SetEnemyID(id++);
        }
    }

    // Método para eliminar un enemigo
    public void RemoveEnemy(GameObject enemy)
    {
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);
        }
    }

    public void KillEnemy(int id)
    {

        foreach (GameObject enemy in enemies)
        {
            EnemiesIA enemyIA = enemy.GetComponent<EnemiesIA>();
            if (enemyIA != null && enemyIA.GetEnemyID() == id)
            {
                enemyIA.Die();
               
                break;
            }
        }
    }

    public void HitEnemy(int id)
    {
        foreach (GameObject enemy in enemies)
        {
            EnemiesIA enemyIA = enemy.GetComponent<EnemiesIA>();
            if (enemyIA != null && enemyIA.GetEnemyID() == id)
            {
                enemyIA.TakeDMG();

                break;
            }
        }
    }

    public void ResetIA()
    {
        foreach (GameObject enemy in enemies)
        {
            EnemiesIA enemyIA = enemy.GetComponent<EnemiesIA>();
            if (enemyIA != null)
            {
                enemyIA.ResetIA();

    
            }
        }
    }

    // Método para acceder a todos los enemigos
    public List<GameObject> GetAllEnemies()
    {
        return enemies;
    }

    // Singleton setup
    void Awake()
    {
        // Verifica si ya hay una instancia
        if (instance == null)
        {
            instance = this; // Si no, esta instancia es la única
        }
        else if (instance != this)
        {
            Destroy(gameObject); // Destruye duplicados
        }

        // No destruir este objeto al cargar una nueva escena
        DontDestroyOnLoad(gameObject);
    }
}
