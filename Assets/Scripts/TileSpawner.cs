using System.Collections.Generic;
using UnityEngine;

namespace InfiniteRunner3D
{
    public class TileSpawner : MonoBehaviour
    {
        [SerializeField] private int tileStartCount = 10;
        [SerializeField] private int minimumStraightTiles = 3;
        [SerializeField] private int maximumStraightTiles = 5;
        [SerializeField] private GameObject startingTile;
        [SerializeField] private List<GameObject> turningTiles;
        [SerializeField] private List<GameObject> obstacles;

        private Vector3 currentTileLocation = Vector3.zero;
        private Vector3 currentTileDirection = Vector3.forward;
        private GameObject previousTile;

        private List<GameObject> currentTiles;
        private List<GameObject> currentObstacles;

         [SerializeField] private List<GameObject> powerUpPrefabs;
         [SerializeField] private float powerUpSpawnChance = 0.2f;

        private void Start()
        {
            // Initializes the lists of tiles and obstacles and randomizes their appearance
            currentTiles = new List<GameObject>();
            currentObstacles = new List<GameObject>();
            Random.InitState(System.DateTime.Now.Millisecond);

            // Spawns initial tiles, which are MANDATORY to be STRAIGHT
            for (int i = 0; i < tileStartCount; i++)
            {
                SpawnTile(startingTile.GetComponent<Tile>());
            }

            // Spawns a MANDATORY TURNING tile after the initial MANDATORY STRAIGHT tiles
            SpawnTile(SelectRandomGameObjectFromList(turningTiles).GetComponent<Tile>());
        }

        // Spawning new tiles, one by one
        private void SpawnTile(Tile tile, bool spawnObstacle = false)
        {
            Quaternion newTileRotation = tile.gameObject.transform.rotation * Quaternion.LookRotation(currentTileDirection, Vector3.up);
            previousTile = GameObject.Instantiate(tile.gameObject, currentTileLocation, newTileRotation);
            currentTiles.Add(previousTile);

             if (Random.value < powerUpSpawnChance)
            {
                SpawnPowerUp();
            }

            // If it is specified in the editor for obstacles to be spawned, the script does so
            if (spawnObstacle) 
                SpawnObstacle();

            // Based on its type, adjusts the next tile's location
            if (tile.type == TileType.STRAIGHT)
                currentTileLocation += Vector3.Scale(previousTile.GetComponent<Renderer>().bounds.size, currentTileDirection);
        }

         private void SpawnPowerUp()
        {
          //spawns random power-up from list

          GameObject powerUpPrefab = SelectRandomGameObjectFromList(powerUpPrefabs);
         // if a powerup exists in the scene, it should be spawned 1 unit higher than its default spawn position ( i added this because with y=0 the cubes were stuck in the ground)
        if (powerUpPrefab != null)
         {
        Vector3 spawnPosition = currentTileLocation;
        spawnPosition.y += 1.0f; 

        Quaternion powerUpRotation = powerUpPrefab.gameObject.transform.rotation * Quaternion.LookRotation(currentTileDirection, Vector3.up);
        Instantiate(powerUpPrefab, spawnPosition, powerUpRotation);
         }
        }


        // Deletes previous tiles and obstacles after passing them and changing direction
        private void DeletePreviousTiles()
        {
            while (currentTiles.Count != 1)
            {
                GameObject tile = currentTiles[0];
                currentTiles.RemoveAt(0);
                Destroy(tile);
            }
            while (currentObstacles.Count != 0)
            {
                GameObject obstacle = currentObstacles[0];
                currentObstacles.RemoveAt(0);
                Destroy(obstacle);
            }
        }

        // Adding a new direction and spawning tiles based on the direction the player is now facing
        public void AddNewDirection(Vector3 direction)
        {
            // Updates the new tile's direction and deletes the previous tiles (which were in the previous direction)
            currentTileDirection = direction;
            DeletePreviousTiles();

            // Sets the location for placing the next tile
            Vector3 tilePlacementScale;
            if (previousTile.GetComponent<Tile>().type == TileType.SIDEWAYS)
                tilePlacementScale = Vector3.Scale(previousTile.GetComponent<Renderer>().bounds.size / 2 + (Vector3.one * startingTile.GetComponent<BoxCollider>().size.z / 2), currentTileDirection);
            else
                tilePlacementScale = Vector3.Scale((previousTile.GetComponent<Renderer>().bounds.size - (Vector3.one * 2)) + (Vector3.one * startingTile.GetComponent<BoxCollider>().size.z / 2), currentTileDirection);

            currentTileLocation += tilePlacementScale;

            // Spawning tiles in a straight line
            int currentPathLength = Random.Range(minimumStraightTiles, maximumStraightTiles);
            for (int i = 0; i < currentPathLength; i++)
            {
                SpawnTile(startingTile.GetComponent<Tile>(), (i == 0) ? false : true);
            }

            // Spawning a turning tile(left or right with a pivot)
            SpawnTile(SelectRandomGameObjectFromList(turningTiles).GetComponent<Tile>(), false);
        }

        // Randomly spawning obstacles along the game sessions (that need to be jumped or slid under)
        private void SpawnObstacle()
        {
            if (Random.value > 1f) return;

            GameObject obstaclePrefab = SelectRandomGameObjectFromList(obstacles);
            Quaternion newObjectRotation = obstaclePrefab.gameObject.transform.rotation * Quaternion.LookRotation(currentTileDirection, Vector3.up);
            GameObject obstacle = Instantiate(obstaclePrefab, currentTileLocation, newObjectRotation);
            currentObstacles.Add(obstacle);
        }

        // A random GameObject from the list is selected
        private GameObject SelectRandomGameObjectFromList(List<GameObject> list)
        {
            if (list.Count == 0) return null;
            return list[Random.Range(0, list.Count)];
        }
    }
}
