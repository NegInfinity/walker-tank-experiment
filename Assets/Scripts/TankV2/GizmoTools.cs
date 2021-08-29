using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TankExtensions;
using IkSolver;
using ValueDriver;

namespace TankV2{

public static class GizmoTools{
	public static void drawConnectionLine(GameObject a, GameObject b){
		if (!a || !b)
			return;
		Gizmos.DrawLine(a.transform.position, b.transform.position);
	}

	public static void drawConnectionLine(Part a, Part b){
		drawConnectionLine(a.obj, b.obj);
	}

	public static void drawGizmoPoint(Vector3 pos, float size){
		var x = Vector3.right * size * 0.5f;
		var y = Vector3.up * size * 0.5f;
		var z = Vector3.forward * size * 0.5f;
		Gizmos.DrawLine(pos - x, pos + x);
		Gizmos.DrawLine(pos - y, pos + y);
		Gizmos.DrawLine(pos - z, pos + z);
	}

	public static void drawGizmoPoint(this Transform transform, Vector3 pos, float size){
		var x = transform.right * size * 0.5f;
		var y = transform.up * size * 0.5f;
		var z = transform.forward * size * 0.5f;
		Gizmos.DrawLine(pos - x, pos + x);
		Gizmos.DrawLine(pos - y, pos + y);
		Gizmos.DrawLine(pos - z, pos + z);
	}

	public static void drawCenterOfMass(Part part){
		if (!part.rigBody)
			return;
		drawGizmoPoint(part.rigBody.worldCenterOfMass, 0.25f);
	}

	public static void drawCenterOfMass(this Transform transform, Part part){
		if (!part.rigBody)
			return;
		transform.drawGizmoPoint(part.rigBody.worldCenterOfMass, 0.25f);
	}

}

}
