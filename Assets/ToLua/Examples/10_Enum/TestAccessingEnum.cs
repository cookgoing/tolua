using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAccessingEnum : MonoBehaviour
{

	enum EN
	{
		A = 0, B, C, D, E, F,
	}

	[ContextMenu("TestEnum")]
	void TestEnum()
	{
		EN a = EN.A;

		print(a.Equals(EN.A));//true
		print(a.Equals(0));//false
	}

}
