using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace VirginiaTech.Fuse.Render
{
    public class RenderFrame
    {
        private int width;
        private int height;

        private AllFramesReadyEventArgs frameData;
        private CoordinateMapper coordinateMapper;

        private const double JointThickness = 3;
        private readonly Brush centerPointBrush = Brushes.Blue;
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        private readonly Brush inferredJointBrush = Brushes.Yellow;
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// Create a new RenderFrame object based off of AllFramesReadyEventArgs object.
        /// </summary>
        /// <param name="frame">The frame data.</param>
        /// <param name="width">The width to render for.</param>
        /// <param name="height">The height to render for.</param>
        public RenderFrame(AllFramesReadyEventArgs frameData, CoordinateMapper coordinateMapper, int width, int height)
        {
            this.frameData = frameData;
            this.width = width;
            this.height = height;
            this.coordinateMapper = coordinateMapper;
        }

        /// <summary>
        /// Return a bitmap representing the frame data. Bitmap is not cached.
        /// </summary>
        /// <returns>A bitmap representing the frame data.</returns>
        public RenderTargetBitmap render()
        {
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                // The color frame and skeleton data might be null.
                // "A frame of color data in a new ColorImageFrame object, or NULL if the data is no longer available."
                // "A frame of skeleton data in a new SkeletonFrame object, or NULL if the data is no longer available."
                using (ColorImageFrame colorImageFrameData = frameData.OpenColorImageFrame())
                {
                    if (colorImageFrameData != null)
                    {
                        drawColorImage(colorImageFrameData, context);
                    }
                }
                using (SkeletonFrame skeletonFrameData = frameData.OpenSkeletonFrame())
                {
                    if (skeletonFrameData != null)
                    {
                        drawSkeletons(skeletonFrameData, context);
                    }
                }
            }
            RenderTargetBitmap bmp = new RenderTargetBitmap(width, height, 96.0, 96.0, PixelFormats.Default);
            bmp.Render(visual);
            return bmp;
        }

        private void drawColorImage(ColorImageFrame colorFrame, DrawingContext context)
        {
            byte[] colorImagePixels = new byte[colorFrame.PixelDataLength];
            colorFrame.CopyPixelDataTo(colorImagePixels);
            BitmapSource bmap = BitmapSource.Create(
              colorFrame.Width,
              colorFrame.Height,
              96, 96,
              PixelFormats.Bgr32,
              null,
              colorImagePixels,
              colorFrame.Width * colorFrame.BytesPerPixel);
            context.DrawImage(bmap, new Rect(0.0, 0.0, width, height));
        }

        private void drawSkeletons(SkeletonFrame skelFrame, DrawingContext context)
        {
            Skeleton[] skelData = new Skeleton[skelFrame.SkeletonArrayLength];
            skelFrame.CopySkeletonDataTo(skelData);
            foreach (Skeleton skel in skelData)
            {
                DrawBonesAndJoints(skel, context);
            }
        }

        /// <summary>
        /// Draw the joints and limbs of a skeleton onto the drawing context.
        /// </summary>
        /// <param name="skeleton">The skeleton to draw.</param>
        /// <param name="drawingContext">The context to draw to.</param>
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {
            // Render Torso
            DrawBones(skeleton, drawingContext,
                JointType.Head, JointType.ShoulderCenter, JointType.Spine, JointType.HipCenter);

            // Left Arm
            DrawBones(skeleton, drawingContext,
                JointType.ShoulderCenter, JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            DrawBones(skeleton, drawingContext,
                JointType.ShoulderCenter, JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight, JointType.HandRight);

            // Left Leg
            DrawBones(skeleton, drawingContext,
                JointType.HipCenter, JointType.HipLeft, JointType.KneeLeft, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            DrawBones(skeleton, drawingContext,
                JointType.HipCenter, JointType.HipRight, JointType.KneeRight, JointType.AnkleRight, JointType.FootRight);

            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, skeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }
        }

        /// <summary>
        /// Convert a skeleton point in 3D space to a 2D screen location.
        /// </summary>
        /// <param name="skelpoint">A 3D skeleton point.</param>
        /// <returns>A 2D screen point.</returns>
        private Point skeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = coordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        /// <summary>
        /// Draw a bone between two joints on the skeleton.
        /// </summary>
        /// <param name="skeleton">The skeleton the bones belong to.</param>
        /// <param name="drawingContext">The context to draw to.</param>
        /// <param name="jointType0">The first joint type.</param>
        /// <param name="jointType1">The second joint type.</param>
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                Point startPoint = skeletonPointToScreen(joint0.Position);
                Point endPoint = skeletonPointToScreen(joint1.Position);
                drawingContext.DrawLine(this.trackedBonePen, startPoint, endPoint);
            }
        }

        /// <summary>
        /// Draw a sequence of bones. This is very useful for drawing long
        /// sequences of bones.
        /// </summary>
        /// <param name="skeleton">The skeleton the bones belong to.</param>
        /// <param name="drawingContext">The context to draw to.</param>
        /// <param name="firstJointType">The first joint type.</param>
        /// <param name="jointTypes">The second joint type.</param>
        private void DrawBones(Skeleton skeleton, DrawingContext drawingContext, JointType firstJointType, params JointType[] jointTypes)
        {
            foreach (JointType jointType in jointTypes)
            {
                DrawBone(skeleton, drawingContext, firstJointType, jointType);
                firstJointType = jointType;
            }
        }
    }
}
