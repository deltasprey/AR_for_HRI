using RosSharp.RosBridgeClient.Messages.Geometry;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient {
    public class TFSubscriber : Subscriber<Messages.TFMessage> {
        public float x { get; private set; } = 0;
        public float z { get; private set; } = 0;
        public float theta { get; private set; } = 0;

        public delegate void MsgReceived();
        public static event MsgReceived initialisedEvent;

        [SerializeField] private string frameToTrack = "odom"; // The name of the child frame to track

        private bool initialised = false;
        private TransformStamped tf;

        protected override void ReceiveMessage(Messages.TFMessage message) {
            // Find the desired transform in the TF message
            tf = FindTransform(frameToTrack, message.transforms);
            
            if (tf != null) {
                // Extract translation and rotation data
                x = tf.transform.translation.z;
                z = tf.transform.translation.x;
                theta = -tf.transform.rotation.eulerAngles.z;
                if (!initialised && initialisedEvent != null) {
                    initialised = true;
                    initialisedEvent.Invoke();
                }
            }
        }

        private TransformStamped FindTransform(string childFrameId, List<TransformStamped> transforms) {
            foreach (var tf in transforms)
                if (tf.header.frame_id == childFrameId) return tf;
            return null;
        }
    }
}
