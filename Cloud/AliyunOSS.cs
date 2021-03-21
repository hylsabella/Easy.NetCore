using Aliyun.OSS;
using Easy.Common.NetCore.Enums;
using Easy.Common.NetCore.Exceptions;
using System.IO;
using System.Threading.Tasks;

namespace Easy.Common.NetCore.Cloud
{
    public class AliyunOSS
    {
        private readonly OssClient _ossClient;
        public string Endpoint { get; }
        public string _accessKeyId { get; }
        public string _accessKeySecret { get; }

        public AliyunOSS(string endpoint, string accessKeyId, string accessKeySecret)
        {
            if (string.IsNullOrWhiteSpace(endpoint)) throw new FException($"endpoint不能为空");
            if (string.IsNullOrWhiteSpace(accessKeyId)) throw new FException($"accessKeyId不能为空");
            if (string.IsNullOrWhiteSpace(accessKeySecret)) throw new FException($"accessKeySecret不能为空");

            this.Endpoint = endpoint;
            this._accessKeyId = accessKeyId;
            this._accessKeySecret = accessKeySecret;

            _ossClient = new OssClient(Endpoint, _accessKeyId, _accessKeySecret);
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="bucketName">bucketName名称</param>
        /// <param name="objectName">文件名</param>
        /// <param name="fileStream">文件流</param>
        /// <param name="fileType">文件类型</param>
        /// <returns>文件访问地址</returns>
        public string UploadFile(string bucketName, string objectName, Stream fileStream, FileType? fileType = null, bool isReplace = true)
        {
            string contentType = GetContentType(fileType);

            string fileUrl = UploadFileAsync(bucketName, objectName, fileStream, contentType, isReplace).Result;

            return fileUrl;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="bucketName">bucketName名称</param>
        /// <param name="objectName">文件名</param>
        /// <param name="fileStream">文件流</param>
        /// <param name="fileType">文件类型</param>
        /// <returns>文件访问地址</returns>
        public string UploadFile(string bucketName, string objectName, string fileFullName, FileType? fileType = null, bool isReplace = true)
        {
            if (!File.Exists(fileFullName)) throw new FException($"文件不存在{fileFullName}");

            using (var fileStream = File.Open(fileFullName, FileMode.Open))
            {
                string contentType = GetContentType(fileType);

                string fileUrl = UploadFileAsync(bucketName, objectName, fileStream, contentType, isReplace).Result;

                return fileUrl;
            }
        }

        public (bool isExist, string fileUrl) IsObjectExist(string bucketName, string objectName)
        {
            if (string.IsNullOrWhiteSpace(bucketName)) throw new FException($"bucketName不能为空");
            if (string.IsNullOrWhiteSpace(objectName)) throw new FException($"objectName不能为空");

            bool isExist = _ossClient.DoesObjectExist(bucketName, objectName);
            string fileUrl = isExist ? $"https://{bucketName}.{Endpoint}/{objectName}" : string.Empty;

            return (isExist: isExist, fileUrl: fileUrl);
        }

        private Task<string> UploadFileAsync(string bucketName, string objectName, Stream fileStream, string contentType, bool isReplace)
        {
            if (string.IsNullOrWhiteSpace(bucketName)) throw new FException($"bucketName不能为空");
            if (string.IsNullOrWhiteSpace(objectName)) throw new FException($"objectName不能为空");
            if (fileStream is null or { Length: <= 0 }) throw new FException($"fileStream不能为空");

            var tcs = new TaskCompletionSource<string>();
            string fileUrl = $"https://{bucketName}.{Endpoint}/{objectName}";

            if (!isReplace)
            {
                bool exist = _ossClient.DoesObjectExist(bucketName, objectName);

                if (exist)
                {
                    tcs.SetResult(fileUrl);
                    return tcs.Task;
                }
            }

            if (fileStream.CanSeek)
            {
                fileStream.Seek(0, SeekOrigin.Begin);
            }

            _ossClient.BeginPutObject(bucketName,
                objectName,
                fileStream,
                string.IsNullOrWhiteSpace(contentType) ? null : new ObjectMetadata { ContentType = contentType },
                asyncResult =>
                {
                    _ossClient.EndPutObject(asyncResult);
                    tcs.SetResult(fileUrl);
                },
                null);

            return tcs.Task;
        }

        private static string GetContentType(FileType? fileType)
        {
            return fileType switch
            {
                FileType.图片 => "image/jpg",
                FileType.视频 => "video/mp4",
                _ => ""
            };
        }
    }
}