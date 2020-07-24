using UnityEngine;

namespace com.jlabarca.cpattern
{
	public class CamFollow : MonoBehaviour {

		public Vector2 viewAngles;
		public float viewDist;
		public float mouseSensitivity;

		private void Start () {
			transform.rotation = Quaternion.Euler(viewAngles.y,viewAngles.x,0f);
		}

		private void LateUpdate ()
		{
			var actor = FarmerManager.instance.firstFarmer;
			if(actor == null) return;
			var pos = actor.GetSmoothWorldPos();
			viewAngles.x += Input.GetAxis("Mouse X") * mouseSensitivity/Screen.height;
			viewAngles.y -= Input.GetAxis("Mouse Y") * mouseSensitivity/Screen.height;
			viewAngles.y = Mathf.Clamp(viewAngles.y,7f,80f);
			viewAngles.x -= Mathf.Floor(viewAngles.x / 360f) * 360f;
			Transform transform1;
			(transform1 = transform).rotation = Quaternion.Euler(viewAngles.y,viewAngles.x,0f);
			transform1.position = pos - transform1.forward * viewDist;
		}
	}
}
