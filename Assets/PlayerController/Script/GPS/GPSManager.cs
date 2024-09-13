using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GPSManager : MonoBehaviour
{
    //显示范围
    [Range(0, 25)]
    public float width, height;
    //存储已登记的图标和物体
    public List<Icon> icons;
    //图标类型
    public enum IconType
    {
        OtherPlayer,
        Enemy,
        FriendlyNPC,
        Target,
        TargetItem,
        Vehicle,
        EnemyVehicle,
    }
    //图标预制体
    public List<IconPrefab> prefabs;
    /// <summary>
    /// 寻找对应的图标预制体
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public GameObject FindIconPrefab(IconType type)
    {
        GameObject iconPrefab = null;
        foreach(IconPrefab prefab in prefabs)
        {
            if(prefab.iconType == type)
            {
                iconPrefab = prefab.prefab;
                break;
            }
        }
        return iconPrefab;
    }
    public void ShowObjectInGPS(GameObject newObjectToShow, IconType type,bool alwaysShowInGPS)
    {
        GameObject newIcon = Instantiate(FindIconPrefab(type),transform);
        icons.Add(new Icon(alwaysShowInGPS,newObjectToShow,newIcon,type));
    }
    void SetIcon()
    {
        foreach(Icon icon in icons)
        {
            if (icon.alwaysShowInGPS)
            {
                icon.icon.transform.position = CalculateIconPosition(icon.target.transform.position) + Vector3.up * 20f;
            }
            else
            {
                icon.icon.transform.position = icon.target.transform.position + Vector3.up * 20f;
            }
        }
    }
    Vector3 CalculateIconPosition(Vector3 target)
    {
        Vector3 position = new Vector3(0,0,0);
        Vector3 targetVector = target - transform.position;
        targetVector = new Vector3(targetVector.x, 0, targetVector.z);
        Vector3 forward = new Vector3(transform.forward.x,0,transform.forward.z);
        Vector3 right = new Vector3(transform.right.x, 0, transform.right.z);
        float angleWithForward = Vector3.Angle(targetVector, forward)*Mathf.Deg2Rad;
        float angleWithRight = Vector3.Angle(targetVector, right) * Mathf.Deg2Rad;
        Vector3 calculatedForward = Mathf.Clamp(targetVector.magnitude * Mathf.Cos(angleWithForward), -height / 2, height / 2) * forward;
        Vector3 calculatedRight = Mathf.Clamp(targetVector.magnitude * Mathf.Cos(angleWithRight), -width / 2, width / 2) * transform.right;
        position = transform.position + (calculatedForward + calculatedRight);
        return position;
    }
    private void Start()
    {
        GPS.manager = this;
    }
    private void Update()
    {
        SetIcon();
    }
    [System.Serializable]
    public class IconPrefab
    {
        public IconType iconType;
        public GameObject prefab; 
    }
    [System.Serializable]
    public class Icon
    {
        public bool alwaysShowInGPS;
        public GameObject target;
        public GameObject icon;
        public IconType type;
        public Icon(bool a,GameObject b,GameObject c,IconType d)
        {
            alwaysShowInGPS = a;
            target = b;
            icon = c;
            type = d;
        }
    }
}
public static class GPS
 {
    public static GPSManager manager;
}
    

