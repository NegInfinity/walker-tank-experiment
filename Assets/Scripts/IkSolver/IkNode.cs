using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IkSolver{

/*
obj coordinate is a coordinate of display object.
joint coordinate is a coordinate of joint.
Both are specified in the same space, however
OBJECT coord acts as the primary transform when nodes are parented.
*/
[System.Serializable]
public class IkNode{
	public Coord jointDefault = Coord.identity;//X is the 1st axis, Y is the 2n, and Z is the 3rd
	public Coord objDefault = Coord.identity;

	public Coord objOffset = Coord.identity;//relative to joint. Doesn't change.

	public IkJointState jointState = IkJointState.zero;
	public IkJointLimits jointLimits = IkJointLimits.defaultNoLimit;

	public Coord jointLocal = Coord.identity;
	public Coord objLocal = Coord.identity;
	public Coord jointWorld = Coord.identity;
	public Coord objWorld = Coord.identity;

	public void moveToParentSpace(IkNode parent, bool affectDefault){
		jointLocal = parent.objWorld.inverseTransformCoord(jointWorld);
		objLocal = jointLocal.transformCoord(objOffset);		
		if (affectDefault){
			jointDefault = parent.objWorld.inverseTransformCoord(jointDefault);
		}
	}

	public void reset(){
		jointState.xRotDeg = 0.0f;
		updateJointCoord();
	}

	public void updateJointCoord(){
		if (jointLimits.xRot.enabled){
			jointState.xRotDeg = Mathf.Clamp(
				jointState.xRotDeg, 
				jointLimits.xRot.min, 
				jointLimits.xRot.max
			);
		}

		var tmpCoord = jointDefault.rotatedAround(
			jointDefault.pos, 
			jointDefault.x,
			jointState.xRotDeg
		);

		jointLocal = tmpCoord;
		objLocal = jointLocal.transformCoord(objOffset);		
	}

	public void update(IkNode parent, bool recalculateCoord){
		if (recalculateCoord)
			updateJointCoord();

		if (parent != null){
			objWorld = parent.objWorld.transformCoord(objLocal);
			jointWorld = parent.objWorld.transformCoord(jointLocal);
		}
		else{
			objWorld = objLocal;
			jointWorld = jointLocal;
		}
	}

	static Vector3 perpVector(Vector3 axis, params Vector3[] args){
		const float epsilon = 0.00001f;
		for(int i = 0; i < args.Length; i++){
			var result = Vector3.ProjectOnPlane(args[i], axis);
			if (result.sqrMagnitude > epsilon)
				return result.normalized;
		}
		throw new System.InvalidOperationException();
	}

	static Vector3 perpUpVector(Vector3 axis){
		const float epsilon = 0.00001f;
		if (axis.sqrMagnitude < epsilon)
			return Vector3.up;

		var result = Vector3.ProjectOnPlane(Vector3.up, axis);
		if (result.sqrMagnitude > epsilon)
			return result.normalized;

		result = Vector3.ProjectOnPlane(Vector3.right, axis);
		if (result.sqrMagnitude > epsilon)
			return result.normalized;

		result = Vector3.ProjectOnPlane(Vector3.forward, axis);
		return result.normalized;
	}

	private void _initDefaultTransforms(){
		objOffset = jointDefault.inverseTransformCoord(objDefault);
		jointLocal = jointDefault;
		objLocal = objDefault;
		jointWorld = jointLocal;
		objWorld = objLocal;
	}

	//Both should be specified in the same space
	public IkNode(Coord jointDefault_, Coord objDefault_){
		jointDefault = jointDefault_;
		objDefault = objDefault_;
		_initDefaultTransforms();
	}

	public IkNode(Coord obj, HingeJoint hinge){
		if(hinge){
			jointDefault.pos = obj.transformPoint(hinge.anchor);
			jointDefault.x = obj.transformVector(hinge.axis);
		}
		else{
			jointDefault.pos = obj.transformPoint(Vector3.zero);
			jointDefault.x = obj.transformVector(Vector3.right);
		}
		var z = perpVector(jointDefault.x, Vector3.forward, Vector3.up, -Vector3.right);
		jointDefault.y = Vector3.Cross(z, jointDefault.x).normalized;
		objDefault = obj;
		_initDefaultTransforms();
	}
}

}
