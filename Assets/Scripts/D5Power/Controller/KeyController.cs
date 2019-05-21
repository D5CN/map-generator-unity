using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyController : MonoBehaviour {

    public Rigidbody target = null;
    // Use this for initialization
    private float speed = 4f;
    private Vector2 dir = new Vector2();
	void Start () {
        target = GetComponent<Rigidbody>();
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
        }else if(dir.x == -1){
            target.velocity = Vector3.left * speed;
        }

        if (dir.y == 1)
        {
            target.velocity = Vector3.forward * speed;
        }
        else if (dir.y == -1)
        {
            target.velocity = Vector3.back * speed;
        }
    }
}
