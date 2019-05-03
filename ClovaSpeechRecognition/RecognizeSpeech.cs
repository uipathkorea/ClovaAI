using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Activities;
using System.IO;
using System.Collections.Specialized;
using System.ComponentModel;
using Newtonsoft;
using Newtonsoft.Json.Linq;

namespace ClovaSpeech
{
    public enum SupportLanguages
    {
        한국어,
        中国,
        English,
        日本
    }
    public sealed class RecognizeSpeech : CodeActivity
    {
        private static SupportLanguages[] _languages = 
            new SupportLanguages[] {SupportLanguages.한국어, SupportLanguages.中国, SupportLanguages.English, SupportLanguages.日本 };
        //음성 파일 정보 
        [Category("Input")]
        [RequiredArgument]
        public InArgument<string> FilePath { get; set; }
    
        //음성 언어 한국어/영어/일본어/중국어 지원 
        [Category("Input")]
        [RequiredArgument]
        public SupportLanguages Lang { get; set; }

        //Clova API를 사용하기 위한 Client ID 
        [Category("Input")]
        [RequiredArgument]
        public InArgument<string> ClientID { get; set; }
         
        //Clova API를 사용하기 위한 Client Secrete 
        [Category("Input")]
        [RequiredArgument]
        public InArgument<string> ClientSecret { get; set; }

        [Category("Output")]
        [RequiredArgument]
        public OutArgument<string> Speech { get; set; }

        //오류 코드 값 
        [Category("Output")]
        [RequiredArgument]
        public OutArgument<string> ErrorCode { get; set; }

        //실제 처리하는 로직 
        protected override void Execute(CodeActivityContext context)
        {
            //입력된 매개변수 정보 가져오기 
            string filePath = context.GetValue(this.FilePath);
            if (string.IsNullOrEmpty(filePath))
            {
                throw new Exception( "음성파일의 경로가 없습니다.");
            }
            // 언어 코드 ( Kor, Jpn, Eng, Chn )
            string lang = "Kor";
            if (Lang == SupportLanguages.한국어)
                lang = "Kor";
            else if (Lang == SupportLanguages.English)
                lang = "Eng";
            else if (Lang == SupportLanguages.中国)
                lang = "Chn";
            else if (Lang == SupportLanguages.日本)
                lang = "Jpn";

            string clientId = context.GetValue(this.ClientID);
            if (string.IsNullOrEmpty(clientId))
            {
                throw new Exception("Clova API를 호출하기 위한 Client ID 값이 없습니다.");
            }
            string clientSecret = context.GetValue(this.ClientSecret);
            if( string.IsNullOrEmpty( clientSecret))
            {
                throw new Exception("Clova API를 호출하기 위한 Client Secret 값이 없습니다.");

            }

            FileStream fs = null;
            byte[] fileData = null;
            try
            {
                fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                fileData = new byte[fs.Length];
                fs.Read(fileData, 0, fileData.Length);
                fs.Close();
            } catch ( IOException ioe)
            {
                throw new IOException(ioe.Message);
            }

            string url = $"https://naveropenapi.apigw.ntruss.com/recog/v1/stt?lang={lang}";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("X-NCP-APIGW-API-KEY-ID", clientId);
            request.Headers.Add("X-NCP-APIGW-API-KEY", clientSecret);
            request.Method = "POST";
            request.ContentType = "application/octet-stream";
            request.ContentLength = fileData.Length;
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(fileData, 0, fileData.Length);
                requestStream.Close();
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string text = reader.ReadToEnd();
            //인식된 문장 저장 
            System.Console.WriteLine(text);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                ErrorCode.Set(context, "OK");
                JObject respJson= JObject.Parse(text);
                Speech.Set(context, respJson["text"].ToString());
            }
            else
            {
                JObject respJson = JObject.Parse(text);
                ErrorCode.Set(context,  respJson["error"]["errorCode"].ToString());
                Speech.Set(context, respJson["error"]["message"].ToString());
            }
            stream.Close();
            response.Close();
            reader.Close();
        }
    }
}
