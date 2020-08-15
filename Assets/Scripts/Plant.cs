using System.Collections.Generic;
using UnityEngine;

namespace com.jlabarca.cpattern
{
	public class Plant {
		public int x;
		public int y;
		public Mesh mesh;
		public float growth;
		public bool reserved;
		public bool harvested;
		public Matrix4x4 matrix;
		public Quaternion rotation;
		public int index;
		public int seed;

		public static Dictionary<int,Mesh> meshLookup;

		public void Init(int posX, int posY, int randSeed) {
			Random.InitState(0);
			seed = randSeed;
			if (meshLookup==null) {
				meshLookup = new Dictionary<int,Mesh>();
			}
			x = posX;
			mesh = GetMesh(randSeed);
			growth = 0f;
			y = posY;
			harvested = false;
			reserved = false;
			var worldPos = new Vector3(posX+.5f,0f,posY+.5f);
			rotation = Quaternion.Euler(Random.Range(-5f,5f),Random.value * 360f,Random.Range(-5f,5f));
			matrix = Matrix4x4.TRS(worldPos,rotation,Vector3.one);
		}

		public void EaseToWorldPosition(float x, float y, float z, float smooth) {
			matrix.m03 += (x - matrix.m03) * smooth*3f;
			matrix.m13 += (y - matrix.m13) * smooth*3f;
			matrix.m23 += (z - matrix.m23) * smooth*3f;
			ApplyMatrixToFarm();
		}
		public void ApplyMatrixToFarm() {
			Map.plantMatrices[seed][index / Map.InstancesPerBatch][index % Map.InstancesPerBatch] = matrix;
		}

		private Mesh GetMesh(int seed) {
			Mesh output;
			if (meshLookup.TryGetValue(seed,out output)) {
				return output;
			} else {
				return GenerateMesh(seed);
			}
		}

		private Mesh GenerateMesh(int seed) {
			var oldRandState = Random.state;
			Random.InitState(seed);

			var vertices = new List<Vector3>();
			var triangles = new List<int>();
			var colors = new List<Color>();
			var uv = new List<Vector2>();

			var color1 = Random.ColorHSV(0f,1f,.5f,.8f,.25f,.9f);
			var color2 = Random.ColorHSV(0f,1f,.5f,.8f,.25f,.9f);

			var height = Random.Range(.4f,1.4f);

			var angle = Random.value * Mathf.PI * 2f;
			var armLength1 = Random.value * .4f + .1f;
			var armLength2 = Random.value * .4f + .1f;
			var armRaise1 = Random.value * .3f;
			var armRaise2 = Random.value * .6f - .3f;
			var armWidth1 = Random.value * .5f + .2f;
			var armWidth2 = Random.value * .5f + .2f;
			var armJitter1 = Random.value * .3f;
			var armJitter2 = Random.value * .3f;
			var stalkWaveStr = Random.value * .5f;
			var stalkWaveFreq = Random.Range(.25f,1f);
			var stalkWaveOffset = Random.value * Mathf.PI * 2f;

			var triCount = Random.Range(15,35);

			for (var i=0;i<triCount;i++) {
				// front face
				triangles.Add(vertices.Count);
				triangles.Add(vertices.Count+1);
				triangles.Add(vertices.Count+2);

				// back face
				triangles.Add(vertices.Count + 1);
				triangles.Add(vertices.Count);
				triangles.Add(vertices.Count + 2);

				var t = i / (triCount-1f);
				var armLength = Mathf.Lerp(armLength1,armLength2,t);
				var armRaise = Mathf.Lerp(armRaise1,armRaise2,t);
				var armWidth = Mathf.Lerp(armWidth1,armWidth2,t);
				var armJitter = Mathf.Lerp(armJitter1,armJitter2,t);
				var stalkWave = Mathf.Sin(t*stalkWaveFreq*2f*Mathf.PI+stalkWaveOffset) * stalkWaveStr;

				var y = t * height;
				vertices.Add(new Vector3(stalkWave,y,0f));
				var armPos = new Vector3(stalkWave + Mathf.Cos(angle)*armLength,y + armRaise,Mathf.Sin(angle)*armLength);
				vertices.Add(armPos + Random.insideUnitSphere * armJitter);
				armPos = new Vector3(stalkWave + Mathf.Cos(angle+armWidth) * armLength,y + armRaise,Mathf.Sin(angle+armWidth) * armLength);
				vertices.Add(armPos+Random.insideUnitSphere*armJitter);

				colors.Add(color1);
				colors.Add(color2);
				colors.Add(color2);
				uv.Add(Vector2.zero);
				uv.Add(Vector2.right);
				uv.Add(Vector2.right);

				// golden angle in radians
				angle += 2.4f;
			}

			var outputMesh = new Mesh();
			outputMesh.name = "Generated Plant (" + seed + ")";

			outputMesh.SetVertices(vertices);
			outputMesh.SetColors(colors);
			outputMesh.SetTriangles(triangles,0);
			outputMesh.RecalculateNormals();

			meshLookup.Add(seed,outputMesh);

			Map.RegisterSeed(seed);
			Random.state = oldRandState;
			return outputMesh;
		}
	}
}
