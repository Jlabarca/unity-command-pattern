using UnityEngine;

namespace com.jlabarca.cpattern
{
	public class Rock {
		public RectInt rect;
		public int health;
		public int batchNumber;
		public int batchIndex;
		public Matrix4x4 matrix;

		private int startHealth;
		private float depth;

		public Rock (RectInt myRect) {
			Random.InitState(0);
			rect = myRect;
			health = (rect.width+1) * (rect.height+1)*15;
			startHealth = health;
			depth = Random.Range(.4f,.8f);
			matrix = GetMatrix();
		}

		public Matrix4x4 GetMatrix() {
			var center2D = rect.center;
			var worldPos = new Vector3(center2D.x+.5f,depth * .5f,center2D.y+.5f);
			var scale = new Vector3(rect.width+.5f,depth,rect.height+.5f);
			return Matrix4x4.TRS(worldPos,Quaternion.identity,scale);
		}

		// returns true when we get destroyed
		public void TakeDamage(int damage) {
			health -= damage;
			var t = (float)health / startHealth;
			matrix.m11 = depth*t + Random.Range(0f,.1f);
			matrix.m13 = matrix.m11 * .5f;
			Map.rockMatrices[batchNumber][batchIndex] = matrix;
			if (health<=0) {
				Map.DeleteRock(this);
			}
		}
	}
}
