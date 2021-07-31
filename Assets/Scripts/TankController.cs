using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TankController : MonoBehaviour{
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
	}

	[System.Serializable]
	public struct Leg{
		public Part hip;
		public Part thigh;
		public Part knee;
		public Part lowerLeg;
		public Part footRoot;
		public Part foot;
		public bool right;
		public bool front;
	}

	[System.Serializable]
	public class LegControl{
		public float legYaw = 0.0f;
		public float footExtraYaw = 0.0f;

		public float thigh = 0.0f;
		public float knee = 0.0f;
		public float lowerLeg = 0.0f;
		public float footRoot = 0.0f;
	}

	public Part turret;
	public Part barrel;
	public Part body;

	public Leg legRF;
	public Leg legLF;
	public Leg legRB;
	public Leg legLB;

	public float turretControlAngle = 0.0f;
	public float barrelControlAngle = 0.0f;
	public LegControl legControlRF = new LegControl();
	public LegControl legControlLF = new LegControl();
	public LegControl legControlRB = new LegControl();
	public LegControl legControlLB = new LegControl();

	public Transform legRFTarget = null;
	public Transform legRBTarget = null;
	public Transform legLFTarget = null;
	public Transform legLBTarget = null;

	public Vector3 lastCenterOfMass = Vector3.zero;
	public Vector3 projectedCenterOfMass = Vector3.zero;

	public Vector3 projectedLF = Vector3.zero;
	public Vector3 projectedRF = Vector3.zero;
	public Vector3 projectedLB = Vector3.zero;
	public Vector3 projectedRB = Vector3.zero;

	public float lastMass = 0.0f;

	static GameObject findObjectWithName(GameObject parent, string name){
		if (parent.name.ToLower() == name)
			return parent;
		foreach(Transform cur in parent.transform){
			if (cur.gameObject.name.ToLower() == name)
				return cur.gameObject;
			GameObject tmpResult = findObjectWithName(cur.gameObject, name);
			if (tmpResult)
				return tmpResult;
		}
		return null;
	}

	void findTankPart(out Part result, string name){
		result = new Part();
		result.name = name;
		result.obj = findObjectWithName(gameObject, name);
		if (result.obj){
			result.hinge = result.obj.GetComponent<HingeJoint>();
			result.body = result.obj.GetComponent<Rigidbody>();
			result.defaultTransform = result.obj.GetComponent<DefaultTransform>();
		}
		else{
			Debug.LogWarning($"Tank part {name} not found in {gameObject}");
		}
	}

	void findTankLeg(out Leg leg, bool front, bool right, string suffix){
		leg = new Leg();
		leg.right = right;
		leg.front = front;

		findTankPart(out leg.hip, $"hip{suffix}");
		findTankPart(out leg.thigh, $"thigh{suffix}");
		findTankPart(out leg.knee, $"knee{suffix}");
		findTankPart(out leg.lowerLeg, $"lowerleg{suffix}");
		findTankPart(out leg.footRoot, $"footroot{suffix}");
		findTankPart(out leg.foot, $"foot{suffix}");
	}

	void drawConnectionLine(Part a, Part b){
		drawConnectionLine(a.obj, b.obj);
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
		drawConnectionLine(body, leg.hip);
		drawConnectionLine(leg.hip, leg.thigh);
		drawConnectionLine(leg.thigh, leg.knee);
		drawConnectionLine(leg.knee, leg.lowerLeg);
		drawConnectionLine(leg.lowerLeg, leg.footRoot);
		drawConnectionLine(leg.footRoot, leg.foot);

		drawCenterOfMass(leg.hip);
		drawCenterOfMass(leg.thigh);
		drawCenterOfMass(leg.knee);
		drawCenterOfMass(leg.lowerLeg);
		drawCenterOfMass(leg.footRoot);
		drawCenterOfMass(leg.foot);
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
		addCenterOfMass(ref center, ref mass, leg.thigh);
		addCenterOfMass(ref center, ref mass, leg.knee);
		addCenterOfMass(ref center, ref mass, leg.lowerLeg);
		addCenterOfMass(ref center, ref mass, leg.footRoot);
		addCenterOfMass(ref center, ref mass, leg.foot);
	}

	void drawGizmoPoint(Vector3 pos, float size){
		var x = transform.right * size * 0.5f;
		var y = transform.up * size * 0.5f;
		var z = transform.forward * size * 0.5f;
		Gizmos.DrawLine(pos - x, pos + x);
		Gizmos.DrawLine(pos - y, pos + y);
		Gizmos.DrawLine(pos - z, pos + z);
	}

	void drawGizmos(Color c){
		var oldColor = Gizmos.color;
		Gizmos.color = c;

		drawLegGizmo(legLF);
		drawLegGizmo(legRF);
		drawLegGizmo(legLB);
		drawLegGizmo(legRB);

		drawConnectionLine(body, turret);
		drawConnectionLine(turret, barrel);
		drawCenterOfMass(body);
		drawCenterOfMass(turret);
		drawCenterOfMass(barrel);

		/*
		drawConnectionLine(legLF.foot, legRF.foot);
		drawConnectionLine(legLB.foot, legRB.foot);
		drawConnectionLine(legLF.foot, legLB.foot);
		drawConnectionLine(legRF.foot, legRB.foot);
		drawConnectionLine(legRF.foot, legLB.foot);
		drawConnectionLine(legLF.foot, legRB.foot);
		*/

		if (lastMass != 0.0f){
			drawGizmoPoint(lastCenterOfMass, 1.0f);
			var massVec = Vector3.Project(lastCenterOfMass - transform.position, Vector3.up);
			var groundMass = lastCenterOfMass - massVec; 
			Gizmos.DrawLine(groundMass, lastCenterOfMass);
			//drawGizmoPoint(groundMass, 1.0f);
		}

		drawGizmoPoint(projectedCenterOfMass, 1.0f);
		Gizmos.DrawLine(projectedLB, projectedRB);
		Gizmos.DrawLine(projectedLF, projectedRF);
		Gizmos.DrawLine(projectedLB, projectedLF);
		Gizmos.DrawLine(projectedRB, projectedRF);
		Gizmos.DrawLine(projectedLB, projectedRF);
		Gizmos.DrawLine(projectedLF, projectedRB);

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
		findTankPart(out turret, "turret");
		findTankPart(out barrel, "barrels");
		findTankPart(out body, "body");

		findTankLeg(out legRF, true, true, "rf");
		findTankLeg(out legLF, true, false, "lf");
		findTankLeg(out legRB, false, true, "rb");
		findTankLeg(out legLB, false, false, "lb");

		if (legRFTarget && legRF.footRoot.obj)
			legRFTarget.transform.position = legRF.footRoot.obj.transform.position;

		if (legRBTarget && legRB.footRoot.obj)
			legRBTarget.transform.position = legRB.footRoot.obj.transform.position;

		if (legLFTarget && legLF.footRoot.obj)
			legLFTarget.transform.position = legLF.footRoot.obj.transform.position;

		if (legLBTarget && legLB.footRoot.obj)
			legLBTarget.transform.position = legLB.footRoot.obj.transform.position;

		controlCoroutineObject = StartCoroutine(controlCoroutine());
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
		applyHingeAngle(leg.foot, -legControl.legYaw + legControl.footExtraYaw);

		applyHingeAngle(leg.thigh, legControl.thigh);
		applyHingeAngle(leg.knee, legControl.knee);
		applyHingeAngle(leg.lowerLeg, legControl.lowerLeg);
		applyHingeAngle(leg.footRoot, legControl.footRoot);
	}

	void applyControl(){
		applyHingeAngle(turret, turretControlAngle);
		applyHingeAngle(barrel, barrelControlAngle);
		applyLegControl(legRF, legControlRF);
		applyLegControl(legLF, legControlLF);
		applyLegControl(legRB, legControlRB);
		applyLegControl(legLB, legControlLB);
	}	

	(Vector3, float) getCenterOfMass(){
		Vector3 centerOfMass = Vector3.zero;
		float mass = 0.0f;

		addCenterOfMass(ref centerOfMass, ref mass, turret);
		addCenterOfMass(ref centerOfMass, ref mass, barrel);
		addCenterOfMass(ref centerOfMass, ref mass, body);
		addCenterOfMass(ref centerOfMass, ref mass, legLB);
		addCenterOfMass(ref centerOfMass, ref mass, legLF);
		addCenterOfMass(ref centerOfMass, ref mass, legRB);
		addCenterOfMass(ref centerOfMass, ref mass, legRF);

		if (mass != 0.0f){
			centerOfMass /= mass;
		}

		return (centerOfMass, mass);
	}
	void computeMass(){
		lastCenterOfMass = Vector3.zero;
		lastMass = 0.0f;

		var data = getCenterOfMass();
		if (data.Item2 != 0.0f){
			lastCenterOfMass = data.Item1;
			lastMass = data.Item2;
		}

		/*
		addCenterOfMass(ref lastCenterOfMass, ref lastMass, turret);
		addCenterOfMass(ref lastCenterOfMass, ref lastMass, barrel);
		addCenterOfMass(ref lastCenterOfMass, ref lastMass, body);
		addCenterOfMass(ref lastCenterOfMass, ref lastMass, legLB);
		addCenterOfMass(ref lastCenterOfMass, ref lastMass, legLF);
		addCenterOfMass(ref lastCenterOfMass, ref lastMass, legRB);
		addCenterOfMass(ref lastCenterOfMass, ref lastMass, legRF);

		if (lastMass != 0.0f){
			lastCenterOfMass /= lastMass;
		}
		*/
	}

	static Vector2 project2d(Vector3 x2d, Vector3 y2d, Vector3 val){
		return new Vector2(Vector3.Dot(x2d, val), Vector3.Dot(y2d, val));
	}

	static Vector2 perp(Vector2 arg){
		return new Vector2(-arg.y, arg.x);
	}

	/*
	public static Vector3 transformPoint(Vector3 arg, Vector3 pos, Vector3 z, Vector3 y){
		Vector3 x = Vector3.Cross(z, y);
	}
	*/

	public struct Coord{
		public Vector3 pos;
		public Vector3 x{
			get => Vector3.Cross(z, y);
		}
		public Vector3 y;
		public Vector3 z;

		public Vector3 right{
			get => x;
		}
		public Vector3 up{
			get => y;
			set => y = value;
		}
		public Vector3 forward{
			get => z;
			set => z = value;
		}

		public static Coord identity{
			get => new Coord(
				new Vector3(0.0f, 0.0f, 0.0f), 
				new Vector3(0.0f, 1.0f, 0.0f), 
				new Vector3(0.0f, 0.0f, 1.0f)
			);
		}

		public static Coord fromTransform(Transform t){
			return new Coord(
				t.position,
				t.forward,
				-t.up
				//stupid coordinate conversion bullshit.
				//t.up, 
				//t.forward
			);
		}

		public static Coord fromTransform(DefaultTransform t, bool local = false){
			if (local){
				return new Coord(
					t.localPos,
					t.localY,
					t.localZ
				);
			}
			return new Coord(
				t.worldPos,
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
				transformVector(arg.y),
				transformVector(arg.z)
			);
		}

		public Coord inverseTransformCoord(Coord arg){
			return new Coord(
				inverseTransformPoint(arg.pos),
				inverseTransformVector(arg.y),
				inverseTransformVector(arg.z)
			);
		}

		public void normalize(){
			z = z.normalized;
			y = (y - Vector3.Project(y, z)).normalized;
		}

		public Coord(Vector3 pos_, Vector3 y_, Vector3 z_){
			pos = pos_;
			y = y_;
			z = z_;
		}

		public Coord(Coord other_){
			pos = other_.pos;
			y = other_.y;
			z = other_.z;
		}
	}

	[System.Serializable]
	public class IkNode{
		public Coord local = Coord.identity;
		public Coord defaultCoord = Coord.identity;
		public Coord world = Coord.identity;

		public Vector3 localAxis = new Vector3(1.0f, 0.0f, 0.0f);
		public Vector3 worldAxis{
			get => world.transformVector(localAxis);
		}

		public void reset(){
			local = defaultCoord;
		}

		public void update(IkNode parent){
			if (parent != null)
				world = parent.world.transformCoord(local);
			else
				world = local;
		}

		public float getAxisRotation(){
			var xVec = Vector3.right;
			var yVec = Vector3.up;
			var zVec = Vector3.forward;			

			var refVec = xVec;
			if (Mathf.Abs(Vector3.Dot(refVec, localAxis)) > Mathf.Abs(Vector3.Dot(yVec, localAxis)))
				refVec = yVec;
			if (Mathf.Abs(Vector3.Dot(refVec, localAxis)) > Mathf.Abs(Vector3.Dot(zVec, localAxis)))
				refVec = zVec;

			refVec = Vector3.ProjectOnPlane(refVec, localAxis).normalized;

			var origVec = defaultCoord.transformVector(refVec);
			var newVec = local.transformVector(refVec);

			var refAxis = local.transformVector(localAxis).normalized;
			origVec = Vector3.ProjectOnPlane(origVec, refAxis).normalized;
			newVec = Vector3.ProjectOnPlane(newVec, refAxis).normalized;
			var origPerp = Vector3.Cross(refAxis, origVec).normalized;

			var x = Vector3.Dot(newVec, origVec);
			var y = Vector3.Dot(newVec, origPerp);

			return -Mathf.Atan2(y, x);
		}

		public float getAxisRotationDeg(){
			return Mathf.Rad2Deg * getAxisRotation();
		}

		public void moveToParentSpace(IkNode parent, bool affectDefault){
			local = parent.world.inverseTransformCoord(world);
			if (affectDefault)
				defaultCoord = parent.world.inverseTransformCoord(defaultCoord);
		}

		public IkNode(Coord coord_, Vector3 localAxis_){
			local = coord_;
			defaultCoord = coord_;
			world = coord_;
			localAxis = localAxis_;
		}		
	}

	static void updateIkChain(List<IkNode> nodes, int firstIndex, bool reset){
		if (reset){
			for(int i = firstIndex; i < nodes.Count; i++){
				nodes[i].reset();
			}
		}

		for(int i = firstIndex; i < nodes.Count; i++){
			nodes[i].update((i > 0) ? nodes[i-1]: null);
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

	static Vector3 vectorRotate(Vector3 arg, Vector3 start, Vector3 end, Vector3 axis){
		var startPerp = Vector3.Cross(start, axis);
		var endPerp = Vector3.Cross(end, axis);

		var x = Vector3.Dot(start, arg);
		var y = Vector3.Dot(axis, arg);
		var z = Vector3.Dot(startPerp, arg);

		var result = 
			x * end +
			y * axis + 
			z * endPerp;

		return result;
	}

	static void solveIkChain(List<IkNode> nodes, Vector3 target, bool reset, int chainLength = -1, float epsilon = 0.001f){
		updateIkChain(nodes, 0, reset);
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

				var toTarget = (target - curNode.world.pos).normalized;
				var toEffector = (lastNode.world.pos - curNode.world.pos).normalized;

				var projToTarget = Vector3.ProjectOnPlane(toTarget, curNode.worldAxis);
				var projToEffector = Vector3.ProjectOnPlane(toEffector, curNode.worldAxis);

				if (almostEqual(projToTarget, Vector3.zero) || almostEqual(projToTarget, Vector3.zero))
					continue;

				projToTarget = projToTarget.normalized;
				projToEffector = projToEffector.normalized;

				var axis = Vector3.Cross(projToEffector, projToTarget);
				if (almostEqual(axis, Vector3.zero))
					continue;

				axis = axis.normalized;

				var rotatedForward = vectorRotate(curNode.world.forward, projToEffector, projToTarget, axis);
				var rotatedUp = vectorRotate(curNode.world.up, projToEffector, projToTarget, axis);
				if (nodeIndex > 0){
					var parentNode = nodes[nodeIndex - 1];

					rotatedForward = parentNode.world.inverseTransformVector(rotatedForward);
					rotatedUp = parentNode.world.inverseTransformVector(rotatedUp);
				}

				curNode.local.forward = rotatedForward;
				curNode.local.up = rotatedUp;

				updateIkChain(nodes, nodeIndex, false);
			}
			var diff = target - nodes[lastIndex].world.pos;
			if (diff.magnitude < epsilon)
				break;
			if (almostEqual(target, nodes[lastIndex].world.pos, epsilon))
				break;
		}
	}

	void solveLegKinematics(Leg leg, LegControl legControl, Vector3 worldTargetPos){
		if (!body.obj || !leg.hip.obj || !leg.footRoot.obj || !leg.lowerLeg.obj || !leg.knee.obj || !leg.thigh.obj)
			return;

		var origBodyCoord = Coord.fromTransform(body.defaultTransform);
		var worldBodyCoord = Coord.fromTransform(body.obj.transform);
		origBodyCoord.forward = transform.forward;
		origBodyCoord.up = transform.up;
		var hipCoord = Coord.fromTransform(leg.hip.defaultTransform);
		var thighCoord = Coord.fromTransform(leg.thigh.defaultTransform);
		var kneeCoord = Coord.fromTransform(leg.knee.defaultTransform);
		var lowerLegCoord = Coord.fromTransform(leg.lowerLeg.defaultTransform);
		var footRootCoord = Coord.fromTransform(leg.footRoot.defaultTransform);

		hipCoord = origBodyCoord.inverseTransformCoord(hipCoord);
		thighCoord = origBodyCoord.inverseTransformCoord(thighCoord);
		kneeCoord = origBodyCoord.inverseTransformCoord(kneeCoord);
		lowerLegCoord = origBodyCoord.inverseTransformCoord(lowerLegCoord);
		footRootCoord = origBodyCoord.inverseTransformCoord(footRootCoord);

		var hipNode = new IkNode(hipCoord, leg.hip.hinge.axis);
		var thighNode = new IkNode(thighCoord, leg.thigh.hinge.axis);
		var kneeNode = new IkNode(kneeCoord, leg.knee.hinge.axis);
		var lowerLegNode = new IkNode(lowerLegCoord, leg.lowerLeg.hinge.axis);
		var footRootNode = new IkNode(footRootCoord, leg.footRoot.hinge.axis);

		var nodes = new List<IkNode>();
		nodes.Add(hipNode);
		nodes.Add(thighNode);
		nodes.Add(kneeNode);
		nodes.Add(lowerLegNode);
		nodes.Add(footRootNode);

		for(int i = nodes.Count-1; i > 0; i--){
			nodes[i].moveToParentSpace(nodes[i-1], true);
		}

		//var targetPos = origBodyCoord.inverseTransformPoint(worldTargetPos);
		var targetPos = worldBodyCoord.inverseTransformPoint(worldTargetPos);
		solveIkChain(nodes, targetPos, true);

		var debugCoord = worldBodyCoord;
		for(int i = 0; (i + 1) < nodes.Count; i++){
			var node = nodes[i];
			var nextNode = nodes[i+1];
			Debug.DrawLine(
				debugCoord.transformPoint(node.world.pos), 
				debugCoord.transformPoint(nextNode.world.pos)
			);
			Debug.DrawLine(
				debugCoord.transformPoint(node.world.pos), 
				debugCoord.transformPoint(node.world.pos + node.worldAxis*0.25f)
			);
		}
		if (nodes.Count > 0){
			Debug.DrawLine(debugCoord.transformPoint(nodes[0].world.pos), debugCoord.transformPoint(targetPos));
			Debug.DrawLine(debugCoord.transformPoint(nodes[nodes.Count-1].world.pos), debugCoord.transformPoint(targetPos));
		}

		legControl.legYaw = hipNode.getAxisRotationDeg();
		legControl.thigh = thighNode.getAxisRotationDeg();
		legControl.knee = kneeNode.getAxisRotationDeg();
		legControl.lowerLeg = lowerLegNode.getAxisRotationDeg();
		legControl.footRoot = 0.0f;
		if (leg.front){
			legControl.footRoot -= legControl.thigh;
			legControl.footRoot -= legControl.knee;
			legControl.footRoot -= legControl.lowerLeg;
		}
		else{
			legControl.footRoot += legControl.thigh;
			legControl.footRoot += legControl.knee;
			legControl.footRoot += legControl.lowerLeg;
		}
	}

	void solveKinematics(){
		if (legRFTarget && legRFTarget.gameObject.activeInHierarchy)
			solveLegKinematics(legRF, legControlRF, legRFTarget.position);
		if (legRBTarget && legRBTarget.gameObject.activeInHierarchy)
			solveLegKinematics(legRB, legControlRB, legRBTarget.position);
		if (legLFTarget && legLFTarget.gameObject.activeInHierarchy)
			solveLegKinematics(legLF, legControlLF, legLFTarget.position);
		if (legLBTarget && legLBTarget.gameObject.activeInHierarchy)
			solveLegKinematics(legLB, legControlLB, legLBTarget.position);
	}

	void computeProjections(){
		var groundPlane = Vector3.up;
		projectedCenterOfMass = Vector3.ProjectOnPlane(lastCenterOfMass, groundPlane);
		projectedLF = Vector3.ProjectOnPlane(legLF.footRoot.obj.transform.position, groundPlane);
		projectedRF = Vector3.ProjectOnPlane(legRF.footRoot.obj.transform.position, groundPlane);
		projectedLB = Vector3.ProjectOnPlane(legLB.footRoot.obj.transform.position, groundPlane);
		projectedRB = Vector3.ProjectOnPlane(legRB.footRoot.obj.transform.position, groundPlane);
	}

	Vector3 getBodyForward(Transform t){
		return -t.up;
	}
	Vector3 getBodyUp(Transform t){
		return t.forward;
	}
	Vector3 getBodyRight(Transform t){
		return t.right;
	}	

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

	void setLegIkRelative(int legIndex, float right, float forward, float height){
		var target = getLegIkTarget(legIndex);
		var rightVec = getBodyRight(body.obj.transform);
		var forwardVec = getBodyForward(body.obj.transform);
		var upVec = getBodyUp(body.obj.transform);
		var pos = body.objWorldPos;

		target.transform.position = pos 
			+ right * rightVec + forward * forwardVec 
			+ upVec * height;
	}
	void setLegIkRelative(int legIndex, Vector3 coord){
		setLegIkRelative(legIndex, coord.x, coord.z, coord.y);
	}
	void setLegIkRelative(Vector3 lf, Vector3 rf, Vector3 lb, Vector3 rb){
		setLegIkRelative(0, lf);
		setLegIkRelative(1, rf);
		setLegIkRelative(2, lb);
		setLegIkRelative(3, rb);
	}
	void setLegIkRelative(LegRelIk relIk){
		setLegIkRelative(relIk.lf, relIk.rf, relIk.lb, relIk.rb);
	}

	[System.Serializable]
	public class LegRelIk{
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

		public void addX(int legIndex, float xAdd){
			addVec(legIndex, new Vector3(xAdd, 0.0f, 0.0f));
		}

		public void addY(int legIndex, float yAdd){
			addVec(legIndex, new Vector3(0.0f, yAdd, 0.0f));
		}

		public void addZ(int legIndex, float zAdd){
			addVec(legIndex, new Vector3(0.0f, 0.0f, zAdd));
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

	IEnumerator driveFloat(float startVal, float add, float time, System.Action<float> onChange, System.Action<float> onFinish = null){
		float timer = 0.0f;
		float speed = add/time;
		while(timer < time){
			var val = startVal + Mathf.Clamp01(timer/time)*add;
			timer += Time.deltaTime;
			onChange(val);
			yield return null;
		}
		onChange(startVal + add);
		if (onFinish != null){
			onFinish(startVal + add);
		}
	}

	IEnumerator driveVector(Vector3 startVal, Vector3 add, float time, System.Action<Vector3> onChange, System.Action<Vector3> onFinish = null){
		float timer = 0.0f;
		var speed = add/time;
		while(timer < time){
			var val = startVal + Mathf.Clamp01(timer/time)*add;
			timer += Time.deltaTime;
			onChange(val);
			yield return null;
		}
		onChange(startVal + add);
		if (onFinish != null){
			onFinish(startVal + add);
		}
	}

	IEnumerator tankShiftY(LegRelIk rel, float height, float time){
		var tmp = new LegRelIk(rel);
		yield return driveFloat(0.0f, height, time, 
			arg => {
				tmp.assign(rel);
				tmp.addY(arg);
				setLegIkRelative(tmp);
			}
		);
		rel.addY(height);
	}

	IEnumerator tankShiftX(LegRelIk rel, float xAdd, float time){
		var tmp = new LegRelIk(rel);
		yield return driveFloat(0.0f, xAdd, time, 
			arg => {
				tmp.assign(rel);
				tmp.addX(arg);
				setLegIkRelative(tmp);
			}
		);
		rel.addX(xAdd);
	}

	IEnumerator tankShiftZ(LegRelIk rel, float zAdd, float time){
		var tmp = new LegRelIk(rel);
		yield return driveFloat(0.0f, zAdd, time, 
			arg => {
				tmp.assign(rel);
				tmp.addZ(arg);
				setLegIkRelative(tmp);
			}
		);
		rel.addZ(zAdd);
	}

	IEnumerator tankShift(LegRelIk rel, Vector3 vecAdd, float time){
		var tmp = new LegRelIk(rel);
		yield return driveVector(Vector3.zero, vecAdd, time, 
			arg => {
				tmp.assign(rel);
				tmp.addVec(arg);
				setLegIkRelative(tmp);
			}
		);
		rel.addVec(vecAdd);
	}

	IEnumerator tankShiftLeg(LegRelIk rel, int legIndex, Vector3 vecAdd, float time){
		var tmp = new LegRelIk(rel);
		yield return driveVector(Vector3.zero, vecAdd, time, 
			arg => {
				tmp.assign(rel);
				tmp.addVec(legIndex, arg);
				setLegIkRelative(tmp);
			}
		);
		rel.addVec(legIndex, vecAdd);
	}

	IEnumerator tankShiftLegX(LegRelIk rel, int legIndex, float xAdd, float time){
		yield return tankShiftLeg(rel, legIndex, new Vector3(xAdd, 0.0f, 0.0f), time);
	}

	IEnumerator tankShiftLegY(LegRelIk rel, int legIndex, float yAdd, float time){
		yield return tankShiftLeg(rel, legIndex, new Vector3(0.0f, yAdd, 0.0f), time);
	}

	IEnumerator tankShiftLegZ(LegRelIk rel, int legIndex, float zAdd, float time){
		yield return tankShiftLeg(rel, legIndex, new Vector3(0.0f, 0.0f, zAdd), time);
	}

	IEnumerator controlCoroutine(){
		if (!legRFTarget || !legRF.footRoot.obj ||
			!legRBTarget || !legRB.footRoot.obj ||
			!legLFTarget || !legLF.footRoot.obj ||
			!legLBTarget || !legLB.footRoot.obj){

			Debug.LogWarning("coroutine init failed");
			yield break;
		}

		legRFTarget.transform.position = legRF.footRoot.obj.transform.position;
		legRBTarget.transform.position = legRB.footRoot.obj.transform.position;
		legLFTarget.transform.position = legLF.footRoot.obj.transform.position;
		legLBTarget.transform.position = legLB.footRoot.obj.transform.position;

		var targetLB = legLBTarget.transform;
		var targetRB = legRBTarget.transform;
		var targetLF = legLFTarget.transform;
		var targetRF = legRFTarget.transform;

		var hipLB = legLB.hip.obj.transform;
		var hipRB = legRB.hip.obj.transform;
		var hipLF = legLF.hip.obj.transform;
		var hipRF = legLF.hip.obj.transform;

		var bodyTr = body.obj.transform;
		var footHeight = new float[]{
			Vector3.Project(legLF.footRoot.objWorldPos, Vector3.up).magnitude,
			Vector3.Project(legRF.footRoot.objWorldPos, Vector3.up).magnitude,
			Vector3.Project(legLB.footRoot.objWorldPos, Vector3.up).magnitude,
			Vector3.Project(legRB.footRoot.objWorldPos, Vector3.up).magnitude
		}.Average();
		float bodyHeight = Vector3.Dot(bodyTr.position, Vector3.up);
		float legSideOffset = new float[]{
			Vector3.Project(legLF.footRoot.objWorldPos - bodyTr.position, getBodyRight(bodyTr)).magnitude,
			Vector3.Project(legRF.footRoot.objWorldPos - bodyTr.position, getBodyRight(bodyTr)).magnitude,
			Vector3.Project(legLB.footRoot.objWorldPos - bodyTr.position, getBodyRight(bodyTr)).magnitude,
			Vector3.Project(legRB.footRoot.objWorldPos - bodyTr.position, getBodyRight(bodyTr)).magnitude
		}.Average();
		float legForwardOffset = new float[]{
			Vector3.Dot(legLF.footRoot.objWorldPos - bodyTr.position, getBodyForward(bodyTr)),
			Vector3.Dot(legRF.footRoot.objWorldPos - bodyTr.position, getBodyForward(bodyTr)),
		}.Average();
		float legBackOffset = new float[]{
			Vector3.Dot(legLB.footRoot.objWorldPos - bodyTr.position, getBodyForward(bodyTr)),
			Vector3.Dot(legRB.footRoot.objWorldPos - bodyTr.position, getBodyForward(bodyTr)),
		}.Average();

		var legHeight = -(bodyHeight - footHeight);
		var defLegHeight = legHeight;

		var relIk = new LegRelIk(legSideOffset, legForwardOffset, legBackOffset, legHeight);
		setLegIkRelative(relIk);
		yield return new WaitForSeconds(2.0f);

		var curHeight = legHeight;
		yield return tankShiftY(relIk, -1.5f, 1.0f);
		yield return tankShiftX(relIk, 1.0f, 1.0f);

		float sideStep = 0.75f;

		yield return tankShiftLegY(relIk, 1, 1.5f, 1.0f);
		yield return tankShiftLeg(relIk, 1, new Vector3(sideStep, 0.0f, -sideStep), 1.0f);
		yield return tankShiftLegY(relIk, 1, -1.5f, 1.0f);

		yield return tankShiftLegY(relIk, 3, 1.5f, 1.0f);
		yield return tankShiftLeg(relIk, 3, new Vector3(sideStep, 0.0f, sideStep), 1.0f);
		yield return tankShiftLegY(relIk, 3, -1.5f, 1.0f);

		yield return tankShiftX(relIk, -2.0f, 2.0f);

		yield return tankShiftLegY(relIk, 0, 1.5f, 1.0f);
		yield return tankShiftLeg(relIk, 0, new Vector3(-sideStep, 0.0f, -sideStep), 1.0f);
		yield return tankShiftLegY(relIk, 0, -1.5f, 1.0f);

		yield return tankShiftLegY(relIk, 2, 1.5f, 1.0f);
		yield return tankShiftLeg(relIk, 2, new Vector3(-sideStep, 0.0f, sideStep), 1.0f);
		yield return tankShiftLegY(relIk, 2, -1.5f, 1.0f);

		yield return tankShiftX(relIk, 1.0f, 1.0f);

		float sx = 0.5f;
		float sz = 1.0f;
		float sz2 = 1.0f;
		float sh = 2.0f;
		float st = 1.0f;
		yield return tankShiftY(relIk, -1.0f, 1.0f);
		{
			float timer = 0.0f;
			float period = 4.0f;
			float rx = 0.75f;
			float rz = 1.0f;
			var subIk = new LegRelIk(relIk);
			yield return tankShiftX(subIk, rx, 1.0f);

			const int numQuadrants = 4;
			var legOffsets = new float[4];
			int[] quadrantToLeg = new int[numQuadrants]{
				1, 0, 2, 3
			};
			System.Func<float, float> sawFunc = (t) => {
				t = Mathf.Clamp01(t);
				float t0 = 0.25f;
				if (t <= t0)
					return Mathf.Lerp(0.0f, 1.0f, Mathf.Clamp01(t/t0));
				return Mathf.Lerp(1.0f, 0.0f, Mathf.Clamp01((t - t0)/(1.0f - t0)));
			};
			while(true){
				float t = timer/period;
				float angle = Mathf.PI*2.0f*t;
				float pulseAngle = angle * 2.0f;
				float sinePulse = Mathf.Abs(Mathf.Sin(pulseAngle));
				
				int currentQuadrant = Mathf.Clamp(Mathf.FloorToInt(4.0f * timer/period), 0, 3);
				timer += Time.deltaTime;
				timer = timer % period;
				float dx = Mathf.Cos(angle) * rx;
				float dz = Mathf.Sin(angle) * rz;
				float stz = 1.0f;
				subIk.assign(relIk);
				subIk.addZ(dz);
				subIk.addX(dx);
				float raiseLeg = sinePulse * 2.0f;
				subIk.addY(quadrantToLeg[currentQuadrant], raiseLeg);
				for(int i = 0; i < numQuadrants; i++){
					int legIndex = quadrantToLeg[i];
					float tDuration = 0.25f;
					float t0 = ((t + 1.0f) - tDuration*((float)i)) % 1.0f;
					float sawVal = sawFunc(t0);
					subIk.addZ(legIndex, Mathf.Lerp(-stz, stz, sawVal));
				}

				setLegIkRelative(subIk);
				yield return null;
			}
		}
		/*
		{
			yield return tankShift(relIk, new Vector3(-sx, 0.0f, sz), 1.0f);
			while(true){
				yield return tankShiftLeg(relIk, 0, new Vector3(0.0f, sh, 0.0f), st);
				yield return tankShiftLeg(relIk, 0, new Vector3(0.0f, 0, sz2), st);
				yield return tankShiftLeg(relIk, 0, new Vector3(0.0f, -sh, 0.0f), st);
				yield return tankShift(relIk, new Vector3(+sx * 2.0f, 0.0f, 0.0f), 1.0f);

				yield return tankShiftLeg(relIk, 1, new Vector3(0.0f, sh, 0.0f), st);
				yield return tankShiftLeg(relIk, 1, new Vector3(0.0f, 0, sz2), st);
				yield return tankShiftLeg(relIk, 1, new Vector3(0.0f, -sh, 0.0f), st);
				yield return tankShift(relIk, new Vector3(0.0f, 0.0f, -2.0f*sz -sz2*0.5f), 1.0f);

				yield return tankShiftLeg(relIk, 3, new Vector3(0.0f, sh, 0.0f), st);
				yield return tankShiftLeg(relIk, 3, new Vector3(0.0f, 0, sz2), st);
				yield return tankShiftLeg(relIk, 3, new Vector3(0.0f, -sh, 0.0f), st);
				yield return tankShift(relIk, new Vector3(-sx * 2.0f, 0.0f, 0.0f), 1.0f);

				yield return tankShiftLeg(relIk, 2, new Vector3(0.0f, sh, 0.0f), st);
				yield return tankShiftLeg(relIk, 2, new Vector3(0.0f, 0, sz2), st);
				yield return tankShiftLeg(relIk, 2, new Vector3(0.0f, -sh, 0.0f), st);
				yield return tankShift(relIk, new Vector3(0.0f, 0.0f, 2.0f*sz -sz2*0.5f), 1.0f);
			}
		}
		*/
	}

	// Update is called once per frame
	void Update(){
		computeMass();
		computeProjections();
		solveKinematics();
		applyControl();		
	}
}
