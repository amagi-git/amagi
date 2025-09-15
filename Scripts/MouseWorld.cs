using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseWorld : MonoBehaviour
{   
    public static MouseWorld instance;

    [SerializeField]//±à¼­Æ÷ÄÜ¿´µ½ 
    private LayerMask mousePlaneLayerMask;


    public void Awake()
    {
        instance = this;
    }
    // Update is called once per frame
    //private void Update()
    //{
        
        
    //    transform.position = MouseWorld.GetPosition();
    //}
    public static Vector3 GetPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue,instance.mousePlaneLayerMask);
        return raycastHit.point;
    }
}
