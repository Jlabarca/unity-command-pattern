using System.Collections.Generic;
using com.jlabarca.cpattern.Enums;
using com.jlabarca.cpattern.Navigation;
using UnityEngine;
using Random = UnityEngine.Random;

namespace com.jlabarca.cpattern
{
	public class Map : MonoBehaviour
	{
		public Vector2Int mapVector;
		public int mapSeed;

		public int storeCount;
		public int rockSpawnAttempts;

		[Space(10)]
		public Mesh rockMesh;
		public Material rockMaterial;
		public Material plantMaterial;
		public Mesh groundMesh;
		public Material groundMaterial;
		public Mesh storeMesh;
		public Material storeMaterial;
		public AnimationCurve soldPlantYCurve;
		public AnimationCurve soldPlantXzScaleCurve;
		public AnimationCurve soldPlantYScaleCurve;


		public static GroundState[,] groundStates;
		public static List<Rock> rocks;
		public static Rock[,] tileRocks;
		public static Dictionary<int, List<Plant>> plants;
		public static Plant[,] tilePlants;
		public static List<List<Matrix4x4>> rockMatrices;
		private static List<int> _plantSeeds;
		public static Dictionary<int, List<List<Matrix4x4>>> plantMatrices;
		private Matrix4x4[][] _groundMatrices;
		private static float[][] _tilledProperties;
		private MaterialPropertyBlock[] _groundMatProps;
		private MaterialPropertyBlock _plantMatProps;
		private float[] _plantGrowthProperties;
		public static bool[,] storeTiles;
		private Matrix4x4[] _storeMatrices;

		private static List<Plant> _pooledPlants;
		private static List<Plant> _soldPlants;
		private static List<float> _soldPlantTimers;

		public static Map instance;
		public static int seedOffset;
		private static int _moneyForFarmers;
		private static int _moneyForDrones;

		public const int InstancesPerBatch = 1023;

		private void TrySpawnRock()
		{
			var width = Random.Range(0, 4);
			var height = Random.Range(0, 4);
			var rockX = Random.Range(0, mapVector.x - width);
			var rockY = Random.Range(0, mapVector.y - height);
			var rect = new RectInt(rockX, rockY, width, height);

			var blocked = false;
			for (var x = rockX; x <= rockX + width; x++)
			{
				for (var y = rockY; y <= rockY + height; y++)
				{
					if (tileRocks[x, y] == null && !storeTiles[x, y]) continue;
					blocked = true;
					break;
				}

				if (blocked) break;
			}

			if (blocked) return;

			var rock = new Rock(rect);
			rocks.Add(rock);
			if (rockMatrices[rockMatrices.Count - 1].Count == InstancesPerBatch)
			{
				rockMatrices.Add(new List<Matrix4x4>());
			}

			rock.batchNumber = rockMatrices.Count - 1;
			rock.batchIndex = rockMatrices[rock.batchNumber].Count;
			rockMatrices[rockMatrices.Count - 1].Add(rock.matrix);

			for (var x = rockX; x <= rockX + width; x++)
			{
				for (var y = rockY; y <= rockY + height; y++)
				{
					tileRocks[x, y] = rock;
				}
			}
		}

		public static void DeleteRock(Rock rock)
		{
			var index = rocks.IndexOf(rock);
			if (index == -1) return;
			rocks.RemoveAt(index);
			rock.matrix.m00 = 0f;
			rock.matrix.m11 = 0f;
			rock.matrix.m22 = 0f;
			rockMatrices[rock.batchNumber][rock.batchIndex] = rock.matrix;
			for (var x = rock.rect.min.x; x <= rock.rect.max.x; x++)
			{
				for (var y = rock.rect.min.y; y <= rock.rect.max.y; y++)
				{
					tileRocks[x, y] = null;
				}
			}
		}

		public static void TillGround(int x, int y)
		{
			groundStates[x, y] = GroundState.Tilled;
			var index = y * instance.mapVector.x + x;
			_tilledProperties[index / InstancesPerBatch][index % InstancesPerBatch] = Random.Range(.8f, 1f);
		}

		public static void RegisterSeed(int seed)
		{
			_plantSeeds.Add(seed);
			plantMatrices.Add(seed,
				new List<List<Matrix4x4>>(
					Mathf.CeilToInt(instance.mapVector.x * instance.mapVector.y / (float) InstancesPerBatch)));
			plantMatrices[seed].Add(new List<Matrix4x4>(InstancesPerBatch));
			plants.Add(seed, new List<Plant>(instance.mapVector.x * instance.mapVector.y));
		}

		public static void SpawnPlant(int x, int y, int seed)
		{
			var plant = _pooledPlants[_pooledPlants.Count - 1];
			_pooledPlants.RemoveAt(_pooledPlants.Count - 1);
			plant.Init(x, y, seed);
			plant.index = plants[seed].Count;
			plants[seed].Add(plant);
			tilePlants[x, y] = plant;
			groundStates[x, y] = GroundState.Plant;

			var matrices = plantMatrices[seed];

			if (matrices[matrices.Count - 1].Count == InstancesPerBatch)
			{
				matrices.Add(new List<Matrix4x4>(InstancesPerBatch));
			}

			matrices[matrices.Count - 1].Add(plant.matrix);
		}

		public static void HarvestPlant(int x, int y)
		{
			tilePlants[x, y].harvested = true;
			tilePlants[x, y] = null;
			groundStates[x, y] = GroundState.Tilled;
		}

		public static void SellPlant(Plant plant, int storeX, int storeY)
		{
			plant.x = storeX;
			plant.y = storeY;
			_soldPlants.Add(plant);
			_soldPlantTimers.Add(0f);
			_moneyForFarmers++;
			if (_moneyForFarmers >= 10)
			{
				if (FarmerManager.farmerCount < FarmerManager.instance.maxFarmerCount)
				{
					FarmerManager.SpawnFarmer(storeX, storeY);
					_moneyForFarmers -= 10;
				}
			}

			_moneyForDrones++;
			if (_moneyForDrones < 50) return;
			for (var i = 0; i < 5; i++)
			{
				if (DroneManager.droneCount < DroneManager.instance.maxDroneCount)
				{
					DroneManager.SpawnDrone(storeX, storeY);
				}
			}

			_moneyForDrones -= 50;
		}

		public static void DeletePlant(Plant plant)
		{
			_pooledPlants.Add(plant);

			var plantList = plants[plant.seed];
			plantList[plant.index] = plantList[plantList.Count - 1];
			plantList[plant.index].index = plant.index;
			plantList.RemoveAt(plantList.Count - 1);

			var matrices = plantMatrices[plant.seed];
			var lastBatch = matrices[matrices.Count - 1];
			if (lastBatch.Count == 0)
			{
				matrices.RemoveAt(matrices.Count - 1);
				lastBatch = matrices[matrices.Count - 1];
			}

			matrices[plant.index / InstancesPerBatch][plant.index % InstancesPerBatch] = lastBatch[lastBatch.Count - 1];
			lastBatch.RemoveAt(lastBatch.Count - 1);
		}

		public static bool IsHarvestable(int x, int y)
		{
			var plant = tilePlants[x, y];
			return plant != null && plant.growth >= 1f;
		}

		public static bool IsHarvestableAndUnreserved(int x, int y)
		{
			var plant = tilePlants[x, y];
			return plant != null && plant.growth >= 1f && plant.reserved == false;
		}

		public static bool IsBlocked(Vector2Int tile)
		{
			return IsBlocked(tile.x, tile.y);
		}

		public static bool IsBlocked(int x, int y)
		{
			if (x < 0 || y < 0 || x >= instance.mapVector.x || y >= instance.mapVector.y)
			{
				return true;
			}

			if (tileRocks[x, y] != null)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		private Matrix4x4 GroundMatrix(int x, int y)
		{
			var pos = new Vector3(x + .5f, 0f, y + .5f);
			var zRot = Random.Range(0, 2) * 180f;
			return Matrix4x4.TRS(pos, Quaternion.Euler(90f, 0f, zRot), Vector3.one);
		}

		private void Awake()
		{
			Random.InitState(mapSeed);
			instance = this;

			_moneyForFarmers = 5;
			_moneyForDrones = 0;

			Pathing.Init();
			seedOffset = Random.Range(int.MinValue, int.MaxValue);

			rocks = new List<Rock>();
			plants = new Dictionary<int, List<Plant>>();
			rockMatrices = new List<List<Matrix4x4>>();
			rockMatrices.Add(new List<Matrix4x4>());
			plantMatrices = new Dictionary<int, List<List<Matrix4x4>>>();
			_plantSeeds = new List<int>();
			_plantMatProps = new MaterialPropertyBlock();
			_plantGrowthProperties = new float[InstancesPerBatch];
			_soldPlants = new List<Plant>(100);
			_soldPlantTimers = new List<float>(100);

			var tileCount = mapVector.x * mapVector.y;
			_pooledPlants = new List<Plant>(tileCount);
			for (var i = 0; i < tileCount; i++)
			{
				_pooledPlants.Add(new Plant());
			}

			groundStates = new GroundState[mapVector.x, mapVector.y];
			tileRocks = new Rock[mapVector.x, mapVector.y];
			tilePlants = new Plant[mapVector.x, mapVector.y];
			storeTiles = new bool[mapVector.x, mapVector.y];
			_storeMatrices = new Matrix4x4[storeCount];
			_groundMatrices = new Matrix4x4[Mathf.CeilToInt((float) tileCount / InstancesPerBatch)][];
			_groundMatProps = new MaterialPropertyBlock[_groundMatrices.Length];
			_tilledProperties = new float[_groundMatrices.Length][];
			for (var i = 0; i < _groundMatrices.Length; i++)
			{
				_groundMatProps[i] = new MaterialPropertyBlock();
				if (i < _groundMatrices.Length - 1)
				{
					_groundMatrices[i] = new Matrix4x4[InstancesPerBatch];
				}
				else
				{
					_groundMatrices[i] = new Matrix4x4[tileCount - i * InstancesPerBatch];
				}

				_tilledProperties[i] = new float[_groundMatrices[i].Length];
			}

			for (var y = 0; y < mapVector.y; y++)
			{
				for (var x = 0; x < mapVector.x; x++)
				{
					groundStates[x, y] = GroundState.Default;
					var index = y * mapVector.x + x;
					_groundMatrices[index / InstancesPerBatch][index % InstancesPerBatch] = GroundMatrix(x, y);
					_tilledProperties[index / InstancesPerBatch][index % InstancesPerBatch] = Random.value * .2f;
				}
			}

			for (var i = 0; i < _tilledProperties.Length; i++)
			{
				_groundMatProps[i].SetFloatArray("_Tilled", _tilledProperties[i]);
			}

			var spawnedStores = 0;

			while (spawnedStores < storeCount)
			{
				var x = Random.Range(0, mapVector.x);
				var y = Random.Range(0, mapVector.y);

				if (storeTiles[x, y]) continue;

				storeTiles[x, y] = true;
				_storeMatrices[spawnedStores] = Matrix4x4.TRS(new Vector3(x + .5f, .6f, y + .5f),
					Quaternion.identity, new Vector3(1f, .6f, 1f));
				spawnedStores++;
			}

			for (var i = 0; i < rockSpawnAttempts; i++)
			{
				TrySpawnRock();
			}
		}

		private Camera _cam;

		private void Update()
		{
			/*
		if (cam==null) {
			cam = Camera.main;
		}
		Ray ray = cam.ScreenPointToRay(Input.mousePosition);
		Plane groundPlane = new Plane(Vector3.up,Vector3.zero);
		float dist;
		if (groundPlane.Raycast(ray, out dist)) {
			Vector3 mousePos = ray.GetPoint(dist);
			Debug.DrawRay(mousePos,Vector3.up * 5f,Color.red);
			int x = Mathf.FloorToInt(mousePos.x);
			int y = Mathf.FloorToInt(mousePos.z);
			if (x>=0 && y>=0 && x<mapSize.x && y<mapSize.y) {
				if (Input.GetKey(KeyCode.Mouse0)) {
					if (groundStates[x,y]==GroundState.Tilled) {
						Plant plant = tilePlants[x,y];
						HarvestPlant(x,y);
						DeletePlant(plant);
					}
				}
			}
		}*/

			var smooth = 1f - Mathf.Pow(.1f, Time.deltaTime);
			for (var i = 0; i < _soldPlants.Count; i++)
			{
				var plant = _soldPlants[i];
				_soldPlantTimers[i] += Time.deltaTime;
				var t = _soldPlantTimers[i];
				var y = soldPlantYCurve.Evaluate(t);
				var x = _soldPlants[i].x + .5f;
				var z = _soldPlants[i].y + .5f;
				var scaleXz = soldPlantXzScaleCurve.Evaluate(t);
				var scaleY = soldPlantYScaleCurve.Evaluate(t);
				plant.EaseToWorldPosition(x, y, z, smooth);
				var pos = new Vector3(plant.matrix.m03, plant.matrix.m13, plant.matrix.m23);
				plant.matrix = Matrix4x4.TRS(pos, plant.rotation, new Vector3(scaleXz, scaleY, scaleXz));
				plant.ApplyMatrixToFarm();
				if (t >= 1f)
				{
					DeletePlant(plant);
					_soldPlants.RemoveAt(i);
					_soldPlantTimers.RemoveAt(i);
					i--;
				}
			}


			Graphics.DrawMeshInstanced(storeMesh, 0, storeMaterial, _storeMatrices);
			for (var i = 0; i < rockMatrices.Count; i++)
			{
				Graphics.DrawMeshInstanced(rockMesh, 0, rockMaterial, rockMatrices[i]);
			}

			for (var i = 0; i < _groundMatrices.Length; i++)
			{
				_groundMatProps[i].SetFloatArray("_Tilled", _tilledProperties[i]);
				Graphics.DrawMeshInstanced(groundMesh, 0, groundMaterial, _groundMatrices[i], _groundMatrices[i].Length,
					_groundMatProps[i]);
			}

			for (var i = 0; i < _plantSeeds.Count; i++)
			{
				var seed = _plantSeeds[i];
				var plantMesh = Plant.meshLookup[seed];

				var plantList = plants[seed];

				var matrices = plantMatrices[seed];
				for (var j = 0; j < matrices.Count; j++)
				{
					for (var k = 0; k < matrices[j].Count; k++)
					{
						var plant = plantList[j * InstancesPerBatch + k];
						plant.growth = Mathf.Min(plant.growth + Time.deltaTime / 10f, 1f);
						_plantGrowthProperties[k] = plant.growth;
					}

					_plantMatProps.SetFloatArray("_Growth", _plantGrowthProperties);
					Graphics.DrawMeshInstanced(plantMesh, 0, plantMaterial, matrices[j], _plantMatProps);
				}
			}
		}
	}
}
