using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingObject : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 1;
    [SerializeField] float rotationVariation = 0.1f;
	private void Start()
	{
		rotationSpeed += Random.Range(-rotationVariation, rotationVariation);
		if(Random.Range(0, 2) == 0)
		{
			rotationSpeed *= -1;
		}
	}
	void Update()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}
