using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
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
                curTf = hit.transform;
                curTf.gameObject.layer = 8;
                oriObjectScreenPos = Camera.main.WorldToScreenPoint(curTf.position);
                oriMousePos = Input.mousePosition;
            }
            if (isClick)
            {
                curTf.gameObject.layer = 9;
            }
            isClick = !isClick;
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
                Collider[] hitColliders = Physics.OverlapSphere(curTf.position, 2.0f,1<<9);
                if (hitColliders != null)
                    foreach (var hitCollider in hitColliders)
                    {
                            Vector3 aimUp = hitCollider.transform.up;
                            Vector3 curUp = curTf.up;
                            curTf.up = Vector3.Dot(curUp,aimUp) >= 0 ? aimUp : -aimUp;
                            Vector3 aimRight = hitCollider.transform.right;
                            Vector3 curRight = curTf.right;
                            Vector3 axis = Vector3.Cross(curRight, aimRight);
                            double theta = Math.Acos(Vector3.Dot(curRight.normalized, aimRight.normalized)) / Math.PI * 180;
                            Debug.Log(hitCollider);
                            curTf.RotateAround(curTf.position, axis, (float)theta);                                                                        
                    }
            }
        }
        
        

    }
}
