using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultTransform: MonoBehaviour{
	public Quaternion localRot = Quaternion.identity;
	public Vector3 localPos = Vector3.zero;
	public Vector3 localScale = Vector3.one;
	public Vector3 localX = new Vector3(1.0f, 0.0f, 0.0f);
	public Vector3 localY = new Vector3(0.0f, 1.0f, 0.0f);
	public Vector3 localZ = new Vector3(0.0f, 0.0f, 1.0f);

	public Quaternion worldRot = Quaternion.identity;
	public Vector3 worldPos = Vector3.zero;
	public Vector3 worldX = new Vector3(1.0f, 0.0f, 0.0f);
	public Vector3 worldY = new Vector3(0.0f, 1.0f, 0.0f);
	public Vector3 worldZ = new Vector3(0.0f, 0.0f, 1.0f);

	public void record(){
		localRot = transform.localRotation;
		localScale = transform.localScale;
		localPos = transform.localPosition;

		localX = transform.localRotation * new Vector3(1.0f, 0.0f, 0.0f);
		localY = transform.localRotation * new Vector3(0.0f, 1.0f, 0.0f);
		localZ = transform.localRotation * new Vector3(0.0f, 0.0f, 1.0f);

		worldRot = transform.rotation;
		worldPos = transform.position;
		worldX = transform.right;
		worldY = transform.up;
		worldZ = transform.forward;
	}

	// Start is called before the first frame update
	void Start()
	{
		
	}

	// Update is called once per frame
	void Update()
	{
		
	}
}
