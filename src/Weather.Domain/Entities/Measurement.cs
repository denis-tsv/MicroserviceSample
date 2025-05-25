namespace Weather.Domain.Entities;

public class Measurement
{
    public int Id { get; set; }
    public string City { get; set; } = null!;
    public float Temperature { get; set; }
    public DateOnly Date { get; set; }
}