using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class RigidbodyBuilder{
	static T getOrCreateComponent<T>(GameObject obj) where T: Component{
		var result = obj.GetComponent<T>();
		if (!result)
			result = obj.gameObject.AddComponent<T>();
		return result;
	}

	static void setupJoints(GameObject obj){
		var rigBody = obj.GetComponent<Rigidbody>();
		if (!rigBody)
			return;
		var parent = obj.transform.parent;
		if (!parent)
			return;
		var parentRigBody = parent.gameObject.GetComponent<Rigidbody>();
		if (!parentRigBody)
			return;					
		if (obj.name.ToLower().StartsWith("barrel")){
			var hinge = getOrCreateComponent<HingeJoint>(obj);
			hinge.connectedBody = parentRigBody;
			hinge.anchor = Vector3.zero;
			hinge.axis = Vector3.right;

			hinge.useSpring = true;
			var spring = hinge.spring;
			spring.spring = 10000000.0f;
			spring.damper = 10000000.0f;
			hinge.spring = spring;
			return;
		}
		if (obj.name.ToLower().StartsWith("turret")){
			var hinge = getOrCreateComponent<HingeJoint>(obj);
			hinge.connectedBody = parentRigBody;
			hinge.anchor = Vector3.zero;
			hinge.axis = new Vector3(0.0f, 0.0f, 1.0f);

			hinge.useSpring = true;
			var spring = hinge.spring;
			spring.spring = 10000000.0f;
			spring.damper = 10000000.0f;
			hinge.spring = spring;
			return;
		}
		if (obj.name.ToLower().StartsWith("hipattach")){
			var hinge = getOrCreateComponent<FixedJoint>(obj);
			hinge.connectedBody = parentRigBody;
			hinge.anchor = Vector3.zero;
			hinge.axis = Vector3.up;

			return;
		}
		if (obj.name.ToLower().StartsWith("hip")){
			var hinge = getOrCreateComponent<HingeJoint>(obj);
			hinge.connectedBody = parentRigBody;
			hinge.anchor = Vector3.zero;
			hinge.axis = new Vector3(0.0f, 0.0f, 1.0f);

			hinge.useSpring = true;
			var spring = hinge.spring;
			spring.spring = 10000000000.0f;
			spring.damper = 100000000.0f;
			hinge.spring = spring;
			return;
		}
		if (obj.name.ToLower().StartsWith("thigh")){
			var hinge = getOrCreateComponent<HingeJoint>(obj);
			hinge.connectedBody = parentRigBody;
			hinge.anchor = Vector3.zero;
			hinge.axis = new Vector3(1.0f, 0.0f, 0.0f);

			hinge.useSpring = true;
			var spring = hinge.spring;
			spring.spring = 10000000000.0f;
			spring.damper = 100000000.0f;
			hinge.spring = spring;
			return;
		}
		if (obj.name.ToLower().StartsWith("knee")){
			var hinge = getOrCreateComponent<HingeJoint>(obj);
			hinge.connectedBody = parentRigBody;
			hinge.anchor = Vector3.zero;
			hinge.axis = new Vector3(1.0f, 0.0f, 0.0f);

			hinge.useSpring = true;
			var spring = hinge.spring;
			spring.spring = 10000000000.0f;
			spring.damper = 100000000.0f;
			hinge.spring = spring;
			return;
		}
		if (obj.name.ToLower().StartsWith("lowerleg")){
			var hinge = getOrCreateComponent<HingeJoint>(obj);
			hinge.connectedBody = parentRigBody;
			hinge.anchor = Vector3.zero;
			hinge.axis = new Vector3(1.0f, 0.0f, 0.0f);

			hinge.useSpring = true;
			var spring = hinge.spring;
			spring.spring = 10000000000.0f;
			spring.damper = 100000000.0f;
			hinge.spring = spring;
			return;
		}
		if (obj.name.ToLower().StartsWith("footroot")){
			var hinge = getOrCreateComponent<HingeJoint>(obj);
			hinge.connectedBody = parentRigBody;
			hinge.anchor = Vector3.zero;
			hinge.axis = new Vector3(1.0f, 0.0f, 0.0f);

			hinge.useSpring = true;
			var spring = hinge.spring;
			spring.spring = 10000000000.0f;
			spring.damper = 100000000.0f;
			hinge.spring = spring;
			return;
		}
		if (obj.name.ToLower().StartsWith("foot")){
			var hinge = getOrCreateComponent<HingeJoint>(obj);
			hinge.connectedBody = parentRigBody;
			hinge.anchor = Vector3.zero;
			hinge.axis = new Vector3(0.0f, 0.0f, 1.0f);

			hinge.useSpring = true;
			var spring = hinge.spring;
			spring.spring = 10000000000.0f;
			spring.damper = 100000000.0f;
			hinge.spring = spring;
			return;
		}

	}

	[MenuItem("GameObject/Utility/Build Rigidbodies", false, 0)]
	public static void buildRigidBodies(){
		var startObj = Selection.activeObject as GameObject;
		Debug.Log($"obj: {startObj}");
		if (!startObj)
			return;

		var targets = new LinkedList<GameObject>();
		targets.AddLast(startObj);
		var densityComp = startObj.GetComponentInParent<DensityData>();
		float density = 1000.0f;
		if (densityComp){
			density = densityComp.density;
		}
		while(targets.Count > 0){
			var cur = targets.First.Value;
			targets.RemoveFirst();
			if (!cur)
				continue;				
			foreach(Transform curChild in cur.transform){
				targets.AddLast(curChild.gameObject);
			}

			if (cur.name.ToLower().EndsWith("target"))
				continue;

			var meshFilter = cur.GetComponent<MeshFilter>();
			if (!meshFilter)
				continue;

			var rigBody = cur.GetComponent<Rigidbody>();
			if (!rigBody){
				rigBody = cur.AddComponent<Rigidbody>();
			}

			var meshCollider = cur.GetComponent<MeshCollider>();
			if (!meshCollider){
				meshCollider = cur.AddComponent<MeshCollider>();
				meshCollider.sharedMesh = meshFilter.sharedMesh;
				meshCollider.convex = true;
			}

			var defaultTranform = cur.GetComponent<DefaultTransform>();
			if (!defaultTranform){
				defaultTranform = cur.AddComponent<DefaultTransform>();
				defaultTranform.record();
			}

			var volumeData = cur.GetComponent<VolumeData>();
			if (!volumeData){
				volumeData = cur.AddComponent<VolumeData>();
			}
			volumeData.rebuild();
			rigBody.mass = 1.0f;
			if (volumeData.calculatedMass > 0.0f)
				rigBody.mass = volumeData.calculatedMass;

			setupJoints(cur);
		}
	}
}
