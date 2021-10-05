namespace pooling
{
    public interface IPooling
    {
		string objectName { get; }
		bool isUsing { get; set; }
        void OnCollect();
        void OnRelease();
    }
}