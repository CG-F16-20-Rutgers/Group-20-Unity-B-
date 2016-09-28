using UnityEngine;
using System.Collections;

public class clickMove : MonoBehaviour {

	NavMeshAgent agent;
	NavMeshObstacle obstacle;
	private bool selected = false;

	public Material[] colors;

	Renderer rend;

	// Use this for initialization
	void Start () {
		rend = GetComponent<Renderer> ();
		rend.sharedMaterial = colors[0];
		agent = GetComponent<NavMeshAgent>();
		obstacle = GetComponent <NavMeshObstacle>();

		selected = false;
	}
	
	// Update is called once per frame
	void Update () {
		//toggle selection with space
		if (Input.GetKeyDown("space"))
		{
			if (!selected) {
				rend.sharedMaterial = colors [1];
				selected = true;
			}else{
				rend.sharedMaterial = colors[0];
				selected = false;
			}
		}
		//set as obstacle when unmoving
		if(!agent.pathPending){
			if(agent.remainingDistance<=agent.stoppingDistance){
				if(!agent.hasPath || agent.velocity.sqrMagnitude ==0f){
					obstacle.enabled=true;
					agent.stoppingDistance = 1;
					agent.enabled=false;
				}else{
					obstacle.enabled=false;
					agent.enabled=true;
				}
			}
		}
		if (Input.GetMouseButtonDown (0)) {
			//locate click position
			Ray clickRay = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast (clickRay, out hit, 100)) {
				if (selected && !hit.collider.CompareTag ("agent")) {
					obstacle.enabled=false;
					agent.enabled=true;
					agent.SetDestination (hit.point);
				}
			}
		}
	}
	//click to select
	void OnMouseDown(){
		if (!selected) {
			rend.sharedMaterial = colors [1];
			selected = true;
		}else{
			rend.sharedMaterial = colors[0];
			selected = false;
		}
	}
	//increase buffer zone when colliding with other agents
	void OnCollisionEnter(Collision otherObject)
	{
		if (otherObject.gameObject.tag == "agent") {
			agent.stoppingDistance += 1;
		}
	}

}
