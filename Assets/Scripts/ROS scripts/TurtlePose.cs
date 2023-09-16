// Custom ROS topic message subscriber

using Newtonsoft.Json;

namespace RosSharp.RosBridgeClient.Messages.Geometry {
    public class TurtlePose : Message {
        [JsonIgnore]
        public const string RosMessageName = "turtlesim/Pose";

        public float x;
        public float y;
        public float theta;
        public float linear_velocity;
        public float angular_velocity;

        public TurtlePose() {
            x = 0;
            y = 0;
            theta = 0;
            linear_velocity = 0;
            angular_velocity = 0;
        }
    }
}