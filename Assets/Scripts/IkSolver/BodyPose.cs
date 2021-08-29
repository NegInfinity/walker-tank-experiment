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

	public Vector3 getLeg(int legIndex){
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

	public void setLeg(int legIndex, Vector3 newVal){
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

	public void bodyAddRel(float dx, float dy, float dz){
		bodyAddRel(new Vector3(dx, dy, dz));
	}

	public void bodyAddWorld(float dx, float dy, float dz){
		bodyAddWorld(new Vector3(dx, dy, dz));
	}

	public void bodyAddRel(Vector3 dv){
		bodyAddWorld(getWorldFromRelVector(dv));
	}

	public void bodyAddWorld(Vector3 dv){
		bodyPos += dv;
	}

	public void legsAddWorld(float dx, float dy, float dz){
		legsAddWorld(new Vector3(dx, dy, dz));
	}

	public void legsAddRel(Vector3 arg){
		legsAddWorld(getWorldFromRelVector(arg));
	}	

	public void legsAddRel(float dx, float dy, float dz){
		legsAddRel(new Vector3(dx, dy, dz));
	}

	public void legAddWorld(int legIndex, Vector3 arg){
		setLeg(legIndex, getLeg(legIndex) + arg);
	}

	public void legAddWorld(int legIndex, float dx, float dy, float dz){
		legAddWorld(legIndex, new Vector3(dx, dy, dz));
	}

	public void legAddRel(int legIndex, Vector3 arg){
		legAddWorld(legIndex, getWorldFromRelVector(arg));
	}

	public void legAddRel(int legIndex, float dx, float dy, float dz){
		legAddRel(legIndex, new Vector3(dx, dy, dz));
	}

	public void vecAddWorld(float dx, float dy, float dz){
		vecAddWorld(new Vector3(dx, dy, dz));
	}

	public void vecAddRel(float dx, float dy, float dz){
		vecAddRel(new Vector3(dx, dy, dz));
	}

	public void vecAddWorld(Vector3 dv){
		bodyAddWorld(dv);
		legsAddWorld(dv);
	}

	public void vecAddRel(Vector3 dv){
		bodyAddRel(dv);
		legsAddRel(dv);
	}

	public Vector3 getAverageLegPos(){
		var result = Vector3.zero;
		for(int i = 0; i < 4; i++)
			result += getLeg(i);
		result /= 4.0f;
		return result;
	}

	public void adjustPositionsToWorld(LayerMask mask, Vector3 up, float rayStartHeight, float rayDist, float madAdjustDist = -1.0f){
		var avgLegPos = getAverageLegPos();
		var bodyDiff = (bodyPos - avgLegPos);
		var bodyPlanarDiff = Vector3.ProjectOnPlane(bodyDiff, up);
		var bodyHeight = Vector3.Dot(bodyDiff, up);

		for(int i = 0; i < 4; i++){
			var legPos = getLeg(i);
			var start = legPos + up * rayStartHeight;
			var hit = new RaycastHit();
			Debug.DrawLine(start, start -up*rayDist, Color.yellow, 1.0f);
			if (!Physics.Raycast(start, -up, out hit, rayDist, mask))
				continue;
			Debug.DrawLine(start, hit.point, Color.red, 1.0f);
			var diff = hit.point - legPos;
			if ((madAdjustDist > 0.0f) && (diff.magnitude > madAdjustDist))
				continue;
			Debug.DrawLine(legPos, hit.point, Color.green, 1.0f);
			setLeg(i, hit.point);
		}

		var newLegPos = getAverageLegPos();
		var newBodyPos = newLegPos + bodyPlanarDiff + bodyHeight * up;
		Debug.DrawLine(bodyPos, newBodyPos, Color.green, 1.0f);

		bodyPos = newBodyPos;
	}

	public void moveTo(Vector3 pos){
		vecAddWorld(pos - bodyPos);
	}

	public void rotateAroundBody(float angleDeg, Vector3 axis){
		rotateAroundBody(Quaternion.AngleAxis(angleDeg, axis));
	}

	public void rotateAroundBody(Quaternion rot){
		for(int i = 0; i < 4; i++){
			var pos = getLeg(i);
			var diff = pos - bodyPos;
			diff = rot * diff;
			pos = diff + bodyPos;
			setLeg(i, pos);
		}
		bodyRot *= rot;
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

	public BodyPose clone(){
		return new BodyPose(this);
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