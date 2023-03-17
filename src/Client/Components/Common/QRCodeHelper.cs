using QRCoder;

namespace FSH.BlazorWebAssembly.Client.Components.Common;

public static class QRCodeHelper
{
    public static string GenerateQRCode(string qrContent, int size)
    {
        QRCodeGenerator qrGenerator = new QRCodeGenerator();
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
        PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
        byte[] qrCodeImage = qrCode.GetGraphic(size);
        return "data:image/png;base64," + Convert.ToBase64String(qrCodeImage.ToArray());
    }

    // private static string GenerateQRCode(string qrContent, int size)
    // {
    //    QRCodeGenerator qrGenerator = new QRCodeGenerator();
    //    QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
    //    BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
    //    byte[] qrCodeAsByteArr = qrCode.GetGraphic(size);
    //    var ms = new MemoryStream(qrCodeAsByteArr);
    //    return "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
    // }

}
