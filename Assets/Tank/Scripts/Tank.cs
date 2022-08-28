using System;
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
    public AudioSource shootAudioSource;
    public AudioClip shootClip;

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


    // 炮弹预制体
    public GameObject bullet;
    // 上一次开炮时间
    public float lastShootTime = 0;
    // 开炮的时间间隔
    private float shootInterval = 1.5f;

    private AI ai;

    //操控类型
    public enum CtrlType
    {
        none,
        player,
        computer
    }
    public CtrlType ctrlType = CtrlType.player;

    // 最大生命和当前生命值
    private float maxHp = 100;
    public float hp = 100;

    // 中心准心
    public Texture2D centerSight;
    // 坦克准心
    public Texture2D tankSight;

    // 生命条UI素材
    public Texture2D hpBarBg;
    public Texture2D hpBar;

    // 击杀提示图标
    public Texture2D killUI;
    private float killUIStartTime = float.MinValue;


    // 焚烧特效
    public GameObject destroyEffect;
    // Start is called before the first frame update
    void Start()
    {
        if (ctrlType == CtrlType.computer)
        {
            ai = gameObject.AddComponent<AI>();
            ai.tank = this;
        }
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
        shootAudioSource = gameObject.AddComponent<AudioSource>();
        shootAudioSource.spatialBlend = 1;
    }

    // Update is called once per frame
    void Update()
    {
        PlayerCtrl();
        ComputerCtrl();
        NoneCtrl();

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
        if (ctrlType != CtrlType.player)
            return;
        // 马力和转向角
        motor = maxMotorTorque * Input.GetAxis("Vertical");
        steering = maxSteeringAngle * Input.GetAxis("Horizontal");

        // 制动
        brakeTorque = 0;
        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.leftWheel.rpm > 5 && motor < 0) // 前进时按下了下
                brakeTorque = maxBrakeTorque;
            else if (axleInfo.leftWheel.rpm < -5 && motor > 0) // 后退按了上
                brakeTorque = maxBrakeTorque;
            continue;
        }
        TargetSignPos();
        CallExplodePoint();

        if (Input.GetMouseButton(0))
            Shoot();
    }
    // 电脑控制
    public void ComputerCtrl()
    {
        if (ctrlType != CtrlType.computer) return;
        // 炮塔目标角度
        Vector3 rot = ai.GetTurretTarget();
        turretRotTarget = rot.y;
        turretRollTarget = rot.x;
        
        // 移动
        steering = ai.GetSteering();
        motor = ai.GetMotor();
        brakeTorque = ai.GetBrakeTorque();

        //发射炮弹
        if (ai.IsShoot())
            Shoot();


    }
    // 无人控制
    public void NoneCtrl()
    {
        if (ctrlType != CtrlType.none) return;
        motor = 0;
        steering = 0;
        brakeTorque = maxBrakeTorque / 2;
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
    public void TrackMove()
    {
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




    public void Shoot()
    {
        // 发射间隔
        if (Time.time - lastShootTime < shootInterval) return;
        // 子弹
        if (bullet == null) return;
        // 发射
        Vector3 pos = gun.position + gun.up * -1 * 5;
        var Quaternion = gun.rotation;

        GameObject bulletObj = Instantiate(bullet, pos, gun.rotation);
        Bullet bulletCmp = bulletObj.GetComponent<Bullet>();
        if (bulletCmp != null) bulletCmp.attackTank = this.gameObject;
        lastShootTime = Time.time;

        shootAudioSource.PlayOneShot(shootClip);
    }

    public void BeAttacked(float att, GameObject attackTank)
    {
        // 坦克已经被摧毁
        if (hp <= 0) return;
        // 击中处理
        if (hp > 0)
        {
            hp -= att;
            if (ai != null)
                ai.OnAttacked(attackTank);
        } 
        if (hp <= 0)
        {
            GameObject destroyObj = (GameObject)Instantiate(destroyEffect);
            destroyObj.transform.SetParent(transform, false);
            destroyObj.transform.localPosition = new Vector3(0, 2.57f, 0);
            ctrlType = CtrlType.none;

            // 显示击杀提示
            if (attackTank != null)
            {
                Tank tankCmp = attackTank.GetComponent<Tank>();
                if (tankCmp != null && tankCmp.ctrlType == CtrlType.player)
                    tankCmp.StartDrawKill();
            }
            Battle.instance.IsWin(attackTank);
        }
    }

    // 计算目标角度
    public void TargetSignPos()
    {
        // 碰撞信息和碰撞点
        Vector3 hitPoint = Vector3.zero;
        RaycastHit raycastHit;
        // 屏幕中心位置
        Vector3 centerVec = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray ray = Camera.main.ScreenPointToRay(centerVec);
        // 射线检测 获取hitPoint
        if (Physics.Raycast(ray, out raycastHit, 400.0f))
        {
            hitPoint = raycastHit.point;
        }
        else
        {
            hitPoint = ray.GetPoint(400);
        }
        // 计算目标角度
        Vector3 dir = hitPoint - turret.position;
        Quaternion angle = Quaternion.LookRotation(dir);
        turretRotTarget = angle.eulerAngles.y;
        turretRollTarget = angle.eulerAngles.x;

        // *TODO 测试代码
        Transform targetCube = GameObject.Find("TargetCube").transform;
        targetCube.position = hitPoint;
    }

    // 计算爆炸位置
    public Vector3 CallExplodePoint()
    {
        // 碰撞信息和碰撞点
        Vector3 hitPoint = Vector3.zero;
        RaycastHit hit;
        // 沿着炮管方向的射线
        Vector3 pos = gun.position - gun.up * 5;
        Ray ray = new Ray(pos, -gun.up);
        // 射线检测
        if (Physics.Raycast(ray, out hit, 400.0f))
        {
            hitPoint = hit.point;
        }
        else
        {
            hitPoint = ray.GetPoint(400);
        }
        // *TODO 调试代码
        Transform explodeCube = GameObject.Find("ExplodeCube").transform;
        explodeCube.position = hitPoint;

        return hitPoint;
    }

    // 绘制准心
    public void DrawSight()
    {
        // 计算实际设计位置
        Vector3 explodePoint = CallExplodePoint();
        // 获取坦克准心的屏幕坐标
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(explodePoint);
        // 绘制坦克准心
        Rect tankRect = new Rect(screenPoint.x - tankSight.width / 2, Screen.height - screenPoint.y - tankSight.height / 2, tankSight.width, tankSight.height);
        GUI.DrawTexture(tankRect, tankSight);

        // 绘制中心准心
        Rect centerRect = new Rect(
            Screen.width / 2 - centerSight.width / 2,
            Screen.height / 2 - centerSight.height / 2,
            centerSight.width,
            centerSight.height);
        GUI.DrawTexture(centerRect, centerSight);
    }

    // 绘制生命条
    public void DrawHp()
    {
        // 底框 
        Rect bgRect = new Rect(30 * 3, Screen.height - hpBarBg.height * 3 - 15 * 3, hpBarBg.width * 3, hpBarBg.height * 3);
        GUI.DrawTexture(bgRect, hpBarBg);

        // 指示条
        float width = hp * 102 / maxHp;
        Rect hpRect = new Rect(bgRect.x * 3 + 29 * 3, bgRect.y * 3 + 9 * 3, width * 3, hpBar.height * 3);
        GUI.DrawTexture(hpRect, hpBar);

        //文字
        string text = Mathf.Ceil(hp).ToString() + "/" + Mathf.Ceil(maxHp).ToString();
        Rect textRect = new Rect(bgRect.x + 80 * 3, bgRect.y - 10 * 3, 50 * 3, 50 * 3);
        GUI.Label(textRect, text);
    }

    public void StartDrawKill()
    {
        killUIStartTime = Time.time;
    }

    private void DrawKillUI()
    {
        if (Time.time - killUIStartTime < 1f)
        {
            Rect rect = new Rect(Screen.width / 2 - killUI.width * 3 / 2, 30 * 3, killUI.width * 3, killUI.height * 3);
            GUI.DrawTexture(rect, killUI);
        }
    }
    void OnGUI()
    {
        if (ctrlType != CtrlType.player) return;
        DrawSight();
        DrawHp();
        DrawKillUI();
    }


}
