using UnityEngine;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Messages.Geometry;
using System.Collections;

public class CmdVelControl : MonoBehaviour {
    [SerializeField] private JoystickControl joystick;
    [SerializeField] private PurePursuit purePursuit;
    [SerializeField] private float linearSpeed = 1f, turnSpeed = 1f;
    [SerializeField] private bool usingTF = true, stopOnLoad = true;

    public delegate void MsgReceived(float x, float z, float theta);
    public static event MsgReceived msgValueChanged;

    private string botCommandTopic;
    private float x, z, theta, oldX, oldZ, oldTheta, forwardSpeed = 0, angularSpeed = 0;
    private bool isGrabbed = false, initialised = false, stop = true;
    private Twist twistMessage;
    private RosSocket rosSocket;
    private TFSubscriber tfRef;
    private ExtOdometrySubscriber odomRef;

    private void Start() {
        Invoke(nameof(setStopVar), 0.2f);

        // ROS initialisation
        botCommandTopic = GetComponent<TwistPublisher>().Topic;
        rosSocket = GetComponent<RosConnector>().RosSocket;
        odomRef = GetComponent<ExtOdometrySubscriber>();
        tfRef = GetComponent<TFSubscriber>();
        twistMessage = new Twist();
    }

    private void OnEnable() {
        ExtOdometrySubscriber.msgValueChanged += poseUpdated;
        TFSubscriber.initialisedEvent += poseInitialised;
        SpeechManager.stop += stopCmds;
        SpeechManager.AddListener("restart", restartCmds);
    }

    private void OnDisable() {
        ExtOdometrySubscriber.msgValueChanged -= poseUpdated;
        TFSubscriber.initialisedEvent -= poseInitialised;
        SpeechManager.stop -= stopCmds;
        SpeechManager.RemoveListener("restart", restartCmds);
    }

    private void Update() {
        if (!stop) {
            // Detect joystick (priority) or pure pursuit input
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
            twistMessage.linear.y = 0;
            twistMessage.angular.z = -angularSpeed;

            // Publish the Twist message to control the robot
            rosSocket.Publish(botCommandTopic, twistMessage);
        }

        // Update the pose of the robot in Unity
        if (usingTF) {
            x = tfRef.x;
            z = tfRef.z;
            theta = tfRef.theta;
        }
        if (initialised) {
            if (x != oldX || z != oldZ || theta != oldTheta) {
                // Invoke pose updated event if the robot has moved
                msgValueChanged?.Invoke(x, z, theta);
                oldX = x;
                oldZ = z;
                oldTheta = theta;
                //Debug.Log($"x: {x}, z: {z}, theta: {theta}");
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

    // Get the inital odometry pose
    private void poseInitialised() {
        if (usingTF) {
            x = tfRef.x; oldX = x;
            z = tfRef.z; oldZ = z;
            theta = tfRef.theta; oldTheta = theta;
            initialised = true;
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
        if (initialised) return (x, z, theta);
        return (0, 0, 404);
    }
}