using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using UnityEngine;

public class Polygonizer
{
	World volume;

	public Polygonizer (World data)
	{
		volume = data;
	}

	public Mesh GenLodCell (World chunk)
	{
		Mesh mesh = new Mesh ();
		int lod = 1;

		for (int x = 0; x < volume.size; x++) {
			for (int z = 0; z < volume.size; z++) {
				for (int y = 0; y < volume.size; y++) {
					Vector3i position;
					position.x = x;
					position.y = y;
					position.z = z;
					PolygonizeCell (new Vector3i (0, 0, 0), position, ref mesh, lod);
				}
			}
		}
		mesh.vertices = vertices.ToArray ();
		mesh.triangles = triangles.ToArray ();
		mesh.RecalculateNormals ();
		return mesh;
	}

	List<Vector3> vertices = new List<Vector3> ();
	List<Vector3> normals = new List<Vector3> ();
	List<int> triangles = new List<int> ();

	ushort[] mappedIndizes = new ushort[15];

	internal void PolygonizeCell (Vector3i offsetPos, Vector3i pos, ref Mesh mesh, int lod)
	{
		offsetPos += pos * lod;
		sbyte[] density = new sbyte[8];

		for (int i = 0; i < density.Length; i++) {
			density [i] = volume [offsetPos + Tables.CornerIndex [i] * lod];
		}

		byte caseCode = (byte)((density [0] >> 7 & 0x01)
		                | (density [1] >> 6 & 0x02)
		                | (density [2] >> 5 & 0x04)
		                | (density [3] >> 4 & 0x08)
		                | (density [4] >> 3 & 0x10)
		                | (density [5] >> 2 & 0x20)
		                | (density [6] >> 1 & 0x40)
		                | (density [7] & 0x80));
		
		if ((caseCode ^ ((density [7] >> 7) & 0xFF)) != 0) {
			byte regCellClass = Tables.RegularCellClass [caseCode];
			ushort[] vertexLocations = Tables.RegularVertexData [caseCode];

			Tables.RegularCell c = Tables.RegularCellData [regCellClass];
			long triangleCount = c.GetTriangleCount ();
			byte[] indexOffset = c.Indizes (); //index offsets for current cell //array with real indizes for current cell

			for (int i = 0; i < c.GetVertexCount (); i++) {
				byte edge = (byte)(vertexLocations [i] & 0xFF);

				byte v0 = (byte)((edge >> 4) & 0x0F); //First Corner Index
				byte v1 = (byte)(edge & 0x0F); //Second Corner Index

				sbyte d0 = density [v0];
				sbyte d1 = density [v1];

				int t = (d1 << 8) / (d1 - d0);
				int u = 0x0100 - t;

				Vector3 P0 = (offsetPos + Tables.CornerIndex [v0] * lod).ToVector3 ();
				Vector3 P1 = (offsetPos + Tables.CornerIndex [v1] * lod).ToVector3 ();
				Vector3 Q = (t * P0 + u * P1) / 256f;
				vertices.Add (Q);

				mappedIndizes [i] = (ushort)(vertices.Count - 1);
			}

			for (int t = 0; t < c.GetTriangleCount (); t++) {
				for (int i = 0; i < 3; i++) {
					triangles.Add (mappedIndizes [indexOffset [t * 3 + i]]);
				}
			}
		}
	}
}