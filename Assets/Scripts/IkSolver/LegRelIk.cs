using UnityEngine;
using System.Collections;
using ValueDriver;

namespace IkSolver{

[System.Serializable]
public class LegRelIk{
	public float weight = 1.0f;

	public Vector3 lf;
	public Vector3 rf;
	public Vector3 lb;
	public Vector3 rb;

	public void setY(float val){
		lf.y = val;
		rf.y = val;
		lb.y = val;
		rb.y = val;
	}
	public void addX(float dx){
		lf.x += dx;
		rf.x += dx;
		lb.x += dx;
		rb.x += dx;
	}
	public void addY(float dy){
		lf.y += dy;
		rf.y += dy;
		lb.y += dy;
		rb.y += dy;
	}
	public void addZ(float dz){
		lf.z += dz;
		rf.z += dz;
		lb.z += dz;
		rb.z += dz;
	}

	public void assign(LegRelIk other){
		lf = other.lf;
		rf = other.rf;
		lb = other.lb;
		rb = other.rb;
	}

	public void addVec(Vector3 vec){
		lf += vec;
		rf += vec;
		lb += vec;
		rb += vec;
	}

	public void setVec(float x, float y, float z){
		setVec(new Vector3(x, y, z));
	}

	public void setVec(Vector3 v){
		lf = rf = lb = rb = v;
	}

	public void addVec(float dx, float dy, float dz){
		addVec(new Vector3(dx, dy, dz));
	}

	public void addX(int legIndex, float xAdd){
		addVec(legIndex, new Vector3(xAdd, 0.0f, 0.0f));
	}

	public void addY(int legIndex, float yAdd){
		addVec(legIndex, new Vector3(0.0f, yAdd, 0.0f));
	}

	public void addZ(int legIndex, float zAdd){
		addVec(legIndex, new Vector3(0.0f, 0.0f, zAdd));
	}

	public void setVec(int legIndex, Vector3 vec){
		switch(legIndex){
			case(0):
				lf = vec;
				break;
			case(1):
				rf = vec;
				break;
			case(2):
				lb = vec;
				break;
			case(3):
				rb = vec;
				break;
			default:
				break;
		}
	}
	
	public void setVec(int legIndex, float x, float y, float z){
		setVec(legIndex, new Vector3(x, y, z));
	}

	public void addVec(int legIndex, Vector3 vec){
		switch(legIndex){
			case(0):
				lf += vec;
				break;
			case(1):
				rf += vec;
				break;
			case(2):
				lb += vec;
				break;
			case(3):
				rb += vec;
				break;
			default:
				break;
		}
	}

	public Vector3 getLegPos(int legIndex){
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

	public void addIk(LegRelIk other){
		float f = Mathf.Clamp01(other.weight);
		lf += other.lf*f;
		lb += other.lb*f;
		rf += other.rf*f;
		rb += other.rb*f;
	}

	public void addVec(int legIndex, float dx, float dy, float dz){
		addVec(legIndex, new Vector3(dx, dy, dz));
	}

	public LegRelIk(){
		lf = rf = lb = rb = Vector3.zero;
	}

	public LegRelIk(LegRelIk other){
		assign(other);
	}

	public LegRelIk(float sideOffset, float forwardOffset, float backOffset, float height){
		lf = new Vector3(-sideOffset, height, forwardOffset);
		rf = new Vector3(+sideOffset, height, forwardOffset);
		lb = new Vector3(-sideOffset, height, backOffset);
		rb = new Vector3(+sideOffset, height, backOffset);
	}
}

public static class LegRelIkExtensions{
	public static IEnumerator shift(this LegRelIk rel, Vector3 vecAdd, float time, System.Action<LegRelIk> setter){
		var orig = new LegRelIk(rel);
		yield return ValDriver.driveVector(Vector3.zero, vecAdd, time, 
			arg => {
				rel.assign(orig);
				rel.addVec(arg);
				setter(rel);
			}
		);
		rel.assign(orig);
		rel.addVec(vecAdd);
	}

	public static IEnumerator shiftLeg(this LegRelIk rel, int legIndex, Vector3 vecAdd, float time, System.Action<LegRelIk> setter){
		var orig = new LegRelIk(rel);
		yield return ValDriver.driveVector(Vector3.zero, vecAdd, time, 
			arg => {
				rel.assign(orig);
				rel.addVec(legIndex, arg);
				setter(rel);
			}
		);
		rel.assign(orig);
		rel.addVec(legIndex, vecAdd);
	}

	public static IEnumerator shift(this LegRelIk rel, Vector3 vecAdd, float time, System.Action onChange){
		return rel.shift(vecAdd, time, _ => onChange());
	}

	public static IEnumerator shiftLeg(this LegRelIk rel, int legIndex, Vector3 vecAdd, float time, System.Action onChange){
		return rel.shiftLeg(legIndex, vecAdd, time, _ => onChange());
	}

	public static IEnumerator shiftX(this LegRelIk rel, float xAdd, float time, System.Action<LegRelIk> setter){
		return rel.shift(new Vector3(xAdd, 0.0f, 0.0f), time, setter);
	}

	public static IEnumerator shiftY(this LegRelIk rel, float yAdd, float time, System.Action<LegRelIk> setter){
		return rel.shift(new Vector3(0.0f, yAdd, 0.0f), time, setter);
	}

	public static IEnumerator shiftZ(this LegRelIk rel, float zAdd, float time, System.Action<LegRelIk> setter){
		return rel.shift(new Vector3(0.0f, 0.0f, zAdd), time, setter);
	}

	public static IEnumerator shiftX(this LegRelIk rel, float xAdd, float time, System.Action onChange){
		return rel.shift(new Vector3(xAdd, 0.0f, 0.0f), time, onChange);
	}

	public static IEnumerator shiftY(this LegRelIk rel, float yAdd, float time, System.Action onChange){
		return rel.shift(new Vector3(0.0f, yAdd, 0.0f), time, onChange);
	}

	public static IEnumerator shiftZ(this LegRelIk rel, float zAdd, float time, System.Action onChange){
		return rel.shift(new Vector3(0.0f, 0.0f, zAdd), time, onChange);
	}

	public static IEnumerator shiftLegX(this LegRelIk rel, int legIndex, float xAdd, float time, System.Action<LegRelIk> setter){
		return rel.shiftLeg(legIndex, new Vector3(xAdd, 0.0f, 0.0f), time, setter);
	}

	public static IEnumerator shiftLegY(this LegRelIk rel, int legIndex, float yAdd, float time, System.Action<LegRelIk> setter){
		return rel.shiftLeg(legIndex, new Vector3(0.0f, yAdd, 0.0f), time, setter);
	}

	public static IEnumerator shiftLegZ(this LegRelIk rel, int legIndex, float zAdd, float time, System.Action<LegRelIk> setter){
		return rel.shiftLeg(legIndex, new Vector3(0.0f, 0.0f, zAdd), time, setter);
	}
	
	public static IEnumerator shiftLegX(this LegRelIk rel, int legIndex, float xAdd, float time, System.Action onChange){
		return rel.shiftLeg(legIndex, new Vector3(xAdd, 0.0f, 0.0f), time, onChange);
	}

	public static IEnumerator shiftLegY(this LegRelIk rel, int legIndex, float yAdd, float time, System.Action onChange){
		return rel.shiftLeg(legIndex, new Vector3(0.0f, yAdd, 0.0f), time, onChange);
	}

	public static IEnumerator shiftLegZ(this LegRelIk rel, int legIndex, float zAdd, float time, System.Action onChange){
		return rel.shiftLeg(legIndex, new Vector3(0.0f, 0.0f, zAdd), time, onChange);
	}

	public static IEnumerator driveWeight(this LegRelIk relIk, float from, float to, float time, System.Action onChange){
		return ValDriver.driveFloat(from, to - from, time, 
			f => {
				relIk.weight = f;
				if (onChange != null)
					onChange();
			}
		);
	}

	public static IEnumerator driveWeight(this LegRelIk relIk, float to, float time, System.Action onChange){
		return relIk.driveWeight(relIk.weight, to, time, onChange);
	}
}


}