using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace nmsAbmsWeb {
    public class ParserSub : ParserBase {
//========================================================================================
// 1) K8s Error Sub Msg
//         {
// 	"insertId": "8i1tv4z7dbin9h9u",
// 	"labels": {
// 		"compute.googleapis.com/resource_name": "gke-kube-stand-abms--kube-stand-abms--35033e8d-g5dr",
// 		"k8s-pod/app": "abms-web-dev",
// 		"k8s-pod/pod-template-hash": "67b6498b55"
// 	},
// 	"logName": "projects/pjt-uplus-abtest-abms-dev/logs/stdout",
// 	"receiveTimestamp": "2021-11-10T02:44:52.422964253Z",
// 	"resource": {
// 		"labels": {
// 			"cluster_name": "kube-stand-abms-dev",
// 			"container_name": "abms-sha256-1",
// 			"location": "asia-northeast3",
// 			"namespace_name": "default",
// 			"pod_name": "abms-web-dev-67b6498b55-l7blx",
// 			"project_id": "pjt-uplus-abtest-abms-dev"
// 		},
// 		"type": "k8s_container"
// 	},
// 	"severity": "INFO",
// 	"textPayload": "2021-11-10 11:44:50.127 INFO  [35.191.13.233] [/abms/api/test] [GET] [END] - [00:00:00.010] [2003] [DB]\n",
// 	"timestamp": "2021-11-10T02:44:50.136731798Z"
// }

//========================================================================================
// 2) HTTP Loadbalancer Error Sub Msg
// {
// 	"httpRequest": {
// 		"latency": "0.010999s",
// 		"remoteIp": "106.245.226.42",
// 		"requestMethod": "GET",
// 		"requestSize": "562",
// 		"requestUrl": "http://34.117.249.185/login?userId=234",
// 		"responseSize": "2094",
// 		"serverIp": "10.124.24.5",
// 		"status": 403,
// 		"userAgent": "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.118 Whale/2.11.126.23 Safari/537.36"
// 	},
// 	"insertId": "dlgbhuf4lqynu",
// 	"jsonPayload": {
// 		"@type": "type.googleapis.com/google.cloud.loadbalancing.type.LoadBalancerLogEntry",
// 		"statusDetails": "response_sent_by_backend"
// 	},
// 	"logName": "projects/pjt-uplus-abtest-abms-dev/logs/requests",
// 	"receiveTimestamp": "2021-11-12T04:36:27.011169191Z",
// 	"resource": {
// 		"labels": {
// 			"backend_service_name": "k8s1-b24025f9-default-abms-web-dev-service-8080-e3762553",
// 			"forwarding_rule_name": "k8s2-fr-d3mr0jj8-default-externel-ingress-web-lb-d0hk3k9a",
// 			"project_id": "pjt-uplus-abtest-abms-dev",
// 			"target_proxy_name": "k8s2-tp-d3mr0jj8-default-externel-ingress-web-lb-d0hk3k9a",
// 			"url_map_name": "k8s2-um-d3mr0jj8-default-externel-ingress-web-lb-d0hk3k9a",
// 			"zone": "global"
// 		},
// 		"type": "http_load_balancer"
// 	},
// 	"severity": "WARNING",
// 	"spanId": "cf18047ee4f38913",
// 	"timestamp": "2021-11-12T04:36:26.26409Z",
// 	"trace": "projects/pjt-uplus-abtest-abms-dev/traces/ebfbdd1cae963b2ef205ad5aff5c3217"
// }
        public ParserSub( ILogger logger, string msg ){
            base.logger = logger;
            base.msg = msg;   
        }

        public enum MsgType
        {
            none,
            k8s_container,
            http_load_balancer
        }

        public class ParserSubRet
        {
            public MsgType msgType{ get; set; }
            public ParsingItemHttpLB parsingItemHttpLB;
            public ParsingItemK8s parsingItemK8s;
            public string projectId { get; set; }
            public string abmsType { get; set; }
            public ParserSubRet()
            {
                parsingItemHttpLB = new ParsingItemHttpLB();
                parsingItemK8s = new ParsingItemK8s();
            }

            public void print( ILogger logger )
            {
                logger.LogInformation( String.Format( "ParserSubRet | [ msgType : {0} ]", msgType.ToString() ) );    
                logger.LogInformation( String.Format( "ParserSubRet | [ projectId : {0} ]", projectId ) );    
                logger.LogInformation( String.Format( "ParserSubRet | [ abmsType : {0} ]", abmsType ) );    
                switch( msgType ){
                    case MsgType.k8s_container:{ parsingItemK8s.print(logger); break; }
                    case MsgType.http_load_balancer:{ parsingItemHttpLB.print(logger); break; }
                    default : { break; }
                }
            }
        }

        public class ParsingItemHttpLB
        {
            public string latency{ get; set; }
            public string latencyConv{ get; set; }
            public string remoteIp{get; set;}
            public string requestMethod{get; set;}
            public string requestSize{ get; set; }
            public string requestUrl{ get; set; }
            public string requestUrlCrop{ get; set; }
            public string responseSize{ get; set; }
            public string serverIp{ get; set; }
            public string status{ get; set; }

            public string userAgent{ get; set; }
            public string referer{ get; set; }
            public string refererCrop{ get; set; }
            public string receiveTimestamp{ get; set; }
            public string receiveTimestampConv{ get; set; }
            public ParsingItemHttpLB()
            {                
                this.latency = "-";
                this.remoteIp = "-";
                this.requestMethod = "-";
                this.requestSize = "-";
                this.requestUrl = "-";
                this.responseSize = "-";
                this.serverIp = "-";
                this.status = "-";
                this.userAgent = "-";
                this.referer = "-";                
            }

            public void print( ILogger logger )
            {                
                logger.LogInformation( String.Format( "ParsingItemHttpLB | [ latency : {0} ]", latency ) );
                logger.LogInformation( String.Format( "ParsingItemHttpLB | [ latencyConv : {0} ]", latencyConv ) );
                logger.LogInformation( String.Format( "ParsingItemHttpLB | [ remoteIp : {0} ]", remoteIp ) );
                logger.LogInformation( String.Format( "ParsingItemHttpLB | [ requestMethod : {0} ]", requestMethod ) );
                logger.LogInformation( String.Format( "ParsingItemHttpLB | [ requestSize : {0} ]", requestSize ) );
                logger.LogInformation( String.Format( "ParsingItemHttpLB | [ requestUrl : {0} ]", requestUrl ) );
                logger.LogInformation( String.Format( "ParsingItemHttpLB | [ requestUrlCrop : {0} ]", requestUrlCrop ) );
                logger.LogInformation( String.Format( "ParsingItemHttpLB | [ responseSize : {0} ]", responseSize ) );
                logger.LogInformation( String.Format( "ParsingItemHttpLB | [ serverIp : {0} ]", serverIp ) );
                logger.LogInformation( String.Format( "ParsingItemHttpLB | [ userAgent : {0} ]", userAgent ) );
                logger.LogInformation( String.Format( "ParsingItemHttpLB | [ referer : {0} ]", referer ) );
                logger.LogInformation( String.Format( "ParsingItemHttpLB | [ refererCrop : {0} ]", refererCrop ) );                
                logger.LogInformation( String.Format( "ParsingItemHttpLB | [ receiveTimestamp : {0} ]", receiveTimestamp ) );
                logger.LogInformation( String.Format( "ParsingItemHttpLB | [ receiveTimestampConv : {0} ]", receiveTimestampConv ) );
            }
        }

        public class ParsingItemK8s
        {
            public string date { get; set; }
            public string time { get; set; }        
            public string ip { get; set; }
            public string api { get; set; }
            public string httpMethod { get; set; }
            public string takenTime { get; set; }
            public string takenTimeConv { get; set; }
            public string errorCode { get; set; }
            public string responseLength { get; set; }
            public string saId { get; set; }
            public string userAgent { get; set; }

            public ParsingItemK8s()
            {                
                this.date = "-";
                this.time = "-";
                this.ip = "-";
                this.api = "-";
                this.httpMethod = "-";
                this.takenTime = "-";
                this.takenTimeConv = "-";
                this.errorCode = "-";
                this.responseLength = "-";
                this.saId = "";
                this.userAgent = "-";
            }

            public void print( ILogger logger )
            {                
                logger.LogInformation( String.Format( "ParsingItemK8s | [ date : {0} ]", date ) );
                logger.LogInformation( String.Format( "ParsingItemK8s | [ time : {0} ]", time ) );
                logger.LogInformation( String.Format( "ParsingItemK8s | [ ip : {0} ]", ip ) );
                logger.LogInformation( String.Format( "ParsingItemK8s | [ api : {0} ]", api ) );
                logger.LogInformation( String.Format( "ParsingItemK8s | [ httpMethod : {0} ]", httpMethod ) );
                logger.LogInformation( String.Format( "ParsingItemK8s | [ takenTime : {0} ]", takenTime ) );
                logger.LogInformation( String.Format( "ParsingItemK8s | [ takenTimeConv : {0} ]", takenTimeConv ) );
                logger.LogInformation( String.Format( "ParsingItemK8s | [ errorCode : {0} ]", errorCode ) );
                logger.LogInformation( String.Format( "ParsingItemK8s | [ responseLength : {0} ]", responseLength ) );
                logger.LogInformation( String.Format( "ParsingItemK8s | [ saId : {0} ]", saId ) );
                logger.LogInformation( String.Format( "ParsingItemK8s | [ userAgent : {0} ]", userAgent ) );
            }
        }
        
        public ParserSubRet parserSubRet = new ParserSubRet();        

        public override bool action()
        {
            bool isRet = false;

            try{
                if( Checker.isEmpty( msg ) ){
                    throw new System.Exception( "ParserSub 파싱할 메시지가 설정되어 있지 않습니다." );
                }

                JObject jsSub = JObject.Parse(msg);
                if( Checker.isNull( jsSub ) ){
                    throw new System.Exception( "json Sub 메시지 파싱에 실패하였습니다." );
                }

                string data = JsonValue.getString( jsSub, "data", false );
                if( Checker.isEmpty( data ) ){
                    throw new Exception( "json Sub Msg 값에서 data 파트 조회에 실패하였습니다." );
                }

                JObject jsdata = JObject.Parse( StringDecoder.base64( data ) );

                logger.LogInformation( jsdata.ToString() );

                this.parserSubRet.msgType = checkMsgType( jsdata );                

                //공통 필요 정보 파싱
                getCommonInfo( jsdata, this.parserSubRet.msgType );

                //메시지 타입 별 파싱
                switch( this.parserSubRet.msgType ){
                    case MsgType.k8s_container:{
                        parsingMsgK8sContainer( jsdata );                        
                        break;
                    }
                    case MsgType.http_load_balancer:{
                        parsingMsgHttpLB( jsdata );                        
                        break;
                    }
                    default:{
                        logger.LogError( String.Format( "매치되는 Msg Type이 존재하지 않습니다. | [ {0} ]", this.parserSubRet.msgType.ToString() ) );
                        break;
                    }
                }

                isRet = true;

            }catch( System.Exception ex ){
                Util.LogError( logger, ex );
            }
            
            return isRet;
        }

        private bool getCommonInfo( JObject jsdata, MsgType msgType )
        {
            bool isRet = false;

            try{                
                if( Checker.isNull( jsdata ) ){
                    throw new Exception( "[getCommonInfo] param jsdata is null" );                    
                }

                if( Checker.isNull( msgType ) ){
                    throw new Exception( "[getCommonInfo] param msgType is null" );                    
                }

                JObject resource = JsonValue.getJObjct( jsdata, "resource", false );
                if( Checker.isNull( resource ) ){
                    throw new Exception( "[getCommonInfo]j son Sub Msg 값에서 resource 파트 조회에 실패하였습니다." );
                }

                JObject labels = JsonValue.getJObjct( resource, "labels", false );
                if( Checker.isNull( labels ) ){
                    throw new Exception( "[getProjectId]j labels 파트 조회에 실패하였습니다." );
                }

                this.parserSubRet.projectId  = getProjectId( labels );
                this.parserSubRet.abmsType   = getAbmsType( labels, msgType );

                isRet = true;
                
            }catch( Exception ex ){
                Util.LogError( logger, ex );
                throw ex;
            }

            return isRet;
        }

        private string getProjectId( JObject labels )
        {
            string result = String.Empty;

            try{
                if( Checker.isNull( labels ) ){
                    throw new Exception( "[getProjectId] param labels is null" );                    
                }

                string temp = JsonValue.getString( labels, "project_id", false );                
                if( Checker.isEmpty(temp) ){
                    throw new Exception( "[getProjectId]j project_id 파트 조회에 실패하였습니다." );
                }

                result = temp;

                logger.LogInformation( String.Format( "[getProjectId] projectId : {0}", result ) );

            }catch( Exception ex ){
                Util.LogError( logger, ex );
                throw ex;
            }

            return result;
        }

        private string getAbmsType( JObject labels, MsgType msgType )
        {
            string result = String.Empty;
            string resultTextabmsWeb = "abms-web";
            string resultTextabmsWas = "abms-was";

            try{
                if( Checker.isNull( labels ) ){
                    throw new Exception( "[getAbmsType] param labels is null" );                    
                }

                if( Checker.isNull( msgType ) ){
                    throw new Exception( "[getAbmsType] param msgType is null" );                    
                }

                string keywordAbmsWeb = String.Empty;
                string keywordAbmsWas = String.Empty;

                Define.dicEnvVals.TryGetValue("keyword_abms_web", out keywordAbmsWeb);
                Define.dicEnvVals.TryGetValue("keyword_abms_was", out keywordAbmsWas);

                if( Checker.isEmpty( keywordAbmsWeb ) ){
                    throw new Exception( "[getAbmsType] keywordAbmsWeb is empty" );
                }

                if( Checker.isEmpty( keywordAbmsWas ) ){
                    throw new Exception( "[getAbmsType] keywordAbmsWas is empty" );
                }

                switch( msgType ){
                    case MsgType.k8s_container:{
                        string pod_name = JsonValue.getString( labels, "pod_name", false );
                        if( Checker.isEmpty( pod_name ) ){
                            throw new Exception( "[getAbmsType] json labels part pod_name is empty" );
                        }
                        
                        do{
                            if( Checker.contains( pod_name, keywordAbmsWeb ) ){
                                result = resultTextabmsWeb;
                                logger.LogInformation( String.Format( "[getAbmsType] abms type check ok | [ {0} ] [ pod_name : {1} ]", result, pod_name ) );
                                break;
                            }

                            if( Checker.contains( pod_name, keywordAbmsWas ) ){
                                result = resultTextabmsWas;
                                logger.LogInformation( String.Format( "[getAbmsType] abms type check ok | [ {0} ] [ pod_name : {1} ]", result, pod_name ) );
                                break;
                            }

                        }while( false );
                        
                        break;
                    }
                    case MsgType.http_load_balancer:{
                        string backend_service_name = JsonValue.getString( labels, "backend_service_name", false );
                        if( Checker.isEmpty( backend_service_name ) ){
                            throw new Exception( "[getAbmsType] json labels part backend_service_name is empty" );
                        }
                        
                        do{
                            if( Checker.contains( backend_service_name, keywordAbmsWeb ) ){
                                result = resultTextabmsWeb;
                                logger.LogInformation( String.Format( "[getAbmsType] abms type check ok | [ {0} ] [ backend_service_name : {1} ]", result, backend_service_name ) );
                                break;
                            }

                            if( Checker.contains( backend_service_name, keywordAbmsWas ) ){
                                result = resultTextabmsWas;
                                logger.LogInformation( String.Format( "[getAbmsType] abms type check ok | [ {0} ] [ backend_service_name : {1} ]", result, backend_service_name ) );
                                break;
                            }

                        }while( false );

                        break;
                    }
                }

                if( Checker.isEmpty( result ) ){
                    throw new Exception( "[getAbmsType] abms type parsing fail" );
                }                

                logger.LogInformation( String.Format( "[getAbmsType] abms type : {0}", result ) );

            }catch( Exception ex ){
                Util.LogError( logger, ex );
                throw ex;
            }

            return result;
        }

        private MsgType checkMsgType( JObject jsdata )
        {
            MsgType msgType = MsgType.none;

            try{
                if( Checker.isNull( jsdata ) ){
                    throw new Exception( "[checkMsgType] param jsdata is empty" );
                }

                JObject resource = (JObject)jsdata["resource"];

                if( Checker.isNull( resource ) ){
                    throw new Exception( "[checkMsgType] resource json data is empty" );
                }
                
                string type = JsonValue.getString( resource, "type", false );

                if( type == MsgType.k8s_container.ToString() ){
                    msgType = MsgType.k8s_container;
                }else if( type == MsgType.http_load_balancer.ToString() ){
                    msgType = MsgType.http_load_balancer;
                }

                logger.LogInformation( String.Format( "[checkMsgType] Msg Type : {0}", msgType.ToString() ) );

            }catch( Exception ex ){
                Util.LogError( logger, ex );
            }

            return msgType;
        }

        private bool parsingMsgHttpLB( JObject jsdata )
        {
            bool isRet = false;

            try{
                if( Checker.isNull( jsdata ) ){
                    throw new Exception( "[parsingMsgHttpLB] param jsdata 값이 빈값 입니다" );
                }

                JObject httpRequest = (JObject)jsdata["httpRequest"];

                if( Checker.isNull( httpRequest ) ){
                    throw new Exception( "[parsingMsgHttpLB] httpRequest 값이 빈값 입니다" );
                }

                logger.LogInformation( httpRequest.ToString() );

                this.parserSubRet.parsingItemHttpLB.latency                 = JsonValue.getString( httpRequest, "latency", "-" ) ;
                this.parserSubRet.parsingItemHttpLB.latencyConv             = convLatency( JsonValue.getString( httpRequest, "latency", false ) );
                this.parserSubRet.parsingItemHttpLB.remoteIp                = JsonValue.getString( httpRequest, "remoteIp", "-" );
                this.parserSubRet.parsingItemHttpLB.requestMethod           = JsonValue.getString( httpRequest, "requestMethod", "-" );
                this.parserSubRet.parsingItemHttpLB.requestSize             = JsonValue.getString( httpRequest, "requestSize", "-" );
                this.parserSubRet.parsingItemHttpLB.requestUrl              = JsonValue.getString( httpRequest, "requestUrl", "-" );
                this.parserSubRet.parsingItemHttpLB.requestUrlCrop          = cropUrl( JsonValue.getString( httpRequest, "requestUrl", false ) );
                this.parserSubRet.parsingItemHttpLB.responseSize            = JsonValue.getString( httpRequest, "responseSize", "-");
                this.parserSubRet.parsingItemHttpLB.serverIp                = JsonValue.getString( httpRequest, "serverIp", "-" );                
                this.parserSubRet.parsingItemHttpLB.userAgent               = JsonValue.getString( httpRequest, "userAgent", "-" );
                this.parserSubRet.parsingItemHttpLB.referer                 = JsonValue.getString( httpRequest, "referer", "-" );
                this.parserSubRet.parsingItemHttpLB.refererCrop             = cropUrl( JsonValue.getString( httpRequest, "referer", false ) );

                int statusNumber = JsonValue.getInteger( httpRequest, "status", -1 );
                this.parserSubRet.parsingItemHttpLB.status                  = String.Format( "{0}", statusNumber == -1 ? "-" : statusNumber.ToString() );

                this.parserSubRet.parsingItemHttpLB.receiveTimestamp        = JsonValue.getString( jsdata, "receiveTimestamp", "-" );
                this.parserSubRet.parsingItemHttpLB.receiveTimestampConv    = filteringHttpLBReceiveTimestamp( this.parserSubRet.parsingItemHttpLB.receiveTimestamp );

                this.parserSubRet.parsingItemHttpLB.print( logger );

                isRet = true;
            }catch( Exception ex ){
                Util.LogError( logger, ex );
            }

            return isRet;
        }

        private string filteringHttpLBReceiveTimestamp( string receiveTimestamp )
        {
            string result = "-";
            string date = String.Empty;
            string time = String.Empty;

            try{
                do{
                    if( Checker.isEmpty( receiveTimestamp ) ){
                        logger.LogInformation( "[filteringHttpLBReceiveTimestamp] param receiveTimestamp is empty, skip this proc" );
                        break;
                    }

                    if( receiveTimestamp == "-" ){
                        logger.LogInformation( "[filteringHttpLBReceiveTimestamp] param receiveTimestamp value is '-', skip this proc" );
                        break;
                    }

                    // full text : 2021-11-12T05:32:07.03504094Z

                    //date
                    //ex : 2021-11-12
                    Regex rgx = new Regex(@"(19|20)\d{2}-(0[1-9]|1[012])-(0[1-9]|[12][0-9]|3[0-1])");
                    Match match = rgx.Match(receiveTimestamp);
                    if( match == null ){
                        logger.LogError( "filteringHttpLBReceiveTimestamp date match result is empty" );
                        break;
                    }

                    date = match.Value;

                    //time
                    //ex: 05:32:07
                    Regex rgx2nd = new Regex(@"(((([0-1][0-9])|(2[0-3])):[0-5][0-9]:[0-5][0-9]))");
                    Match match2nd = rgx2nd.Match(receiveTimestamp);
                    if( match2nd == null ){
                        logger.LogError( "filteringHttpLBReceiveTimestamp time match result is empty" );
                        break;
                    }

                    time = match2nd.Value;

                    result = String.Format("{0} {1}", date, time);

                    logger.LogInformation( String.Format("filteringHttpLBReceiveTimestamp result : {0}", result) );

                }while(false);
            }catch( Exception ex ){
                Util.LogError( logger, ex );
            }

            return result;
        }

        // private string cropUrl( string requestUrl )
        // {
        //     string result = "-";
        //     string temp = String.Empty;

        //     try{
        //         do{
        //             if( Checker.isEmpty( requestUrl ) ){
        //                 break;
        //             }

        //             temp = requestUrl.Replace( "http://", "" ).Replace( "https://", "" );

        //             Regex rgx = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
        //             Match match = rgx.Match(temp);
        //             if( match == null ){
        //                 logger.LogError( "cropUrl match result is empty" );
        //                 break;
        //             }

        //             temp = temp.Replace( match.Value, "" );

        //             result = temp;

        //             logger.LogInformation( String.Format( "[cropUrl] complete : {0}", result ) );

        //         }while(false);
        //     }catch( Exception ex ){
        //         Util.LogError( logger, ex );
        //     }

        //     return result;
        // }

        private string cropUrl( string requestUrl )
        {
            string result = "-";
            string temp = String.Empty;

            try{
                do{
                    if( Checker.isEmpty( requestUrl ) ){
                        break;
                    }

                    temp = requestUrl.Replace( "http://", "" ).Replace( "https://", "" );
                    temp = temp.Substring( temp.IndexOf( "/" ) );
                    
                    result = temp;

                    logger.LogInformation( String.Format( "[cropUrl] complete : {0}", result ) );

                }while(false);
            }catch( Exception ex ){
                Util.LogError( logger, ex );
            }

            return result;
        }

        private string convLatency( string latency )
        {
            string result = "-";
            do{
                if( Checker.isEmpty( latency ) ){
                    break;
                }

                latency = latency.Replace("s", "");
                double latencyNumType = Double.Parse( latency );
                latencyNumType = (Int32)( latencyNumType * 1000 );
                result = latencyNumType.ToString();

            }while( false );

            return result;            
        }

        private bool parsingMsgK8sContainer( JObject jsdata )
        {
            bool isRet = false;

            try{
                if( Checker.isNull( jsdata ) ){
                    throw new Exception( "[parsingMsgK8sContainer] param jsdata 값이 빈값 입니다" );
                }

                string textPayload = JsonValue.getString( jsdata, "textPayload", false );

                if( Checker.isEmpty( textPayload ) ){
                    throw new Exception( "[parsingMsgK8sContainer] textPayload 값이 빈값 입니다" );
                }

                logger.LogInformation( "==============================" );
                logger.LogInformation( String.Format( "textPayload : {0}", textPayload ) );
                logger.LogInformation( "==============================" );

                filteringK8sDate( textPayload );
                filteringK8sTime( textPayload );
                filteringK8sIp( textPayload );
                filteringK8sApi( textPayload );
                filteringK8sHttpMethod( textPayload );
                filteringK8sTakenTime( textPayload );
                filteringK8sErrorCode( textPayload );
                filteringK8sSaId( textPayload );
                filteringK8sUserAgent( textPayload );
                this.parserSubRet.parsingItemK8s.responseLength = String.Format("{0}", textPayload.Length );

                this.parserSubRet.parsingItemK8s.print( logger );

                isRet = true;
            }catch( Exception ex ){
                Util.LogError( logger, ex );                
            }

            return isRet;
        }

        private bool filteringK8sDate( string data )
        {
            bool isRet = false;

            do{
                if( Checker.isEmpty( data ) ){
                    logger.LogError( "filteringDate param data is empty" );
                    break;
                }
                
                //ex : 2021-11-10
                Regex rgx = new Regex(@"(19|20)\d{2}-(0[1-9]|1[012])-(0[1-9]|[12][0-9]|3[0-1])");
                Match match = rgx.Match(data);
                if( match == null ){
                    logger.LogError( "filteringDate match result is empty" );
                    break;
                }

                this.parserSubRet.parsingItemK8s.date = match.Value;

                logger.LogInformation( String.Format( "filter result | [ Date : {0} ]", this.parserSubRet.parsingItemK8s.date ) );

                isRet = true;

            }while(false);

            return isRet;                
        }

        private bool filteringK8sTime( string data )
        {
            bool isRet = false;

            do{
                if( Checker.isEmpty( data ) ){
                    logger.LogError( "filteringK8sTime param data is empty" );
                    break;
                }
                
                // ex : 11:44:50.127
                Regex rgx = new Regex(@"(((([0-1][0-9])|(2[0-3])):?[0-5][0-9]:?[0-5][0-9]+.[0-9][0-9][0-9]))");
                MatchCollection matchs = rgx.Matches(data);
                if( matchs == null ){
                    logger.LogError( "filteringK8sTime match result is empty" );
                    break;
                }

                Match mach = matchs[0];

                rgx = new Regex(@"\.\d+$");
                Match match2nd = rgx.Match(mach.Value);
                if( match2nd == null ){
                    logger.LogError( "filteringK8sTime 2nd match result is empty" );
                    break;
                }

                string temp = String.Empty;

                if( !Checker.isNull( match2nd ) && !Checker.isEmpty(match2nd.Value) ){
                    temp = mach.Value.Replace( match2nd.Value, "" );
                }else{
                    temp = mach.Value;
                }                

                this.parserSubRet.parsingItemK8s.time = temp;

                logger.LogInformation( String.Format( "filter result | [ Time : {0} ]", this.parserSubRet.parsingItemK8s.time ) );

                isRet = true;

            }while(false);

            return isRet;                
        }

        private bool filteringK8sIp( string data )
        {
            bool isRet = false;

            do{
                if( Checker.isEmpty( data ) ){
                    logger.LogError( "filteringK8sIp param data is empty" );
                    break;
                }
                
                //35.191.13.233 
                Regex rgx = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
                Match match = rgx.Match(data);
                if( match == null ){
                    logger.LogError( "filteringK8sIp match result is empty" );
                    break;
                }

                this.parserSubRet.parsingItemK8s.ip = match.Value;

                logger.LogInformation( String.Format( "filter result | [ IP : {0} ]", this.parserSubRet.parsingItemK8s.ip ) );

                isRet = true;

            }while(false);

            return isRet;                
        }

        private bool filteringK8sApi( string data )
        {
            bool isRet = false;

            do{
                if( Checker.isEmpty( data ) ){
                    logger.LogError( "filteringK8sApi param data is empty" );
                    break;
                }
                
                // ex : [/abms/api/test]
                Regex rgx = new Regex(@"(\[\/\S*\])");
                Match match = rgx.Match(data);
                if( match == null ){
                    logger.LogError( "filteringK8sApi match result is empty" );
                    break;
                }

                this.parserSubRet.parsingItemK8s.api = match.Value.Replace( "[", "" ).Replace( "]", "" );

                logger.LogInformation( String.Format( "filter result | [ API : {0} ]", this.parserSubRet.parsingItemK8s.api ) );

                isRet = true;

            }while(false);

            return isRet;                
        }

        private bool filteringK8sHttpMethod( string data )
        {
            bool isRet = false;

            do{
                if( Checker.isEmpty( data ) ){
                    logger.LogError( "filteringK8sHttpMethod param data is empty" );
                    break;
                }
                
                // ex : GET or POST or PUT or DELETE or HEAD
                Regex rgx = new Regex(@"(GET|POST|PUT|DELETE|HEAD)");
                Match match = rgx.Match(data);
                if( match == null ){
                    logger.LogError( "filteringK8sHttpMethod match result is empty" );
                    break;
                }

                this.parserSubRet.parsingItemK8s.httpMethod = match.Value;

                logger.LogInformation( String.Format( "filter result | [ Http Method : {0} ]", this.parserSubRet.parsingItemK8s.httpMethod ) );

                isRet = true;

            }while(false);

            return isRet;                
        }

        private bool filteringK8sTakenTime( string data )
        {
            bool isRet = false;

            do{
                if( Checker.isEmpty( data ) ){
                    logger.LogError( "filteringK8sTakenTime param data is empty" );
                    break;
                }
                
                // ex : [00:00:00.123]
                Regex rgx = new Regex(@"(\[(((([0-1][0-9])|(2[0-3])):?[0-5][0-9]:?[0-5][0-9]+.[0-9][0-9][0-9]))\])");
                Match match = rgx.Match(data);
                if( match == null ){
                    logger.LogError( "filteringK8sTakenTime match result is empty" );
                    break;
                }

                this.parserSubRet.parsingItemK8s.takenTime = match.Value.Replace( "[", "" ).Replace( "]", "" );

                logger.LogInformation( String.Format( "filter result | [ Taken Time : {0} ]", this.parserSubRet.parsingItemK8s.takenTime ) );

                DateTime dateTimeTekenTime = DateTime.ParseExact( this.parserSubRet.parsingItemK8s.takenTime, "HH:mm:ss.fff", CultureInfo.InvariantCulture );
                this.parserSubRet.parsingItemK8s.takenTimeConv = Util.dateTimeToMillisec( dateTimeTekenTime ).ToString();

                logger.LogInformation( String.Format( "filter result | [ Taken Time Conv : {0} ]", this.parserSubRet.parsingItemK8s.takenTimeConv ) );

                isRet = true;

            }while(false);

            return isRet;                
        }

        private bool filteringK8sErrorCode( string data )
        {
            bool isRet = false;

            do{
                if( Checker.isEmpty( data ) ){
                    logger.LogError( "filteringK8sErrorCode param data is empty" );
                    break;
                }
                
                // ex : [2001]
                Regex rgx = new Regex(@"\[\d*\]");
                Match match = rgx.Match(data);
                if( match == null ){
                    logger.LogError( "filteringK8sErrorCode match result is empty" );
                    break;
                }

                this.parserSubRet.parsingItemK8s.errorCode = match.Value.Replace( "[", "" ).Replace( "]", "" );

                logger.LogInformation( String.Format( "filter result | [ Error Code : {0} ]", this.parserSubRet.parsingItemK8s.errorCode ) );

                isRet = true;

            }while(false);

            return isRet;                
        }

        private bool filteringK8sSaId( string data )
        {
            bool isRet = false;

            do{
                if( Checker.isEmpty( data ) ){
                    logger.LogError( "filteringK8sSaId param data is empty" );
                    break;
                }
                
                // ex : [saId=500196983182]
                // abms-was api 호출 시 가입자번호 확인
                Regex rgx = new Regex(@"(\[saId=\S+\])");
                Match match = rgx.Match(data);
                if( match == null ){
                    logger.LogError( "filteringK8sSaId match result is empty" );
                    break;
                }

                this.parserSubRet.parsingItemK8s.saId = match.Value.Replace( "saId=", "" ).Replace( "[", "" ).Replace( "]", "" );

                logger.LogInformation( String.Format( "filter result | [ saId : {0} ]", this.parserSubRet.parsingItemK8s.saId ) );

                isRet = true;

            }while(false);

            return isRet;                
        }

        private bool filteringK8sUserAgent( string data )
        {
            bool isRet = false;

            do{
                if( Checker.isEmpty( data ) ){
                    logger.LogError( "filteringK8sSaId param data is empty" );
                    break;
                }
                
                // ex : [userAgent=Chrome]                
                Regex rgx = new Regex(@"\[userAgent=[a-zA-Z0-9!@#$&()\\-`.+,/]*\]");
                Match match = rgx.Match(data);
                if( match == null ){
                    logger.LogError( "filteringK8sUserAgent match result is empty" );
                    break;
                }

                String subStr = match.Value.Substring( 0, match.Value.Length - 1 );
                subStr = subStr.Substring( 1 );

                this.parserSubRet.parsingItemK8s.userAgent = subStr.Replace( "userAgent=", "" );

                logger.LogInformation( String.Format( "filter result | [ userAgent : {0} ]", this.parserSubRet.parsingItemK8s.userAgent ) );

                isRet = true;

            }while(false);

            return isRet;                
        }

    }
}
