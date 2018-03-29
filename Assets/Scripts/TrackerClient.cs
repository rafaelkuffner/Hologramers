using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class TrackerClient : MonoBehaviour
{
    public float rotationTreshold = -0.1f;
	// Filter parameters
	private bool isNewFrame;
	private DateTime frameTime;

	// Human
	public float avatarHeight;
	private Human trackedHuman;
	private string trackedHumanId;
	private Dictionary<string, Human> humans;
    public Transform bar;
    // Body transforms and joints

    //neck
    public Transform neck;

	// Spine
	public Transform spineBase;
	public Transform spineShoulder;

	private PointSmoothing spineBaseJoint;
	private PointSmoothing spineShoulderJoint;

	// Left arm
	public Transform leftShoulder;
	public Transform leftElbow;
	public Transform leftArm;

	private PointSmoothing leftShoulderJoint;
	private PointSmoothing leftElbowJoint;
	private PointSmoothing leftWristJoint;

	// Left leg
	public Transform leftHip;
	public Transform leftKnee;

	private PointSmoothing leftHipJoint;
	private PointSmoothing leftKneeJoint;
	private PointSmoothing leftAnkleJoint;

	// Right arm
	public Transform rightShoulder;
	public Transform rightElbow;
	public Transform rightArm;

	private PointSmoothing rightShoulderJoint;
	private PointSmoothing rightElbowJoint;
	private PointSmoothing rightWristJoint;

	// Right leg
	public Transform rightHip;
	public Transform rightKnee;

	private PointSmoothing rightHipJoint;
	private PointSmoothing rightKneeJoint;
	private PointSmoothing rightAnkleJoint;
    private float bardifference = 0.12f;

    Vector3 lastTrackerForward;
    Vector3 lastCameraForward;


	void Start()
	{
		isNewFrame = false;
		frameTime = DateTime.Now;

		trackedHumanId = string.Empty;
		humans = new Dictionary<string, Human>();

		spineBaseJoint = new PointSmoothing();
		spineShoulderJoint = new PointSmoothing();

		leftShoulderJoint = new PointSmoothing();
		leftElbowJoint = new PointSmoothing();
		leftWristJoint = new PointSmoothing();
		leftHipJoint = new PointSmoothing();
		leftKneeJoint = new PointSmoothing();
		leftAnkleJoint = new PointSmoothing();

		rightShoulderJoint = new PointSmoothing();
		rightElbowJoint = new PointSmoothing();
		rightWristJoint = new PointSmoothing();
		rightHipJoint = new PointSmoothing();
		rightKneeJoint = new PointSmoothing();
		rightAnkleJoint = new PointSmoothing();

        lastCameraForward = Camera.main.transform.forward;
        lastTrackerForward = spineBase.transform.forward;
	}

   

	void Update()
	{
        if(trackedHumanId== string.Empty)
        {
            
            trackedHumanId = GetFirstHuman();
            if(trackedHumanId != string.Empty) { 
                trackedHuman = humans[trackedHumanId];
                AdjustAvatarHeight();
            }

        }
        //if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.PageDown) ||Input.GetKeyDown(KeyCode.Joystick1Button1)) // Mouse tap
        //{
        //	string currentHumanId = GetHumanIdWithHandUp();

        //	if (humans.ContainsKey(currentHumanId)) 
        //	{
        //		trackedHumanId = currentHumanId;
        //		trackedHuman = humans[trackedHumanId];

        //		AdjustAvatarHeight();
        //		UnityEngine.XR.InputTracking.Recenter();
        //	}
        //}

        if (humans.ContainsKey(trackedHumanId)) 
		{
			trackedHuman = humans[trackedHumanId];
			UpdateAvatarBody();
		}

		// Finally
		CleanDeadHumans();
		isNewFrame = false;
	}

	/// <summary>
	/// Gets the first human identifier with a hand above the head.
	/// </summary>
	private string GetHumanIdWithHandUp()
	{
		foreach (Human h in humans.Values) 
		{
			if (h.body.Joints[BodyJointType.leftHand].y  > h.body.Joints[BodyJointType.head].y ||
				h.body.Joints[BodyJointType.rightHand].y > h.body.Joints[BodyJointType.head].y)
			{
				return h.id;
			}
		}
		return string.Empty;
	}

    private string GetFirstHuman()
    {
        if (humans.Values.Count > 0)
            return humans.Values.First().id;
        else return string.Empty;
    }
	/// <summary>
	/// Adjusts avatar's height by calculating the 
	/// ratio between the user and avatar's height.
	/// </summary>
	private void AdjustAvatarHeight()
	{
		float lowerFootY = Mathf.Min(trackedHuman.body.Joints[BodyJointType.rightFoot].y, trackedHuman.body.Joints[BodyJointType.leftFoot].y);

		float userHeight = (trackedHuman.body.Joints[BodyJointType.head].y + 0.1f) - lowerFootY;
		float scaleRatio = userHeight / avatarHeight;
      
		spineBase.transform.localScale = new Vector3(scaleRatio, scaleRatio, scaleRatio);
	}

    public Vector3 GetHeadPos()
    {
        if (trackedHuman != null)
            return trackedHuman.body.Joints[BodyJointType.head];
        else return Vector3.zero;
    }

    public Vector3 GetForward()
    {
        Vector3 spineUp = Utils.GetBoneDirection(spineShoulderJoint.Value, spineBaseJoint.Value);
        Vector3 spineRight = Utils.GetBoneDirection(rightShoulderJoint.Value, leftShoulderJoint.Value);
        Vector3 spineForward = Vector3.Cross(spineRight, spineUp);
        spineForward = transform.TransformDirection(spineForward);
        return spineForward;
    }
	/// <summary>
	/// Updates the avatar body by filtering body 
	/// joints and applying them through rotations.
	/// </summary>
	private void UpdateAvatarBody()
	{
		ApplyFilterToJoints();
        Vector3 headRot = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z); ; //UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.CenterEye).eulerAngles;
        
        // Spine
        Vector3 spineUp = Utils.GetBoneDirection(spineShoulderJoint.Value, spineBaseJoint.Value);
		Vector3 spineRight = Utils.GetBoneDirection(rightShoulderJoint.Value, leftShoulderJoint.Value);
		Vector3 spineForward = Vector3.Cross(spineRight,spineUp);
        Vector3 projForward = new Vector3(spineForward.x, 0, spineForward.z);

        float dotprod = Vector3.Dot(projForward, headRot);
        //Debug.Log("DOT " + dotprod);
         Debug.DrawRay(spineBase.transform.position, spineForward, Color.blue);

        /*if (dotprod < rotationTreshold)
        {
            //   spineForward = -spineForward;
            //    Debug.Log("passssssseeeeeeeii");

            spineRight = Utils.GetBoneDirection(leftShoulderJoint.Value, rightShoulderJoint.Value);
            spineForward = Vector3.Cross(spineRight, spineUp);
            Debug.Log("$$$$");



            //spineRight = Utils.GetBoneDirection(leftShoulderJoint.Value, rightShoulderJoint.Value);
            //spineForward = Vector3.Cross(spineRight, spineUp);

            spineBase.position = spineBaseJoint.Value + new Vector3(0.0f, 0.15f, 0.0f);
            spineBase.rotation = Quaternion.LookRotation(spineForward, spineUp);

            // Left Arm
            leftShoulder.rotation = Utils.GetQuaternionFromRightUp(-Utils.GetBoneDirection(rightShoulderJoint.Value, spineShoulderJoint.Value), spineUp);
            leftElbow.rotation = Utils.GetQuaternionFromRightUp(-Utils.GetBoneDirection(rightWristJoint.Value, rightElbowJoint.Value), spineUp);
            leftArm.rotation = Utils.GetQuaternionFromRightUp(-Utils.GetBoneDirection(rightElbowJoint.Value, rightShoulderJoint.Value), spineUp);

        //    // Left Leg
            leftHip.rotation = Utils.GetQuaternionFromUpRight(-Utils.GetBoneDirection(rightKneeJoint.Value, rightHipJoint.Value), spineRight);
            leftKnee.rotation = Utils.GetQuaternionFromUpRight(-Utils.GetBoneDirection(rightAnkleJoint.Value, rightKneeJoint.Value), spineRight);

        //    // Right Arm
            rightShoulder.rotation = Utils.GetQuaternionFromRightUp(Utils.GetBoneDirection(leftShoulderJoint.Value, spineShoulderJoint.Value),spineUp);
            rightElbow.rotation = Utils.GetQuaternionFromRightUp( Utils.GetBoneDirection(leftWristJoint.Value, leftElbowJoint.Value),spineUp);
            rightArm.rotation = Utils.GetQuaternionFromRightUp( Utils.GetBoneDirection(leftElbowJoint.Value, leftShoulderJoint.Value),spineUp);

            // Right Leg
            rightHip.rotation = Utils.GetQuaternionFromUpRight(-Utils.GetBoneDirection(leftKneeJoint.Value, leftHipJoint.Value), spineRight);
            rightKnee.rotation = Utils.GetQuaternionFromUpRight(-Utils.GetBoneDirection(leftAnkleJoint.Value, leftKneeJoint.Value), spineRight);
           // InputTracking.Recenter();
            // neck.transform.eulerAngles = headRot;
        }
        else*/
        {
            spineBase.localPosition = spineBaseJoint.Value + new Vector3(0.0f, 0.15f, 0.0f);
            spineBase.localRotation = Quaternion.LookRotation(spineForward, spineUp);

            // Left Arm
            leftShoulder.localRotation = Utils.GetQuaternionFromRightUp(-Utils.GetBoneDirection(leftShoulderJoint.Value, spineShoulderJoint.Value), spineUp);
            leftElbow.localRotation = Utils.GetQuaternionFromRightUp(-Utils.GetBoneDirection(leftWristJoint.Value, leftElbowJoint.Value), spineUp);
            leftArm.localRotation = Utils.GetQuaternionFromRightUp(-Utils.GetBoneDirection(leftElbowJoint.Value, leftShoulderJoint.Value), spineUp);

            // Left Leg
            leftHip.localRotation = Utils.GetQuaternionFromUpRight(-Utils.GetBoneDirection(leftKneeJoint.Value, leftHipJoint.Value), spineRight);
            leftKnee.localRotation = Utils.GetQuaternionFromUpRight(-Utils.GetBoneDirection(leftAnkleJoint.Value, leftKneeJoint.Value), spineRight);

            // Right Arm
            rightShoulder.localRotation = Utils.GetQuaternionFromRightUp(Utils.GetBoneDirection(rightShoulderJoint.Value, spineShoulderJoint.Value), spineUp);
            rightElbow.localRotation = Utils.GetQuaternionFromRightUp(Utils.GetBoneDirection(rightWristJoint.Value, rightElbowJoint.Value), spineUp);
            rightArm.localRotation = Utils.GetQuaternionFromRightUp(Utils.GetBoneDirection(rightElbowJoint.Value, rightShoulderJoint.Value), spineUp);

            // Right Leg
            rightHip.localRotation = Utils.GetQuaternionFromUpRight(-Utils.GetBoneDirection(rightKneeJoint.Value, rightHipJoint.Value), spineRight);
            rightKnee.localRotation = Utils.GetQuaternionFromUpRight(-Utils.GetBoneDirection(rightAnkleJoint.Value, rightKneeJoint.Value), spineRight);
        }

        lastTrackerForward = spineForward;
        lastCameraForward = Camera.main.transform.forward;

        //Debug.DrawRay(leftShoulder.transform.position, leftShoulder.transform.up, Color.green);
        //Debug.DrawRay(leftShoulder.transform.position, leftShoulder.transform.forward, Color.blue);
        //Debug.DrawRay(leftShoulder.transform.position, leftShoulder.transform.right, Color.red);

       /* Debug.DrawRay(rightShoulder.transform.position, rightShoulder.transform.up, Color.green);
        Debug.DrawRay(rightShoulder.transform.position, rightShoulder.transform.forward, Color.blue);
        Debug.DrawRay(rightShoulder.transform.position, rightShoulder.transform.right, Color.red);*/
    }

    /// <summary>
    /// Applies the noise filter to joints.
    /// </summary>
    private void ApplyFilterToJoints()
	{
        // testes para inverter
        bool invert = false;

        Vector3 headRot = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z); ; //UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.CenterEye).eulerAngles;

        // Spine
        Vector3 spineUp = Utils.GetBoneDirection(trackedHuman.body.Joints[BodyJointType.spineShoulder], trackedHuman.body.Joints[BodyJointType.spineBase]);
        Vector3 spineRight = Utils.GetBoneDirection(trackedHuman.body.Joints[BodyJointType.rightShoulder], trackedHuman.body.Joints[BodyJointType.leftShoulder]);
        Vector3 spineForward = Vector3.Cross(spineRight, spineUp);
        Vector3 projForward = new Vector3(spineForward.x, 0, spineForward.z);

        //float dotprod = Vector3.Dot(projForward, headRot);

        //if (dotprod < rotationTreshold)
        //    invert = true;

        // fim testes para inverter

        // Spine
        spineBaseJoint.ApplyFilter(trackedHuman.body.Joints[BodyJointType.spineBase], isNewFrame, frameTime);
		spineShoulderJoint.ApplyFilter(trackedHuman.body.Joints[BodyJointType.spineShoulder], isNewFrame, frameTime);

        // Left arm
        leftShoulderJoint.ApplyFilter(trackedHuman.body.Joints[BodyJointType.leftShoulder], isNewFrame, frameTime);
        leftElbowJoint.ApplyFilter(trackedHuman.body.Joints[BodyJointType.leftElbow], isNewFrame, frameTime);
        leftWristJoint.ApplyFilter(trackedHuman.body.Joints[BodyJointType.leftWrist], isNewFrame, frameTime);

        // Right arm
        rightShoulderJoint.ApplyFilter(trackedHuman.body.Joints[BodyJointType.rightShoulder], isNewFrame, frameTime);
        rightElbowJoint.ApplyFilter(trackedHuman.body.Joints[BodyJointType.rightElbow], isNewFrame, frameTime);
        rightWristJoint.ApplyFilter(trackedHuman.body.Joints[BodyJointType.rightWrist], isNewFrame, frameTime);

		// Left leg
		leftHipJoint.ApplyFilter(trackedHuman.body.Joints[BodyJointType.leftHip], isNewFrame, frameTime);
		leftKneeJoint.ApplyFilter(trackedHuman.body.Joints[BodyJointType.leftKnee], isNewFrame, frameTime);
		leftAnkleJoint.ApplyFilter(trackedHuman.body.Joints[BodyJointType.leftAnkle], isNewFrame, frameTime);

		
		// Right leg
		rightHipJoint.ApplyFilter(trackedHuman.body.Joints[BodyJointType.rightHip], isNewFrame, frameTime);
		rightKneeJoint.ApplyFilter(trackedHuman.body.Joints[BodyJointType.rightKnee], isNewFrame, frameTime);
		rightAnkleJoint.ApplyFilter(trackedHuman.body.Joints[BodyJointType.rightAnkle], isNewFrame, frameTime);
	}

	/// <summary>
	/// Updates frame with new body data if any.
	/// </summary>
	public void SetNewFrame(Body[] bodies)
	{
		isNewFrame = true;
		frameTime = DateTime.Now;

		foreach (Body b in bodies) 
		{
            try
            {  
				string bodyID = b.Properties[BodyPropertiesType.UID];
				if (!humans.ContainsKey(bodyID)) 
				{
					humans.Add(bodyID, new Human());
				}
				humans[bodyID].Update(b);
			} 
			catch (Exception e) 
			{
				Debug.LogError("[TrackerClient] ERROR: " + e.StackTrace);
			}
		}
	}

	void CleanDeadHumans()
	{
		List<Human> deadhumans = new List<Human>();

		foreach (Human h in humans.Values) 
		{
			if (DateTime.Now > h.lastUpdated.AddMilliseconds(1000))
			{
				deadhumans.Add(h);
			}
		}
		foreach (Human h in deadhumans) 
		{
			humans.Remove(h.id);
		}
	}
}