using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TankExtensions;
using IkSolver;
using ValueDriver;

namespace TankV2{

using static CenterOfMassTools;
using static LegIkSolver;

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

	public void applyControl(DirectControl directControl){
		turret.applyHingeAngle(directControl.turretControlAngle);
		barrel.applyHingeAngle(directControl.barrelControlAngle);
		legRF.applyControl(directControl.legControlRF);
		legLF.applyControl(directControl.legControlLF);
		legRB.applyControl(directControl.legControlRB);
		legLB.applyControl(directControl.legControlLB);
	}	

	public void loadFromObject(GameObject gameObject){
		if (!gameObject)
			return;

		gameObject.findTankPart(out turret, "turret");
		gameObject.findTankPart(out barrel, "barrels");
		//if (!findTankPart(out body, "body"))
		body = new Part(gameObject);

		gameObject.findTankLeg(out legRF, true, true, "rf");
		gameObject.findTankLeg(out legLF, true, false, "lf");
		gameObject.findTankLeg(out legRB, false, true, "rb");
		gameObject.findTankLeg(out legLB, false, false, "lb");
	}

	public void solveKinematics(DirectControl outDirectCtrl, IkControl inIkCtrl){
		if (inIkCtrl.legRFTarget && inIkCtrl.legRFTarget.gameObject.activeInHierarchy)
			solveLegKinematics(body, legRF, outDirectCtrl.legControlRF, inIkCtrl.legRFTarget.position);
		if (inIkCtrl.legRBTarget && inIkCtrl.legRBTarget.gameObject.activeInHierarchy)
			solveLegKinematics(body, legRB, outDirectCtrl.legControlRB, inIkCtrl.legRBTarget.position);
		if (inIkCtrl.legLFTarget && inIkCtrl.legLFTarget.gameObject.activeInHierarchy)
			solveLegKinematics(body, legLF, outDirectCtrl.legControlLF, inIkCtrl.legLFTarget.position);
		if (inIkCtrl.legLBTarget && inIkCtrl.legLBTarget.gameObject.activeInHierarchy)
			solveLegKinematics(body, legLB, outDirectCtrl.legControlLB, inIkCtrl.legLBTarget.position);
	}
}

}
