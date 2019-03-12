//------------------------------------------------------------------------------
// <copyright file="DoubleExponentialFilter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
public class MyDoubleExponentialFilter
{
    public class FilteredTransform
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public FilteredTransform()
        {
            Position = Vector3.zero;
            Rotation = Quaternion.identity;
        }

        public FilteredTransform(Vector3 position, Quaternion rotation)
        {
            this.Position = position;
            this.Rotation = rotation;
        }

        public void Reset()
        {
            Position = Vector3.zero;
            Rotation = Quaternion.identity;
        }

        internal static FilteredTransform Add(FilteredTransform joint, FilteredTransform prevJoint)
        {
            FilteredTransform op = new FilteredTransform();

            op.Position = joint.Position + prevJoint.Position;
            op.Rotation = joint.Rotation * prevJoint.Rotation;

            return op;
        }

        internal static FilteredTransform Subtract(FilteredTransform joint, FilteredTransform prevJoint)
        {
            FilteredTransform op = new FilteredTransform();

            op.Position = joint.Position - prevJoint.Position;
            op.Rotation = joint.Rotation * Quaternion.Inverse(prevJoint.Rotation);

            return op;
        }

        internal static FilteredTransform Scale(FilteredTransform joint, float scale)
        {
            FilteredTransform op = new FilteredTransform();

            op.Position = joint.Position * scale;
            op.Rotation = Quaternion.Slerp(Quaternion.identity, joint.Rotation, scale);

            return op;
        }
    }

    public struct TRANSFORM_SMOOTH_PARAMETERS
    {
        public float fSmoothing;             // [0..1], lower values closer to raw data
        public float fCorrection;            // [0..1], lower values slower to correct towards the raw data
        public float fPrediction;            // [0..n], the number of frames to predict into the future
        public float fJitterRadius;          // The radius in meters for jitter reduction
        public float fMaxDeviationRadius;    // The maximum radius in meters that filtered positions are allowed to deviate from raw data
    }

    public class DoubleExponentialFilterData
    {
        public FilteredTransform RawJoint;
        public FilteredTransform FilteredJoint;
        public FilteredTransform Trend;

        public int FrameCount;
    }

    // Holt Double Exponential Smoothing filter
    private FilteredTransform filteredTransform;
    private  DoubleExponentialFilterData history;

    private TRANSFORM_SMOOTH_PARAMETERS smoothParams;
    public TRANSFORM_SMOOTH_PARAMETERS SmoothingParameters
    {
        get { return this.smoothParams; }
        set
        {
            if (!value.Equals(this.smoothParams))
            {
                this.smoothParams = value;
            }
        }
    }

    public MyDoubleExponentialFilter()
    {
        filteredTransform = new FilteredTransform();
        history = new DoubleExponentialFilterData();
        Init();
    }

    public void SetFilterValues(float fSmoothing = 0.25f, float fCorrection = 0.25f, float fPrediction = 0.25f, float fJitterRadius = 0.03f, float fMaxDeviationRadius = 0.05f)
    {
        smoothParams.fCorrection = fCorrection;                     // How much to correct back from prediction.  Can make things springy
        smoothParams.fJitterRadius = fJitterRadius;                 // Size of the radius where jitter is removed. Can do too much smoothing when too high
        smoothParams.fMaxDeviationRadius = fMaxDeviationRadius;     // Size of the max prediction radius Can snap back to noisy data when too high
        smoothParams.fPrediction = fPrediction;                     // Amount of prediction into the future to use. Can over shoot when too high
        smoothParams.fSmoothing = fSmoothing;
    }

        public void Init(float fSmoothing = 0.25f, float fCorrection = 0.25f, float fPrediction = 0.25f, float fJitterRadius = 0.03f, float fMaxDeviationRadius = 0.05f)
    {
        if (filteredTransform == null || history == null)
        {
            return;
        }

        // Check for divide by zero. Use an epsilon of a 10th of a millimeter
        fJitterRadius = Mathf.Max(0.0001f, fJitterRadius);

        smoothParams.fCorrection = fCorrection;                     // How much to correct back from prediction.  Can make things springy
        smoothParams.fJitterRadius = fJitterRadius;                 // Size of the radius where jitter is removed. Can do too much smoothing when too high
        smoothParams.fMaxDeviationRadius = fMaxDeviationRadius;     // Size of the max prediction radius Can snap back to noisy data when too high
        smoothParams.fPrediction = fPrediction;                     // Amount of prediction into the future to use. Can over shoot when too high
        smoothParams.fSmoothing = fSmoothing;                       // How much smothing will occur.  Will lag when too high

        filteredTransform.Reset();
        history.RawJoint = new FilteredTransform();
        history.FilteredJoint = new FilteredTransform();
        history.Trend = new FilteredTransform();
        history.FrameCount = 0;
    }
 

    //--------------------------------------------------------------------------------------
    // Implementation of a Holt Double Exponential Smoothing filter. The double exponential
    // smooths the curve and predicts.  There is also noise jitter removal. And maximum
    // prediction bounds.  The paramaters are commented in the init function.
    //--------------------------------------------------------------------------------------
    public void UpdateValue(Vector3 position, Quaternion rotation)
    {
      
        // Check for divide by zero. Use an epsilon of a 10th of a millimeter
        smoothParams.fJitterRadius = Mathf.Max(0.0001f, smoothParams.fJitterRadius);

        TRANSFORM_SMOOTH_PARAMETERS smoothingParams;

        
        smoothingParams.fSmoothing = smoothParams.fSmoothing;
        smoothingParams.fCorrection = smoothParams.fCorrection;
        smoothingParams.fPrediction = smoothParams.fPrediction;
        smoothingParams.fJitterRadius = smoothParams.fJitterRadius;
        smoothingParams.fMaxDeviationRadius = smoothParams.fMaxDeviationRadius;

        FilteredTransform fj = new FilteredTransform(position,rotation);
        Update( fj, smoothingParams);
        
    }

    private void Update(FilteredTransform rawJoint, TRANSFORM_SMOOTH_PARAMETERS smoothingParams)
    {
        FilteredTransform prevFilteredJoint;
        FilteredTransform prevRawJoint;
        FilteredTransform prevTrend;

        FilteredTransform predictedJoint;
        FilteredTransform diff;
        FilteredTransform trend;
        float fDiff;

        prevFilteredJoint = history.FilteredJoint;
        prevTrend = history.Trend;
        prevRawJoint = history.RawJoint;
        
        // initial start values
        if (history.FrameCount == 0)
        {
            filteredTransform = rawJoint;
            trend = new FilteredTransform();
            history.FrameCount++;
        }
        else if (history.FrameCount == 1)
        {
            filteredTransform = FilteredTransform.Scale(FilteredTransform.Add(rawJoint, prevRawJoint), 0.5f);
            diff = FilteredTransform.Subtract(filteredTransform, prevFilteredJoint);
            trend = FilteredTransform.Add(FilteredTransform.Scale(diff, smoothingParams.fCorrection), FilteredTransform.Scale(prevTrend, 1.0f - smoothingParams.fCorrection));
            history.FrameCount++;
        }
        else
        {
            // First apply jitter filter
            diff = FilteredTransform.Subtract(rawJoint, prevFilteredJoint);
            fDiff = diff.Position.magnitude;

            if (fDiff <= smoothingParams.fJitterRadius)
            {
                filteredTransform = FilteredTransform.Add(FilteredTransform.Scale(rawJoint, fDiff / smoothingParams.fJitterRadius),
                    FilteredTransform.Scale(prevFilteredJoint, 1.0f - fDiff / smoothingParams.fJitterRadius));
            }
            else
            {
                filteredTransform = rawJoint;
            }

            // Now the double exponential smoothing filter
            filteredTransform = FilteredTransform.Add(FilteredTransform.Scale(filteredTransform, 1.0f - smoothingParams.fSmoothing),
                FilteredTransform.Scale(FilteredTransform.Add(prevFilteredJoint, prevTrend), smoothingParams.fSmoothing));


            diff = FilteredTransform.Subtract(filteredTransform, prevFilteredJoint);
            trend = FilteredTransform.Add(FilteredTransform.Scale(diff, smoothingParams.fCorrection), FilteredTransform.Scale(prevTrend, 1.0f - smoothingParams.fCorrection));
        }

        // Predict into the future to reduce latency
        predictedJoint = FilteredTransform.Add(filteredTransform, FilteredTransform.Scale(trend, smoothingParams.fPrediction));

        // Check that we are not too far away from raw data
        diff = FilteredTransform.Subtract(predictedJoint, rawJoint);
        fDiff = diff.Position.magnitude;

        if (fDiff > smoothingParams.fMaxDeviationRadius)
        {
            predictedJoint = FilteredTransform.Add(FilteredTransform.Scale(predictedJoint, smoothingParams.fMaxDeviationRadius / fDiff),
                FilteredTransform.Scale(rawJoint, 1.0f - smoothingParams.fMaxDeviationRadius / fDiff));
        }

        // Save the data from this frame
        history.RawJoint = rawJoint;
        history.FilteredJoint = filteredTransform;
        history.Trend = trend;

        // Output the data
        filteredTransform = predictedJoint;
    }

    public FilteredTransform GetFilteredTransform()
    {
        return filteredTransform;
    }

    
}

