using UnityEngine;

namespace IkSolver{
	public static class Vec3Extensions{
		//https://en.wikipedia.org/wiki/Rodrigues%27_rotation_formula
		public static Vector3 rotatedAroundAxisRad(this Vector3 v, Vector3 kAxis, float angleRad){
			var cosVal = Mathf.Cos(angleRad);
			var sinVal = Mathf.Sin(angleRad);

			var result = v * cosVal 
				+ Vector3.Cross(kAxis, v) * sinVal
				+ kAxis * Vector3.Dot(v, kAxis) * (1.0f - cosVal);

			return result;
		}

		public static Vector3 rotatedAroundAxis(this Vector3 v, Vector3 axis, float angleDeg){
			return rotatedAroundAxisRad(v, axis, angleDeg * Mathf.Deg2Rad);
		}

		public static Vector3 rotatedAroundPointRad(this Vector3 v, Vector3 refPoint, Vector3 axis, float angleRad){
			return refPoint + rotatedAroundAxisRad(v - refPoint, axis, angleRad);
		}

		public static Vector3 rotatedAroundPoint(this Vector3 v, Vector3 refPoint, Vector3 axis, float angleDeg){
			return rotatedAroundPoint(v, refPoint, axis, angleDeg * Mathf.Deg2Rad);
		}
	}
}