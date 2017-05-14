using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SomeDrawProceduralTest : MonoBehaviour
{

	public Material mat;
	[Range (1f, 2000000f)]
	public int numberParticles;
	[Range (0.0001f, 100f)]
	public float sizeParticle;

	public int seed = 0;
	public int numBodies = DEFALUT_SIZE;
	public float positionScale = 16.0f;
	public float velocityScale = 1.0f;
	public float damping = 0.96f;
	public float softeningSquared = 0.1f;
	public float speed = 1.0f;


	//	m_numBodies: Sets the number of bodies in the simulation.  This
	//	should be a multiple of 256.
	//
	//	p: Sets the width of the tile used in the simulation.
	//	The default is 64.
	//
	//	q: Sets the height of the tile used in the simulation.
	//	The default is 4.
	//
	//	Note: q is the number of threads per body, and p*q should be
	//	less than or equal to 256.

	//p must match the value of NUM_THREADS in the IntegrateBodies shader


	void Start ()
	{

		if (_p * _q > 256)
			Debug.Log ("NBodySim::Start - p*q must be <= 256. Simulation will have errors.");

		if (numBodies % 256 != 0) {
			while (numBodies % 256 != 0)
				numBodies++;

			Debug.Log ("NBodySim::Start - numBodies must be a multiple of 256. Changing numBodies to " + numBodies);
		}

		_positions = new ComputeBuffer[2];
		_velocities = new ComputeBuffer[2];

		_positions [READ] = new ComputeBuffer (numBodies, sizeof(float) * 4);
		_positions [WRITE] = new ComputeBuffer (numBodies, sizeof(float) * 4);

		_velocities [READ] = new ComputeBuffer (numBodies, sizeof(float) * 4);
		_velocities [WRITE] = new ComputeBuffer (numBodies, sizeof(float) * 4);

		Random.InitState (seed);

		_quadCorners = new ComputeBuffer (6, 4 * sizeof(float));
		_quadCorners.SetData (
			new Vector4[] {
				new Vector4 (-sizeParticle, -sizeParticle, 0f, 0f),
				new Vector4 (-sizeParticle, sizeParticle, 0f, 0f),
				new Vector4 (sizeParticle, sizeParticle, 0f, 0f),
				new Vector4 (sizeParticle, sizeParticle, 0f, 0f),
				new Vector4 (sizeParticle, -sizeParticle, 0f, 0f),
				new Vector4 (-sizeParticle, -sizeParticle, 0f, 0f)
			}
		);

		mat.SetBuffer ("_quadCornersBuffer", _quadCorners);

		ConfigRandom ();

	}

	void ConfigRandom ()
	{
		float scale = positionScale * Mathf.Max (1, numBodies / DEFALUT_SIZE);
		float vscale = velocityScale * scale;

		Vector4[] positions = new Vector4[numBodies];
		Vector4[] velocities = new Vector4[numBodies];

		int i = 0;
		while (i < numBodies) {
			Vector3 pos = new Vector3 (Random.Range (-1.0f, 1.0f), Random.Range (-1.0f, 1.0f), Random.Range (-1.0f, 1.0f));
			Vector3 vel = new Vector3 (Random.Range (-1.0f, 1.0f), Random.Range (-1.0f, 1.0f), Random.Range (-1.0f, 1.0f));

			if (Vector3.Dot (pos, pos) > 1.0)
				continue;
			if (Vector3.Dot (vel, vel) > 1.0)
				continue;

			positions [i] = new Vector4 (pos.x * scale, pos.y * scale, pos.z * scale, 1.0f);
			velocities [i] = new Vector4 (vel.x * vscale, vel.y * vscale, vel.z * vscale, 1.0f);

			i++;
		}

		_positions [READ].SetData (positions);
		_positions [WRITE].SetData (positions);

		_velocities [READ].SetData (velocities);
		_velocities [WRITE].SetData (velocities);

	}

	void Swap (ComputeBuffer[] buffer)
	{
		ComputeBuffer tmp = buffer [READ];
		buffer [READ] = buffer [WRITE];
		buffer [WRITE] = tmp;
	}

	void Update ()
	{
		_simParticlesComputeShader.SetFloat ("_DeltaTime", Time.deltaTime * speed);
		_simParticlesComputeShader.SetFloat ("_Damping", damping);
		_simParticlesComputeShader.SetFloat ("_SofteningSquared", softeningSquared);
		_simParticlesComputeShader.SetInt ("_NumBodies", numBodies);
		_simParticlesComputeShader.SetVector ("_ThreadDim", new Vector4 (_p, _q, 1, 0));
		_simParticlesComputeShader.SetVector ("_GroupDim", new Vector4 (1f * numBodies / _p, 1, 1, 0));
		_simParticlesComputeShader.SetBuffer (0, "_ReadPos", _positions [READ]);
		_simParticlesComputeShader.SetBuffer (0, "_WritePos", _positions [WRITE]);
		_simParticlesComputeShader.SetBuffer (0, "_ReadVel", _velocities [READ]);
		_simParticlesComputeShader.SetBuffer (0, "_WriteVel", _velocities [WRITE]);

		_simParticlesComputeShader.Dispatch (0, numBodies / _p, 1, 1);

		Swap (_positions);
		Swap (_velocities);

	}

	void OnRenderObject ()
	{
		mat.SetPass (0);
		mat.SetBuffer ("_positionsBuffer", _positions [READ]);

		Graphics.DrawProcedural (MeshTopology.Triangles, 6 * numberParticles);
	}

	void OnDestroy ()
	{
		_positions [READ].Release ();
		_positions [WRITE].Release ();
		_velocities [READ].Release ();
		_velocities [WRITE].Release ();
		_quadCorners.Release ();
	}

	private ComputeBuffer _quadCorners;
	private ComputeBuffer[] _positions, _velocities;
	private int _p = 64;
	private int _q = 4;

	[SerializeField]
	public ComputeShader _simParticlesComputeShader;

	const int READ = 0;
	const int WRITE = 1;
	const int DEFALUT_SIZE = 512;
}
