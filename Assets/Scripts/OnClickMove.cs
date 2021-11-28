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
                int no_Collider = Physics.OverlapSphereNonAlloc(curTf.position, 2.0f, hitColliders, 1 << 9);
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
                       /* curTf.position = hitColliders[0].transform.position;
                        float y =(hitColliders[0].GetComponent<MeshFilter>().mesh.bounds.size.y/2+ hitColliders[0].GetComponent<MeshFilter>().mesh.bounds.center.y)* hitColliders[0].transform.localScale.y+(curTf.GetComponent<MeshFilter>().mesh.bounds.size.y / 2- curTf.GetComponent<MeshFilter>().mesh.bounds.center.y)*curTf.localScale.y;
                        Debug.Log(y);
                        curTf.Translate(0.0f, y, 0.0f, Space.Self);*/
                        Debug.Log("完成对齐");
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
        Vector3 aim2cur=(curTransform.position - aimTransform.position).normalized;
        curTransform.position = aimTransform.position;
        MoveDirection[] normalDirections = { new MoveUp(aimTransform.up.normalized),new MoveRight(aimTransform.right.normalized), new MoveForward(aimTransform.forward.normalized)
                , new MoveDown(-aimTransform.up.normalized),new MoveRight(-aimTransform.right.normalized),  new MoveForward(-aimTransform.forward.normalized )};
        int translateDirIndex = 0;
        float maxDot = 0.0f;
        for(int i=0;i<normalDirections.Length;i++)
        {
            float tempDot = Vector3.Dot(aim2cur, normalDirections[i].getDirection());
            if (tempDot>0&&tempDot >= maxDot)
            {
                maxDot = tempDot;
                translateDirIndex = i;
            }     
        }
        normalDirections[translateDirIndex].move(aimTransform, curTransform);
    }

}
namespace JointDirection
{
    interface MoveDirection {
        Vector3 getDirection();
        void move(Transform aimTransform, Transform curTransform);
    }
    class MoveUp: MoveDirection
    {
        private Vector3 up;
        public MoveUp(Vector3 up) { this.up = up; }
        public Vector3 getDirection() { return up; }
        public void move(Transform aimTransform, Transform curTransform)
        {
            float distance;
            if (Vector3.Dot(aimTransform.up, curTransform.up) > 0)
            {
                distance= (aimTransform.GetComponent<MeshFilter>().mesh.bounds.size.y / 2 + aimTransform.GetComponent<MeshFilter>().mesh.bounds.center.y) * aimTransform.localScale.y 
                    + (curTransform.GetComponent<MeshFilter>().mesh.bounds.size.y / 2 - curTransform.GetComponent<MeshFilter>().mesh.bounds.center.y) * curTransform.localScale.y;
            }
            else
            {
                distance = (aimTransform.GetComponent<MeshFilter>().mesh.bounds.size.y / 2 + aimTransform.GetComponent<MeshFilter>().mesh.bounds.center.y) * aimTransform.localScale.y 
                    + (curTransform.GetComponent<MeshFilter>().mesh.bounds.size.y / 2 + curTransform.GetComponent<MeshFilter>().mesh.bounds.center.y) * curTransform.localScale.y;

            }
            curTransform.Translate(up*distance, Space.World);
            Debug.Log("向上对齐");
        }
    }
    class MoveDown : MoveDirection
    {
        private Vector3 down;
        public MoveDown(Vector3 down) { this.down = down; }
        public Vector3 getDirection() { return down; }
        public void move(Transform aimTransform, Transform curTransform)
        {
            float distance;
            if (Vector3.Dot(aimTransform.up, curTransform.up) > 0)
            {
                distance = (aimTransform.GetComponent<MeshFilter>().mesh.bounds.size.y / 2 - aimTransform.GetComponent<MeshFilter>().mesh.bounds.center.y) * aimTransform.localScale.y
                    + (curTransform.GetComponent<MeshFilter>().mesh.bounds.size.y / 2 + curTransform.GetComponent<MeshFilter>().mesh.bounds.center.y) * curTransform.localScale.y;
            }
            else
            {
                distance = (aimTransform.GetComponent<MeshFilter>().mesh.bounds.size.y / 2 - aimTransform.GetComponent<MeshFilter>().mesh.bounds.center.y) * aimTransform.localScale.y
                    + (curTransform.GetComponent<MeshFilter>().mesh.bounds.size.y / 2 - curTransform.GetComponent<MeshFilter>().mesh.bounds.center.y) * curTransform.localScale.y;

            }
            curTransform.Translate(down * distance, Space.World);
            Debug.Log("向下对齐");
        }
    }
    class MoveRight: MoveDirection
    {
        private Vector3 right;
        public MoveRight(Vector3 right) { this.right = right; }
        public Vector3 getDirection() { return right; }
        public void move(Transform aimTransform, Transform curTransform)
        {
            float distance = (aimTransform.GetComponent<MeshFilter>().mesh.bounds.size.x / 2 ) * aimTransform.localScale.x
                   + (curTransform.GetComponent<MeshFilter>().mesh.bounds.size.x / 2 ) * curTransform.localScale.x;
            curTransform.Translate(right*distance, Space.World);
            Debug.Log("左右对齐");
        }

    }
    class MoveForward: MoveDirection
    {
        private Vector3 forward;
        public MoveForward(Vector3 forward) { this.forward = forward; }
        public Vector3 getDirection() { return forward; }
        public void move(Transform aimTransform, Transform curTransform)
        {
            float distance = (aimTransform.GetComponent<MeshFilter>().mesh.bounds.size.z / 2) * aimTransform.localScale.z
                  + (curTransform.GetComponent<MeshFilter>().mesh.bounds.size.z / 2) * curTransform.localScale.z;
            curTransform.Translate(forward * distance, Space.World);
            Debug.Log("前后对齐");
        }
    }
}