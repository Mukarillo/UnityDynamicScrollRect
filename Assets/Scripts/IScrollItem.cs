namespace dynamicscroll
{
    public interface IScrollItem
    {
        void reset();
        int currentIndex { get; set; }
    }
}