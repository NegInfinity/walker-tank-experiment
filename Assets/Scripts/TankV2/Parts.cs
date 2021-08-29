using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TankExtensions;
using IkSolver;
using ValueDriver;

namespace TankV2{

using static CenterOfMassTools;

[System.Serializable]
public class Parts{
	public Part turret;
	public Part barrel;
	public Part body;

	public Leg legRF;
	public Leg legLF;
	public Leg legRB;
	public Leg legLB;

	public Leg getLeg(int legIndex){
		switch(legIndex){
			case(0):
				return legLF;
			case(1):
				return legRF;
			case(2):
				return legLB;
			case(3):
				return legRB;
			default:
				throw new System.ArgumentOutOfRangeException();
		}
	}

	public BodyPose getBodyPose(){
		var result = new BodyPose();
		getBodyPose(result);
		return result;
	}

	public void getBodyPose(BodyPose pose){
		pose.bodyPos = body.objWorldPos;
		pose.bodyRot = body.objWorldRot;
		for(int i = 0; i < 4; i++)
			pose.setLeg(i, getLeg(i).tip.objWorldPos);
	}

	public (Vector3, float) getCenterOfMass(){
		Vector3 centerOfMass = Vector3.zero;
		float mass = 0.0f;

		addCenterOfMass(ref centerOfMass, ref mass, turret);
		addCenterOfMass(ref centerOfMass, ref mass, barrel);
		addCenterOfMass(ref centerOfMass, ref mass, body);
		addCenterOfMass(ref centerOfMass, ref mass, legLB);
		addCenterOfMass(ref centerOfMass, ref mass, legLF);
		addCenterOfMass(ref centerOfMass, ref mass, legRB);
		addCenterOfMass(ref centerOfMass, ref mass, legRF);

		if (mass != 0.0f){
			centerOfMass /= mass;
		}

		return (centerOfMass, mass);
	}
}

}
