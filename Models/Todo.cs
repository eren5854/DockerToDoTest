namespace DockerToDoTest.Models;

public sealed class Todo
{
    public Todo()
    {
        Id = Guid.NewGuid();
    }
    public Guid Id { get; set; }
    public string Note { get; set; } = string.Empty;
    public bool Complated { get; set; }
    public bool IsDeleted { get; set; } = false;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string? UpdatedBy { get; set;}
    public DateTime? UpdatedDate { get; set; }
}
