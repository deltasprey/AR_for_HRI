using UnityEngine;
#if WINDOWS_UWP
using Windows.Perception.Spatial;
using Microsoft.MixedReality.OpenXR;
#endif
using Microsoft.MixedReality.Toolkit.Utilities;

namespace QRTracking {
    public class SpatialGraphCoordinateSystem : MonoBehaviour {
#if WINDOWS_UWP
        private SpatialCoordinateSystem CoordinateSystem = null;
#endif
        private Quaternion rotation;
        private Vector3 translation;
        private Pose prevPose;
        private System.Guid id;
        
        public System.Guid Id {
            get {
                return id;
            }

            set {
                id = value;
#if WINDOWS_UWP
                CoordinateSystem = Windows.Perception.Spatial.Preview.SpatialGraphInteropPreview.CreateCoordinateSystemForNode(id);
                if (CoordinateSystem == null)
                {
                    Debug.Log("Id= " + id + " Failed to acquire coordinate system");
                }
#endif
            }
        }

        // Use this for initialization
        void Start() {
            prevPose = new(Vector3.zero, Quaternion.identity);
#if WINDOWS_UWP
            if (CoordinateSystem == null) {
                CoordinateSystem = Windows.Perception.Spatial.Preview.SpatialGraphInteropPreview.CreateCoordinateSystemForNode(id);
                if (CoordinateSystem == null) {
                    Debug.Log("Id= " + id + " Failed to acquire coordinate system");
                }
            }
#endif
        }

        private void UpdateLocation() {
            {
#if WINDOWS_UWP
                if (CoordinateSystem == null) {
                    CoordinateSystem = Windows.Perception.Spatial.Preview.SpatialGraphInteropPreview.CreateCoordinateSystemForNode(id);

                    if (CoordinateSystem == null) {
                        Debug.Log("Id= " + id + " Failed to acquire coordinate system");
                    }
                }

                if (CoordinateSystem != null) {
                    //System.IntPtr rootCoordnateSystemPtr = UnityEngine.XR.WindowsMR.WindowsMREnvironment.OriginSpatialCoordinateSystem;
                    //SpatialCoordinateSystem rootSpatialCoordinateSystem = (SpatialCoordinateSystem)System.Runtime.InteropServices.Marshal.GetObjectForIUnknown(rootCoordnateSystemPtr);
                    SpatialCoordinateSystem rootSpatialCoordinateSystem = PerceptionInterop.GetSceneCoordinateSystem(UnityEngine.Pose.identity) as SpatialCoordinateSystem;

                    // Get the relative transform from the unity origin
                    System.Numerics.Matrix4x4? relativePose = CoordinateSystem.TryGetTransformTo(rootSpatialCoordinateSystem);

                    if (relativePose != null) {
                        System.Numerics.Vector3 scale;
                        System.Numerics.Quaternion rotation1;
                        System.Numerics.Vector3 translation1;

                        System.Numerics.Matrix4x4 newMatrix = relativePose.Value;

                        // Platform coordinates are all right handed and unity uses left handed matrices. so we convert the matrix
                        // from rhs-rhs to lhs-lhs 
                        // Convert from right to left coordinate system
                        newMatrix.M13 = -newMatrix.M13;
                        newMatrix.M23 = -newMatrix.M23;
                        newMatrix.M43 = -newMatrix.M43;

                        newMatrix.M31 = -newMatrix.M31;
                        newMatrix.M32 = -newMatrix.M32;
                        newMatrix.M34 = -newMatrix.M34;

                        System.Numerics.Matrix4x4.Decompose(newMatrix, out scale, out rotation1, out translation1);
                        translation = new Vector3(translation1.X, translation1.Y, translation1.Z);
                        rotation = new Quaternion(rotation1.X, rotation1.Y, rotation1.Z, rotation1.W);
                        Pose pose = new(translation, rotation);

                        // If there is a parent to the camera that means we are using teleport and we should not apply the teleport
                        // to these objects so apply the inverse
                        if (CameraCache.Main.transform.parent != null) {
                            pose = pose.GetTransformedBy(CameraCache.Main.transform.parent);
                        }

                        if ((pose.position - prevPose.position).magnitude > 0.01 || (pose.rotation.eulerAngles - prevPose.rotation.eulerAngles).magnitude > 1) {
                            transform.SetPositionAndRotation(pose.position, pose.rotation);
                        }
                        //Debug.Log("Id= " + id + " QRPose = " +  pose.position.ToString("F7") + " QRRot = "  +  pose.rotation.ToString("F7"));
                        prevPose = pose;
                    } else {
                        // Debug.Log("Id= " + id + " Unable to locate qrcode" );
                    }
                } else {
                    gameObject.SetActive(false);
                }
#endif
            }
        }

        // Update is called once per frame
        void Update() {
            UpdateLocation();
        }
    }
}