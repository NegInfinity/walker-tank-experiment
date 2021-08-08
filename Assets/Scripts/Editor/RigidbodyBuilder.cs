using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RigidbodyHelpers;
using System.Linq;


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

	static void processSelectedRecursively(System.Action<GameObject> callback){
		var startObj = Selection.activeObject as GameObject;
		Debug.Log($"obj: {startObj}");
		processObjectRecursively(startObj, callback);
	}

	static void processObjectRecursively(GameObject startObj, System.Action<GameObject> callback){
		if (!startObj)
			return;

		var targets = new LinkedList<GameObject>();
		targets.AddLast(startObj);
		while(targets.Count > 0){
			var cur = targets.First.Value;
			targets.RemoveFirst();
			if (!cur)
				continue;
			foreach(Transform curChild in cur.transform){
				targets.AddLast(curChild.gameObject);
			}
			if (callback != null)
				callback(cur);
		}
	}

	[MenuItem("GameObject/Utility/Create Mesh Colliders", false, 0)]
	public static void createMeshColliders(){
		processSelectedRecursively(cur =>{
			var meshFilter = cur.GetComponent<MeshFilter>();
			if (!meshFilter)
				return;

			cur.getOrCreateComponent<MeshCollider>(arg => {
				arg.sharedMesh = meshFilter.sharedMesh;
				arg.convex = true;
			});
		});
	}

	[MenuItem("GameObject/Utility/Create rigidbodies", false, 0)]
	public static void createRigidBodies(){
		var startObj = Selection.activeObject as GameObject;
		processSelectedRecursively(cur =>{
			bool firstObj = (cur.transform.parent && (cur.transform.parent == startObj));
			var meshCollider = cur.GetComponent<MeshCollider>();
			bool hasCollider = meshCollider;

			if (firstObj != hasCollider){
				cur.getOrCreateComponent<Rigidbody>();
			}
		});
	}

	[MenuItem("GameObject/Utility/Create default transforms", false, 0)]
	public static void createDefaultTransforms(){
		processSelectedRecursively(cur =>{
			var rigBody = cur.GetComponent<Rigidbody>();
			if (!rigBody)
				return;
			var defaultTranform = cur.getOrCreateComponent<DefaultTransform>();
			defaultTranform.record();
		});
	}

	[MenuItem("GameObject/Utility/Create density data", false, 0)]
	public static void createDensityData(){
		processSelectedRecursively(cur =>{
			var rigBody = cur.GetComponent<Rigidbody>();
			if (!rigBody)
				return;

			cur.getOrCreateComponent<DensityData>(
				c => {
					c.density = 1000.0f;
				}
			);
		});
	}

	[MenuItem("GameObject/Utility/Create volume data", false, 0)]
	public static void createVolumeData(){
		processSelectedRecursively(cur =>{
			var rigBody = cur.GetComponent<Rigidbody>();
			if (!rigBody)
				return;

			var volumeData = cur.getOrCreateComponent<VolumeData>();
			volumeData.rebuild();
		});
	}

	[MenuItem("GameObject/Utility/Calcualte masses", false, 0)]
	public static void calculateMasses(){
		var massDict = new Dictionary<Rigidbody, float>();
		float defaultDensity = 1000.0f;

		processSelectedRecursively(cur =>{
			var volData = cur.GetComponent<VolumeData>();
			var density = cur.GetComponentInParent<DensityData>()?.density ?? defaultDensity;
			var rigBody = cur.GetComponentInParent<Rigidbody>();
			if (!rigBody || !volData)
				return;
			massDict[rigBody] = massDict.getValueOrDefault(rigBody, 0.0f) + volData.calculatedVolume * density;
		});

		foreach(var entry in massDict){
			entry.Key.mass = entry.Value;
		}
	}

	[System.Serializable]
	public struct SpringDampParams{
		public float spring;
		public float damper;
	}

	[System.Serializable]
	public class HingeLimit{
		public bool directional = false;
		public float min = 0.0f;
		public float max = 90.0f;
	}

	static void setupHinge(Rigidbody thisBody, Rigidbody parentBody, Vector3 axis, SpringDampParams springDamp, HingeLimit limit){
		var hinge = thisBody.gameObject.getOrCreateComponent<HingeJoint>();
		hinge.connectedBody = parentBody;
		hinge.anchor = Vector3.zero;
		hinge.axis = axis;

		hinge.useSpring = true;
		var springObj = hinge.spring;
		springObj.spring = springDamp.spring;
		springObj.damper = springDamp.damper;
		hinge.spring = springObj;

		if (limit != null){
			hinge.useLimits = true;
			var jointLim = hinge.limits;
			if (limit.directional && 
					(thisBody.name.ToLower().EndsWith("lf") || 
					thisBody.name.ToLower().EndsWith("rb"))
			){
				jointLim.min = -limit.max;
				jointLim.max = -limit.min;
			}
			else{
				jointLim.min = limit.min;
				jointLim.max = limit.max;
			}
			hinge.limits = jointLim;
		}
	}

	[MenuItem("GameObject/Utility/Create joints", false, 0)]
	public static void createJoints(){
		processSelectedRecursively(cur =>{
			if (!cur.transform.parent)
				return;
			var thisBody = cur.GetComponent<Rigidbody>();
			var parentBody = cur.transform.parent.gameObject.GetComponentInParent<Rigidbody>();
			if (!thisBody || !parentBody)
				return;

			var thisName = cur.name.ToLower();
			var spring1 = new SpringDampParams(){
				spring = 10000000.0f,
				damper = 10000000.0f
			};
			var spring2 = new SpringDampParams(){
				spring = 1000000000.0f,
				damper = 100000000.0f
			};
			var barrelLimit = new HingeLimit(){
				directional = false,
				min = -20.0f,
				max = 80.0f
			};
			var hipLimit = new HingeLimit(){
				directional = true,
				min = -20.0f,
				max = 120.0f
			};
			var upperLimit = new HingeLimit(){
				directional = false,
				min = -10.0f,
				max = 170.0f
			};
			var lowerLimit = new HingeLimit(){
				directional = false,
				min = -170.0f,
				max = 0.0f
			};

			if (thisName.StartsWith("barrel"))
				setupHinge(thisBody, parentBody, Vector3.right, spring1, barrelLimit);
			if (thisName.StartsWith("turret"))
				setupHinge(thisBody, parentBody, Vector3.up, spring1, null);
			if (thisName.StartsWith("hip"))
				setupHinge(thisBody, parentBody, Vector3.up, spring2, hipLimit);
			if (thisName.StartsWith("upperleg"))
				setupHinge(thisBody, parentBody, Vector3.right, spring2, upperLimit);
			if (thisName.StartsWith("lowerleg"))
				setupHinge(thisBody, parentBody, Vector3.right, spring2, lowerLimit);
		});
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
