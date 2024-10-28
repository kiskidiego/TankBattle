using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Explosion : NetworkBehaviour
{
    void Start()
	{
		StartCoroutine(DestroyAfterTime());
	}
	IEnumerator DestroyAfterTime()
	{
		yield return new WaitForSeconds(1);
		Destroy(gameObject);
	}
}
