using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static Vector3 FlattenVec3(Vector3 vec)
	{
		return new Vector3(vec.x, 0, vec.z);
	}
}
