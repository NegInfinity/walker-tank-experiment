using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TankExtensions;
using IkSolver;
using ValueDriver;

namespace TankV2{

[System.Serializable]
public class IkControl{
	public Transform body = null;
	public Transform legRFTarget = null;
	public Transform legRBTarget = null;
	public Transform legLFTarget = null;
	public Transform legLBTarget = null;
	
	public Transform getLegIkTarget(int legIndex){
		switch(legIndex){
			case(0):
				return legLFTarget;
			case(1):
				return legRFTarget;
			case(2):
				return legLBTarget;
			case(3):
				return legRBTarget;
			default:
				return legLFTarget;
		}
	}

	public void setRelLegIk(int legIndex, float right, float forward, float height){
		var target = getLegIkTarget(legIndex);
		var rightVec = body.right;
		var forwardVec = body.forward;
		var upVec = body.up;
		var pos = body.position;

		target.transform.position = pos 
			+ right * rightVec + forward * forwardVec 
			+ upVec * height;
	}

	public void setRelLegIk(int legIndex, Vector3 coord){
		setRelLegIk(legIndex, coord.x, coord.z, coord.y);
	}

	public void setRelLegIk(Vector3 lf, Vector3 rf, Vector3 lb, Vector3 rb){
		setRelLegIk(0, lf);
		setRelLegIk(1, rf);
		setRelLegIk(2, lb);
		setRelLegIk(3, rb);
	}

	public void setRelLegIk(LegRelIk relIk){
		setRelLegIk(relIk.lf, relIk.rf, relIk.lb, relIk.rb);
	}
}

}