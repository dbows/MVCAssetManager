using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.S3;
using System.Configuration;
using System.Collections.Specialized;
using Amazon.S3.Model;
using System.IO;

namespace AssetManager
{
    public class Helpers
    {
    }

    public class AmazonHelper
    {
        public static string DocExtensions() {
            var configVal = ConfigurationManager.AppSettings["Assets_Amazon_DocExtensions"];
            return String.IsNullOrWhiteSpace(configVal) ? @"(?i)(\.|\/)(xlsx?|pptx?|pdf|docx?|txt|csv|vsd|psd|ai)$" : configVal;
        }

        public static string ImgExtensions() { 
            var configVal = ConfigurationManager.AppSettings["Assets_Amazon_ImageExtensions"];
            return String.IsNullOrWhiteSpace(configVal) ? @"(?i)(\.|\/)(gif|jpe?g|png)$" : configVal;             
        }
        public static string MediaExtensions() { 
            var configVal = ConfigurationManager.AppSettings["Assets_Amazon_MediaExtensions"];
            return String.IsNullOrWhiteSpace(configVal) ? @"(?i)(\.|\/)(aac|mp3|flac|mov|mpeg4|m4p)$" : configVal;
        }
        public static AmazonS3Client GetS3Client()
        {
            if (!checkRequiredFields())
            {
                return null;
            }

            var appConfig = ConfigurationManager.AppSettings;
            string accessKeyID = appConfig["Assets_Amazon_Key"];
            string secretAccessKeyID = appConfig["Assets_Amazon_Secret"];
            var client = new AmazonS3Client(accessKeyID, secretAccessKeyID);       
            return client;
        }

        public static bool checkRequiredFields()
        {
            NameValueCollection appConfig = ConfigurationManager.AppSettings;

            if (string.IsNullOrEmpty(appConfig["Assets_Amazon_Key"]))
            {
                Console.WriteLine("AWSAccessKey was not set in the App.config file.");
                return false;
            }
            if (string.IsNullOrEmpty(appConfig["Assets_Amazon_Secret"]))
            {
                Console.WriteLine("AWSSecretKey was not set in the App.config file.");
                return false;
            }
            return true;
        }



        public static void WriteObjectRequest(string bucketName, string fileName, Stream fileContent, AmazonS3Client s3Client)
        {
                if (String.IsNullOrEmpty(fileName))
                {
                    return;
                }

                PutObjectRequest putObjectRequest = new PutObjectRequest();
              
                putObjectRequest.WithBucketName(bucketName)
                    .WithKey(fileName)
                    .WithStorageClass(S3StorageClass.Standard)
                    .WithCannedACL(S3CannedACL.PublicRead)
                    .WithInputStream(fileContent);
                    

                S3Response response = s3Client.PutObject(putObjectRequest);
                response.Dispose();
            

        }


        public static void DeletingAnObject(AmazonS3Client client, string bucketName, string keyName)
        {
        
                DeleteObjectRequest request = new DeleteObjectRequest();
                request.WithBucketName(bucketName)
                    .WithKey(keyName);
                S3Response response = client.DeleteObject(request);
                response.Dispose();

        }

     }

}