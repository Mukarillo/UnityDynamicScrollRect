using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace dynamicscroll.tests {
    public class DynamicScrollRectTests {

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

        [Test]
        public void InitiateScroll_Success() {
            dynamicScroll.Initiate(scrollRect, data,0, referenceObject);
            
            Assert.Greater(dynamicScroll.objectPool.Count, 0);
            Assert.AreEqual(dynamicScroll.RawDataList.Count, data.Count);
        }
        
        [Test]
        public void InitiateScroll_WithInvalid_ScrollRect_Failed() {
            
            Assert.That(() => dynamicScroll.Initiate(null, data,0, referenceObject), 
                Throws.TypeOf<System.Exception>());
        }
        
        [Test]
        public void InitiateScroll_WithInvalid_Index_Failed() {

            Assert.That(() => dynamicScroll.Initiate(scrollRect, data,10000, referenceObject), 
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