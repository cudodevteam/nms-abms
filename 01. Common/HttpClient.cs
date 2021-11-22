using System;
using System.Net;
using System.IO;
using Microsoft.Extensions.Logging;

namespace nmsAbmsWeb {
    public class HttpClient {
        private ILogger logger;
        public string contentType = "text/plain";
        public string responseText;
        public HttpStatusCode httpStatusCode;
        public int timeout = 3 * 1000;
        public int continueTimeout = 3 * 1000;
        public int readWriteTimeout = 3 * 1000;

        public HttpClient( ILogger logger ) {
            this.logger = logger;
        }

        public bool post(string url, string data) {
            bool isRet = false;

            try {       
                if( url == null || url == string.Empty ){
                    throw new Exception( "param post url is empty" );
                }

                logger.LogInformation( String.Format( "HttpClient POST | [ Url : {0} ]", url ) );

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create( url );
                request.Method = "POST";
                request.KeepAlive = false;                
                request.Timeout = this.timeout;      
                request.ContinueTimeout = this.continueTimeout;
                request.ReadWriteTimeout = this.readWriteTimeout;
                request.ContentType = this.contentType;      

                do{
                    if( data == null || data == String.Empty ){
                        break;
                    }

                    logger.LogInformation( String.Format( "HttpClient POST | [ Data : {0} ]", data ) );

                    byte[] byteData = System.Text.Encoding.UTF8.GetBytes( data );

                    using( Stream dataStream = request.GetRequestStream( ) ) {
                        dataStream.Write( byteData, 0, byteData.Length );
                    }

                }while( false );

                logger.LogInformation( String.Format( "HttpClient POST | [ ContentType : {0} ]", request.ContentType ) );

                using( HttpWebResponse response = (HttpWebResponse)request.GetResponse( ) ) {
                    
                    using (var reader = new System.IO.StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8 ))
                    {
                        this.responseText = reader.ReadToEnd();
                    }

                    this.httpStatusCode = response.StatusCode;                    
                }

                logger.LogInformation( "HttpClient POST Resp Result [ Url : {0} ] [ Status Code : {1} ] [ responseText : {2} ]", url, this.httpStatusCode.ToString(), this.responseText );

                isRet = true;

            } catch (System.Exception ex) {
                Util.LogError( logger, ex );
                throw ex;
            }

            return isRet;
        }

        public bool get(string url) {
            bool isRet = false;

            try {       
                if( url == null || url == string.Empty ){
                    throw new Exception( "param get url is empty" );
                }

                logger.LogInformation( String.Format( "HttpClient GET | [ Url : {0} ]", url ) );

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create( url );
                request.Method = "GET";
                request.KeepAlive = false;                
                request.Timeout = this.timeout;      
                request.ContinueTimeout = this.continueTimeout;
                request.ReadWriteTimeout = this.readWriteTimeout;
                request.ContentType = this.contentType;                                                  

                using( HttpWebResponse response = (HttpWebResponse)request.GetResponse( ) ) {
                    
                    using (var reader = new System.IO.StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8 ))
                    {
                        this.responseText = reader.ReadToEnd();
                    }

                    this.httpStatusCode = response.StatusCode;                    
                }

                logger.LogInformation( "HttpClient GET Resp Result [ Url : {0} ] [ Status Code : {1} ] [ responseText : {2} ]", url, this.httpStatusCode.ToString(), this.responseText );

                isRet = true;

            } catch (System.Exception ex) {
                Util.LogError( logger, ex );
                throw ex;
            }

            return isRet;
        }

    }
}
