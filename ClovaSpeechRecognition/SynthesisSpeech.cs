using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Net;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

namespace ClovaSpeech
{
    public enum SpeakerSex {
        Female = 0,
        Male
    };
    public enum SpeakLanguages
    {
        한국어 = 0,
        English,
        中国,
        日本,
        Spain
    }

    public sealed class SynthesizeSpeech : CodeActivity //AsyncCodeActivity
    {
        private static readonly string[,] speakers = new string[5, 2] { {"mijin", "jinho"},
                                                    {"clara","matt"},
                                                    {"meimei","liangliang"},
                                                    {"shinji","shinji"},
                                                    {"carmen", "jose"} };
        //음성으로 변활된 텍스트 정보 
        [Category("Input")]
        [RequiredArgument]
        public InArgument<string> Text { get; set; }

        //음성 언어 한국어/영어/일본어/중국어/스페인어  지원 
        [Category("Input")]
        [RequiredArgument]
        public SpeakLanguages Lang { get; set; }

        //남성/여성 구분자 
        [Category("Input")]
        [RequiredArgument]
        public SpeakerSex Sex { get; set; }

        //Clova API를 사용하기 위한 Client ID 
        [Category("Input")]
        [RequiredArgument]
        public InArgument<string> ClientID { get; set; }

        //Clova API를 사용하기 위한 Client Secrete 
        [Category("Input")]
        [RequiredArgument]
        public InArgument<string> ClientSecret { get; set; }

        //음성으로 전환된 파일 경로 
        [Category("Output")]
        [RequiredArgument]
        public OutArgument<string> FilePath { get; set; }

        [Category("Output")]
        [RequiredArgument]
        public OutArgument<string> ErrorCode { get; set; }

        //음성합성 로직 처리 
        //protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        protected override void Execute(CodeActivityContext context)
        {
            //입력 받은 매개변수 가져오기 
            string text = context.GetValue(this.Text);
            if (string.IsNullOrEmpty(text))
            {
                throw new Exception("음성으로 만들어 낼  입력 텍스트가 없습니다.");
            }
            // 화자 - 언어 및 성별 구분 
            string speaker = speakers[(int)Lang, (int)Sex];
            string clientId = context.GetValue(this.ClientID);
            if (string.IsNullOrEmpty(clientId))
            {
                throw new Exception("Clova API를 호출하기 위한 Client ID 값이 없습니다.");
            }
            string clientSecret = context.GetValue(this.ClientSecret);
            if (string.IsNullOrEmpty(clientSecret))
            {
                throw new Exception("Clova API를 호출하기 위한 Client Secret 값이 없습니다.");
            }

            string url = "https://naveropenapi.apigw.ntruss.com/voice/v1/tts";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("X-NCP-APIGW-API-KEY-ID", clientId);
            request.Headers.Add("X-NCP-APIGW-API-KEY", clientSecret);
            request.Method = "POST";
            var querystr = string.Format("speaker={0}&speed=0&text={1}", speaker, text);
            //System.Console.WriteLine("POST BODY : " + querystr);
            byte[] byteDataParams = Encoding.UTF8.GetBytes(querystr);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteDataParams.Length;
            Stream st = request.GetRequestStream();
            st.Write(byteDataParams, 0, byteDataParams.Length);
            st.Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            //System.Console.WriteLine("Status : " + response.StatusCode.ToString());
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var tempPath = Path.GetTempFileName();
                tempPath = tempPath.Replace(".tmp", ".mp3");
                using (Stream output = File.OpenWrite(tempPath))
                using (Stream input = response.GetResponseStream())
                {
                    input.CopyTo(output);
                }
                FilePath.Set(context, tempPath);
                ErrorCode.Set(context, "OK");
                MediaPlayer.MediaPlayerClass _player;
                _player = new MediaPlayer.MediaPlayerClass();
                _player.FileName = tempPath;
                _player.Play();
                /*
                Action< object > action = ( object obj) =>
                {
                    MediaPlayer.MediaPlayer player = (MediaPlayer.MediaPlayer)obj;

                    while( player.PlayState == MediaPlayer.MPPlayStateConstants.mpPlaying || player.PlayState == MediaPlayer.MPPlayStateConstants.mpWaiting)
                    {
                        System.Console.WriteLine(" 상태 : {0}", player.PlayState);
                        Thread.Sleep(1000);
                    }
                };
                Task t = new Task(action, mediaPlayer);
                t.Start();
                t.Wait();
                */
            }
            else
            {
                FilePath.Set(context, "");
                ErrorCode.Set(context, "");
            }
        }
    }
}
