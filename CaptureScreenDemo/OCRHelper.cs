using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Media.Imaging;
using TencentCloud.Common;
using TencentCloud.Common.Profile;
using TencentCloud.Ocr.V20181119;
using TencentCloud.Ocr.V20181119.Models;

namespace CaptureScreenDemo {
    class OCRHelper {
        public static string SecretID = "";
        public static string SecretKey = "";

        public static string Img2Base64(BitmapSource source) {
            using (MemoryStream ms = new MemoryStream()) {
                Bitmap bmp = new Bitmap(source.PixelWidth, source.PixelHeight, PixelFormat.Format32bppPArgb);
                BitmapData data = bmp.LockBits(new Rectangle(System.Drawing.Point.Empty, bmp.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
                source.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
                bmp.UnlockBits(data);

                bmp.Save(ms, ImageFormat.Jpeg);
                byte[] arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);
                ms.Close();
                return "data:image/jpg;base64," + Convert.ToBase64String(arr);
            }
        }

        public static string recognize(string img) {
            try {
                Credential cred = new Credential {
                    SecretId = SecretID,
                    SecretKey = SecretKey
                };

                ClientProfile clientProfile = new ClientProfile();
                HttpProfile httpProfile = new HttpProfile();
                httpProfile.Endpoint = ("ocr.tencentcloudapi.com");
                clientProfile.HttpProfile = httpProfile;

                OcrClient client = new OcrClient(cred, "ap-beijing", clientProfile);
                GeneralBasicOCRRequest req = new GeneralBasicOCRRequest();
                string strParams = String.Format("{{\"ImageBase64\":\"{0}\"}}", img);
                req = GeneralBasicOCRRequest.FromJsonString<GeneralBasicOCRRequest>(strParams);
                GeneralBasicOCRResponse resp = client.GeneralBasicOCRSync(req);

                JavaScriptSerializer js = new JavaScriptSerializer();
                var dic = js.Deserialize<Dictionary<string, object>>(AbstractModel.ToJsonString(resp));
                var list = (System.Collections.ArrayList)dic["TextDetections"];
                var output = new StringBuilder();
                foreach (var item in list) {
                    output.Append(((Dictionary<string, object>)item)["DetectedText"]).Append("\n");
                }

                return output.ToString();
            }
            catch (Exception e) {
                return e.ToString();
            }
        }
    }
}
