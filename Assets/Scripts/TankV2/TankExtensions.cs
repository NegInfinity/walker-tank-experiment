using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TankExtensions;
using IkSolver;
using ValueDriver;

namespace TankV2{

public static class TankExtensions{
	public static bool findTankPart(this GameObject gameObject, out Part result, string name){
		var obj = gameObject.findObjectWithLowerName(name);
		if (obj){
			result = new Part(obj);
			return true;
		}
		else{
			result = new Part();
			Debug.LogWarning($"Part {name} not found in {gameObject}");
			return false;
		}
	}

	public static void findTankLeg(this GameObject gameObject, out Leg leg, bool front, bool right, string suffix){
		leg = new Leg();
		leg.right = right;
		leg.front = front;

		gameObject.findTankPart(out leg.hip, $"hip{suffix}");
		gameObject.findTankPart(out leg.upper, $"upperleg{suffix}");
		gameObject.findTankPart(out leg.lower, $"lowerleg{suffix}");
		gameObject.findTankPart(out leg.tip, $"tip{suffix}");
	}

	public static Vector3 invRelTransformNoScale(Transform t, Vector3 p){
		var diff = p - t.position;
		return new Vector3(
			Vector3.Dot(diff, t.right),
			Vector3.Dot(diff, t.up),
			Vector3.Dot(diff, t.forward)
		);
	}

	public static void combineIks(LegRelIk result, LinkedList<LegRelIk> iks, LegRelIk untilIk){
		result.setVec(Vector3.zero);
		bool first = true;
		foreach(var cur in iks){
			if (cur == untilIk)
				break;
			if (first){
				result.assign(cur);
				first = false;
			}
			else
				result.addIk(cur);
		}
	}
}

}