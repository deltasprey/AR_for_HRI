using Newtonsoft.Json;
using RosSharp.RosBridgeClient.Messages.Geometry;
using RosSharp.RosBridgeClient.Messages.Standard;
using System.Collections.Generic;

namespace RosSharp.RosBridgeClient.Messages.Geometry {
    public class Transform : Message {
        [JsonIgnore]
        public const string RosMessageName = "geometry_msgs/Transform";

        public UnityEngine.Vector3 translation;
        public UnityEngine.Quaternion rotation;

        public Transform() {
            translation = new UnityEngine.Vector3();
            rotation = new UnityEngine.Quaternion();
        }
    }

    public class TransformStamped : Message {
        [JsonIgnore]
        public const string RosMessageName = "geometry_msgs/TransformStamped";

        public Header header;
        public string child_frame_id;
        public Transform transform;

        public TransformStamped() {
            header = new Header();
            child_frame_id = "";
            transform = new Transform();
        }
    }
}

namespace RosSharp.RosBridgeClient.Messages {
    public class TFMessage : Message {
        [JsonIgnore]
        public const string RosMessageName = "tf2_msgs/TFMessage";

        public List<TransformStamped> transforms;

        public TFMessage() {
            transforms = new List<TransformStamped>();
        }
    }
}