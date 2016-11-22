using UnityEngine;
using System;
using System.Collections;
using TreeSharpPlus;

public class MainBehaviorTree : MonoBehaviour
{
	public Transform wander1;
	public Transform wander2;
	public Transform wander3;
	public Transform patrol1;
	public Transform patrol2;
	public Transform patrol3;
	public Transform patrol4;
	public Transform positionToMove;
	public GameObject participant;
	public GameObject actor;
	public GameObject police;
	public Transform mousepoint;

	private BehaviorAgent behaviorAgent;
	// Use this for initialization
	void Start ()
	{
		behaviorAgent = new BehaviorAgent (this.BuildTreeRoot ());
		BehaviorManager.Instance.Register (behaviorAgent);
		behaviorAgent.StartBehavior ();
	}

	// Update is called once per frame
	void Update ()
	{
		if(Input.GetMouseButtonDown (0))
		{
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;

			if(Physics.Raycast(ray, out hit, 100))
			{
				positionToMove.position = (Val.V (() =>  hit.point)).Value;
				UpdatePlayerPosition();
			}
		}
	}

	protected Node ST_ApproachAndWait(GameObject participant, Transform target)
	{
		Val<Vector3> position = Val.V (() => target.position);
		return new Sequence( participant.GetComponent<BehaviorMecanim>().Node_GoTo(position), new LeafWait(1000));
	}

	//get close to and face target point
	protected Node ST_ApproachAndOrient(GameObject participant, Transform target)
	{
		Val<Vector3> position = Val.V (() => target.position);
		return new Sequence( 
			participant.GetComponent<BehaviorMecanim>().Node_GoToUpToRadius(position, 2.0f), 
			new LeafWait(1000),
			participant.GetComponent<BehaviorMecanim>().Node_OrientTowards(position),
			new LeafWait(1000)
			);
	}
		

	protected Node BuildTreeRoot()
	{
		Val<float> pp = Val.V (() => police.transform.position.z);
		Val<Vector3> position1 = Val.V (() => participant.transform.position);
		Val<Vector3> position2 = Val.V (() => actor.transform.position);
		Func<bool> detect = () => (police.transform.position.z > 9.9f);
		Func<bool> act = () => (police.transform.position.z < 10.1f);
		//actors roam at random
		Node roaming1 = new DecoratorLoop (
			new Sequence(
				new SequenceShuffle(
					this.ST_ApproachAndWait(this.participant, this.wander1),
					this.ST_ApproachAndWait(this.participant, this.wander2),
					this.ST_ApproachAndWait(this.participant, this.wander3))));
		Node roaming2 = new DecoratorLoop(
			new Sequence(
				new SequenceShuffle(
					this.ST_ApproachAndWait(this.actor, this.wander3),
					this.ST_ApproachAndWait(this.actor, this.wander2),
					this.ST_ApproachAndWait(this.actor, this.wander1))));
		Node patrol1 = new DecoratorLoop(
			new Sequence(
				this.ST_ApproachAndWait(this.police, this.patrol4),
				this.ST_ApproachAndWait(this.police, this.patrol3),
				this.ST_ApproachAndWait(this.police, this.patrol2)));
		Node patrol2 = new DecoratorLoop(
			new Sequence(
				this.ST_ApproachAndWait(this.police, this.patrol2),
				this.ST_ApproachAndWait(this.police, this.patrol1),
				this.ST_ApproachAndWait(this.police, this.patrol4)));
		Node trigger = new DecoratorLoop (new LeafAssert (act));
		Node meetTrigger = new DecoratorLoop (new LeafAssert (detect));
		Node hide1 = new Sequence(
				new SequenceShuffle(
					this.ST_ApproachAndWait(this.participant, this.wander1),
					this.ST_ApproachAndWait(this.participant, this.wander2),
					this.ST_ApproachAndWait(this.participant, this.wander3)));
		Node meetup = new DecoratorLoop (
						new Sequence (
							ST_GreetActors(participant, actor),
							new LeafWait(1000),
							ST_ConverseActors(participant, actor))
					);

		Node clicker = new DecoratorLoop (
			new Sequence(this.ST_ApproachAndWait(this.police, this.positionToMove)));

		//ROOT NODE
		Node root = new DecoratorLoop (new SequenceParallel(
			
			new DecoratorForceStatus (RunStatus.Success, 
				new SequenceParallel(roaming1, roaming2, clicker))
			
			)
			);
		return root;
	}

	protected Node ST_GreetActors(GameObject actor1, GameObject actor2)
	{
		return new SequenceParallel (
			ST_ApproachAndOrient(actor1, actor2.transform),
			ST_ApproachAndOrient(actor2, actor1.transform),
			actor1.GetComponent<BehaviorMecanim>().ST_PlayHandGesture("WAVE", 2000),
			actor2.GetComponent<BehaviorMecanim>().ST_PlayHandGesture("WAVE", 2000)
			);
	}

	protected Node ST_ConverseActors(GameObject actor1, GameObject actor2)
	{
		return new DecoratorLoop(
			new Sequence (
			actor1.GetComponent<BehaviorMecanim>().ST_PlayHandGesture("THINK", 3000),
			actor2.GetComponent<BehaviorMecanim>().ST_PlayHandGesture("CLAP", 3000)
			)
		);
	}

	protected Node ST_DetectActors(GameObject actor1, GameObject actor2)
	{
		Val<Vector3> position1 = Val.V (() => actor1.transform.position);
		Val<Vector3> position2 = Val.V (() => actor2.transform.position);
		float detectdist = 5.0f;
		float xc = (position1.Value - position2.Value).magnitude;
		if ( xc <= detectdist) {
			return new Sequence (
				ST_GreetActors (participant, actor),
				new LeafWait (1000),
				ST_ConverseActors (participant, actor));
		}else{
			return new LeafWait(1000);
		}
	}
		
	void PointOnMap()
	{
		Val<Ray> ray = Val.V (() => Camera.main.ScreenPointToRay (Input.mousePosition));
		RaycastHit hit;
		Physics.Raycast (ray.Value, out hit, 100);
		Val.V (() => Physics.Raycast (ray.Value, out hit, 100));
		positionToMove.position = (Val.V (() =>  hit.point)).Value;
		UpdatePlayerPosition();
	}

	void UpdatePlayerPosition()
	{
		Val<Vector3> position = positionToMove.position;
		participant.GetComponent<SteeringController> ().Target = position.Value;
	}
		
}

