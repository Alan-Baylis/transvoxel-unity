using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Transvoxel.Math;

public class World : MonoBehaviour
{
	public sbyte[,,] values;
	public int size = 64;
	Polygonizer extractor;
	public RawImage texture;

	void Start ()
	{
		values = new sbyte[size, size, size];
		extractor = new Polygonizer (this);

		for (int x = 0; x < size; x++) {
			for (int z = 0; z < size; z++) {
				float noise = 0f;//(float)((SimplexNoise.noise (x * 1f, z * 1f) * 0.01f));
				for (int y = 0; y < size; y++) {
					if (y < 8)
						values [x, y, z] = (sbyte)Random.Range (0, 127);//(sbyte)Mathf.Clamp (noise - y, -127, 127);//(sbyte)Mathf.Clamp (((((noise + 1f) * 32f) + 0.5f) - (y + 0.5f)), -127, 127);
					else
						values [x, y, z] = (sbyte)Random.Range (-127, 0);
				}
			}
		}
		Polygonize ();
	}

	public sbyte this [int x, int y, int z] {
		get { 
			if (x >= size)
				x = size - 1;

			if (y >= size)
				y = size - 1;

			if (z >= size)
				z = size - 1;

			if (x < 0)
				x = 0;

			if (y < 0)
				y = 0;

			if (z < 0)
				z = 0;
			return values [x, y, z];
		}
	}

	public sbyte this [Vector3i v] {
		get {
			int x = v.x, y = v.y, z = v.z; 
			if (x >= size)
				x = size - 1;

			if (y >= size)
				y = size - 1;

			if (z >= size)
				z = size - 1;

			if (x < 0)
				x = 0;

			if (y < 0)
				y = 0;

			if (z < 0)
				z = 0;
			return values [x, y, z];
		}
	}

	public void Polygonize ()
	{
		Mesh mesh = extractor.GenLodCell (this);
		GetComponent<MeshFilter> ().mesh = mesh;
	}
}