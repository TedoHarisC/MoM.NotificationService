namespace MoM.NotificationService.Dto;

public class MoMAttachmentDto
{
    public int AttachmentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;

    public bool IsImage => !string.IsNullOrEmpty(FileType) &&
                           FileType.ToLower() is "png" or "jpg" or "jpeg" or "gif";

    public string FullUrl => !string.IsNullOrEmpty(FilePath)
                             ? $"http://10.2.182.24{FilePath}"
                             : string.Empty;
}
