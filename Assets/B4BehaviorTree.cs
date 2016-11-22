using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using TreeSharpPlus;

public class B4BehaviorTree : MonoBehaviour
{
	
	public Transform[] hidingSpots;
	public Transform playerPosition;
	public Transform countspot;
	public Transform meetspot;
	public Transform winspot;
	public GameObject friend1;
	public GameObject friend2;
	public GameObject friend3;
	public GameObject friend4;
	public GameObject playagent;

	public Text scoretext;
	public Text wintext;
	public Text timer;

	public int score;
	public float timeleft;

	public bool f1found;
	public bool f2found;
	public bool f3found;
	public bool f4found;
	public bool timeup;

	private BehaviorAgent behaviorAgent;
	// Use this for initialization
	void Start ()
	{
		wintext.text = "";
		f1found = false;
		f2found = false;
		f3found = false;
		f4found = false;
		timeup = false;
		score = 0;
		this.updateScore();
		behaviorAgent = new BehaviorAgent (this.BuildTreeRoot ());
		BehaviorManager.Instance.Register (behaviorAgent);
		behaviorAgent.StartBehavior ();
		timeleft = 300.0f;
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
				//if (hit.transform.tag == "friend") {
				//	f1found = true;
				//	score++;
				//	updateScore ();
				//}
				playerPosition.position = (Val.V (() =>  hit.point)).Value;
				UpdatePlayerPosition();
			}
		}
		timeleft -= Time.deltaTime;
		int timei = (int)Math.Round(timeleft);
		if (timei <= 0) {
			timei = 0;
			timeup = true;
			wintext.text = "Time's up!";
		}
		timer.text = "time remaining: " + timei;
	}

	void updateScore(){
		score = 0;
		if (f1found)
			score++;
		if (f2found)
			score++;
		if (f3found)
			score++;
		if (f4found)
			score++;
		scoretext.text = "Friends found: " + score;
		if (score == 4)
			scoretext.text = "You found everyone!";
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

		//part one ai controlled
		Node searcher = new DecoratorLoop (
			new Sequence(
				new SequenceShuffle(
					this.ST_ApproachAndWait(playagent, hidingSpots[UnityEngine.Random.Range(0,hidingSpots.Length-1)]),
					this.ST_ApproachAndWait(playagent, hidingSpots[UnityEngine.Random.Range(0,hidingSpots.Length-1)]),
					this.ST_ApproachAndWait(playagent, hidingSpots[UnityEngine.Random.Range(0,hidingSpots.Length-1)]),
					this.ST_ApproachAndWait(playagent, hidingSpots[UnityEngine.Random.Range(0,hidingSpots.Length-1)]),
					this.ST_ApproachAndWait(playagent, hidingSpots[UnityEngine.Random.Range(0,hidingSpots.Length-1)]),
					this.ST_ApproachAndWait(playagent, hidingSpots[UnityEngine.Random.Range(0,hidingSpots.Length-1)]),
					this.ST_ApproachAndWait(playagent, hidingSpots[UnityEngine.Random.Range(0,hidingSpots.Length-1)])
				))  );

		//player control part2
		Node clicker = new DecoratorLoop (
			new Sequence(this.ST_ApproachAndWait(playagent, playerPosition)));

		//start nodes
		Node startcount = new Sequence (
			this.ST_ApproachAndWait(this.playagent, playerPosition),
			playagent.GetComponent<BehaviorMecanim>().ST_PlayHandGesture("WAVE", 2000),
			this.ST_ApproachAndWait(this.playagent, countspot), 
			new LeafWait(9000),
			this.ST_ApproachAndWait(this.playagent, playerPosition));

		//ROOT NODE-------------------------------------------------------------------------------------------------------------
		Node root = new DecoratorLoop (
			
			new SequenceParallel(
				//use searcher for part1, clicker for part2
				new Sequence(startcount, searcher), 
				this.friendNode(friend1),
				this.friendNode(friend2),
				this.friendNode(friend3),
				this.friendNode(friend4),
				this.AssertFoundAll()
			)
			
		);

		return root;
	}

	protected Node friendNode(GameObject friend){
		return new Sequence (
			new LeafWait(8000),
			this.ST_ApproachAndWait(friend, hidingSpots[UnityEngine.Random.Range(0,hidingSpots.Length-1)]),
			this.AssertFriendFound(friend),
			new DecoratorLoop(friend.GetComponent<BehaviorMecanim>().ST_PlayHandGesture("CLAP", 3000))
		);
	}

	protected Node ST_GreetActors(GameObject actor1, GameObject actor2)
	{
		return new Sequence (
			this.ST_ApproachAndOrient(actor2, actor1.transform),
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

	bool DetectActors(GameObject detector)
	{
		Collider[] Colliders;
		Colliders = Physics.OverlapSphere(detector.transform.position, 3);
		for(int i =0; i<Colliders.Length; i++){
			if(Colliders[i].gameObject.tag == "Player"){
				if (detector.Equals(this.friend1)) {
					f1found = true;
				}
				if (detector.Equals(this.friend2)) {
					f2found = true;
				}
				if (detector.Equals(this.friend3)) {
					f3found = true;
				}
				if (detector.Equals(this.friend4)) {
					f4found = true;
				}
				updateScore ();
				return true;
			}
		}
		return false;
	}
		


	//mouse controls
	void PointOnMap()
	{
		Val<Ray> ray = Val.V (() => Camera.main.ScreenPointToRay (Input.mousePosition));
		RaycastHit hit;
		Physics.Raycast (ray.Value, out hit, 100);
		Val.V (() => Physics.Raycast (ray.Value, out hit, 100));
		playerPosition.position = (Val.V (() =>  hit.point)).Value;
		UpdatePlayerPosition();
	}

	void UpdatePlayerPosition()
	{
		Val<Vector3> position = playerPosition.position;
		playagent.GetComponent<SteeringController> ().Target = position.Value;
	}

	protected Node AssertFriendFound(GameObject friend)
	{
		return new DecoratorLoop(new Sequence(new DecoratorInvert(new DecoratorLoop (new DecoratorInvert(new Sequence(this.CheckFound (friend))))),
			this.friend1Found(friend)
			//new LeafWait(1000)
		));
	}

	protected Node CheckFound(GameObject friend)
	{
		return new LeafAssert (() => DetectActors(friend));
	}

	protected Node friend1Found(GameObject friend){
		//wintext.text="you found "+friend.name;
		return new Sequence(
			playagent.GetComponent<BehaviorMecanim>().ST_PlayHandGesture("WAVE", 2000),
			this.ST_ApproachAndOrient(friend, playagent.transform),
			friend.GetComponent<BehaviorMecanim>().ST_PlayHandGesture("WAVE", 2000),
			this.ST_ApproachAndWait(friend, meetspot),
			friend.GetComponent<BehaviorMecanim>().Node_OrientTowards(winspot.position)
		);
	}

	protected Node AssertFoundAll()
	{
		return new DecoratorLoop(new Sequence(new DecoratorInvert(new DecoratorLoop (new DecoratorInvert(new Sequence(this.CheckFoundAll ())))),
			new LeafWait(5000),
			this.endgame()
		));
	}

	protected Node CheckFoundAll()
	{
		return new LeafAssert (() => score==4);
	}

	protected Node AssertTimeCheck()
	{
		return new DecoratorLoop(new Sequence(new DecoratorInvert(new DecoratorLoop (new DecoratorInvert(new Sequence(this.CheckTime ())))),
			new LeafWait(5000),
			this.endgame()

		));
	}

	protected Node CheckTime()
	{
		return new LeafAssert (() => timeleft<=0.1f);
	}

	protected Node endgame(){
		if(score==4)
			playerPosition = winspot;
		return new Sequence (
			this.ST_ApproachAndWait (playagent, winspot),
			playagent.GetComponent<BehaviorMecanim>().Node_OrientTowards(meetspot.position),
		
			playagent.GetComponent<BehaviorMecanim> ().ST_PlayHandGesture ("SURPRISED", 3000)
			
		);
	}
}

