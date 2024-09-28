using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUseDelegate : MonoBehaviour
{
	private Action action;

	void Start()
	{
		print(action == null);

		action += TestAction;

		action();
	}


	void TestAction()
	{
		print("TestAction");
	}




}
