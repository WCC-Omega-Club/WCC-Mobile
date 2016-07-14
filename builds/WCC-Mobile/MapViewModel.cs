using System.Collections.Generic;

public class ScheduleViewModel
{
    private string image;
    public string Image
    {
        get { return this.image; }
        set { this.image = value; }
    }


    private long id;
    /// <summary>
    /// Gets or sets the unique ID for the menu
    /// </summary>
    public long Id
    {
        get { return this.id; }
        set { this.id = value; }
    }

    private string title = string.Empty;
    /// <summary>
    /// Gets or sets the name of the menu
    /// </summary>
    public string Title
    {
        get { return this.title; }
        set { this.title = value; }
    }

    public void Init(int id, string title, string image)
    {
        this.Id = id;
        this.Title = title;
        this.Image = image;
        //this.Items = DataProvider.LoadCourses();
        this.Items.RemoveRange(0, this.Items.Count - 2);
    }

    private List<ScheduleViewModel> m_Items;
    public List<ScheduleViewModel> Items
    {
        get { return this.m_Items; }
        set { this.m_Items = value; }
    }
}