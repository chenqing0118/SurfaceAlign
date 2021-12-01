using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using JointDirection;
public class OnClickMove : MonoBehaviour
{
    private bool isClick = false;
    private Transform curTf = null;
    private Collider collider = null;
    private Vector3 oriMousePos;
    private Vector3 oriObjectScreenPos;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, 100))
            {
                isClick = !isClick;
                if (isClick)
                {
                    curTf = hit.transform;
                    curTf.gameObject.layer = 8;
                    oriObjectScreenPos = Camera.main.WorldToScreenPoint(curTf.position);
                    oriMousePos = Input.mousePosition;
                }
                else
                {
                    curTf.gameObject.layer = 9;
                }  
            }
        }
        if (isClick)
        {
            if (curTf != null)
            {
                Vector3 curMousePos = Input.mousePosition;
                Vector3 mouseOffset = curMousePos - oriMousePos;
                Vector3 curObjectScreenPos = oriObjectScreenPos + mouseOffset;
                Vector3 curObjectWorldPos = Camera.main.ScreenToWorldPoint(curObjectScreenPos);
                curTf.position = curObjectWorldPos;
                Collider[] hitColliders = new Collider[1];
                int no_Collider = Physics.OverlapSphereNonAlloc(curTf.position, 1.0f, hitColliders, 1 << 9);
                if (no_Collider > 0)
                {
                    if (Input.GetKeyDown(KeyCode.A))
                    {
                        align(hitColliders[0].transform, curTf);
                    }
                    
                    if (Input.GetKeyDown(KeyCode.J))
                    {
                        curTf.gameObject.layer = 9;
                        isClick = false;
                        joint(hitColliders[0].transform, curTf);
                        Debug.Log("完成拼接");
                    }
                }
                
            }
        }



    }
    private void align(Transform aimTransform, Transform curTransform)
    {
        bool sameUp = Vector3.Dot(curTransform.up, aimTransform.up) >=0.0f;
        curTransform.rotation = aimTransform.rotation;
        if (!sameUp)
        {
            curTransform.RotateAround(curTransform.position, curTransform.forward, 180.0f);
        }
    }
    private void joint(Transform aimTransform, Transform curTransform)
    {
        Vector3 alignBoundsCenter = curTransform.GetComponent<Renderer>().bounds.center - aimTransform.GetComponent<Renderer>().bounds.center;
        curTransform.Translate(-alignBoundsCenter, Space.World);
        Vector3.Normalize(alignBoundsCenter);
        MoveDirection[] normalDirections = { new MoveUp(aimTransform.up.normalized),new MoveRight(aimTransform.right.normalized), new MoveForward(aimTransform.forward.normalized)
                , new MoveUp(-aimTransform.up.normalized),new MoveRight(-aimTransform.right.normalized),  new MoveForward(-aimTransform.forward.normalized )};
        int translateDirIndex = 0;
        float maxDot = 0.0f;
        for(int i=0;i<normalDirections.Length;i++)
        {
            float tempDot = Vector3.Dot(alignBoundsCenter, normalDirections[i].getDirection());
            if (tempDot>0&&tempDot >= maxDot)
            {
                maxDot = tempDot;
                translateDirIndex = i;
            }     
        }
        Debug.Log(translateDirIndex);
        normalDirections[translateDirIndex].move(aimTransform, curTransform);
    }

}
namespace JointDirection
{
    class MoveDirection {
        protected Vector3 direction;
        public MoveDirection(Vector3 direction)
        {
            this.direction = direction;
        }
        public Vector3 getDirection() { return direction; }
        public virtual void move(Transform aimTransform, Transform curTransform) { }
    }
    class MoveUp: MoveDirection
    {
        public MoveUp(Vector3 up):base(up) {  }
        public override void move(Transform aimTransform, Transform curTransform)
        {
            float distance = (aimTransform.GetComponent<MeshFilter>().mesh.bounds.size.y / 2 ) * aimTransform.localScale.y 
                    + (curTransform.GetComponent<MeshFilter>().mesh.bounds.size.y / 2) * curTransform.localScale.y;
            curTransform.Translate(direction*distance, Space.World);
            Debug.Log("上下拼接");
        }
    }
    class MoveRight: MoveDirection
    {
        public MoveRight(Vector3 right) : base(right) { }
        public override void move(Transform aimTransform, Transform curTransform)
        {
            float distance = (aimTransform.GetComponent<MeshFilter>().mesh.bounds.size.x / 2 ) * aimTransform.localScale.x
                   + (curTransform.GetComponent<MeshFilter>().mesh.bounds.size.x / 2 ) * curTransform.localScale.x;
            curTransform.Translate(direction * distance, Space.World);
            Debug.Log("左右拼接");
        }

    }
    class MoveForward: MoveDirection
    {
        public MoveForward(Vector3 forward):base(forward) { }
        public override void move(Transform aimTransform, Transform curTransform)
        {
            float distance = (aimTransform.GetComponent<MeshFilter>().mesh.bounds.size.z / 2) * aimTransform.localScale.z
                  + (curTransform.GetComponent<MeshFilter>().mesh.bounds.size.z / 2) * curTransform.localScale.z;
            curTransform.Translate(direction * distance, Space.World);
            Debug.Log("前后拼接");
        }
    }
}