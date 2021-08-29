using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TankExtensions;
using IkSolver;
using ValueDriver;

namespace TankV2{

public static class LegIkSolver{
	public static void solveLegKinematics(Part body, Leg leg, LegControl legControl, Vector3 worldTargetPos){
		if (!body.obj || !leg.hip.obj || !leg.upper.obj || !leg.lower.obj || !leg.tip.obj)
			return;

		var origBodyCoord = Coord.fromTransform(body.defaultTransform);
		var worldBodyCoord = Coord.fromTransform(body.obj.transform, false);
		var hipCoord = Coord.fromTransform(leg.hip.defaultTransform);
		var upperCoord = Coord.fromTransform(leg.upper.defaultTransform);
		var lowerCoord = Coord.fromTransform(leg.lower.defaultTransform);
		var tipCoord = Coord.fromTransform(leg.tip.defaultTransform);

		hipCoord = origBodyCoord.inverseTransformCoord(hipCoord);
		upperCoord = origBodyCoord.inverseTransformCoord(upperCoord);
		lowerCoord = origBodyCoord.inverseTransformCoord(lowerCoord);
		tipCoord = origBodyCoord.inverseTransformCoord(tipCoord);

		var hipNode = new IkNode(hipCoord, leg.hip.hinge);
		var upperNode = new IkNode(upperCoord, leg.upper.hinge);
		var lowerNode = new IkNode(lowerCoord, leg.lower.hinge);
		var tipNode = new IkNode(tipCoord, leg.tip.hinge);

		var nodes = new List<IkNode>();
		nodes.Add(hipNode);
		nodes.Add(upperNode);
		nodes.Add(lowerNode);
		nodes.Add(tipNode);

		for(int i = nodes.Count-1; i > 0; i--){
			nodes[i].moveToParentSpace(nodes[i-1], true);
		}

		var targetPos = worldBodyCoord.inverseTransformPoint(worldTargetPos);
		Solver.solveIkChain(nodes, targetPos, true);

		//updateIkChain(nodes, 0, false, false);
		var debugCoord = worldBodyCoord;
		for(int i = 0; (i + 1) < nodes.Count; i++){
			var node = nodes[i];
			var nextNode = nodes[i+1];
			Debug.DrawLine(
				debugCoord.transformPoint(node.objWorld.pos), 
				debugCoord.transformPoint(nextNode.objWorld.pos)
			);
		}

		legControl.legYaw = hipNode.jointState.xRotDeg;
		legControl.upperLeg = upperNode.jointState.xRotDeg;
		legControl.lowerLeg = lowerNode.jointState.xRotDeg;
	}
}

}
