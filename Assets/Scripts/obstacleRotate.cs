using UnityEngine;
using System.Collections;

public class obstacleRotate : MonoBehaviour {
	public int speed=0;
	NavMeshObstacle obs;
	void Start () {
		obs = GetComponent<NavMeshObstacle>();
	}
	void Update () {
		transform.Rotate(Vector3.up * speed*Time.deltaTime);
	}
}
