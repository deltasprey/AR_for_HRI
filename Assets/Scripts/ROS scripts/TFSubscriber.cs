using RosSharp;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Messages.Geometry;
using UnityEngine;

public class TFSubscriber : MonoBehaviour {
    public string botSubscribeTopic = "/tf";

    private float x = 0, z = 0, theta = 0;
    private float oldX, oldZ, oldTheta;
    private bool initialised = false, offsetted = false;
    private RosSocket rosSocket;
    private UnityEngine.Vector3 translation;
    private UnityEngine.Quaternion rotation;
    
    void Start() {
        rosSocket = GetComponent<RosConnector>().RosSocket;
        if (botSubscribeTopic != "") {
            // Subscribe to the ROS topic using the custom subscription message script TurtlePose
            rosSocket.Subscribe<TransformStamped>(botSubscribeTopic, poseCallback);
        }
    }
    
    // Update is called once per frame
    void Update() {
        if (initialised && offsetted) {
            if (x != oldX || z != oldZ || theta != oldTheta) {
                // Update GameObject transform
                oldX = x;
                oldZ = z;
                oldTheta = theta;
                //Debug.Log($"{position.x}, {position.y}, {position.z}");
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

    // Called when a pose message is recevied from the subscribed ROS topic
    private void poseCallback(TransformStamped msg) {
        //Debug.Log($"{msg.x}, {msg.y}, {msg.theta}, {msg.linear_velocity}, {msg.angular_velocity}");
        //print(initialised);
        translation = TransformExtensions.Ros2Unity(msg.transform.translation);
        rotation = TransformExtensions.Ros2Unity(msg.transform.rotation);
        x = translation.x;
        z = translation.z;
        theta = rotation.eulerAngles.y;
        if (!initialised) initialised = true;
    }
}