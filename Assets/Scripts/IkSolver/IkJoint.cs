using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IkSolver{

[System.Serializable]
public struct IkJointLimit{
	public bool enabled;
	public float min;
	public float max;
	public static IkJointLimit defaultNoLimit{
		get => new IkJointLimit(0.0f, 0.0f, false);
	}
	public IkJointLimit(float min_, float max_, bool enabled_){
		enabled = enabled_;
		min = min_;
		max = max_;
	}
}

[System.Serializable]
public struct IkJointState{
	public float xRotDeg;

	public static IkJointState zero{
		get => new IkJointState(0.0f);
	}

	public IkJointState(float xRotDeg_){
		xRotDeg = xRotDeg_;
	}
}

[System.Serializable]
public struct IkJointLimits{
	public IkJointLimit xRot;
	public static IkJointLimits defaultNoLimit{
		get => new IkJointLimits(IkJointLimit.defaultNoLimit);
	}

	public IkJointLimits(IkJointLimit xRot_){
		xRot = xRot_;
	}
}

[System.Serializable]
public struct IkJointAxes{
	public Vector3 anchor;
	public Vector3 xAxis;

	public static IkJointAxes defaultAxes {
		get => new IkJointAxes(Vector3.zero, new Vector3(1.0f, 0.0f, 0.0f));
	}

	public IkJointAxes(Vector3 anchor_, Vector3 xAxis_){
		anchor = anchor_;
		xAxis = xAxis_;
	}

	public IkJointAxes transformedBy(Coord coord){
		return new IkJointAxes(
			coord.transformPoint(anchor),
			coord.transformVector(xAxis)
		);
	}
}

}