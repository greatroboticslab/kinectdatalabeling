using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Newtonsoft.Json;
using VirginiaTech.Fuse.Util;

namespace VirginiaTech.Fuse.Util
{
    enum FootState
    {
        NullFoot,
        LeftFoot,
        RightFoot,
    }

    public class StepCounterMachineLearning
    {
        private readonly double[] rightVector;
        private readonly double[] leftVector;
        private FootState state = FootState.NullFoot;
        private int steps = 0;
        private readonly List<Tuple<JointType, JointType>> relevantJointPairs;

        public StepCounterMachineLearning(double[] rightVector, double[] leftVector)
        {
            this.rightVector = rightVector;
            this.leftVector = leftVector;
            this.relevantJointPairs = retrieveRelevantJointPairs();
        }

        private static List<Tuple<JointType, JointType>> retrieveRelevantJointPairs()
        {
            List<JointType> relevantJoints;
            using (var writer = new StreamReader(Util.FileControl.RelevantJointsFile))
            {
                relevantJoints = JsonConvert.DeserializeObject<List<JointType>>(writer.ReadToEnd());
            }
            var relevantJointPairs = new List<Tuple<JointType, JointType>>();
            foreach (var jointOne in relevantJoints)
            {
                foreach (var jointTwo in relevantJoints)
                {
                    if (jointOne < jointTwo)
                    {
                        relevantJointPairs.Add(Tuple.Create(jointOne, jointTwo));
                    }
                }
            }
            return relevantJointPairs;
        }

        private static bool isMatch(Skeleton skeleton, double[] internalVector,List<Tuple<JointType, JointType>> relevantJointPairs)
        {
            double[] skeletonVector = new double[relevantJointPairs.Count()];
            for (int i = 0; i < relevantJointPairs.Count(); i++ )
            {
                var jointPair = relevantJointPairs[i];
                var firstJointType = jointPair.Item1;
                var otherJointType = jointPair.Item2;
                var firstVector = ExperimentUtil.skeletonPointToVector(skeleton.Joints[firstJointType].Position);
                var otherVector = ExperimentUtil.skeletonPointToVector(skeleton.Joints[otherJointType].Position);
                skeletonVector[i] = firstVector.subtract(otherVector).magnitude();
            }

            double score = 0;
            for (int i = 0; i < internalVector.Length; i++)
            {
                score += skeletonVector[i] * internalVector[i];
            }
            return score > 0;
        }

        private bool isRight(Skeleton skeleton)
        {
            return isMatch(skeleton, rightVector, relevantJointPairs);
        }

        private bool isLeft(Skeleton skeleton)
        {
            return isMatch(skeleton, leftVector, relevantJointPairs);
        }

        private bool isOnlyRight(Skeleton skeleton)
        {
            return isRight(skeleton) && !isLeft(skeleton);
        }

        private bool isOnlyLeft(Skeleton skeleton)
        {
            return isLeft(skeleton) && !isRight(skeleton);
        }

        public int count()
        {
            return steps;
        }

        public void processFrameData(object sender, SkeletonFrameReadyEventArgs readyFrame)
        {
            Skeleton[] skeletons;
            using (SkeletonFrame skeletonFrame = readyFrame.OpenSkeletonFrame())
            {
                if (skeletonFrame == null)
                {
                    return; // Have to exit if it fails.
                }
                skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength]; //TODO This breaks the GC.
                skeletonFrame.CopySkeletonDataTo(skeletons);
            }
            foreach (Skeleton skeleton in skeletons)
            {
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    if ((state == FootState.RightFoot || state == FootState.NullFoot) && isOnlyLeft(skeleton))
                    {
                        state = FootState.LeftFoot;
                        steps++;
                    }
                    else if ((state == FootState.LeftFoot || state == FootState.NullFoot) && isOnlyRight(skeleton))
                    {
                        state = FootState.RightFoot;
                        steps++;
                    }
                }
            }
        }


    }

    public class StepCounter
    {
        private static readonly double footHeightThreshold = 0.75; // Emperical measurement in meters.

        private FootState state = FootState.NullFoot;
        private int steps = 0;

        public StepCounter()
        {

        }

        public int count()
        {
            return steps;
        }

        private bool leftUp(Skeleton skeleton)
        {
            Joint footJoint = skeleton.Joints[JointType.FootLeft];
            Joint hipJoint = skeleton.Joints[JointType.HipLeft];
            if (footJoint.TrackingState != JointTrackingState.Tracked || footJoint.TrackingState != JointTrackingState.Tracked)
            {
                return false;
            }
            Vector3 footVec = ExperimentUtil.skeletonPointToVector(footJoint.Position).dropZ();
            Vector3 hipVec = ExperimentUtil.skeletonPointToVector(hipJoint.Position).dropZ();
            return footVec.subtract(hipVec).magnitude() < footHeightThreshold;
        }

        private bool rightUp(Skeleton skeleton)
        {
            Joint footJoint = skeleton.Joints[JointType.FootRight];
            Joint hipJoint = skeleton.Joints[JointType.HipRight];
            if (footJoint.TrackingState != JointTrackingState.Tracked || footJoint.TrackingState != JointTrackingState.Tracked)
            {
                return false;
            }
            Vector3 footVec = ExperimentUtil.skeletonPointToVector(footJoint.Position).dropZ();
            Vector3 hipVec = ExperimentUtil.skeletonPointToVector(hipJoint.Position).dropZ();
            return footVec.subtract(hipVec).magnitude() < footHeightThreshold;
        }

        private bool onlyLeftUp(Skeleton skeleton)
        {
            return leftUp(skeleton) && !rightUp(skeleton);
        }

        private bool onlyRightUp(Skeleton skeleton)
        {
            return rightUp(skeleton) && !leftUp(skeleton);
        }

        private bool isClose(Skeleton skeleton)
        {
            Vector3 vec = ExperimentUtil.skeletonPointToVector(skeleton.Joints[JointType.HipCenter].Position);
            return vec.dropX().dropY().magnitude() < 1.5; // Check if center of mass is within 1.5 meters of sensor.
        }

        public void processFrameData(object sender, SkeletonFrameReadyEventArgs readyFrame)
        {
            Skeleton[] skeletons;
            using (SkeletonFrame skeletonFrame = readyFrame.OpenSkeletonFrame())
            {
                if (skeletonFrame == null)
                {
                    return; // Have to exit if it fails.
                }
                skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength]; //TODO This breaks the GC.
                skeletonFrame.CopySkeletonDataTo(skeletons);
            }
            foreach (Skeleton skeleton in skeletons)
            {
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked && !isClose(skeleton))
                {
                    switch (state)
                    {
                        case FootState.NullFoot:
                            if (onlyLeftUp(skeleton))
                            {
                                state = FootState.LeftFoot;
                            }
                            else if (onlyRightUp(skeleton))
                            {
                                state = FootState.RightFoot;
                            }
                            break;

                        case FootState.LeftFoot:
                            if (onlyRightUp(skeleton))
                            {
                                steps++;
                                state = FootState.RightFoot;
                            }
                            break;

                        case FootState.RightFoot:
                            if (onlyLeftUp(skeleton))
                            {
                                steps++;
                                state = FootState.LeftFoot;
                            }
                            break;
                    }
                }
            }
        }
        
    }
}
