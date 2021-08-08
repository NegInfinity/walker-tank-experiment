using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IkSolver{

[System.Serializable]
public struct Coord{
	public Vector3 pos;
	public Vector3 x;
	public Vector3 y;
	public Vector3 z{
		get => Vector3.Cross(x, y);
	}

	public Vector3 right{
		get => x;
		set => x = value;
	}
	public Vector3 up{
		get => y;
		set => y = value;
	}
	public Vector3 forward{
		get => z;
	}

	public static Coord identity{
		get => new Coord(
			new Vector3(0.0f, 0.0f, 0.0f), 
			new Vector3(1.0f, 0.0f, 0.0f), 
			new Vector3(0.0f, 1.0f, 0.0f),
			new Vector3(0.0f, 0.0f, 1.0f)
		);
	}

	public static Coord fromTransform(Transform t, bool blenderTransform){
		if (blenderTransform)
			return new Coord(
				t.position,
				t.right,
				t.forward,
				-t.up
				//stupid coordinate conversion bullshit.
				//t.up, 
				//t.forward
			);

		return new Coord(
			t.position,
			t.right, 
			t.up,
			t.forward
		);
	}

	public static Coord fromTransform(DefaultTransform t, bool local = false){
		if (local){
			return new Coord(
				t.localPos,
				t.localX,
				t.localY,
				t.localZ
			);
		}
		return new Coord(
			t.worldPos,
			t.worldX,
			t.worldY,
			t.worldZ
		);
	}
	
	public Vector3 transformPoint(Vector3 p){
		return pos +
			transformVector(p);
	}

	public Vector3 transformVector(Vector3 v){
		return 
			x * v.x +
			y * v.y +
			z * v.z;
	}

	public Vector3 inverseTransformVector(Vector3 v){
		return new Vector3(
			Vector3.Dot(x, v),
			Vector3.Dot(y, v),
			Vector3.Dot(z, v)
		);
	}

	public Vector3 inverseTransformPoint(Vector3 p){
		var diff = p - pos;
		return inverseTransformVector(diff);
	}

	public Coord transformCoord(Coord arg){
		return new Coord(
			transformPoint(arg.pos),
			transformVector(arg.x),
			transformVector(arg.y),
			transformVector(arg.z)
		);
	}

	public Coord rotatedAround(Vector3 refPos, Vector3 axis, float angleDeg){
		return new Coord(
			refPos + (pos - refPos).rotatedAroundAxis(axis, angleDeg),
			x.rotatedAroundAxis(axis, angleDeg),
			y.rotatedAroundAxis(axis, angleDeg),
			z.rotatedAroundAxis(axis, angleDeg)
		);
	}

	public Coord inverseTransformCoord(Coord arg){
		return new Coord(
			inverseTransformPoint(arg.pos),
			inverseTransformVector(arg.x),
			inverseTransformVector(arg.y),
			inverseTransformVector(arg.z)
		);
	}

	public void normalize(){
		y = y.normalized;
		x = Vector3.ProjectOnPlane(x, y).normalized;
	}

	public Coord(Vector3 pos_, Vector3 x_, Vector3 y_, Vector3 z_){
		pos = pos_;
		x = x_;
		y = y_;
	}

	public Coord(Coord other_){
		pos = other_.pos;
		x = other_.x;
		y = other_.y;
	}
}

}

