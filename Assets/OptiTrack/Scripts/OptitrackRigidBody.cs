//======================================================================================================
// Copyright 2016, NaturalPoint Inc.
//======================================================================================================

using System;
using UnityEngine;


public class OptitrackRigidBody : MonoBehaviour
{
    public OptitrackStreamingClient StreamingClient;
    public Int32 RigidBodyId;

    public MyDoubleExponentialFilter trans;

    public bool Filtering = false;

    [Range(0f, 1f)]
    public float fSmoothing = 0.25f;            // [0..1], lower values closer to raw data
    [Range(0f, 1f)]
    public float fCorrection = 0.25f;           // [0..1], lower values slower to correct towards the raw data
    [Range(0f, 1f)]
    public float fPrediction = 0.25f;           // [0..n], the number of frames to predict into the future
    [Range(0f, 1f)]
    public float fJitterRadius = 0.03f;          // The radius in meters for jitter reduction
    [Range(0f, 1f)]
    public float fMaxDeviationRadius = 0.01f;


    void Start()
    {
        trans = new MyDoubleExponentialFilter();

        // If the user didn't explicitly associate a client, find a suitable default.
        if ( this.StreamingClient == null )
        {
            this.StreamingClient = OptitrackStreamingClient.FindDefaultClient();

            // If we still couldn't find one, disable this component.
            if ( this.StreamingClient == null )
            {
                Debug.LogError( GetType().FullName + ": Streaming client not set, and no " + typeof( OptitrackStreamingClient ).FullName + " components found in scene; disabling this component.", this );
                this.enabled = false;
                return;
            }
        }
    }


#if UNITY_2017_1_OR_NEWER
    void OnEnable()
    {
        Application.onBeforeRender += OnBeforeRender;
    }


    void OnDisable()
    {
        Application.onBeforeRender -= OnBeforeRender;
    }


    void OnBeforeRender()
    {
        UpdatePose();
    }
#endif


    void Update()
    {
        trans.SetFilterValues(fSmoothing, fCorrection, fPrediction, fJitterRadius, fMaxDeviationRadius);
        UpdatePose();
    }


    void UpdatePose()
    {
        OptitrackRigidBodyState rbState = StreamingClient.GetLatestRigidBodyState( RigidBodyId );
        if ( rbState != null )
        {
            trans.UpdateValue(rbState.Pose.Position, rbState.Pose.Orientation);

            this.transform.localPosition = Filtering ? trans.GetFilteredTransform().Position : rbState.Pose.Position;
            this.transform.localRotation = Filtering ? trans.GetFilteredTransform().Rotation : rbState.Pose.Orientation;
        }
    }
}
