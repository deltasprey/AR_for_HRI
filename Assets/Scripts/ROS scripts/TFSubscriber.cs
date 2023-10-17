using RosSharp.RosBridgeClient.Messages.Geometry;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient {
    public class TFSubscriber : Subscriber<Messages.TFMessage> {
        [SerializeField] private string frameToTrack = "odom"; // The name of the child frame to track

        private float x = 0, z = 0, theta = 0;
        private float oldX, oldZ, oldTheta;
        private bool initialised = false, offsetted = false;

        private void Update() {
            if (initialised && offsetted) {
                if (x != oldX || z != oldZ || theta != oldTheta) {
                    oldX = x;
                    oldZ = z;
                    oldTheta = theta;
                    Debug.Log($"TFSubscriber x: {x}, z: {z}, theta: {theta}");
                }
            } else if (initialised) {
                // Gets the offset between the initial positions of the robot and the bot in the Unity scene
                // Does not set the rotation offset
                oldX = x;
                oldZ = z;
                oldTheta = theta;
                offsetted = true;
            }
        }

        protected override void ReceiveMessage(Messages.TFMessage message) {
            // Find the desired transform in the TF message
            TransformStamped tf = FindTransform(frameToTrack, message.transforms);

            if (tf != null) {
                // Extract translation and rotation data
                UnityEngine.Vector3 translation = new(tf.transform.translation.x, tf.transform.translation.y, tf.transform.translation.z);
                UnityEngine.Quaternion rotation = new(tf.transform.rotation.x, tf.transform.rotation.y, tf.transform.rotation.z, tf.transform.rotation.w);

                x = translation.x;
                z = translation.z;
                theta = rotation.eulerAngles.y;
                if (!initialised) initialised = true;
            }
        }

        private TransformStamped FindTransform(string childFrameId, List<TransformStamped> transforms) {
            foreach (var tf in transforms)
                if (tf.child_frame_id == childFrameId) return tf;
            return null;
        }
    }
}
