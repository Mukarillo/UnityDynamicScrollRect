using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace dynamicscroll.tests
{
    public class DynamicScrollRectPlayModeTests
    {
        private DynamicScroll<MockData, MockObject> dynamicScroll;
        private DynamicScrollRect scrollRect;
        private List<MockData> data;
        private GameObject referenceObject;
       
        [SetUp]
        public void Setup() {
            
            referenceObject = GameObject.Instantiate(Resources.Load<GameObject>("ItemTest"));
            scrollRect = GameObject.Instantiate(Resources.Load<DynamicScrollRect>("VerticalScrollViewTest"));
            dynamicScroll = new DynamicScroll<MockData, MockObject>();
            
            data = new List<MockData> {
                new MockData("0x12345"),
                new MockData("0x22345"),
                new MockData("0x32345"),
                new MockData("0x42345")
            };
            
        }
        
        [UnityTest]
        public IEnumerator MoveToIndex_Success()
        {
            var destinyIndex = 3;
            dynamicScroll.Initiate(scrollRect, data,0, referenceObject);
            
            dynamicScroll.MoveToIndex(destinyIndex, 0.2f);
            
            yield return new WaitForSeconds(1);
            
            Assert.AreEqual(dynamicScroll.CentralizedObject.CurrentIndex,destinyIndex);
        }
        
        [Test]
        public void MoveToIndex_Failed() {
            var destinyIndex = 1000;
            dynamicScroll.Initiate(scrollRect, data,0, referenceObject);
            
            Assert.That(() => dynamicScroll.MoveToIndex(destinyIndex, 0.2f), 
                Throws.TypeOf<System.Exception>());
        }
        
    }
    
    public class MockObject : DynamicScrollObject<MockData> {
        
    }

    [System.Serializable]
    public class MockData {
        public string dataId;

        public MockData(string dataId) {
            dataId = this.dataId;
        }
    }
}
