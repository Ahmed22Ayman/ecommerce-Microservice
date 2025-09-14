namespace PaymentService.Models;

public class Payment
{
    public long PaymentId { get; set; }
    public long OrderId { get; set; }
    public long UserId { get; set; }
    public double Amount { get; set; }
    public string Status { get; set; } = "PENDING"; // PENDING, SUCCESS, FAILED
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
}
