namespace MoM.NotificationService.Dto;

public class MoMLevel2Dto
{
    public int MoMId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string CorrectiveAction { get; set; } = string.Empty;
    public DateTime? DueDate1 { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Dept { get; set; } = string.Empty;
    public string LatestProgress { get; set; } = "-";

    public List<string> PICs { get; set; } = new();
    public List<MoMAttachmentDto> Attachments { get; set; } = new();
}

public class Level2PICRawDto
{
    public int MoMId { get; set; }
    public string email { get; set; } = string.Empty;
    public string nama { get; set; } = string.Empty;
}

public class AttachmentRawDto
{
    public int AttachmentId { get; set; }
    public int MoMId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
}