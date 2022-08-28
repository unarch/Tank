

using UnityEngine;

public class AI : MonoBehaviour
{
    // 所控制的坦克
    public Tank tank;
    // 状态枚举
    public enum Status
    {
        Patrol,
        Attack,
    }

    private Status status = Status.Attack;

    // 锁定的坦克
    public GameObject target;
    // 视野范围
    private float sightDistance = 30;
    //上一次搜寻时间
    private float lastSearchTargetTime = 0;
    // 搜寻间隔
    private float searchTargetInterval = 3;

    // 路径
    private TankPath path = new TankPath();
    // 上次更新路径时间
    private float lastUpdateWaypointTime = float.MinValue;
    // 更新路径cd
    private float updateWaypointInterval = 10;

    // 初始化路点
    void InitWaypoint()
    {
        path.wayPoint = transform.position;
        GameObject obj = GameObject.Find("WaypointContainer");
        if (obj && obj.transform.GetChild(0) != null)
        {
            Debug.Log("obj.transform.GetChild(0) = "+ obj.transform.GetChild(0).name);
            Vector3 targetPos = obj.transform.GetChild(0).position;
            path.InitByNavMeshPath(transform.position, targetPos);
        }
    }

    void OnDrawGizmos()
    {
        path.DrawWaypoints();
    }

    void Start()
    {
        InitWaypoint();
    }


    public void ChangeStatus(Status status)
    {
        if (status == Status.Patrol)
            PatrolStart();
        else if (status == Status.Attack)
            AttackStart();
    }
    // 状态处理
    private void Update()
    {
        if (tank.ctrlType != Tank.CtrlType.computer) return;
        if (status == Status.Patrol)
            PatrolUpdate();
        if (status == Status.Attack)
            AttackUpdate();
        TargetUpdate();
        // 行走
        if (path.IsReach(transform))
        {
            path.NextWaypoint();
        }
    }

    // 训练开始
    void PatrolStart()
    {
        status = Status.Patrol;
    }

    // 攻击开始
    void AttackStart()
    {
        Vector3 targetPos = target.transform.position;
        path.InitByNavMeshPath(transform.position, targetPos);
        status = Status.Attack;
    }

    // 巡逻中
    void PatrolUpdate()
    {
        if (target != null) {
            ChangeStatus(Status.Attack);
            return;
        }
        float interval = Time.time - lastUpdateWaypointTime;
        if (interval < updateWaypointInterval) return;
        lastUpdateWaypointTime = Time.time;
        // 处理巡逻点
        if (path.wayPoints == null || path.isFinish)
        {
            GameObject obj = GameObject.Find("WaypointContainer");
            {
                int count = obj.transform.childCount;
                if (count == 0) return;
                int index = Random.Range(0, count);
                Vector3 targetPos = obj.transform.GetChild(index).position;
                Debug.Log("开始寻找下一个巡逻点 " + targetPos);
                path.InitByNavMeshPath(transform.position, targetPos);
            }
        }
    }

    // 攻击中
    void AttackUpdate()
    {
        if (target == null) {
            ChangeStatus(Status.Patrol);
             return;
        }
        float interval = Time.time - lastUpdateWaypointTime;
        if (interval < updateWaypointInterval) return;
        lastUpdateWaypointTime = Time.time;
        //  更新路径
        Vector3 targetPos = target.transform.position;
        path.InitByNavMeshPath(transform.position, targetPos);
    }

    // 搜寻目标
    void TargetUpdate()
    {
        //cd时间
        float interval = Time.time - lastSearchTargetTime;
        if (interval < searchTargetInterval) return;
        lastSearchTargetTime = Time.time;

        //已有目标，判断是否丢失目标
        if (target != null)
            HasTarget();
        else
            NoTarget();
    }

    // 已有目标的情况，判断是否丢失目标
    void HasTarget()
    {
        Tank targetTank = target.GetComponent<Tank>();
        Vector3 pos = transform.position;
        Vector3 targetPos = target.transform.position;

        if (targetTank.ctrlType == Tank.CtrlType.none)
        {
            Debug.Log("目标死亡，丢失目标！");
            target = null;
        }
        if (Vector3.Distance(pos, targetPos) > sightDistance)
        {
            Debug.Log("距离过远，丢失目标！");
            target = null;
        }
    }
    // 没有目标的情况，搜索视野中的坦克
    void NoTarget()
    {
        float minHp = float.MaxValue;
        // 遍历所有坦克
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Tank");
        for (int i = 0; i < targets.Length; i++)
        {
            GameObject nowTank = targets[i];
            Tank tank = nowTank.GetComponent<Tank>();
            if (tank == null) continue;
            if (nowTank == gameObject) continue;
            if (Battle.instance.IsSameCamp(gameObject, nowTank)) continue;
            if (tank.ctrlType == Tank.CtrlType.none) continue;
            // 判断距离
            Vector3 pos = transform.position;
            Vector3 targetPos = nowTank.transform.position;
            if (Vector3.Distance(pos, targetPos) > sightDistance) continue;
            if (minHp > tank.hp)
            {
                target = tank.gameObject;
                minHp = tank.hp;
            }

        }
        // 调试
        if (target != null)
            Debug.Log("获取目标 " + target.name);
    }

    // 被攻击
    public void OnAttacked(GameObject attackTank)
    {
        // 队友
        if (Battle.instance.IsSameCamp(gameObject, attackTank))
            return;
        target = attackTank;
    }

    // 获取炮管和炮塔的目标角度
    public Vector3 GetTurretTarget()
    {
        // 没有目标， 朝着炮塔坦克前方
        if (target == null)
        {
            float y = transform.eulerAngles.y;
            Vector3 rot = new Vector3(0, y, 0);
            return rot;
        }
        else 
        {
            Vector3 pos = transform.position;
            Vector3 targetPos = target.transform.position;
            Vector3 vec = targetPos - pos;
            return Quaternion.LookRotation(vec).eulerAngles;
        }
    }

    // 是否发射炮弹
    public bool IsShoot()
    {
        if (target == null) return false;
        // 目标角度差
        float turretRoll = tank.turret.eulerAngles.y;
        float angle = turretRoll - GetTurretTarget().y;
        if (angle < 0) angle += 360;
        // 30度以内发射炮弹
        if (angle < 30 || angle > 330)
            return true;
        return false;
    }


    // 获取转向角
    public float GetSteering()
    {
        if (tank == null) return 0;
        Vector3 itp = transform.InverseTransformPoint(path.wayPoint);
        if (itp.x > path.deviation / 5)
            return tank.maxSteeringAngle;
        else if (itp.x < - path.deviation / 5)
            return - tank.maxSteeringAngle;
        else 
            return 0;
    }

    // 获取马力
    public float GetMotor()
    {
        if (tank == null) return 0;
        Vector3 itp = transform.InverseTransformPoint(path.wayPoint);
        float x = itp.x;
        float z = itp.z;
        float r = 6;
        if (z < 0 && Mathf.Abs(x) < -z && Mathf.Abs(x) < r)
            return -tank.maxMotorTorque;
        else
            return tank.maxMotorTorque;
    }

    // 获取刹车马力
    public float GetBrakeTorque()
    {
        if (path.isFinish)
            return tank.maxMotorTorque;
        else 
            return 0;
    }
}
