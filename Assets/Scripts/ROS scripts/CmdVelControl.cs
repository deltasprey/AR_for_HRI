using UnityEngine;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Messages.Geometry;
using System.Collections;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using UnityEngine.UIElements;

public class CmdVelControl : MonoBehaviour, IMixedRealitySpeechHandler {
    public JoystickControl joystick;
    public HandController handControl;
    //public  trackPad
    public Transform bot;
    //public string botSubscribeTopic = "/turtle1/pose";
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
        SafetyZone.stop += stopCmds;
        SafetyZone.restart += restartCmds;

        // ROS initialisation
        botCommandTopic = GetComponent<TwistPublisher>().Topic;
        rosSocket = GetComponent<RosConnector>().RosSocket;
        odomRef = GetComponent<ExtOdometrySubscriber>();
        //if (botSubscribeTopic != "") {
        //    rosSocket.Subscribe<TurtlePose>(botSubscribeTopic, poseCallback);
        //}
        twistMessage = new Twist();
    }

    private void OnEnable() {
        ExtOdometrySubscriber.msgValueChanged += poseUpdated;
        CoreServices.InputSystem?.RegisterHandler<IMixedRealitySpeechHandler>(this);
    }

    private void OnDisable() {
        ExtOdometrySubscriber.msgValueChanged -= poseUpdated;
        try {
            CoreServices.InputSystem.UnregisterHandler<IMixedRealitySpeechHandler>(this);
        } catch { }
    }

    private void Update() {
        if (!stop) {
            // Detect joystick input
            if (joystick.isGrabbed) {
                forwardSpeed = joystick.rotation.x * linearSpeed;
                angularSpeed = joystick.rotation.y * turnSpeed;
                isGrabbed = true;
            } else if (handControl.tracking) {
                forwardSpeed = handControl.rotation.x * linearSpeed;
                strafeSpeed = handControl.rotation.z * linearSpeed;
                angularSpeed = handControl.rotation.y * turnSpeed;
                isGrabbed = true;
            } else if (isGrabbed) {
                forwardSpeed = 0;
                angularSpeed = 0;
                isGrabbed = false;
            }

            // Detect keyboard input
            //if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0) {
            //    forwardSpeed = Input.GetAxis("Vertical") * linearSpeed;
            //    angularSpeed = Input.GetAxis("Horizontal") * turnSpeed;
            //}

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
                    bot.SetPositionAndRotation(position, rotation);

                    // Invoke pose updated event if the robot has moved (poseCallback has been called)
                    //msgValueChanged?.Invoke(position.x, position.z, theta + thetaOffset);
                    msgValueChanged?.Invoke(x, z, theta);

                    oldX = x;
                    oldZ = z;
                    oldTheta = theta;
                    //Debug.Log($"{position.x}, {position.y}, {position.z}");
                }
            } else if (initialised) {
                // Coordinate frame transform
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
    }

    // Callback for Pose message subscription
    // Uses the custom ROS subscriber script
    //private void poseCallback(TurtlePose msg) {
    //    //Debug.Log($"{msg.x}, {msg.y}, {msg.theta}, {msg.linear_velocity}, {msg.angular_velocity}");
    //    //print(initialised);
    //    x = msg.x;
    //    z = msg.y;
    //    theta = msg.theta;
    //    if (!initialised) {
    //        initialised = true;
    //    }
    //}

    private void poseUpdated() {
        x = odomRef.position.x;
        z = odomRef.position.z;
        theta = odomRef.rotation.eulerAngles.y;
        if (!initialised) {
            initialised = true;
        }
    }

    // Callback for stop commands from:
    //      Saftey zone stop event
    //      "Stop" voice command
    private void stopCmds() {
        StartCoroutine(floodPublish());
        stop = true;
    }

    // Callback for restart commands from:
    //      Saftey zone restart event
    //      "Restart" voice command
    private void restartCmds() {
        StopAllCoroutines();
        stop = false;
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

    // Voice command callback
    void IMixedRealitySpeechHandler.OnSpeechKeywordRecognized(SpeechEventData eventData) {
        if (eventData.Command.Keyword.ToLower() == "stop") {
            stopCmds();
        } else if (eventData.Command.Keyword.ToLower() == "restart") {
            restartCmds();
        }
    }

    // Return private pose variables
    public (float x, float z, float theta) initPos() { 
        if (initialised) { // && offsetted) {
            //return (position.x, position.z, theta + thetaOffset);
            return (x, z, theta);
        }
        return (0, 0, 404);
    }

#region ButtonControl
    public void forwardPress() {
        forwardSpeed += linearSpeed;
    }

    public void forwardRelease() {
        forwardSpeed -= linearSpeed;
    }

    public void backwardPress() {
        forwardSpeed -= linearSpeed;
    }

    public void backwardRelease() {
        forwardSpeed += linearSpeed;
    }

    public void rightPress() {
        strafeSpeed += linearSpeed;
    }

    public void rightRelease() {
        strafeSpeed -= linearSpeed;
    }

    public void leftPress() {
        strafeSpeed -= linearSpeed;
    }

    public void leftRelease() {
        strafeSpeed += linearSpeed;
    }

    public void clockwisePress() {
        angularSpeed += turnSpeed;
    }

    public void clockwiseRelease() {
        angularSpeed -= turnSpeed;
    }

    public void anitclockwisePress() {
        angularSpeed -= turnSpeed;
    }

    public void anitclockwiseRelease() {
        angularSpeed += turnSpeed;
    }
#endregion
}