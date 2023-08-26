using UnityEngine;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Messages.Geometry;
using System.Collections;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;

public class TurtleControl : MonoBehaviour, IMixedRealitySpeechHandler {
    public JoystickControl joystick;
    public HandController handControl;
    public Transform turtleBot;
    public string turtlebotCommandTopic = "/turtle1/cmd_vel", turtlebotSubscribeTopic = "/turtle1/pose";
    public float linearSpeed = 1f, turnSpeed = 1f;
    public bool stopOnLoad = true;

    public delegate void MsgReceived(float x, float z, float theta);
    public static event MsgReceived msgValueChanged;

    private float x = 0, z = 0, theta = 0;
    private float oldX, oldZ, oldTheta;
    private float forwardSpeed = 0, strafeSpeed = 0, angularSpeed = 0;
    private bool isGrabbed = false, initialised = false, offsetted = false, stop = true;
    private Twist twistMessage;
    private RosSocket rosSocket;
    private UnityEngine.Vector3 offset, position;

    private void Start() {
        Invoke(nameof(setStopVar), 0.2f);
        SafetyZone.stop += stopCmds;
        SafetyZone.restart += restartCmds;

        rosSocket = GetComponent<RosConnector>().RosSocket;
        rosSocket.Subscribe<TurtlePose>(turtlebotSubscribeTopic, poseCallback);
        twistMessage = new Twist();
    }

    private void OnEnable() {
        CoreServices.InputSystem?.RegisterHandler<IMixedRealitySpeechHandler>(this);
    }

    private void OnDisable() {
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

            // Publish the Twist message to control the Turtlebot
            rosSocket.Publish(turtlebotCommandTopic, twistMessage);

            // Update the pose of the turtlebot in Unity
            if (initialised && offsetted) {
                if (x != oldX || z != oldZ || theta != oldTheta) {
                    msgValueChanged?.Invoke(x, z, theta);
                    position = new(x + offset.x, offset.y, z + offset.z);
                    UnityEngine.Quaternion rotation = UnityEngine.Quaternion.Euler(90, 0, theta * Mathf.Rad2Deg - 90);
                    turtleBot.SetPositionAndRotation(position, rotation);
                    oldX = x;
                    oldZ = z;
                    oldTheta = theta;
                    //Debug.Log($"{position.x}, {position.y}, {position.z}");
                }
            } else if (initialised) {
                offset = new(turtleBot.position.x - x, turtleBot.position.y, turtleBot.position.z - z);
                oldX = x;
                oldZ = z;
                oldTheta = theta;
                offsetted = true;
            }
        }
    }

    private void setStopVar() {
        stop = stopOnLoad;
    }

    private void poseCallback(TurtlePose msg) {
        //Debug.Log($"{msg.x}, {msg.y}, {msg.theta}, {msg.linear_velocity}, {msg.angular_velocity}");
        //print(initialised);
        x = msg.x;
        z = msg.y;
        theta = msg.theta;
        if (!initialised) {
            initialised = true;
        }
    }

    private void stopCmds() {
        StartCoroutine(floodPublish());
        stop = true;
    }

    private void restartCmds() {
        StopAllCoroutines();
        stop = false;
    }

    private IEnumerator floodPublish() {
        Twist floodTwistMessage = new() {
            linear = new RosSharp.RosBridgeClient.Messages.Geometry.Vector3(),
            angular = new RosSharp.RosBridgeClient.Messages.Geometry.Vector3()
        };
        while (true) {
            rosSocket.Publish(turtlebotCommandTopic, floodTwistMessage);
            yield return new WaitForSeconds(0.02f);
        }
    }

    void IMixedRealitySpeechHandler.OnSpeechKeywordRecognized(SpeechEventData eventData) {
        if (eventData.Command.Keyword.ToLower() == "stop") {
            stopCmds();
        } else if (eventData.Command.Keyword.ToLower() == "restart") {
            restartCmds();
        }
    }

    public (float x, float z, float theta) initPos() { return (x, z, theta); }

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
}