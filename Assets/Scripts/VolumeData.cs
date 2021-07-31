using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeData : MonoBehaviour{
	// Start is called before the first frame update
	public Vector3 minPoint;
	public Vector3 maxPoint;

	public float calculatedVolume = 0.0f;
	public float calculatedMass = 0.0f;

	public Vector3 worldSpaceSize = Vector3.zero;
	public int xRes = 1;
	public int yRes = 1;
	public int zRes = 1;
	void Start(){
		
	}

	public void rebuild(){
		var meshFilter = GetComponent<MeshFilter>();
		minPoint = maxPoint = Vector3.zero;
		if (!meshFilter){
			return;
		}
		var verts = meshFilter.sharedMesh.vertices;
		if (verts.Length == 0){
			return;
		}
		minPoint = maxPoint = verts[0];
		foreach(var cur in verts){
			minPoint = Vector3.Min(minPoint, cur);
			maxPoint = Vector3.Max(maxPoint, cur);
		}

		float density = 1000.0f;
		float voxelResolution = 0.1f;
		var densityData = gameObject.GetComponentInParent<DensityData>();
		if (densityData){
			voxelResolution = densityData.voxelResolution;
			density = densityData.density;
		}
		if (voxelResolution <= 0.0f){
			voxelResolution = 0.1f;
		}

		var xSize = (transform.TransformPoint(minPoint) 
			- transform.TransformPoint(minPoint + new Vector3(maxPoint.x - minPoint.x, 0.0f, 0.0f))).magnitude;
		var ySize = (transform.TransformPoint(minPoint) 
			- transform.TransformPoint(minPoint + new Vector3(0.0f, maxPoint.y - minPoint.y, 0.0f))).magnitude;
		var zSize = (transform.TransformPoint(minPoint) 
			- transform.TransformPoint(minPoint + new Vector3(0.0f, 0.0f, maxPoint.z - minPoint.z))).magnitude;
		Debug.Log($"size({gameObject}):{xSize}, {ySize}, {zSize}");

		worldSpaceSize = new Vector3(xSize, ySize, zSize);

		float maxResolution = 20;
		xRes = Mathf.CeilToInt(Mathf.Min(maxResolution, xSize/voxelResolution));
		yRes = Mathf.CeilToInt(Mathf.Min(maxResolution, ySize/voxelResolution));
		zRes = Mathf.CeilToInt(Mathf.Min(maxResolution, zSize/voxelResolution));
		Debug.Log($"xRes: {xRes}; yRes: {yRes}; zRes: {zRes}");

		//var mesh = meshFilter.sharedMesh;
		calculatedVolume = xSize * ySize * zSize;
		calculatedMass = calculatedVolume * density;
	}

	void drawGizmoLine(float ax, float ay, float az, float bx, float by, float bz){
		Gizmos.DrawLine(new Vector3(ax, ay, az), new Vector3(bx, by, bz));
	}

	void drawAabbGizmo(Vector3 a, Vector3 b){
		drawGizmoLine(a.x, a.y, a.z, b.x, a.y, a.z);		
		drawGizmoLine(a.x, b.y, a.z, b.x, b.y, a.z);		
		drawGizmoLine(a.x, a.y, b.z, b.x, a.y, b.z);		
		drawGizmoLine(a.x, b.y, b.z, b.x, b.y, b.z);		

		drawGizmoLine(a.x, a.y, a.z, a.x, b.y, a.z);		
		drawGizmoLine(b.x, a.y, a.z, b.x, b.y, a.z);		
		drawGizmoLine(a.x, a.y, b.z, a.x, b.y, b.z);		
		drawGizmoLine(b.x, a.y, b.z, b.x, b.y, b.z);		

		drawGizmoLine(a.x, a.y, a.z, a.x, a.y, b.z);		
		drawGizmoLine(b.x, a.y, a.z, b.x, a.y, b.z);		
		drawGizmoLine(a.x, b.y, a.z, a.x, b.y, b.z);		
		drawGizmoLine(b.x, b.y, a.z, b.x, b.y, b.z);		
	}

	void drawGizmos(Color c){
		if (Application.isPlaying)
			return;
		var oldColor = Gizmos.color;
		var oldMatrix = Gizmos.matrix;
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.color = c;

		drawAabbGizmo(minPoint, maxPoint);

		Gizmos.color = oldColor;
		Gizmos.matrix = oldMatrix;
	}

	void OnDrawGizmos(){
		drawGizmos(new Color(1.0f, 1.0f, 0.0f, 0.2f));
	}

	void OnDrawGizmosSelected(){
		drawGizmos(new Color(1.0f, 1.0f, 1.0f, 0.5f));
	}

	// Update is called once per frame
	void Update(){
		
	}
}
