# UnityDynamicScrollRect
An optimized approach to lists with dozens of elements.

<p align="center">
  <img src="https://github.com/Mukarillo/UnityDynamicScrollRect/blob/master/ReadmeAssets/dynamic_list_example.gif?raw=true" alt="Example"/>
</p>

## How to use
*you can find a pratical example inside this repository in DynamicScrollScene scene*

### 1 - Create a class to store all the information that each element of the list will need.
```c#
public class ExampleData
{
      public int postId;
      public int id;
      public string name;
      public string email;
      public string body;
}
```
### 2 - Create a class that extends `DynamicScrollObject<ExampleData>` and implement its abstract members (make sure to call `base.updateScrollObject(item, index);`) and set the object width and height in `currentWidth` and `currentHeight`.
```c#
public class ExampleDynamicObject : DynamicScrollObject<ExampleData>
{
  public override float currentHeight { get; set; }
  public override float currentWidth { get; set; }

  private Text idText;
  private Text nameEmailText;
  private Text bodyText;

  public void Awake()
  {
    currentHeight = GetComponent<RectTransform>().rect.height;
    currentWidth = GetComponent<RectTransform>().rect.width;

    idText = transform.Find("PostId").GetComponent<Text>();
    nameEmailText = transform.Find("NameEmail").GetComponent<Text>();
    bodyText = transform.Find("Body").GetComponent<Text>();         
  }

  public override void updateScrollObject(ExampleData item, int index)
  {
    base.updateScrollObject(item, index);

    idText.text = item.id.ToString();
    nameEmailText.text = string.Format("{0} ({1})", item.name, item.email);
    bodyText.text = item.body;
  }
}
```
### 3 - Create a class to initiate the DynamicList (use DynamicScrollRect instead of ScrollRect)
```c#
public class ExampleScroll : MonoBehaviour
{
  public DynamicScrollRect verticalScroll;
  public GameObject referenceObject;

  private DynamicScroll<ExampleData, ExampleDynamicObject> mVerticalDynamicScroll = new DynamicScroll<ExampleData, ExampleDynamicObject>();

  public IEnumerator Start()
  {
    WWW www = new WWW(@"https://jsonplaceholder.typicode.com/comments");
    yield return www;
    var data = JsonHelper.getJsonArray<ExampleData>(www.text);

    mVerticalDynamicScroll.spacing = 5f;
    mVerticalDynamicScroll.Initiate(verticalScroll, data, 0, referenceObject);
  }      
}
```

## DynamicScroll<T, T1> `public` overview
### Properties
|name  |type  |description  |
|--|--|--|
|`spacing` |**float** |*Value that represent the spacing between elements of the list*  |
|`centralizeOnStop` |**bool** |*If the list should centralize the closest element to the center of the viewport after stop moving*  |
|`objectPool` |**readonly Pooling < T1 >** |*The elements of the list*  |
|`OnDragEvent` |**Action < Vector2 >** |*Event that triggers whenever the user scrolls the list, the parameter represent the velocity of the drag*  |
|`OnBeginDragEvent` |**UnityEvent < PointerEventData >** |*Event that triggers in the first frame of dragging*  |
|`OnEndDragEvent` |**UnityEvent < PointerEventData >** |*Event that triggers in the last frame of dragging*  |

### Methods

> `dynamicScroll.Initiate`
- *Description*: Initiate the scroll rect with `objReference` objects applying `infoList` data.

- *Parameters*:

|name  |type  |description  |
|--|--|--|
|`scrollRect` |**ScrollRect** |*a reference to the scroll rect*  |
|`infoList` |**T[]** |*the list with the data information*  |
|`startIndex` |**int** |*the item of index `startindex` will be the first element of the list*  |
|`objReference` |**GameObject** |*a reference of the object that will be inside the list*  |
|`createMoreIfNeeded` |**bool** |*if the list needs more itens, it will create more if `createMoreIfNeeded` == true*  |
|`forceAmount` |**int?** |*if setted, it will force `forceAmount` objects to be created at start*  |


> `dynamicScroll.ChangeList`
- *Description*:
Change the current list of the scroll rect.

- *Parameters* :

|name  |type  |description  |
|--|--|--|
|`infoList` |**T[]** |*the list with the data information*  |
|`startIndex` |**int** |*the item of index `startindex` will be the first element of the list. If -1, the current index will be setted.*  |
|`resetContentPosition` |**bool** |*reset list position*  |


> `dynamicScroll.RefreshPosition`
- *Description*: Repaint the whole scroll rect. This is useful if any item inside the scroll rect changes the size (`currentWidth` and `currentHeight`).


> `dynamicScroll.ToggleScroll`
- *Description*: Enable or Disable the ability to scroll the list.

- *Parameters* :

|name  |type  |description  |
|--|--|--|
|`active` |**bool** |*enable or Disable the ability to scroll the list*  |


> `dynamicScroll.CanMove`
- *Description*: Returns true if all directions send thro parameter are available.

- *Parameters* :

|name  |type  |description  |
|--|--|--|
|`directions` |**ScrollDirection** |*Enum flag with all the directions you want to know if are available*  |


> `dynamicScroll.MoveToIndex`
- *Description*: Tweens the content to centralize the object of index specified in the parameters.

- *Parameters* :

|name  |type  |description  |
|--|--|--|
|`i` |**int** |*Index of the element to be centralized*  |
|`totalTime` |**float?** |*Total time to the animation happen (if you choose to input this value, the next one will be ignored)*  |
|`timePerElement` |**float?** |*This value will be multiplied by the difference between the current centralized element and the target element to get the totalTime*  |

> `dynamicScroll.GetCentralizedObject`
- *Description*: Returns the closest element to the center of the viewport.

> `dynamicScroll.GetLowest`
- *Description*: Returns the most left (if horizontal scroll) or most bottom (if vertical scroll) T1 object.

> `dynamicScroll.GetHighest`
- *Description*: Returns the most right (if horizontal scroll) or most upper (if vertical scroll) T1 object.
