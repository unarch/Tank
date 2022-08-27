using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
    // 炮塔
    public Transform turret;
    // 炮塔旋转速度
    private float turretRotSpeed = 0.5f;
    // 炮塔目标角度
    private float turretRotTarget = 0;
    // 炮管目标角度
    private float turretRollTarget = 0;
    
    // 炮管
    public Transform gun;
    // 炮管的旋转范围
    private float maxRoll = 10f;
    private float minRoll = -10f;

    //马达音源
    public AudioSource motorAudioSource;
    //马达音效
    public AudioClip motorClip;

    // 轮轴
    public List<AxleInfo> axleInfos;
    // 马力 / 最大马力
    private float motor = 0;
    public float maxMotorTorque;

    // 制动 / 最大制动
    private float brakeTorque = 0;
    public float maxBrakeTorque = 100;
    // 转向角 / 最大转向角
    private float steering = 0;
    public float maxSteeringAngle;
    // 车轮
    private Transform wheels;

    // 履带
    private Transform tracks;

    // Start is called before the first frame update
    void Start()
    {
        // 找到炮塔
        turret = transform.Find("Turret");
        // 找到炮管
        gun = turret.Find("Turret/Gun");
        // 获取轮子
        wheels = transform.Find("Wheels");
        // 获取履带
        tracks = transform.Find("Tracks");

        motorAudioSource = gameObject.AddComponent<AudioSource>();
        motorAudioSource.spatialBlend = 1;
    }

    // Update is called once per frame
    void Update()
    {
        PlayerCtrl();

        foreach (AxleInfo axleInfo in axleInfos)
        {
            // 转向
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            // 马力
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
            // 制动
            axleInfo.leftWheel.brakeTorque = brakeTorque;
            axleInfo.rightWheel.brakeTorque = brakeTorque;
        }
        // 转动轮子履带
        var motorAxle = axleInfos[1];
        if (motorAxle != null)
        {
            WheelsRotation(motorAxle.leftWheel);
            TrackMove();
        }

        TurretRotation();
        TurretRoll();
        MotorSound();
    }

    // 玩家控制
    public void PlayerCtrl()
    {
        // 马力和转向角
        motor = maxMotorTorque * Input.GetAxis("Vertical");
        steering = maxSteeringAngle * Input.GetAxis("Horizontal");

        // 制动
        brakeTorque = 0;
        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.leftWheel.rpm > 5 && motor < 0)
                brakeTorque = maxBrakeTorque;
            else if (axleInfo.leftWheel.rpm < -5 && motor > 0)
                brakeTorque = maxBrakeTorque;
            continue;
        }

        // 炮塔炮管角度
        turretRotTarget = Camera.main.transform.eulerAngles.y;
        turretRollTarget = Camera.main.transform.eulerAngles.x;
    }

    // 车轮旋转
    public void WheelsRotation(WheelCollider collider)
    {
        if (wheels == null)
            return;
        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);
        foreach (Transform wheel in wheels) 
        {
            wheel.rotation = rotation;
        }
    }
    //履带滚动
    public void TrackMove() {
        if (tracks == null)
            return;
        float offset = 0;
        if (wheels.GetChild(0) != null)
            offset = wheels.GetChild(0).localEulerAngles.x / 90.0f;
        foreach (Transform track in tracks)
        {
            MeshRenderer mr = track.gameObject.GetComponent<MeshRenderer>();
            if (mr == null) continue;
            Material mt = mr.material;
            mt.mainTextureOffset = new Vector2(0, offset);
        }
    }
    // 马达音效
    void MotorSound()
    {
        if (motor != 0 && !motorAudioSource.isPlaying)
        {
            motorAudioSource.loop = true;
            motorAudioSource.clip = motorClip;
            motorAudioSource.Play();
        }
        if (motor == 0)
            motorAudioSource.Pause();
    }
    // 炮塔旋转
    public void TurretRotation()
    {
        if (Camera.main == null)
            return;
        if (turret == null)
            return;

        //归一化角度
        float angle = turret.eulerAngles.y - turretRotTarget;
        if (angle < 0) angle += 360;

        if (angle > turretRotSpeed && angle < 180)
            turret.Rotate(0f, -turretRotSpeed, 0f);
        else if (angle > 180 && angle < 360 - turretRotSpeed)
            turret.Rotate(0f, turretRotSpeed, 0f);
    }

    public void TurretRoll()
    {
        if (Camera.main == null)
            return;
        if (turret == null)
            return;
        // 获取角度
        Vector3 worldEuler = gun.eulerAngles;
        Vector3 localEuler = gun.localEulerAngles;

        // 世界坐标系角度计算
        localEuler.x = turretRollTarget;
        gun.localEulerAngles = localEuler;
        // 本地坐标系角度限制
        Vector3 euler = gun.localEulerAngles;
        if (euler.x > 180)
            euler.x -= 360;
        if (euler.x > maxRoll)
            euler.x = maxRoll;
        if (euler.x < minRoll)
            euler.x = minRoll;
        gun.localEulerAngles = new Vector3(euler.x, localEuler.y, localEuler.z);

    }
}
