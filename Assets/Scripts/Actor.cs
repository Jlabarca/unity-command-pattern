using com.jlabarca.cpattern.Enums;
using com.jlabarca.cpattern.Navigation;
using UnityEngine;

namespace com.jlabarca.cpattern
{
    public class Actor
    {
        public int id;
        public Vector2 position;
        public Vector2 smoothPosition;
        public Path path;
        public Matrix4x4 matrix;
        public float speed = 4f;

        public Intention intention;

        internal int TileX => Mathf.FloorToInt(position.x);

        internal int TileY => Mathf.FloorToInt(position.y);

        public Actor(Vector2 initialPosition)
        {
            id = FarmerManager.instance.farmers.Count;
            position = initialPosition;
            smoothPosition = initialPosition;
            intention = Intention.None;
            path = new Path();
            matrix = Matrix4x4.Translate(new Vector3(smoothPosition.x,.5f,smoothPosition.y)) * Matrix4x4.Scale(Vector3.one * .5f);
        }

        public void SetIntention(Intention newIntention)
        {
            path.Clear();
            intention = newIntention;
        }
    }
}
