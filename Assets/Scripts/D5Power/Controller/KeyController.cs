using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyController : MonoBehaviour {

    public Rigidbody target = null;
    // Use this for initialization
    private float speed = 2f;
    private Vector2 dir = new Vector2();
    private Vector2 p = new Vector2();

    private MapGraph.MapNode inNode;
    /**
     * 主场景的缩放比例
     */ 
    private float K;
	void Start () {
        target = GetComponent<Rigidbody>();
        K = GameObject.Find("MapGenerator").transform.localScale.x;
    }
	
	// Update is called once per frame
	void Update () {
        if (target == null) return;
		if(Input.GetKeyDown(KeyCode.W))
        {
            dir.y = 1;
        }else if(Input.GetKeyDown(KeyCode.S)){
            dir.y = -1;
        }else if(Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.W)){
            dir.y = 0;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            dir.x = -1;
        }else if (Input.GetKeyDown(KeyCode.D)){
            dir.x = 1;
        }else if(Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D)){
            dir.x = 0;
        }

        if(dir.x == 1)
        {
            target.velocity = Vector3.right * speed;
            check();
        }else if(dir.x == -1){
            target.velocity = Vector3.left * speed;
            check();
        }

        if (dir.y == 1)
        {
            target.velocity = Vector3.forward * speed;
            check();
        }
        else if (dir.y == -1)
        {
            target.velocity = Vector3.back * speed;
            check();
        }

        
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
}
