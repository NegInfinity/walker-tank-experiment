using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TankExtensions;
using IkSolver;
using ValueDriver;

namespace TankV2{

[System.Serializable]
public struct Part{
	public string name;
	public GameObject obj;
	public HingeJoint hinge;
	public Rigidbody rigBody;
	public DefaultTransform defaultTransform;		

	public Vector3 objWorldPos{
		get => obj.transform.position;
	}

	public Quaternion objWorldRot{
		get => obj.transform.rotation;
	}

	public Vector3 velocity{
		get => rigBody.velocity;
	}

	public void applyHingeAngle(float angle){
		if (!hinge)
			return;
		var spring = hinge.spring;
		spring.targetPosition = angle;
		hinge.spring = spring;
	}

	public Part(GameObject obj_){
		obj = obj_;
		name = obj.name;
		hinge = null;
		rigBody = null;
		defaultTransform = null;
		if (obj){
			hinge = obj.GetComponent<HingeJoint>();
			rigBody = obj.GetComponent<Rigidbody>();
			defaultTransform = obj.GetComponent<DefaultTransform>();
		}
	}
}

}
