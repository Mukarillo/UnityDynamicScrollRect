using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using dynamicscroll;

namespace example
{
	public class ExampleScroll : MonoBehaviour
	{
		public DynamicScrollRect verticalScroll;
		public DynamicScrollRect horizontalScroll;
		public GameObject referenceObject;

		private ExampleData[] mData;
		private DynamicScroll<ExampleData, ExampleDynamicObject> mVerticalDynamicScroll = new DynamicScroll<ExampleData, ExampleDynamicObject>();
		private DynamicScroll<ExampleData, ExampleDynamicObject> mHorizontalDynamicScroll = new DynamicScroll<ExampleData, ExampleDynamicObject>();

		public IEnumerator Start()
		{
			WWW www = new WWW(@"https://jsonplaceholder.typicode.com/comments");
			yield return www;
			mData = JsonHelper.getJsonArray<ExampleData>(www.text);

			mVerticalDynamicScroll.spacing = 5f;
			mVerticalDynamicScroll.Initiate(verticalScroll, mData, 0, referenceObject);

			mHorizontalDynamicScroll.spacing = 5f;
			mHorizontalDynamicScroll.Initiate(horizontalScroll, mData, 0, referenceObject);
		}
	}
}
