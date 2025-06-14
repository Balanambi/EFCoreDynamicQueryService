// Sample entity for demonstration
public class MyEntity
{
    public int Id { get; set; }
    public int IntColumn { get; set; } // This maps to nvarchar in DB
    public string Name { get; set; }
    public DateTime CreatedDate { get; set; }
}