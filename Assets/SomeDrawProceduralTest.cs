using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SomeDrawProceduralTest : MonoBehaviour
{

	public Material mat;
	[Range (1f, 500f)]
	public int numberParticles;
	[Range (0.0001f, 100f)]
	public float sizeParticle;

	void OnRenderObject ()
	{
		mat.SetPass (0);
		mat.SetFloat ("_numberParticles", numberParticles);
		mat.SetFloat ("_sizeParticle", sizeParticle);

		Graphics.DrawProcedural (MeshTopology.Quads, 6 * numberParticles);
	}
}
