using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class TankPath
{
    // 所有路点
    public Vector3[] wayPoints;
    // 当前路点索引
    public int index = -1;
    // 当前的路点
    public Vector3 wayPoint;
    // 是否循环
    bool isLoop = false;
    // 到达误差
    public float deviation = 5;
    // 是否完成
    public bool isFinish = false;

    // 是否到达目的地
    public bool IsReach(Transform trans)
    {
        Vector3 pos = trans.position;
        float distance = Vector3.Distance(wayPoint, pos);
        return distance < deviation;
    }

    // 下一个路点
    public void NextWaypoint()
    {
        if (index < 0) return;
        if (index < wayPoints.Length - 1) index ++;
        else {
            if (isLoop) index = 0;
            else isFinish = true;
        }
        wayPoint = wayPoints[index];
    }

    public void InitByObj(GameObject obj, bool isLoop = false)
    {
        int len = obj.transform.childCount;
        // 没有子物体
        if (len == 0)
        {
            wayPoints = null;
            index = -1;
            Debug.LogWarning("Path.InitByObj length = 0");
            return;
        }
        // 遍历子物体生成路点
        wayPoints = new Vector3[len];
        for (int i = 0; i < len; i++)
        {
            Transform trans = obj.transform.GetChild(i);
            wayPoints[i] = trans.position;
        }
        index = 0;
        wayPoint = wayPoints[index];
        this.isLoop = isLoop;
        isFinish = false;
    }

    // 导航图 来初始化路径
    public void InitByNavMeshPath(Vector3 pos, Vector3 targetPos)
    {
        // 重置
        wayPoints = null;
        index = -1;
        // 计算路径
        NavMeshPath navPath = new NavMeshPath();
        bool hasFoundPath = NavMesh.CalculatePath(pos, targetPos, NavMesh.AllAreas, navPath);
        Debug.Log("pos = " + pos + " target = " + targetPos);
        Debug.Log("是否找到！"+hasFoundPath);
        if (!hasFoundPath) return ;
        // 生成路径
        int length = navPath.corners.Length;
        wayPoints = new Vector3[length];
        Debug.Log("关键点个数！"+length);
        for (int i = 0; i < length; i++)
        {
            wayPoints[i] = navPath.corners[i];
        }
        index = 0;
        wayPoint = wayPoints[index];
        isFinish = false;

    }

    // 调试路径
    public void DrawWaypoints()
    {
        if (wayPoints == null) return;
        int length = wayPoints.Length;
        for (int i = 0; i < length; i++)
        {
            if (i == index)
                Gizmos.DrawSphere(wayPoints[i], 1);
            else 
                Gizmos.DrawCube(wayPoints[i], Vector3.one);
            if (i + 1 < length)
                Debug.DrawLine(wayPoints[i], wayPoints[i + 1], Color.red);
        }
    }
}
