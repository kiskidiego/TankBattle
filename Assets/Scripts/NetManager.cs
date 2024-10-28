using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetManager : MonoBehaviour
{
	public static bool isHost = false;
	private static NetworkManager NetworkManager;

	void Start()
	{
		NetworkManager = GetComponent<NetworkManager>();
		if (isHost)
		{
			StartHost();
		}
		else
		{
			StartClient();
			StartCoroutine(IfCantConnect());
		}
	}
	void StartHost()
	{
		NetworkManager.StartHost();
		MapGenerator.Singleton.Generate();
	}
	void StartClient()
	{
		NetworkManager.StartClient();
	}
	IEnumerator IfCantConnect()
	{
		yield return new WaitForSeconds(3);
		if (NetworkManager.IsClient && !NetworkManager.IsConnectedClient)
		{
			SceneManager.LoadScene("MainMenu");
		}
	}
}
