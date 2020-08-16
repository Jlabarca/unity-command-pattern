using System;
using com.jlabarca.cpattern.Enums;
using com.jlabarca.cpattern.Navigation;
using com.jlabarca.cpattern.Utils;
using UnityEngine;

namespace com.jlabarca.cpattern
{
	[Serializable]
	public class Farmer : Actor
	{

		private Rock _targetRock;
		private RectInt _tillingZone;
		private Plant _heldPlant;

		private bool _foundTillingZone;
		private bool _attackingARock;
		private bool _hasBoughtSeeds;

		public Farmer(Vector2 pos) : base(pos)
		{
		}

		public override void Tick(float moveSmooth) {
			_attackingARock = false;
			switch (intention)
			{
				case Intention.None:
					break;
				case Intention.SmashRocks:
					SmashRocksAi();
					break;
				case Intention.TillGround:
					TillGroundAi();
					break;
				case Intention.PlantSeeds:
					PlantSeedsAI();
					break;
				case Intention.SellPlants:
					SellPlantsAI(moveSmooth);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			FollowPath(moveSmooth);

			smoothPosition = Vector3.Lerp(smoothPosition,position, moveSmooth * Time.timeScale);

			var xOffset = 0f;
			var zOffset = 0f;
			if (_attackingARock) {
				var rand = NoRandom.value * .5f;
				xOffset = (path.xPositions[0]+.5f - smoothPosition.x) * rand;
				zOffset = (path.yPositions[0]+.5f - smoothPosition.y) * rand;
			}

			// update our x and z position in our matrix:
			matrix.m03 = position.x+xOffset;
			matrix.m23 = position.y+zOffset;
		}

		public Vector3 GetSmoothWorldPos()
		{
			return new Vector3(smoothPosition.x, 0f, smoothPosition.y);
		}

		public bool IsTillableInZone(int x, int y)
		{
			if (Map.groundStates[x, y] != GroundState.Default) return false;
			if (x < _tillingZone.xMin || x > _tillingZone.xMax) return false;
			return y >= _tillingZone.yMin && y <= _tillingZone.yMax;
		}

		private int _rand;
		public void PickRandomIntention()
		{
			path.Clear();
			if (_rand > 3) _rand = 0;

			switch (_rand)
			{
				case 0:
					intention = Intention.SmashRocks;
					break;
				case 1:
					intention = Intention.TillGround;
					_foundTillingZone = false;
					break;
				case 2:
					intention = Intention.PlantSeeds;
					break;
				case 3:
					intention = Intention.SellPlants;
					break;
			}
			_rand++;
		}

		private void SmashRocksAi()
		{
			if (_targetRock == null)
			{
				_targetRock = Pathing.FindNearbyRock(TileX, TileY, 20, path);
				if (_targetRock == null)
				{
					intention = Intention.None;
				}
			}
			else
			{
				if (path.xPositions.Count == 1)
				{
					_attackingARock = true;

					_targetRock.TakeDamage(1);
				}

				if (_targetRock.health > 0) return;
				_targetRock = null;
				intention = Intention.None;
			}
		}

		private void TillGroundAi()
		{
			if (_foundTillingZone == false)
			{
				var width = NoRandom.Range(1, 8);
				var height = NoRandom.Range(1, 8);
				var minX = TileX + NoRandom.Range(-10, 10 - width);
				var minY = TileY + NoRandom.Range(-10, 10 - height);
				if (minX < 0) minX = 0;
				if (minY < 0) minY = 0;
				if (minX + width >= Map.instance.mapVector.x) minX = Map.instance.mapVector.x - 1 - width;
				if (minY + height >= Map.instance.mapVector.y) minY = Map.instance.mapVector.y - 1 - height;

				var blocked = false;
				for (var x = minX; x <= minX + width; x++)
				{
					for (var y = minY; y <= minY + height; y++)
					{
						var groundState = Map.groundStates[x, y];
						if (groundState != GroundState.Default && groundState != GroundState.Tilled)
						{
							blocked = true;
							break;
						}

						if (Map.tileRocks[x, y] != null || Map.storeTiles[x, y])
						{
							blocked = true;
							break;
						}
					}

					if (blocked)
					{
						break;
					}
				}

				if (blocked == false)
				{
					_tillingZone = new RectInt(minX, minY, width, height);
					_foundTillingZone = true;
				}
				else
				{
					if (NoRandom.value < .2f)
					{
						intention = Intention.None;
					}
				}
			}
			else
			{
				Debug.DrawLine(new Vector3(_tillingZone.min.x, .1f, _tillingZone.min.y),
					new Vector3(_tillingZone.max.x + 1f, .1f, _tillingZone.min.y), Color.green);
				Debug.DrawLine(new Vector3(_tillingZone.max.x + 1f, .1f, _tillingZone.min.y),
					new Vector3(_tillingZone.max.x + 1f, .1f, _tillingZone.max.y + 1f), Color.green);
				Debug.DrawLine(new Vector3(_tillingZone.max.x + 1f, .1f, _tillingZone.max.y + 1f),
					new Vector3(_tillingZone.min.x, .1f, _tillingZone.max.y + 1f), Color.green);
				Debug.DrawLine(new Vector3(_tillingZone.min.x, .1f, _tillingZone.max.y + 1f),
					new Vector3(_tillingZone.min.x, .1f, _tillingZone.min.y), Color.green);
				if (IsTillableInZone(TileX, TileY))
				{
					path.Clear();
					Map.TillGround(TileX, TileY);
				}
				else
				{
					if (path.count != 0) return;
					var tileHash = Pathing.SearchForOne(TileX, TileY, 25, Pathing.IsNavigableDefault,
						Pathing.IsTillable, _tillingZone);
					if (tileHash != -1)
					{
						int tileX, tileY;
						Pathing.Unhash(tileHash, out tileX, out tileY);
						Pathing.AssignLatestPath(path, tileX, tileY);
					}
					else
					{
						intention = Intention.None;
					}
				}
			}
		}

		private void PlantSeedsAI()
		{
			if (_hasBoughtSeeds == false)
			{
				if (Map.storeTiles[TileX, TileY])
				{
					_hasBoughtSeeds = true;
				}
				else if (path.count == 0)
				{
					Pathing.WalkTo(TileX, TileY, 40, Pathing.IsStore, path);
					if (path.count == 0)
					{
						intention = Intention.None;
					}
				}
			}
			else if (Pathing.IsReadyForPlant(TileX, TileY))
			{
				path.Clear();
				var seed = Mathf.FloorToInt(Mathf.PerlinNoise(TileX / 10f, TileY / 10f) * 10) + Map.seedOffset;
				Map.SpawnPlant(TileX, TileY, seed);
			}
			else
			{
				if (path.count == 0)
				{
					if (NoRandom.value < .1f)
					{
						intention = Intention.None;
					}
					else
					{
						var tileHash = Pathing.SearchForOne(TileX, TileY, 25, Pathing.IsNavigableDefault,
							Pathing.IsReadyForPlant, Pathing.fullMapZone);
						if (tileHash != -1)
						{
							int tileX, tileY;
							Pathing.Unhash(tileHash, out tileX, out tileY);
							Pathing.AssignLatestPath(path, tileX, tileY);
						}
						else
						{
							intention = Intention.None;
						}
					}
				}
			}
		}

		private void SellPlantsAI(float moveSmooth = 0f)
		{
			if (_heldPlant == null)
			{
				if (Map.IsHarvestable(TileX, TileY))
				{
					_heldPlant = Map.tilePlants[TileX, TileY];
					Map.HarvestPlant(TileX, TileY);
					path.Clear();
				}
				else if (path.count == 0)
				{
					Pathing.WalkTo(TileX, TileY, 25, Map.IsHarvestableAndUnreserved, path);
					if (path.count == 0)
					{
						intention = Intention.None;
					}
					else
					{
						Map.tilePlants[path.xPositions[0], path.yPositions[0]].reserved = true;
					}
				}

			}
			else
			{
				_heldPlant.EaseToWorldPosition(smoothPosition.x, 1f, smoothPosition.y, moveSmooth);
				if (Map.storeTiles[TileX, TileY])
				{
					Map.SellPlant(_heldPlant, TileX, TileY);
					_heldPlant = null;
					path.Clear();

				}
				else if (path.count == 0)
				{
					Pathing.WalkTo(TileX, TileY, 40, Pathing.IsStore, path);
				}
			}
		}

		private void FollowPath(float moveSmooth)
		{
			if (path.count <= 0) return;
			for (var i = 0; i < path.xPositions.Count - 1; i++)
			{
				Debug.DrawLine(new Vector3(path.xPositions[i] + .5f, .5f, path.yPositions[i] + .5f),
					new Vector3(path.xPositions[i + 1] + .5f, .5f, path.yPositions[i + 1] + .5f), Color.red);
			}

			var nextTileX = path.xPositions[path.xPositions.Count - 1];
			var nextTileY = path.yPositions[path.yPositions.Count - 1];
			if (TileX == nextTileX && TileY == nextTileY)
			{
				path.xPositions.RemoveAt(path.xPositions.Count - 1);
				path.yPositions.RemoveAt(path.yPositions.Count - 1);
			}
			else
			{
				if (Map.IsBlocked(nextTileX, nextTileY) != false) return;
				var offset = .5f;
				if (Map.groundStates[nextTileX, nextTileY] == GroundState.Plant)
				{
					offset = .01f;
				}

				var targetPos = new Vector2(nextTileX + offset, nextTileY + offset);
				position = MoveTowards(position, targetPos, speed *  moveSmooth);
			}
		}

		public static Vector2 MoveTowards(
			Vector2 current,
			Vector2 target,
			float maxDistanceDelta)
		{
			var num1 = target.x - current.x;
			var num2 = target.y - current.y;
			var num3 = num1 * num1 + num2 * num2;
			if (maxDistanceDelta >= 0.0 && num3 <= maxDistanceDelta * maxDistanceDelta)
				return target;
			var num4 = (float) Math.Sqrt(num3);
			return new Vector2(current.x + num1 / num4 * maxDistanceDelta, current.y + num2 / num4 * maxDistanceDelta);
		}
	}
}
