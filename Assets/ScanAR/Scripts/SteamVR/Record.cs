using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Record : MonoBehaviour {

    List<Vector3> tracker_poses;
    List<Quaternion> tracker_rots;

    public List<float> angles;
    public List<Vector3> eulerAngles;
    public List<float> diss;

	// Use this for initialization
	void Start () {
        tracker_poses = new List<Vector3>();
        tracker_rots = new List<Quaternion>();
        angles = new List<float>();
        eulerAngles = new List<Vector3>();
        diss = new List<float>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void RecordMatrix()
    {
        if (GetComponent<SteamVR_TrackedObject>().isValid)
        {
            tracker_poses.Add(transform.position);
            tracker_rots.Add(transform.rotation);
        }
    }

    public void Calculate()
    {
        // calculate the rotation between each two continuous quaternion
        angles.Clear();
        eulerAngles.Clear();
        for (int i = 0; i < tracker_rots.Count-1; i++)
        {
            Quaternion relative = Quaternion.Inverse(tracker_rots[i]) * tracker_rots[i+1];
            Vector3 cur_eulerAngle = relative.eulerAngles;
            float cur_angle = Quaternion.Angle(tracker_rots[i], tracker_rots[i + 1]);

            eulerAngles.Add(cur_eulerAngle);
            angles.Add(cur_angle);

            diss.Add((tracker_poses[i] - tracker_poses[i + 1]).magnitude);
        }
    }
}
