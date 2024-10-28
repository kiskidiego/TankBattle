using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.SceneManagement;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] int mapSize = 10;
    [SerializeField] int tileSize = 10;
    [SerializeField] int tileAmount = 10;
    [SerializeField] float tileVariationProbability = .25f;
    [SerializeField] int seed;
    [SerializeField] GameObject tilePrefab;
    [SerializeField] GameObject[] tileVariationPrefabs;
    [SerializeField] float bulletSpawnTime = 1;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] GameObject netManagerPrefab;
    List<Vector2> tilecoords = new List<Vector2>();
    public bool ingame = false;
    float timer = 0;
    public static MapGenerator Singleton;
    void Awake()
	{
        Singleton = this;
		SceneManager.MoveGameObjectToScene(Instantiate(netManagerPrefab), SceneManager.GetActiveScene());
	}
	public void Generate()
    {
        if(!NetworkManager.Singleton.IsHost)
        {
			return;
		}
        if (seed == 0)
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
        }
        Random.InitState(seed);
        GameObject[,] tiles = new GameObject[mapSize, mapSize];
        int x = mapSize / 2;
        int y = mapSize / 2;
        for (int i = 0; i < tileAmount; i++)
        {
            tilecoords.Add(new Vector2(x, y));
            int rotation = Random.Range(0, 4);
            rotation *= 90;
            if (Random.Range(0f, 1f) > tileVariationProbability)
            {
                tiles[x, y] = Instantiate(tilePrefab, new Vector3(x * tileSize - mapSize * tileSize / 2, 0, y * tileSize - mapSize * tileSize / 2), Quaternion.Euler(0, rotation, 0), transform);
            }
            else
            {
                tiles[x, y] = Instantiate(tileVariationPrefabs[Random.Range(0, tileVariationPrefabs.Length)], new Vector3(x * tileSize - mapSize * tileSize / 2, 0, y * tileSize - mapSize * tileSize / 2), Quaternion.Euler(0, rotation, 0), transform);
            }
            tiles[x, y].GetComponent<NetworkObject>().Spawn();
            switch (rotation)
            {
                case 0:
                    break;

                case 90:
                    GameObject north = tiles[x, y].transform.Find("North").gameObject;
                    GameObject south = tiles[x, y].transform.Find("South").gameObject;
                    GameObject east = tiles[x, y].transform.Find("East").gameObject;
                    GameObject west = tiles[x, y].transform.Find("West").gameObject;

                    north.name = "East";
                    south.name = "West";
                    east.name = "South";
                    west.name = "North";
                    break;

                    case 180:
                    north = tiles[x, y].transform.Find("North").gameObject;
                    south = tiles[x, y].transform.Find("South").gameObject;
                    east = tiles[x, y].transform.Find("East").gameObject;
                    west = tiles[x, y].transform.Find("West").gameObject;
                    
                    north.name = "South";
                    south.name = "North";
                    east.name = "West";
                    west.name = "East";
                    break;

                    case 270:
                    north = tiles[x, y].transform.Find("North").gameObject;
                    south = tiles[x, y].transform.Find("South").gameObject;
                    east = tiles[x, y].transform.Find("East").gameObject;
                    west = tiles[x, y].transform.Find("West").gameObject;

                    north.name = "West";
                    south.name = "East";
                    east.name = "North";
                    west.name = "South";
                    break;
            }
            if (x > 0 && tiles[x - 1, y] != null)
            {
                tiles[x - 1, y].transform.Find("East").gameObject.SetActive(false);
                tiles[x, y].transform.Find("West").gameObject.SetActive(false);
            }
            if (x < mapSize - 1 && tiles[x + 1, y] != null)
            {
                tiles[x + 1, y].transform.Find("West").gameObject.SetActive(false);
                tiles[x, y].transform.Find("East").gameObject.SetActive(false);
            }
            if (y > 0 &&tiles[x, y - 1] != null)
            {
                tiles[x, y - 1].transform.Find("North").gameObject.SetActive(false);
                tiles[x, y].transform.Find("South").gameObject.SetActive(false);
            }
            if (y < mapSize - 1 && tiles[x, y + 1] != null)
            {
                tiles[x, y + 1].transform.Find("South").gameObject.SetActive(false);
                tiles[x, y].transform.Find("North").gameObject.SetActive(false);
            }
            bool valid = false;
            while (!valid)
            {
                int direction = Random.Range(0, 4);
                Random.InitState(++seed);
                switch (direction)
                {
                    case 0:
                        if (x < mapSize - 1)
                        {
                            x++;
                        }
                        break;
                    case 1:
                        if (x > 0)
                        {
                            x--;
                        }
                        break;
                    case 2:
                        if (y < mapSize - 1)
                        {
                            y++;
                        }
                        break;
                    case 3:
                        if (y > 0)
                        {
                            y--;
                        }
                        break;
                }
				if (tiles[x, y] == null)
				{
					valid = true;
				}
			}
        }
        ingame = true;
    }
	private void Update()
	{
        if (!ingame)
        {
			return;
        }
        timer += Time.deltaTime;
        if(timer >= bulletSpawnTime)
        {
			timer = 0;
			Vector2 tile = tilecoords[Random.Range(0, tilecoords.Count)];
			Vector3 basePosition = new Vector3(tile.x * tileSize - mapSize * tileSize / 2, .4f, tile.y * tileSize - mapSize * tileSize / 2);
			Vector3 position;
			do
			{
				position = basePosition + new Vector3(Random.Range(-tileSize / 2, tileSize / 2), .4f, Random.Range(-tileSize / 2, tileSize / 2));
				Random.InitState(++seed);
			} while (Physics.OverlapSphere(position, .35f).Length > 0);
            Instantiate(bulletPrefab, position, Quaternion.identity).GetComponent<NetworkObject>().Spawn();
		}
	}
    public Vector3 GetRandomValidCoordinates()
    {
		Vector2 tile = tilecoords[Random.Range(0, tilecoords.Count)];
		Vector3 basePosition = new Vector3(tile.x * tileSize - mapSize * tileSize / 2, .4f, tile.y * tileSize - mapSize * tileSize / 2);
		Vector3 position;
		do
		{
			position = basePosition + new Vector3(Random.Range(-tileSize / 2, tileSize / 2), .4f, Random.Range(-tileSize / 2, tileSize / 2));
			Random.InitState(++seed);
		} while (Physics.OverlapSphere(position, .35f).Length > 0);
        return position;
	}
}