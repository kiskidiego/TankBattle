using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class NetPlayerController : NetworkBehaviour
{
	[SerializeField] GameObject cameraSpot;
	[SerializeField] GameObject bulletSpawn;
	[SerializeField] float maxHealth = 100;
	[SerializeField] float acceleration = 100f;
	[SerializeField] float drag = 50f;
	[SerializeField] float maxSpeed = 1f;
	[SerializeField] float rotationSpeed = 1f;
	[SerializeField] float minChargeStrength = 3;
	[SerializeField] float maxChargeStrength = 15;
	[SerializeField] float chargeSpeed = 3;
	[SerializeField] GameObject model;
	[SerializeField] GameObject bulletPrefab;
	[SerializeField] GameObject hud;
	[SerializeField] PauseMenu pauseMenu;
	Text ammoText;
	Text scoreText;
	Image healthWheel;
	Image chargeBar;
	uint score;
	float forwardMovement;
	float rotation;
	Rigidbody rb;
	PlayerInput playerInput;
	int ammo = 0;
	float chargeStrength = 0;
	bool charging = false;
	public float health { get; private set; }
	

	public override void OnNetworkSpawn()
	{
		AudioManager.Singleton.PlayMusic(Music.Game);
		NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnect;
		health = maxHealth;
		playerInput = GetComponent<PlayerInput>();
		rb = GetComponent<Rigidbody>();
		if (IsOwner)
		{
			hud = Instantiate(hud);
			GameObject camera = new GameObject("Camera");
			camera.AddComponent<Camera>();
			camera.transform.parent = cameraSpot.transform;
			camera.transform.localPosition = Vector3.zero;
			camera.transform.localRotation = Quaternion.identity;
			pauseMenu = Instantiate(pauseMenu);
			pauseMenu.SetPlayer(this);
			SetHud();
		}
		
	}

	[Rpc(SendTo.Server)]
	void UpdateInputRpc(float movement, float rotation)
	{
		if(charging && ammo > 0) 
		{
			forwardMovement = 0;
		}
		else
		{
			forwardMovement = movement;
		}
		this.rotation = rotation;
	}

	[Rpc(SendTo.Server)]
	void BeginChargingRpc()
	{
		if(ammo <= 0)
			return;
		charging = true;
		chargeStrength = minChargeStrength;
	}

	[Rpc(SendTo.Server)]
	void ShootRpc()
	{
		if (ammo <= 0 || !charging)
			return;
		charging = false;
		Rigidbody bullet = Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.transform.rotation).GetComponent<Rigidbody>();
		Physics.IgnoreCollision(bullet.GetComponent<Collider>(), model.GetComponent<Collider>());
		bullet.AddForce(bulletSpawn.transform.forward * chargeStrength, ForceMode.VelocityChange);
		bullet.GetComponent<Bullet>().owner = this;
		bullet.GetComponent<NetworkObject>().Spawn();
		AddAmmoRpc(-1);
	}

	[Rpc(SendTo.Server)]
	public void TakeDamageRpc(float damage)
	{
		health -= damage;
		if (health <= 0)
		{
			transform.position = MapGenerator.Singleton.GetRandomValidCoordinates();
			health = maxHealth;
		}
		SetHudHealthRpc(health);
	}

	void Update()
	{
		if (IsClient && IsOwner)
		{
			ClientUpdate();
		}
		if(IsServer)
		{
			ServerUpdate();
		}
	}
	void ServerUpdate()
	{
		if(charging && ammo > 0 && chargeStrength < maxChargeStrength)
		{
			chargeStrength += (chargeStrength * chargeSpeed + chargeSpeed) * Time.deltaTime;
			Debug.Log("Charge: " + chargeStrength);
			if (chargeStrength > maxChargeStrength)
			{
				chargeStrength = maxChargeStrength;
			}
		}
	}
	void ClientUpdate()
	{
		if (playerInput.actions["Pause"].triggered)
		{
			pauseMenu.gameObject.SetActive(!pauseMenu.gameObject.activeSelf);
			UpdateInputRpc(0, 0);
		}
		if (pauseMenu.gameObject.activeSelf)
			return;

        if (playerInput.actions["Charge"].triggered)
		{
			BeginChargingRpc();
			charging = true;
			chargeStrength = minChargeStrength;
		}
		if (playerInput.actions["Shoot"].triggered)
		{
			ShootRpc();
			chargeBar.fillAmount = 0;
			chargeStrength = 0;
			charging = false;
		}
		if(charging && ammo > 0)
		{
			Debug.Log("Charge: " + chargeStrength);
			chargeStrength += (chargeStrength * chargeSpeed + chargeSpeed) * Time.deltaTime;
			if (chargeStrength > maxChargeStrength)
			{
				chargeStrength = maxChargeStrength;
			}
			chargeBar.fillAmount = (chargeStrength - minChargeStrength) / (maxChargeStrength - minChargeStrength);
		}
		UpdateInputRpc(playerInput.actions["Vertical"].ReadValue<float>(), playerInput.actions["Horizontal"].ReadValue<float>());
	}
	private void FixedUpdate()
	{
		if(IsServer)
		{
			ServerFixedUpdate();
		}
	}

	private void ServerFixedUpdate()
	{
		model.transform.Rotate(Vector3.up, rotation * rotationSpeed);
		rb.AddForce(Utils.FlattenVec3(model.transform.forward) * forwardMovement * acceleration, ForceMode.Acceleration);
		if (Mathf.Approximately(forwardMovement, 0))
			rb.AddForce(Utils.FlattenVec3(-rb.velocity) * drag);
		rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
	}
	private void OnTriggerEnter(Collider other)
	{
		if(!IsServer)
			return;
		if (other.gameObject.tag is "Ammo")
		{
			AddAmmoRpc(1);
			Destroy(other.gameObject);
			SetHudAmmoRpc(ammo);
			AudioManager.Singleton.PlaySfx(Sfx.Pickup, gameObject);
		}
	}
	[Rpc(SendTo.Server)]
	public void AddScoreRpc(uint score)
	{
		this.score += score;
		SetHudScoreRpc(this.score);
	}
	public void SetHud()
	{
		ammoText = hud.transform.GetChild(0).Find("Ammo").GetComponent<Text>();
		scoreText = hud.transform.GetChild(0).Find("Score").GetComponent<Text>();
		healthWheel = hud.transform.GetChild(0).Find("Health").GetComponent<Image>();
		chargeBar = hud.transform.GetChild(0).Find("Charge").GetComponent<Image>();
		ammoText.text = "Ammo: " + ammo;
		scoreText.text = "Score: " + score;
		healthWheel.fillAmount = health / maxHealth;
		chargeBar.fillAmount = 0;
	}
	[Rpc(SendTo.Server)]
	public void AddAmmoRpc(int ammo)
	{
		AudioManager.Singleton.PlaySfx(Sfx.Pickup, gameObject);
		this.ammo += ammo;
		SetHudAmmoRpc(this.ammo);
	}
	[Rpc(SendTo.Owner)]
	public void SetHudAmmoRpc(int ammo)
	{
		this.ammo = ammo;
		ammoText.text = "Ammo: " + ammo;
	}
	[Rpc(SendTo.Owner)]
	public void SetHudScoreRpc(uint score)
	{
		scoreText.text = "Score: " + score;
	}
	[Rpc(SendTo.Owner)]
	public void SetHudHealthRpc(float health)
	{
		healthWheel.fillAmount = health / maxHealth;
	}
	public void DisconnectClient()
	{
		if (IsServer)
		{
			Debug.Log("Disconnecting server");
			DisconnectServerRpc();
		}
		else
		{
			Debug.Log("Disconnecting client");
			DisconnectClientfromServerRpc(OwnerClientId);
		}
	}
	[Rpc(SendTo.Server)]
	void DisconnectClientfromServerRpc(ulong id)
	{
		NetworkManager.Singleton.DisconnectClient(id);
	}
	[Rpc(SendTo.Server)]
	void DisconnectServerRpc()
	{
		NetworkManager.Singleton.Shutdown();
	}
	private void OnDisconnect(ulong id)
	{
		if (id == NetworkManager.Singleton.LocalClientId)
		{
			StopAllCoroutines();
			SceneManager.LoadScene("MainMenu");
		}
	}
}