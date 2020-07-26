using System.Collections.Generic;

namespace com.jlabarca.cpattern.Navigation
{
	public class Path {
		public List<int> xPositions;
		public List<int> yPositions;

		public int count {
			get {
				return xPositions.Count;
			}
		}

		public Path() {
			xPositions = new List<int>(100);
			yPositions = new List<int>(100);
		}

		public void Clear() {
			xPositions.Clear();
			yPositions.Clear();
		}
	}
}
