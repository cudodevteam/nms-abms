using CloudNative.CloudEvents;
using Google.Cloud.Functions.Framework;
using Google.Events.Protobuf.Cloud.PubSub.V1;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace nmsAbmsWeb {


    //public class Function: IHttpFunction {

    public class Function : ICloudEventFunction<MessagePublishedData>{

        private readonly ILogger logger;

        public Function(ILogger<Function> _logger) => logger = _logger;


        // public async Task HandleAsync(HttpContext context) {
        //     logger.LogInformation("Function received request");

        //     try {
        //         initialize();

        //         string jsSubMsg = "";
        //         //string jsSubMsg = readMsgSubK8s();
        //         //string jsSubMsg = readMsgSubHttpLoadbalancer();

        //         logger.LogInformation(jsSubMsg);

        //         ParserSub parserSub = new ParserSub(logger, jsSubMsg);
        //         parserSub.filteringK8sUserAgent( "[userAgent=PostmanRuntime/7.28.4]" );
        //         if (!parserSub.action()) {
        //             throw new Exception(String.Format("Sub 메시지 파싱 실패 | {0}", jsSubMsg));
        //         }

        //         MakerMsgNms makerMsgNms = new MakerMsgNms( logger, parserSub.parserSubRet );
        //         if( !makerMsgNms.action() ){
        //             throw new Exception(String.Format("MakerMsgNms action 실패 | {0}", jsSubMsg));     
        //         }

        //         logger.LogInformation( "NMS Req MSG : {0}", makerMsgNms.msg.ToString() );

        //         getNmsUrl( parserSub.parserSubRet.projectId );

        //         logger.LogInformation( "nms msg send complete !!" );

        //     } catch (System.Exception ex) {
        //         Util.LogError(logger, ex);
        //     }

        //     await context.Response.WriteAsync("Hello, Functions Framework.");
        // }

        public async Task HandleAsync(CloudEvent cloudEvent, MessagePublishedData data, CancellationToken cancellationToken)
        {            
            try{              
                logger.LogInformation("Subcription Data Received, Proc Start!!!");                
                logger.LogInformation( String.Format( "Message : {0}", data.Message ));
                logger.LogInformation( String.Format( "Subscription : {0}", data.Subscription ));

                initialize();

                string jsSubMsg = data.Message.ToString();                

                logger.LogInformation(jsSubMsg);

                ParserSub parserSub = new ParserSub(logger, jsSubMsg);
                if (!parserSub.action()) {
                    throw new Exception(String.Format("Sub 메시지 파싱 실패 | {0}", jsSubMsg));
                }

                MakerMsgNms makerMsgNms = new MakerMsgNms( logger, parserSub.parserSubRet );
                if( !makerMsgNms.action() ){
                    throw new Exception(String.Format("MakerMsgNms action 실패 | {0}", jsSubMsg));     
                }

                logger.LogInformation( "NMS Req MSG : {0}", makerMsgNms.msg.ToString() );

                Exception exChk = null;
                for( int i = 0; i < 3; i++ ){
                    try{
                        HttpClient httpClient = new HttpClient(logger);
                        httpClient.post( getNmsUrl( parserSub.parserSubRet.projectId ), makerMsgNms.msg.ToString() );                                
                        exChk = null;
                        break;
                    }catch( Exception ex ){
                        Util.LogError( logger, ex );
                        exChk = ex;
                        Thread.Sleep( 1000 );
                    }                  
                }

                if( !Checker.isNull( exChk ) ){
                    throw exChk;
                }

                logger.LogInformation( "nms msg send complete !!" );

            }catch( System.Exception ex ) {
                Util.LogError(logger, ex);
            }finally{
                logger.LogInformation("Subcription Proc End!!!");                
            }
                      
        }
 
        public void initialize()
        {      

            #if DEBUG
                logger.LogInformation( "[initialize] nms_url_dev env val proc debug 처리" );
                Define.dicEnvVals.TryAdd( "nms_url_dev", "http://127.0.0.1:8081" );
            #else
            if( !Checker.isEmpty( Environment.GetEnvironmentVariable( "nms_url_dev") ) ){
                Define.dicEnvVals.TryAdd( "nms_url_dev", Environment.GetEnvironmentVariable( "nms_url_dev" ) );

                string check = String.Empty;
                Define.dicEnvVals.TryGetValue( "nms_url_dev", out check );
                logger.LogInformation( String.Format( "Env Vals | [ nms_url_dev : {0} ]", check ) );
            }else{
                logger.LogInformation( String.Format( "Env Vals is not reg | [ nms_url_dev ]" ) );
            }
            #endif    
            
            #if DEBUG
                logger.LogInformation( "[initialize] nms_url_prod env val proc debug 처리" );
                Define.dicEnvVals.TryAdd( "nms_url_prod", "http://127.0.0.1:8081" );
            #else
            if( !Checker.isEmpty( Environment.GetEnvironmentVariable( "nms_url_prod") ) ){
                Define.dicEnvVals.TryAdd( "nms_url_prod", Environment.GetEnvironmentVariable( "nms_url_prod") );

                string check = String.Empty;
                Define.dicEnvVals.TryGetValue( "nms_url_prod", out check );
                logger.LogInformation( String.Format( "Env Vals | [ nms_url_prod : {0} ]", check ) );                
            }else{
                logger.LogInformation( String.Format( "Env Vals is not reg | [ nms_url_prod ]" ) );
            }
            #endif

            #if DEBUG
                logger.LogInformation( "[initialize] keyword_abms_web env val proc debug 처리" );
                Define.dicEnvVals.TryAdd( "keyword_abms_web", "web" );
            #else
            if( !Checker.isEmpty( Environment.GetEnvironmentVariable( "keyword_abms_web") ) ){
                Define.dicEnvVals.TryAdd( "keyword_abms_web", Environment.GetEnvironmentVariable( "keyword_abms_web") );

                string check = String.Empty;
                Define.dicEnvVals.TryGetValue( "keyword_abms_web", out check );
                logger.LogInformation( String.Format( "Env Vals | [ keyword_abms_web : {0} ]", check ) );                
            }else{
                logger.LogInformation( String.Format( "Env Vals is not reg | [ keyword_abms_web ]" ) );
            }
            #endif

            #if DEBUG
                logger.LogInformation( "[initialize] keyword_abms_was env val proc debug 처리" );
                Define.dicEnvVals.TryAdd( "keyword_abms_was", "was" );
            #else
            if( !Checker.isEmpty( Environment.GetEnvironmentVariable( "keyword_abms_was") ) ){
                Define.dicEnvVals.TryAdd( "keyword_abms_was", Environment.GetEnvironmentVariable( "keyword_abms_was") );

                string check = String.Empty;
                Define.dicEnvVals.TryGetValue( "keyword_abms_was", out check );
                logger.LogInformation( String.Format( "Env Vals | [ keyword_abms_was : {0} ]", check ) );                
            }else{
                logger.LogInformation( String.Format( "Env Vals is not reg | [ keyword_abms_was ]" ) );
            }
            #endif

            #if DEBUG
                logger.LogInformation( "[initialize] pjt_prod env val proc debug 처리" );
                Define.dicEnvVals.TryAdd( "pjt_prod", "pjt-uplus-abtest-abms-prod" );
            #else
            if( !Checker.isEmpty( Environment.GetEnvironmentVariable( "pjt_prod") ) ){
                Define.dicEnvVals.TryAdd( "pjt_prod", Environment.GetEnvironmentVariable( "pjt_prod") );

                string check = String.Empty;
                Define.dicEnvVals.TryGetValue( "pjt_prod", out check );
                logger.LogInformation( String.Format( "Env Vals | [ pjt_prod : {0} ]", check ) );                
            }else{
                logger.LogInformation( String.Format( "Env Vals is not reg | [ pjt_prod ]" ) );
            }
            #endif
        }

        private string getNmsUrl( string projectIdInMsg )
        {
            string reusult = String.Empty;

            if( Checker.isEmpty( projectIdInMsg ) ){
                throw new Exception( String.Format( "[getNmsUrl] param projectIdInMsg is empty" ) );
            }

            string projectIdProd = String.Empty;
            string nmsUrlDev = String.Empty;
            string nmsUrlProd = String.Empty;

            Define.dicEnvVals.TryGetValue( "pjt_prod", out projectIdProd );
            Define.dicEnvVals.TryGetValue( "nms_url_dev", out nmsUrlDev );
            Define.dicEnvVals.TryGetValue( "nms_url_prod", out nmsUrlProd );

            if( Checker.isEmpty( projectIdProd ) ){
                throw new Exception( "[getNmsUrl] env val pjt_prod is empty" );
            }

            if( Checker.compare( projectIdProd, projectIdInMsg ) ){

                logger.LogInformation( String.Format( "[getNmsUrl] compare ok prod project nams | [ project in msg : {0} ] == [ project env val : {1} ] [ nms_url_prod : {2} ]", projectIdInMsg, projectIdProd, nmsUrlProd ) );
                // 상용 URL 설정
                if( Checker.isEmpty( nmsUrlProd ) ){
                    throw new Exception( "[getNmsUrl] env nms_url_prod is empty" );
                }

                reusult = nmsUrlProd;

            }else{
                logger.LogInformation( String.Format( "[getNmsUrl] not compare prod project nams | [ project in msg : {0} ] != [ project env val : {1} ] [ nms_url_dev : {2} ]", projectIdInMsg, projectIdProd, nmsUrlProd ) );

                // 개발 URL 설정
                if( Checker.isEmpty( nmsUrlDev ) ){
                    throw new Exception( "[getNmsUrl] env nms_url_dev is empty" );
                }

                reusult = nmsUrlDev;
            }

            logger.LogInformation( String.Format( "[getNmsUrl] nums url : {0}", reusult ) );

            return reusult;
        }



        private string readMsgSubK8s() {
            string path = "F:\\01. Work\\16. LGU+ AMBS\\01. Source\\nms_abms_web\\sub_msg_k8s_2.txt";

            if (!Util.isExistFile(path)) {
                throw new Exception(String.Format("Sub Msg Text 파일이 존재하지 않습니다. | {0}", path));
            }

            return System
                .IO
                .File
                .ReadAllText(path);
        }

        private string readMsgSubHttpLoadbalancer() {
            string path = "F:\\01. Work\\16. LGU+ AMBS\\01. Source\\nms_abms_web\\sub_msg_http_loadbalancer.txt";

            if (!Util.isExistFile(path)) {
                throw new Exception(String.Format("Sub Msg Text 파일이 존재하지 않습니다. | {0}", path));
            }

            return System
                .IO
                .File
                .ReadAllText(path);
        }
    }
}
