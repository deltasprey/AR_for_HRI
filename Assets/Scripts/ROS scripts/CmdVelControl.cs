using UnityEngine;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Messages.Geometry;
using System.Collections;

public class CmdVelControl : MonoBehaviour {
    public JoystickControl joystick;
    public PurePursuit purePursuit;
    public Transform bot;
    public float linearSpeed = 1f, turnSpeed = 1f;
    public bool stopOnLoad = true;

    public delegate void MsgReceived(float x, float z, float theta);
    public static event MsgReceived msgValueChanged;

    private string botCommandTopic = "/turtle1/cmd_vel";
    private float x = 0, z = 0, theta = 0, offsetTheta;
    private float oldX, oldZ, oldTheta;
    private float forwardSpeed = 0, strafeSpeed = 0, angularSpeed = 0;
    private bool isGrabbed = false, initialised = false, offsetted = false, stop = true;
    private Twist twistMessage;
    private RosSocket rosSocket;
    private ExtOdometrySubscriber odomRef;
    private UnityEngine.Vector3 position;
    private UnityEngine.Quaternion rotation;
    private Matrix4x4 transformationMatrix;

    private void Start() {
        Invoke(nameof(setStopVar), 0.2f);

        // ROS initialisation
        botCommandTopic = GetComponent<TwistPublisher>().Topic;
        rosSocket = GetComponent<RosConnector>().RosSocket;
        odomRef = GetComponent<ExtOdometrySubscriber>();
        twistMessage = new Twist();
    }

    private void OnEnable() {
        ExtOdometrySubscriber.msgValueChanged += poseUpdated;
        SpeechManager.stop += stopCmds;
        SpeechManager.AddListener("restart", restartCmds);
    }

    private void OnDisable() {
        ExtOdometrySubscriber.msgValueChanged -= poseUpdated;
        SpeechManager.stop -= stopCmds;
        SpeechManager.RemoveListener("restart", restartCmds);
    }

    private void Update() {
        if (!stop) {
            // Detect joystick input
            if (joystick.isGrabbed) {
                forwardSpeed = joystick.rotation.x * linearSpeed;
                angularSpeed = joystick.rotation.y * turnSpeed;
                isGrabbed = true;
            } else if (purePursuit.navigating) {
                forwardSpeed = purePursuit.forward * linearSpeed;
                angularSpeed = purePursuit.turn * turnSpeed;
                isGrabbed = true;
            } else if (isGrabbed) {
                forwardSpeed = 0;
                angularSpeed = 0;
                isGrabbed = false;
            }

            // Set linear and angular velocities in Twist message
            twistMessage.linear.x = forwardSpeed;
            twistMessage.linear.y = -strafeSpeed;
            twistMessage.angular.z = -angularSpeed;

            // Publish the Twist message to control the robot
            rosSocket.Publish(botCommandTopic, twistMessage);

            // Update the pose of the robot in Unity
            if (initialised && offsetted) {
                if (x != oldX || z != oldZ || theta != oldTheta) {
                    // Update GameObject transform
                    rotation = UnityEngine.Quaternion.Euler(90, theta + offsetTheta, 0);
                    position = transformationMatrix.MultiplyPoint(new UnityEngine.Vector3(x, 0, z));
                    if (bot != null) bot.SetPositionAndRotation(position, rotation);

                    // Invoke pose updated event if the robot has moved (poseCallback has been called)
                    msgValueChanged?.Invoke(x, z, theta);

                    oldX = x;
                    oldZ = z;
                    oldTheta = theta;
                    //Debug.Log($"{position.x}, {position.y}, {position.z}");
                }
            } else if (initialised) {
                // Coordinate frame transform (not working)
                UnityEngine.Quaternion botRot = UnityEngine.Quaternion.Euler(0, bot.rotation.eulerAngles.y, 0);
                UnityEngine.Quaternion rotationAB = UnityEngine.Quaternion.Inverse(odomRef.rotation) * botRot;
                Matrix4x4 rotationMatrix = Matrix4x4.TRS(UnityEngine.Vector3.zero, rotationAB, UnityEngine.Vector3.one);
                transformationMatrix = Matrix4x4.Translate(bot.position - odomRef.position) * rotationMatrix;

                // Offsets and n-1 values
                offsetTheta = bot.rotation.eulerAngles.y - theta;
                oldX = x;
                oldZ = z;
                oldTheta = theta;
                offsetted = true;
            }
        }
    }

    // Set the stop parameter on application start
    private void setStopVar() {
        stop = stopOnLoad;
        joystick.stopped = stopOnLoad;
    }

    private void poseUpdated() {
        x = odomRef.position.x;
        z = odomRef.position.z;
        theta = odomRef.rotation.eulerAngles.y;
        if (!initialised) {
            initialised = true;
            msgValueChanged?.Invoke(x, z, theta);
        }
    }

    // Callback for stop commands from:
    //      Saftey zone stop event
    //      "Stop" voice command
    private void stopCmds() {
        StartCoroutine(floodPublish());
        stop = true;
        joystick.stopped = true;
    }

    // Callback for restart commands from:
    //      Saftey zone restart event
    //      "Restart" voice command
    private void restartCmds() {
        StopAllCoroutines();
        stop = false;
        joystick.stopped = false;
    }

    // Spam the ROS messages with stop messages (zero linear or angular velocity)
    private IEnumerator floodPublish() {
        Twist floodTwistMessage = new() {
            linear = new RosSharp.RosBridgeClient.Messages.Geometry.Vector3(),
            angular = new RosSharp.RosBridgeClient.Messages.Geometry.Vector3()
        };
        while (true) {
            rosSocket.Publish(botCommandTopic, floodTwistMessage);
            yield return new WaitForSeconds(0.02f);
        }
    }

    // Return private pose variables
    public (float x, float z, float theta) initPos() { 
        if (initialised) { // && offsetted) {
            //return (position.x, position.z, theta + thetaOffset);
            return (x, z, theta);
        }
        return (0, 0, 404);
        //return (1, 1, 0); // testing
    }
}