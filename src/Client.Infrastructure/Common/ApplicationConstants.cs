namespace FSH.BlazorWebAssembly.Client.Infrastructure.Common;

public static class ApplicationConstants
{
    public static readonly List<string> SupportedImageFormats = new()
    {
        ".jpeg",
        ".jpg",
        ".png"
    };
    public static readonly string StandardImageFormat = "image/jpeg";
    public static readonly long MaxImageFileSize = 1000000;

    public static readonly int MaxImageWidth = 1500;
    public static readonly int MaxImageHeight = 1500;

    public static readonly List<string> SupportedExcelFormats = new()
    {
        ".xls",
        ".xlsx"
    };
    public static readonly string StandardExcelFormat = "excel/xlsx";
    public static readonly long MaxExcelFileSize = 20000000;

    public static readonly List<string> SupportedQuizMediaFormats = new()
    {
        ".zip",
    };
    public static readonly string StandardQuizMediaFormat = "quizmedia/zip";
    public static readonly long MaxQuizMediaFileSize = 20000000;

    public static readonly List<string> SupportedDoccumentFormats = new()
    {
        ".pdf",
        ".doc",
        ".zip",
        ".rar"
    };
    public static readonly string StandardDoccumentFormat = "doccument/pdf";
    public static readonly long MaxDoccumentFileSize = 20000000;
}