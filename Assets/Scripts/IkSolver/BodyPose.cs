using UnityEngine;
using System.Collections;
using ValueDriver;

namespace IkSolver{

[System.Serializable]
public class BodyPose{
	public Vector3 bodyPos = Vector3.zero;
	public Quaternion bodyRot = Quaternion.identity;
	public Vector3 lf = Vector3.zero;
	public Vector3 rf = Vector3.zero;
	public Vector3 lb = Vector3.zero;
	public Vector3 rb = Vector3.zero;

	Vector3 getLeg(int legIndex){
		switch(legIndex){
			case(0):
				return lf;
			case(1):
				return rf;
			case(2):
				return lb;
			case(3):
				return rb;
			default:
				throw new System.ArgumentOutOfRangeException();
		}
	}

	void setLeg(int legIndex, Vector3 newVal){
		switch(legIndex){
			case(0):
				lf = newVal;
				break;
			case(1):
				rf = newVal;
				break;
			case(2):
				lb = newVal;
				break;
			case(3):
				rb = newVal;
				break;
			default:
				break;
		}
	}

	public void assign(BodyPose other){
		bodyPos = other.bodyPos;
		bodyRot = other.bodyRot;
		lf = other.lf;
		rf = other.rf;
		lb = other.lb;
		rb = other.rb;
	}

	public Vector3 right{
		get => bodyX;
	}

	public Vector3 up{
		get => bodyY;
	}

	public Vector3 forward{
		get => bodyZ;
	}

	public Vector3 bodyX{
		get => bodyRot * Vector3.right;
	}

	public Vector3 bodyY{
		get => bodyRot * Vector3.up;
	}

	public Vector3 bodyZ{
		get => bodyRot * Vector3.forward;
	}

	public Vector3 getWorldFromRelVector(Vector3 arg){
		return bodyRot * arg;
	}

	public Vector3 getWorldFromRelPoint(Vector3 arg){
		return getWorldFromRelVector(arg) + bodyPos;
	}

	public Vector3 getRelFromWorldVector(Vector3 arg){
		return new Vector3(
			Vector3.Dot(right, arg),
			Vector3.Dot(up, arg),
			Vector3.Dot(forward, arg)
		);
	}

	public Vector3 getRelFromWorldPoint(Vector3 arg){
		return getRelFromWorldVector(arg - bodyPos);
	}

	public void legsAddWorld(Vector3 arg){
		for(int i = 0; i < 4; i++)
			setLeg(i, getLeg(i) + arg);
	}

	public void legsAddRel(Vector3 arg){
		legsAddWorld(getWorldFromRelVector(arg));
	}	

	public void legAddWorld(int legIndex, Vector3 arg){
		setLeg(legIndex, getLeg(legIndex) + arg);
	}

	public void legAddRel(int legIndex, Vector3 arg){
		legAddWorld(legIndex, getWorldFromRelVector(arg));
	}

	public void getRelIk(LegRelIk result){
		for(int legIndex = 0; legIndex < 4; legIndex++){
			var worldPod = getLeg(legIndex);
			result.setVec(legIndex, getRelFromWorldPoint(worldPod));
		}
	}

	public void setRelIk(LegRelIk arg){
		for(int legIndex = 0; legIndex < 4; legIndex++){
			var relPos = arg.getLegPos(legIndex);
			setLeg(legIndex, getWorldFromRelPoint(relPos));
		}
	}

	public BodyPose(){		
	}

	public BodyPose(BodyPose other){
		assign(other);
	}
}

public static class BodyPoseExtensions{
	public static IEnumerator driveAddVector(this BodyPose pose, Vector3 vecAdd, float time, 
			System.Action<BodyPose, Vector3> onChanged, System.Action onFinished){
		var orig = new BodyPose(pose);
		yield return ValDriver.driveVector(Vector3.zero, vecAdd, time, 
			arg => {
				pose.assign(orig);
				if (onChanged != null)
					onChanged(pose, arg);
			}
		);

		pose.assign(orig);
		if (onChanged != null)
			onChanged(pose, vecAdd);
		if (onFinished != null)
			onFinished();
	}

	public static IEnumerator shiftLegsWorld(this BodyPose pose, Vector3 vecAdd, float time, System.Action onChanged){
		return pose.driveAddVector(vecAdd, time, 
			(p, v) => {
				p.legsAddWorld(v);
				if (onChanged != null)
					onChanged();
			}, 
			null
		);
	}

	public static IEnumerator shiftLegWorld(this BodyPose pose, int legIndex, Vector3 vecAdd, float time, System.Action onChanged){
		return pose.driveAddVector(vecAdd, time, 
			(p, v) => {
				p.legAddWorld(legIndex, v);
				if (onChanged != null)
					onChanged();
			}, 
			null
		);
	}

	public static IEnumerator shiftLegRel(this BodyPose pose, int legIndex, Vector3 vecAdd, float time, System.Action onChanged){
		return pose.driveAddVector(vecAdd, time, 
			(p, v) => {
				p.legAddRel(legIndex, v);
				if (onChanged != null)
					onChanged();
			}, 
			null
		);
	}
}

}