using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using dynamicscroll;

namespace example
{
	public class ExampleScroll : MonoBehaviour
	{
		public ScrollRect verticalScroll;
		public ScrollRect horizontalScroll;
		public GameObject referenceObject;

		private ExampleData[] mData;
		private AssukarDynamicScroll<ExampleData, ExampleDynamicObject> mVerticalDynamicScroll = new AssukarDynamicScroll<ExampleData, ExampleDynamicObject>();
		private AssukarDynamicScroll<ExampleData, ExampleDynamicObject> mHorizontalDynamicScroll = new AssukarDynamicScroll<ExampleData, ExampleDynamicObject>();

		public IEnumerator Start()
		{
			WWW www = new WWW(@"https://jsonplaceholder.typicode.com/comments");
			yield return www;
			mData = JsonHelper.getJsonArray<ExampleData>(www.text);

			mVerticalDynamicScroll.spacing = 5f;
			mVerticalDynamicScroll.Initiate(verticalScroll, mData, 0, referenceObject, 7);

			mHorizontalDynamicScroll.spacing = 5f;
			mHorizontalDynamicScroll.Initiate(horizontalScroll, mData, 0, referenceObject, 3);
		}      
	}
}
