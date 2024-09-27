using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class MyTestCoroutine2 : MonoBehaviour
{
    // void Awake()
	// {
	// 	StartCoroutine(TestIE());
	// }

	// void Start()
	// {
	// 	StartCoroutine(TestIE());
	// }

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.A))
			StartCoroutine(TestIE());
	}

	IEnumerator TestIE()
	{
		print($"0. frame: {Time.frameCount}; time: {Time.time}");
		yield return new WaitForSeconds(1);
		print($"WaitForSeconds. frame: {Time.frameCount}; time: {Time.time}");
		yield return new WaitForFixedUpdate();
		print($"WaitForFixedUpdate. frame: {Time.frameCount}");
		yield return new WaitForEndOfFrame();
		print($"WaitForEndOfFrame. frame: {Time.frameCount}");
		yield return null;
		print($"null. frame: {Time.frameCount}");
		yield return 0;
		print($"0. frame: {Time.frameCount}");
	}

}
