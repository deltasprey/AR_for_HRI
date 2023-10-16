using UnityEngine;

namespace RosSharp.RosBridgeClient {
    public class ExtOdometrySubscriber : Subscriber<Messages.Navigation.Odometry> {
        public Vector3 position { get; private set; }
        public Quaternion rotation { get; private set; }
        public Vector3 linearVelocity { get; private set; }
        public Vector3 angularVelocity { get; private set; }

        public delegate void MsgReceived();
        public static event MsgReceived msgValueChanged;

        private bool isMessageReceived;
        
        private void Update() {
            if (isMessageReceived) ProcessMessage();
        }

        protected override void ReceiveMessage(Messages.Navigation.Odometry message) {
            position = GetPosition(message).Ros2Unity();
            rotation = GetRotation(message).Ros2Unity();
            linearVelocity = GetLinearVelocity(message).Ros2Unity();
            angularVelocity = GetAngularVelocity(message).Ros2Unity();
            isMessageReceived = true;
        }

        private void ProcessMessage() {
            msgValueChanged?.Invoke();
            isMessageReceived = true;
        }

        private Vector3 GetPosition(Messages.Navigation.Odometry message) {
            return new Vector3(
                message.pose.pose.position.x,
                message.pose.pose.position.y,
                message.pose.pose.position.z);
        }

        private Quaternion GetRotation(Messages.Navigation.Odometry message) {
            return new Quaternion(
                message.pose.pose.orientation.x,
                message.pose.pose.orientation.y,
                message.pose.pose.orientation.z,
                message.pose.pose.orientation.w);
        }

        private Vector3 GetLinearVelocity(Messages.Navigation.Odometry message) {
            return new Vector3(
                message.twist.twist.linear.x,
                message.twist.twist.linear.y,
                message.twist.twist.linear.z);
        }

        private Vector3 GetAngularVelocity(Messages.Navigation.Odometry message) {
            return new Vector3(
                message.twist.twist.angular.x,
                message.twist.twist.angular.y,
                message.twist.twist.angular.z);
        }
    }
}