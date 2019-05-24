using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyController : MonoBehaviour {

    [Range(0.1f, 10f)]
    public float moveSpeed;
    [Range(.1f, 2.66f)]
    public float jumpHeight;
    [Range(10, 360f)]
    public float rotationSpeed;

    public Rigidbody target = null;
    // Use this for initialization
    private float speed = 2f;
    private Vector2 dir = new Vector2();
    private Vector2 p = new Vector2();

    private MapGraph.MapNode inNode;
    private Transform Chan;
    private Vector3[] direction;
    private bool rotating;
    private Matrix4x4 rotate45 = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 45, 0), Vector3.one);
    private float angle;

    public Transform CameraforChan;
    /**
     * 主场景的缩放比例
     */
    private float K;
	void Start () {
        target = GetComponent<Rigidbody>();
        Chan = this.transform;
        direction = new Vector3[2];
        rotating = false;
        K = GameObject.Find("MapGenerator").transform.localScale.x;
        if(CameraforChan==null)
        {
            CameraforChan = GameObject.Find("Camera") ? GameObject.Find("Camera").transform : null;
        }
    }
	
	// Update is called once per frame
	private void Update () {
        if (target == null) return;
        //       currentBaseState = An.GetCurrentAnimatorStateInfo(0);
        Movepos(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Jump(Input.GetAxis("Jump"));


    }

    private void Jump(float var)
    {
        //An.SetBool("Jump", false);
        if (var > 0) //&& !An.IsInTransition(0)
        {
            //An.SetBool("Jump", true);
            target.velocity = new Vector3(0, Mathf.Sqrt(2 * 9.8f * jumpHeight), 0);
        }
    }
    private void Movepos(float LR, float FB)
    {
        float var = FB != 0 ? FB : LR;
        //       currentBaseState = An.GetCurrentAnimatorStateInfo(0);
        //       if (currentBaseState.fullPathHash != jumpState)
        //       {
        //           An.SetFloat("Speed", Mathf.Abs(var));
        if (var != 0)
            {
                Calculation(FB > 0 && LR < 0 ? 4 : FB > 0 && LR > 0 ? 5 : FB < 0 && LR < 0 ? 6 : FB < 0 && LR > 0 ? 7
                    : FB > 0 ? 0 : FB < 0 ? 1 : LR < 0 ? 2 : 3);
         //       if (!rotating)
         //       {
                    target.MovePosition(Chan.position + direction[1] * Time.fixedDeltaTime * moveSpeed);
         //       }
            }
 //       }
    }

    private void check()
    {
        p.x = target.position.x;
        p.y = target.position.z;
        p = p / K;
        if (inNode == null)
        {
            inNode = MapMeshGenerator.isIn(p);
        }
        else
        {
            if(!inNode.GetBoundingRectangle().Contains(p))
            {
                inNode = MapMeshGenerator.isIn(p);
                GameObject.Find("MapGenerator").SendMessage("changeCenter",inNode);
            }
            
        }
        
    }

    private void Calculation(int LRFB)
    {
        direction[0] = Chan.forward;
        switch (LRFB)
        {
            case 4:
                direction[1] = rotate45.MultiplyPoint3x4(-CameraforChan.right);
                break;
            case 5:
                direction[1] = rotate45.MultiplyPoint3x4(CameraforChan.forward);
                break;
            case 6:
                direction[1] = rotate45.MultiplyPoint3x4(-CameraforChan.forward);
                break;
            case 7:
                direction[1] = rotate45.MultiplyPoint3x4(CameraforChan.right);
                break;
            case 0:
                direction[1] = CameraforChan.forward;
                break;
            case 1:
                direction[1] = -CameraforChan.forward;
                break;
            case 2:
                direction[1] = -CameraforChan.right;
                break;
            case 3:
                direction[1] = CameraforChan.right;
                break;
        }
        angle = (Vector3.Dot(Chan.right, direction[1]) > 0 ? 1 : -1)
                    * Vector2.Angle(new Vector2(direction[0].x, direction[0].z), new Vector2(direction[1].x, direction[1].z));
        if (Mathf.Abs(angle) < 90)
        {
            rotating = false;
        }
        else
        {
            rotating = true;
        }
        
        if (Mathf.Abs(angle) > 10)
        {
            target.MoveRotation(Quaternion.Euler(Chan.rotation.eulerAngles + new Vector3(0, angle * rotationSpeed * Time.fixedDeltaTime, 0)));
        }
    }
}
