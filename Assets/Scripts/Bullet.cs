using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [SerializeField] float explosionRadius = 3;
    [SerializeField] float explosionForce = 1000;
    [SerializeField] float damage = 30;
	[SerializeField] GameObject explosionPrefab;

	public NetPlayerController owner;

	[Rpc(SendTo.Server)]
	void CollisionRpc()
	{
		Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, LayerMask.GetMask("Player"));
		foreach (Collider collider in colliders)
		{
			GameObject tank = collider.transform.parent.gameObject;
			Debug.Log("Hit: " + collider.gameObject.name);
			tank.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, Utils.FlattenVec3(transform.position), explosionRadius);
			float dmg = damage / (Vector3.Distance(transform.position, tank.transform.position) + 1);
			NetPlayerController tankController = tank.GetComponent<NetPlayerController>();
			if(tankController != owner && dmg >= tankController.health)
			{
				owner.AddScoreRpc(1);
			}
			tankController.TakeDamageRpc(dmg);
		}
		Instantiate(explosionPrefab, transform.position, Quaternion.identity).GetComponent<NetworkObject>().Spawn();
		Destroy(gameObject);
		AudioManager.Singleton.PlaySfx(Sfx.Explosion, gameObject);
	}
	void OnTriggerEnter(Collider other)
    {
		if(!IsServer)
			return;
		CollisionRpc();
    }
}
