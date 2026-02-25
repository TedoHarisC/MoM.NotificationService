public class MoMLevel2Dto
{
    public int MoMId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public DateTime? DueDate1 { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Dept { get; set; } = string.Empty;

    public List<string> PICs { get; set; } = new();
}