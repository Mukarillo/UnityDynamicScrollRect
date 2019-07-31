using dynamicscroll;
using UnityEngine.UI;
using UnityEngine;

namespace example
{
	public class ExampleDynamicObject : DynamicScrollObject<ExampleData>
	{
		public override string objectName
		{
			get
			{
				return "ExampleObject";
			}
		}

		private Text idText;
		private Text nameEmailText;
		private Text bodyText;

		public void Awake()
		{
			idText = transform.Find("PostId").GetComponent<Text>();
			nameEmailText = transform.Find("NameEmail").GetComponent<Text>();
			bodyText = transform.Find("Body").GetComponent<Text>();         
		}

		public override void UpdateScrollObject(ExampleData item, int index)
		{
			base.UpdateScrollObject(item, index);

			idText.text = item.id.ToString();
			nameEmailText.text = string.Format("{0} ({1})", item.name, item.email);
			bodyText.text = item.body;
		}
	}
}
