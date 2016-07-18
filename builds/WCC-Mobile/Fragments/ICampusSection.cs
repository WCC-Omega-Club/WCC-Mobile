namespace WCCMobile
{
    public interface ICampusSection
    {
		string Name { get; }
        string Title { get; }
        void RefreshData();
    }
}