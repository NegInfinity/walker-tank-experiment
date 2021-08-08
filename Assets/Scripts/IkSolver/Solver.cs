using UnityEngine;
using System.Collections.Generic;

namespace IkSolver{
public static class Solver{
	public static bool almostEqual(float a, float b, float epsilon = 0.001f){
		if (b > a)
			return (b - a) <= epsilon;
		else
			return (a - b) <= epsilon;
	}

	public static bool almostEqual(Vector3 a, Vector3 b, float epsilon = 0.001f){
		return 
			almostEqual(a.x, b.x, epsilon) &&
			almostEqual(a.y, b.y, epsilon) &&
			almostEqual(a.z, b.z, epsilon);
	}

	public static void updateIkChain(List<IkNode> nodes, int firstIndex, bool reset, bool recalculateCoord){
		if (reset){
			for(int i = firstIndex; i < nodes.Count; i++){
				nodes[i].reset();
			}
		}

		for(int i = firstIndex; i < nodes.Count; i++){
			nodes[i].update((i > 0) ? nodes[i-1]: null, recalculateCoord);
		}
	}

	public static void solveIkChain(List<IkNode> nodes, Vector3 target, bool reset, int chainLength = -1, float epsilon = 0.001f){
		updateIkChain(nodes, 0, reset, true);
		var lastIndex = nodes.Count - 1;
		if (lastIndex < 0)
			return;

		int maxIterations = 10;
		for(int iterationIndex = 0; iterationIndex < maxIterations; iterationIndex++){
			for(int nodeIndex = 0; nodeIndex < lastIndex; nodeIndex++){
				if ((chainLength >= 0) && (nodeIndex >= chainLength))
					break;

				var curNode = nodes[nodeIndex];
				var lastNode = nodes[lastIndex];

				var toTarget = (target - curNode.jointWorld.pos).normalized;
				var toEffector = (lastNode.jointWorld.pos - curNode.jointWorld.pos).normalized;

				var axis = curNode.jointWorld.x;

				var projToTarget = Vector3.ProjectOnPlane(toTarget, axis);
				var projToEffector = Vector3.ProjectOnPlane(toEffector, axis);

				if (almostEqual(projToTarget, Vector3.zero) || almostEqual(projToEffector, Vector3.zero))
					continue;

				projToTarget = projToTarget.normalized;
				projToEffector = projToEffector.normalized;

				//we use limits, hence Atan2. The engine can handlle it anyway, but we COULD optimize it.
				var xVec = projToEffector;
				var yVec = Vector3.Cross(axis, xVec).normalized;

				var yVal = Vector3.Dot(yVec, projToTarget);
				var xVal = Vector3.Dot(xVec, projToTarget);

				var targetAngle = Mathf.Atan2(yVal, xVal) * Mathf.Rad2Deg;
				curNode.jointState.xRotDeg += targetAngle;

				/*
				var rotatedForward = vectorRotate(curNode.world.forward, projToEffector, projToTarget, axis);
				var rotatedUp = vectorRotate(curNode.world.up, projToEffector, projToTarget, axis);
				if (nodeIndex > 0){
					var parentNode = nodes[nodeIndex - 1];

					rotatedForward = parentNode.world.inverseTransformVector(rotatedForward);
					rotatedUp = parentNode.world.inverseTransformVector(rotatedUp);
				}

				curNode.local.forward = rotatedForward;
				curNode.local.up = rotatedUp;
				*/

				updateIkChain(nodes, nodeIndex, false, true);
			}
			var diff = target - nodes[lastIndex].jointWorld.pos;
			if (diff.magnitude < epsilon)
				break;
			if (almostEqual(target, nodes[lastIndex].jointWorld.pos, epsilon))
				break;
		}
	}	
}
}