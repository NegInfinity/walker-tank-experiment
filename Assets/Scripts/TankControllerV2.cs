using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TankExtensions;
using IkSolver;

public class TankControllerV2: MonoBehaviour{
	[System.Serializable]
	public struct Part{
		public string name;
		public GameObject obj;
		public HingeJoint hinge;
		public Rigidbody body;
		public DefaultTransform defaultTransform;		

		public Vector3 objWorldPos{
			get => obj.transform.position;
		}

		public Part(GameObject obj_){
			obj = obj_;
			name = obj.name;
			hinge = null;
			body = null;
			defaultTransform = null;
			if (obj){
				hinge = obj.GetComponent<HingeJoint>();
				body = obj.GetComponent<Rigidbody>();
				defaultTransform = obj.GetComponent<DefaultTransform>();
			}
		}
	}

	[System.Serializable]
	public struct Leg{
		public Part hip;
		public Part upper;
		public Part lower;
		public Part tip;
		public bool right;
		public bool front;
	}

	[System.Serializable]
	public class LegControl{
		public float legYaw = 0.0f;
		public float upperLeg = 0.0f;
		public float lowerLeg = 0.0f;
	}

	[System.Serializable]
	public class Parts{
		public Part turret;
		public Part barrel;
		public Part body;

		public Leg legRF;
		public Leg legLF;
		public Leg legRB;
		public Leg legLB;
	}

	public Parts parts = new Parts();

	[System.Serializable]
	public class DirectControl{
		public float turretControlAngle = 0.0f;
		public float barrelControlAngle = 0.0f;
		public LegControl legControlRF = new LegControl();
		public LegControl legControlLF = new LegControl();
		public LegControl legControlRB = new LegControl();
		public LegControl legControlLB = new LegControl();
	}
	public DirectControl directControl = new DirectControl();

	[System.Serializable]
	public class IkControl{
		public Transform legRFTarget = null;
		public Transform legRBTarget = null;
		public Transform legLFTarget = null;
		public Transform legLBTarget = null;
		
		Transform getLegIkTarget(int legIndex){
			switch(legIndex){
				case(0):
					return legLFTarget;
				case(1):
					return legRFTarget;
				case(2):
					return legLBTarget;
				case(3):
					return legRBTarget;
				default:
					return legLFTarget;
			}
		}
	}
	public IkControl ikControl = new IkControl();


	bool findTankPart(out Part result, string name){
		var obj = gameObject.findObjectWithLowerName(name);
		if (obj){
			result = new Part(obj);
			return true;
		}
		else{
			result = new Part();
			Debug.LogWarning($"Part {name} not found in {gameObject}");
			return false;
		}
	}

	void findTankLeg(out Leg leg, bool front, bool right, string suffix){
		leg = new Leg();
		leg.right = right;
		leg.front = front;

		findTankPart(out leg.hip, $"hip{suffix}");
		findTankPart(out leg.upper, $"upperleg{suffix}");
		findTankPart(out leg.lower, $"lowerleg{suffix}");
		findTankPart(out leg.tip, $"tip{suffix}");
	}

	void drawConnectionLine(Part a, Part b){
		drawConnectionLine(a.obj, b.obj);
	}

	void drawGizmoPoint(Vector3 pos, float size){
		var x = transform.right * size * 0.5f;
		var y = transform.up * size * 0.5f;
		var z = transform.forward * size * 0.5f;
		Gizmos.DrawLine(pos - x, pos + x);
		Gizmos.DrawLine(pos - y, pos + y);
		Gizmos.DrawLine(pos - z, pos + z);
	}

	void drawConnectionLine(GameObject a, GameObject b){
		if (!a || !b)
			return;
		Gizmos.DrawLine(a.transform.position, b.transform.position);
	}

	void drawCenterOfMass(Part part){
		if (!part.body)
			return;
		drawGizmoPoint(part.body.worldCenterOfMass, 0.25f);
	}

	void drawLegGizmo(Leg leg){
		drawConnectionLine(parts.body, leg.hip);
		drawConnectionLine(leg.hip, leg.upper);
		drawConnectionLine(leg.upper, leg.lower);
		drawConnectionLine(leg.lower, leg.tip);

		drawCenterOfMass(leg.hip);
		drawCenterOfMass(leg.upper);
		drawCenterOfMass(leg.lower);
	}

	void addCenterOfMass(ref Vector3 center, ref float mass, Part part){
		if (!part.body)
			return;
		var worldCenter = part.body.worldCenterOfMass;
		var partMass = part.body.mass;
		center += worldCenter * partMass;
		mass += partMass;
	}

	void addCenterOfMass(ref Vector3 center, ref float mass, Leg leg){
		addCenterOfMass(ref center, ref mass, leg.hip);
		addCenterOfMass(ref center, ref mass, leg.upper);
		addCenterOfMass(ref center, ref mass, leg.lower);
	}

	(Vector3, float) getCenterOfMass(){
		Vector3 centerOfMass = Vector3.zero;
		float mass = 0.0f;

		addCenterOfMass(ref centerOfMass, ref mass, parts.turret);
		addCenterOfMass(ref centerOfMass, ref mass, parts.barrel);
		addCenterOfMass(ref centerOfMass, ref mass, parts.body);
		addCenterOfMass(ref centerOfMass, ref mass, parts.legLB);
		addCenterOfMass(ref centerOfMass, ref mass, parts.legLF);
		addCenterOfMass(ref centerOfMass, ref mass, parts.legRB);
		addCenterOfMass(ref centerOfMass, ref mass, parts.legRF);

		if (mass != 0.0f){
			centerOfMass /= mass;
		}

		return (centerOfMass, mass);
	}

	void drawGizmos(Color c){
		var oldColor = Gizmos.color;
		Gizmos.color = c;

		drawLegGizmo(parts.legLF);
		drawLegGizmo(parts.legRF);
		drawLegGizmo(parts.legLB);
		drawLegGizmo(parts.legRB);

		drawConnectionLine(parts.body, parts.turret);
		drawConnectionLine(parts.turret, parts.barrel);
		drawCenterOfMass(parts.body);
		drawCenterOfMass(parts.turret);
		drawCenterOfMass(parts.barrel);

		var cmData = getCenterOfMass();
		var mass = cmData.Item2;
		var centerOfMass = cmData.Item1;

		if (mass != 0.0f){
			drawGizmoPoint(centerOfMass, 1.0f);
			var massVec = Vector3.Project(centerOfMass - transform.position, Vector3.up);
			var groundMass = centerOfMass - massVec; 
			Gizmos.DrawLine(groundMass, centerOfMass);
			//drawGizmoPoint(groundMass, 1.0f);

			var groundPlane = Vector3.up;
			var projectedCenterOfMass = Vector3.ProjectOnPlane(centerOfMass, groundPlane);
			var projectedLF = Vector3.ProjectOnPlane(parts.legLF.tip.obj.transform.position, groundPlane);
			var projectedRF = Vector3.ProjectOnPlane(parts.legRF.tip.obj.transform.position, groundPlane);
			var projectedLB = Vector3.ProjectOnPlane(parts.legLB.tip.obj.transform.position, groundPlane);
			var projectedRB = Vector3.ProjectOnPlane(parts.legRB.tip.obj.transform.position, groundPlane);

			drawGizmoPoint(projectedCenterOfMass, 1.0f);
			Gizmos.DrawLine(projectedLB, projectedRB);
			Gizmos.DrawLine(projectedLF, projectedRF);
			Gizmos.DrawLine(projectedLB, projectedLF);
			Gizmos.DrawLine(projectedRB, projectedRF);
			Gizmos.DrawLine(projectedLB, projectedRF);
			Gizmos.DrawLine(projectedLF, projectedRB);
		}

		Gizmos.color = oldColor;
	}

	void OnDrawGizmos(){
		drawGizmos(Color.yellow);
	}

	void OnDrawSelected(){
		drawGizmos(Color.white);
	}

	Coroutine controlCoroutineObject = null;

	void Start(){
		findTankPart(out parts.turret, "turret");
		findTankPart(out parts.barrel, "barrels");
		//if (!findTankPart(out body, "body"))
		parts.body = new Part(gameObject);

		findTankLeg(out parts.legRF, true, true, "rf");
		findTankLeg(out parts.legLF, true, false, "lf");
		findTankLeg(out parts.legRB, false, true, "rb");
		findTankLeg(out parts.legLB, false, false, "lb");

		if (ikControl.legRFTarget && parts.legRF.tip.obj)
			ikControl.legRFTarget.transform.position = parts.legRF.tip.obj.transform.position;

		if (ikControl.legRBTarget && parts.legRB.tip.obj)
			ikControl.legRBTarget.transform.position = parts.legRB.tip.obj.transform.position;

		if (ikControl.legLFTarget && parts.legLF.tip.obj)
			ikControl.legLFTarget.transform.position = parts.legLF.tip.obj.transform.position;

		if (ikControl.legLBTarget && parts.legLB.tip.obj)
			ikControl.legLBTarget.transform.position = parts.legLB.tip.obj.transform.position;

		controlCoroutineObject = StartCoroutine(controlCoroutine());
	}

	IEnumerator controlCoroutine(){
		yield break;
	}

	void applyHingeAngle(HingeJoint hinge, float angle){
		if (!hinge)
			return;
		var spring = hinge.spring;
		spring.targetPosition = angle;
		hinge.spring = spring;
	}

	void applyHingeAngle(Part part, float angle){
		applyHingeAngle(part.hinge, angle);
	}

	void applyLegControl(Leg leg, LegControl legControl){
		applyHingeAngle(leg.hip, legControl.legYaw);
		applyHingeAngle(leg.upper, legControl.upperLeg);
		applyHingeAngle(leg.lower, legControl.lowerLeg);
	}


	void applyControl(){
		applyHingeAngle(parts.turret, directControl.turretControlAngle);
		applyHingeAngle(parts.barrel, directControl.barrelControlAngle);
		applyLegControl(parts.legRF, directControl.legControlRF);
		applyLegControl(parts.legLF, directControl.legControlLF);
		applyLegControl(parts.legRB, directControl.legControlRB);
		applyLegControl(parts.legLB, directControl.legControlLB);
	}	

	static void updateIkChain(List<IkNode> nodes, int firstIndex, bool reset, bool recalculateCoord){
		if (reset){
			for(int i = firstIndex; i < nodes.Count; i++){
				nodes[i].reset();
			}
		}

		for(int i = firstIndex; i < nodes.Count; i++){
			nodes[i].update((i > 0) ? nodes[i-1]: null, recalculateCoord);
		}
	}

	static bool almostEqual(float a, float b, float epsilon = 0.001f){
		if (b > a)
			return (b - a) <= epsilon;
		else
			return (a - b) <= epsilon;
	}

	static bool almostEqual(Vector3 a, Vector3 b, float epsilon = 0.001f){
		return 
			almostEqual(a.x, b.x, epsilon) &&
			almostEqual(a.y, b.y, epsilon) &&
			almostEqual(a.z, b.z, epsilon);
	}

	static void solveIkChain(List<IkNode> nodes, Vector3 target, bool reset, int chainLength = -1, float epsilon = 0.001f){
		updateIkChain(nodes, 0, reset, true);
		var lastIndex = nodes.Count - 1;
		if (lastIndex < 0)
			return;

		int maxIterations = 10;
		for(int iterationIndex = 0; iterationIndex < maxIterations; iterationIndex++){
			for(int nodeIndex = 0; nodeIndex < lastIndex; nodeIndex++){
				if ((chainLength >= 0) && (nodeIndex >= chainLength))
					break;

				var curNode = nodes[nodeIndex];
				var lastNode = nodes[lastIndex];

				var toTarget = (target - curNode.jointWorld.pos).normalized;
				var toEffector = (lastNode.jointWorld.pos - curNode.jointWorld.pos).normalized;

				var axis = curNode.jointWorld.x;

				var projToTarget = Vector3.ProjectOnPlane(toTarget, axis);
				var projToEffector = Vector3.ProjectOnPlane(toEffector, axis);

				if (almostEqual(projToTarget, Vector3.zero) || almostEqual(projToEffector, Vector3.zero))
					continue;

				projToTarget = projToTarget.normalized;
				projToEffector = projToEffector.normalized;

				//we use limits, hence Atan2. The engine can handlle it anyway, but we COULD optimize it.
				var xVec = projToEffector;
				var yVec = Vector3.Cross(axis, xVec).normalized;

				var yVal = Vector3.Dot(yVec, projToTarget);
				var xVal = Vector3.Dot(xVec, projToTarget);

				var targetAngle = Mathf.Atan2(yVal, xVal) * Mathf.Rad2Deg;
				curNode.jointState.xRotDeg += targetAngle;

				/*
				var rotatedForward = vectorRotate(curNode.world.forward, projToEffector, projToTarget, axis);
				var rotatedUp = vectorRotate(curNode.world.up, projToEffector, projToTarget, axis);
				if (nodeIndex > 0){
					var parentNode = nodes[nodeIndex - 1];

					rotatedForward = parentNode.world.inverseTransformVector(rotatedForward);
					rotatedUp = parentNode.world.inverseTransformVector(rotatedUp);
				}

				curNode.local.forward = rotatedForward;
				curNode.local.up = rotatedUp;
				*/

				updateIkChain(nodes, nodeIndex, false, true);
			}
			var diff = target - nodes[lastIndex].jointWorld.pos;
			if (diff.magnitude < epsilon)
				break;
			if (almostEqual(target, nodes[lastIndex].jointWorld.pos, epsilon))
				break;
		}
	}

	void solveLegKinematics(Leg leg, LegControl legControl, Vector3 worldTargetPos){
		if (!parts.body.obj || !leg.hip.obj || !leg.upper.obj || !leg.lower.obj || !leg.tip.obj)
			return;

		var origBodyCoord = Coord.fromTransform(parts.body.defaultTransform);
		var worldBodyCoord = Coord.fromTransform(parts.body.obj.transform, false);
		var hipCoord = Coord.fromTransform(leg.hip.defaultTransform);
		var upperCoord = Coord.fromTransform(leg.upper.defaultTransform);
		var lowerCoord = Coord.fromTransform(leg.lower.defaultTransform);
		var tipCoord = Coord.fromTransform(leg.tip.defaultTransform);

		hipCoord = origBodyCoord.inverseTransformCoord(hipCoord);
		upperCoord = origBodyCoord.inverseTransformCoord(upperCoord);
		lowerCoord = origBodyCoord.inverseTransformCoord(lowerCoord);
		tipCoord = origBodyCoord.inverseTransformCoord(tipCoord);

		var hipNode = new IkNode(hipCoord, leg.hip.hinge);
		var upperNode = new IkNode(upperCoord, leg.upper.hinge);
		var lowerNode = new IkNode(lowerCoord, leg.lower.hinge);
		var tipNode = new IkNode(tipCoord, leg.tip.hinge);

		var nodes = new List<IkNode>();
		nodes.Add(hipNode);
		nodes.Add(upperNode);
		nodes.Add(lowerNode);
		nodes.Add(tipNode);

		for(int i = nodes.Count-1; i > 0; i--){
			nodes[i].moveToParentSpace(nodes[i-1], true);
		}

		var targetPos = worldBodyCoord.inverseTransformPoint(worldTargetPos);
		solveIkChain(nodes, targetPos, true);

		//updateIkChain(nodes, 0, false, false);
		var debugCoord = worldBodyCoord;
		for(int i = 0; (i + 1) < nodes.Count; i++){
			var node = nodes[i];
			var nextNode = nodes[i+1];
			Debug.DrawLine(
				debugCoord.transformPoint(node.objWorld.pos), 
				debugCoord.transformPoint(nextNode.objWorld.pos)
			);
		}
		/*
		if (nodes.Count > 0){
			Debug.DrawLine(debugCoord.transformPoint(nodes[0].objWorld.pos), debugCoord.transformPoint(targetPos));
			Debug.DrawLine(debugCoord.transformPoint(nodes[nodes.Count-1].objWorld.pos), debugCoord.transformPoint(targetPos));
		}
		*/

		legControl.legYaw = hipNode.jointState.xRotDeg;
		legControl.upperLeg = upperNode.jointState.xRotDeg;
		legControl.lowerLeg = lowerNode.jointState.xRotDeg;
	}

	void solveKinematics(){
		if (ikControl.legRFTarget && ikControl.legRFTarget.gameObject.activeInHierarchy)
			solveLegKinematics(parts.legRF, directControl.legControlRF, ikControl.legRFTarget.position);
		if (ikControl.legRBTarget && ikControl.legRBTarget.gameObject.activeInHierarchy)
			solveLegKinematics(parts.legRB, directControl.legControlRB, ikControl.legRBTarget.position);
		if (ikControl.legLFTarget && ikControl.legLFTarget.gameObject.activeInHierarchy)
			solveLegKinematics(parts.legLF, directControl.legControlLF, ikControl.legLFTarget.position);
		if (ikControl.legLBTarget && ikControl.legLBTarget.gameObject.activeInHierarchy)
			solveLegKinematics(parts.legLB, directControl.legControlLB, ikControl.legLBTarget.position);
	}

	void Update(){
		//computeMass();
		//computeProjections();
		solveKinematics();
		applyControl();		
	}

}
