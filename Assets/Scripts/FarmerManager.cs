using System;
using System.Collections.Generic;
using com.jlabarca.cpattern.Core;
using com.jlabarca.cpattern.Core.Commands;
using UnityEngine;
using Random = UnityEngine.Random;

namespace com.jlabarca.cpattern
{
	public class FarmerManager : MonoBehaviour {
		public Mesh farmerMesh;
		public Material farmerMaterial;
		public int initialFarmerCount;
		public int maxFarmerCount;
		[Range(0f,1f)]
		public float movementSmooth;
		public Farmer firstFarmer;

		public List<Farmer> farmers;
		public List<Matrix4x4> farmerMatrices;

		public static FarmerManager instance;
		public static int farmerCount;

		public void SpawnFarmer() {
			var mapSize = Map.instance.mapVector;
			for (var i = 0; i < initialFarmerCount; i++) {
				Random.InitState((int) Math.Pow(i,i));
				var spawnPos = new Vector2Int(Random.Range(0,mapSize.x),Random.Range(0,mapSize.y));
				if (Map.IsBlocked(spawnPos)) continue;
				SpawnFarmer(spawnPos.x,spawnPos.y);
			}
		}

		public static void SpawnFarmer(int x, int y) {
			var pos = new Vector2(x + .5f,y + .5f);
			var farmer = new Farmer(pos);
			instance.farmers.Add(farmer);
			instance.farmerMatrices.Add(farmer.matrix);
			farmerCount++;
		}

		private void Awake() {
			instance = this;
		}

		private void Start() {
			farmers = new List<Farmer>();
			farmerMatrices = new List<Matrix4x4>();
			farmerCount = 0;

			SpawnFarmer();
			firstFarmer = farmers[0];
			var canvasList = FindObjectsOfType<Canvas>();
			foreach (var canvas in canvasList)
			{
				Debug.Log(canvas.gameObject.name);
			}
		}

		private float _smooth;

		public void Tick()
		{
			_smooth = 0.016f; //1f - Mathf.Pow(movementSmooth,Time.timeScale * Time.deltaTime);

			if (farmers.Count <= 0) return;

			for (var i = 0; i < farmers.Count; i++) {

				CommandManager.instance.AddCommand(new ActorTickCommand(farmers[i], _smooth));
				farmerMatrices[i] = farmers[i].matrix;
			}
		}

		private void Update()
		{
			Graphics.DrawMeshInstanced(farmerMesh,0,farmerMaterial,farmerMatrices);
		}
	}
}
