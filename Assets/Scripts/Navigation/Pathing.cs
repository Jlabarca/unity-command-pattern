using System.Collections.Generic;
using com.jlabarca.cpattern.Enums;
using UnityEngine;

namespace com.jlabarca.cpattern.Navigation
{
	public static class Pathing {

		public delegate bool IsNavigableDelegate(int x,int y);
		public delegate bool CheckMatchDelegate(int x,int y);

		private static int[,] visitedTiles;
		private static List<int> activeTiles;
		private static List<int> nextTiles;
		private static List<int> outputTiles;

		private static int[] dirsX = { 1,-1,0,0 };
		private static int[] dirsY = { 0,0,1,-1 };

		private static int mapWidth;
		private static int mapHeight;

		public static RectInt fullMapZone;

		public static int Hash(int x,int y) {
			return y * mapWidth + x;
		}
		public static void Unhash(int hash,out int x,out int y) {
			y = hash / mapWidth;
			x = hash % mapHeight;
		}

		public static bool IsNavigableDefault(int x, int y) {
			return (Map.tileRocks[x,y] == null);
		}
		public static bool IsNavigableAll(int x, int y) {
			return true;
		}

		public static bool IsRock(int x, int y) {
			return (Map.tileRocks[x,y] != null);
		}
		public static bool IsStore(int x, int y) {
			return Map.storeTiles[x,y];
		}
		public static bool IsTillable(int x, int y) {
			return (Map.groundStates[x,y] == GroundState.Default);
		}
		public static bool IsReadyForPlant(int x,int y) {
			return (Map.groundStates[x,y] == GroundState.Tilled);
		}

		public static void Init() {
			fullMapZone = new RectInt(0,0,Map.instance.mapVector.x,Map.instance.mapVector.y);
		}

		public static Rock FindNearbyRock(int x, int y, int range, Path outputPath) {
			var rockPosHash = SearchForOne(x,y,range,IsNavigableDefault,IsRock,fullMapZone);
			if (rockPosHash == -1) {
				return null;
			}

			Unhash(rockPosHash, out var rockX,out var rockY);
			if (outputPath != null) {
				AssignLatestPath(outputPath,rockX,rockY);
			}
			return Map.tileRocks[rockX,rockY];
		}

		public static void WalkTo(int x, int y, int range, CheckMatchDelegate CheckMatch, Path outputPath) {
			var storePosHash = SearchForOne(x,y,range,IsNavigableDefault,CheckMatch,fullMapZone);
			if (storePosHash == -1) return;
			Unhash(storePosHash, out var storeX,out var storeY);
			if (outputPath != null) {
				AssignLatestPath(outputPath,storeX,storeY);
			}
		}

		public static int SearchForOne(int startX,int startY, int range, IsNavigableDelegate IsNavigable, CheckMatchDelegate CheckMatch, RectInt requiredZone) {
			outputTiles = Search(startX,startY,range,IsNavigable,CheckMatch,requiredZone,1);
			if (outputTiles.Count==0) {
				return -1;
			}

			return outputTiles[0];
		}

		public static List<int> Search(int startX,int startY,int range,IsNavigableDelegate IsNavigable,CheckMatchDelegate CheckMatch,RectInt requiredZone, int maxResultCount=0) {
			mapWidth = Map.instance.mapVector.x;
			mapHeight = Map.instance.mapVector.y;

			if (visitedTiles==null) {
				visitedTiles = new int[mapWidth,mapHeight];
				activeTiles = new List<int>();
				nextTiles = new List<int>();
				outputTiles = new List<int>();
			}

			for (var x=0;x<mapWidth;x++) {
				for (var y = 0; y < mapHeight; y++) {
					visitedTiles[x,y] = -1;
				}
			}
			outputTiles.Clear();
			visitedTiles[startX,startY] = 0;
			activeTiles.Clear();
			nextTiles.Clear();
			nextTiles.Add(Hash(startX,startY));

			var steps = 0;

			while (nextTiles.Count > 0 && (steps<range || range==0)) {
				var temp = activeTiles;
				activeTiles = nextTiles;
				nextTiles = temp;
				nextTiles.Clear();

				steps++;

				foreach (var t in activeTiles)
				{
					int x, y;
					Unhash(t,out x,out y);

					for (var j=0;j<dirsX.Length;j++) {
						var x2 = x + dirsX[j];
						var y2 = y + dirsY[j];

						if (x2<0 || y2<0 || x2>=mapWidth || y2>=mapHeight) {
							continue;
						}

						if (visitedTiles[x2, y2] != -1 && visitedTiles[x2, y2] <= steps) continue;
						var hash = Hash(x2,y2);
						if (IsNavigable(x2,y2)) {
							visitedTiles[x2,y2] = steps;
							nextTiles.Add(hash);
						}

						if (x2 < requiredZone.xMin || x2 > requiredZone.xMax) continue;
						if (y2 < requiredZone.yMin || y2 > requiredZone.yMax) continue;
						if (!CheckMatch(x2, y2)) continue;
						outputTiles.Add(hash);
						if (maxResultCount != 0 && outputTiles.Count >= maxResultCount) {
							return outputTiles;
						}
					}
				}
			}

			return outputTiles;
		}

		public static void AssignLatestPath(Path target,int endX, int endY) {
			target.Clear();

			var x = endX;
			var y = endY;

			target.xPositions.Add(x);
			target.yPositions.Add(y);

			var dist = int.MaxValue;
			while (dist>0) {
				var minNeighborDist = int.MaxValue;
				var bestNewX = x;
				var bestNewY = y;
				for (var i=0;i<dirsX.Length;i++) {
					var x2 = x + dirsX[i];
					var y2 = y + dirsY[i];
					if (x2 < 0 || y2 < 0 || x2 >= mapWidth || y2 >= mapHeight) {
						continue;
					}

					var newDist = visitedTiles[x2,y2];
					if (newDist !=-1 && newDist < minNeighborDist) {
						minNeighborDist = newDist;
						bestNewX = x2;
						bestNewY = y2;
					}
				}
				x = bestNewX;
				y = bestNewY;
				dist = minNeighborDist;
				target.xPositions.Add(x);
				target.yPositions.Add(y);
			}
		}
	}
}
