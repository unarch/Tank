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

    // Start is called before the first frame update
    void Start()
    {
        // 找到炮塔
        turret = transform.Find("Turret");
        // 找到炮管
        gun = turret.Find("Turret/Gun");
    }

    // Update is called once per frame
    void Update()
    {
        // 旋转
        float steer = 20;
        float x = Input.GetAxis("Horizontal");
        transform.Rotate(0, x * steer * Time.deltaTime, 0);
        // 前进后退
        float speed = 3f;
        float y = Input.GetAxis("Vertical");
        Vector3 s = y * transform.forward * Time.deltaTime * speed;
        transform.transform.position += s;

        turretRotTarget = Camera.main.transform.eulerAngles.y;
        turretRollTarget = Camera.main.transform.eulerAngles.x;
        TurretRotation();
        TurretRoll();
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
