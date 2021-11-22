using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace nmsAbmsWeb {
    public class MakerMsgNms : MakerBase {        
        protected ParserSub.ParserSubRet parserSubRet;
        public JObject msg;

        public MakerMsgNms( ILogger logger, ParserSub.ParserSubRet parserSubRet )
        {
            base.logger = logger;
            this.parserSubRet = parserSubRet;
        }
        
        public override bool action()
        {
            bool isRet = false;

            try{
                if( Checker.isNull( parserSubRet ) ){
                    throw new System.Exception( "[MakerMsgNms] ParserSubRet 값이 설정되어 있지 않습니다." );
                }

                if( Checker.isNull( parserSubRet.msgType ) ){
                    throw new System.Exception( "[MakerMsgNms] ParserSubRet 값이 설정되어 있지 않습니다." );
                }

                switch( parserSubRet.msgType ){
                    case ParserSub.MsgType.k8s_container : {
                        msg = makeMsgK8s( parserSubRet );
                        break;
                    }
                    case ParserSub.MsgType.http_load_balancer : {
                        msg = makeMsgHttpLB( parserSubRet );
                        break;
                    }
                    default : {
                        break;
                    }
                }

                isRet = true;
            }catch( System.Exception ex ){
                Util.LogError( logger, ex );
            }

            return isRet;
        }

        public JObject makeMsgHttpLB( ParserSub.ParserSubRet parserSubRet )
        {
            JObject result = new JObject();

            try{
                if( Checker.isNull( parserSubRet.parsingItemHttpLB ) ){
                    throw new Exception("[makeMsgHttpLB] param parserSubRet.parsingItemHttpLB is null");
                }

                result["status"]            = parserSubRet.parsingItemHttpLB.status;
                result["protocol"]          = "HTTP/1.1";
                result["httpMethod"]        = parserSubRet.parsingItemHttpLB.requestMethod;
                result["servicename"]       = "IPTV" ;
                result["appname"]           = parserSubRet.abmsType;
                result["ip"]                = parserSubRet.parsingItemHttpLB.remoteIp;
                result["responseLength"]    = parserSubRet.parsingItemHttpLB.responseSize;
                result["resourcePath"]      = makeMsgResoucePathHttpLB(parserSubRet);
                result["user"]              = "-";
                result["duration"]          = parserSubRet.parsingItemHttpLB.latencyConv;
                if( !Checker.isEmpty( parserSubRet.parsingItemHttpLB.receiveTimestampConv ) && parserSubRet.parsingItemHttpLB.receiveTimestampConv != "-"  ){
                    result["requestTime"]   = parserSubRet.parsingItemHttpLB.receiveTimestampConv;
                }else{
                    result["requestTime"]   = "-";
                }
                
                result["caller"]            = "-";
                result["logtype"]           = "A";
                result["requestId"]         = "-";
                result["useragent"]         = parserSubRet.parsingItemHttpLB.userAgent;

            }catch( Exception ex ){
                Util.LogError( logger, ex );
            }

            return result;
        }

        private string makeMsgResoucePathHttpLB( ParserSub.ParserSubRet parserSubRet )
        {
            string result = "-";
            string temp = String.Empty;

            try{
                do{
                    if( Checker.isNull( parserSubRet ) ){
                        logger.LogError( "[makeMsgResoucePathHttpLB] param parserSubRet is null" );
                        break;
                    }

                    if( Checker.isNull( parserSubRet.parsingItemK8s ) ){
                        logger.LogError( "[makeMsgResoucePathHttpLB] param parserSubRet.parsingItemK8s is null" );
                        break;
                    }

                    temp = String.Format("{0} - - [{1}] \"{2} {3}\"", 
                    parserSubRet.parsingItemHttpLB.remoteIp,
                    parserSubRet.parsingItemHttpLB.receiveTimestampConv,                    
                    parserSubRet.parsingItemHttpLB.requestMethod,
                    parserSubRet.parsingItemHttpLB.requestUrlCrop + "?sa_id=&code="                    
                    );

                    result = temp;

                    logger.LogInformation( String.Format("[makeMsgResoucePathHttpLB] result : {0}", result) );

                }while(false);
            }catch( Exception ex ){
                Util.LogError( logger, ex );
            }

            return result;
        }

        public JObject makeMsgK8s( ParserSub.ParserSubRet parserSubRet )
        {
            JObject result = new JObject();

            try{
                if( Checker.isNull( parserSubRet.parsingItemK8s ) ){
                    throw new Exception("[makeMsgK8s] param parserSubRet.parsingItemK8s is null");
                }

                result["status"]            = "500";
                result["protocol"]          = "HTTP/1.1";
                result["httpMethod"]        = parserSubRet.parsingItemK8s.httpMethod;
                result["servicename"]       = "IPTV" ;
                result["appname"]           = parserSubRet.abmsType;
                result["ip"]                = parserSubRet.parsingItemK8s.ip;
                result["responseLength"]    = parserSubRet.parsingItemK8s.responseLength;
                result["resourcePath"]      = makeMsgResoucePathK8s(parserSubRet);
                result["user"]              = "-";
                result["duration"]          = parserSubRet.parsingItemK8s.takenTimeConv;
                if( !Checker.isEmpty( parserSubRet.parsingItemK8s.date ) && parserSubRet.parsingItemK8s.date != "-" && !Checker.isEmpty( parserSubRet.parsingItemK8s.time ) && parserSubRet.parsingItemK8s.time != "-" ){
                    result["requestTime"]   = String.Format("{0} {1}", parserSubRet.parsingItemK8s.date, parserSubRet.parsingItemK8s.time );
                }else{
                    result["requestTime"]   = "-";
                }
                
                result["caller"]            = "-";
                result["logtype"]           = "A";
                result["requestId"]         = "-";
                result["useragent"]         = parserSubRet.parsingItemK8s.userAgent;

            }catch( Exception ex ){
                Util.LogError( logger, ex );
            }

            return result;
        }

        private string makeMsgResoucePathK8s( ParserSub.ParserSubRet parserSubRet )
        {
            string result = "-";
            string temp = String.Empty;

            try{
                do{
                    if( Checker.isNull( parserSubRet ) ){
                        logger.LogError( "[makeMsgResoucePathK8s] param parserSubRet is null" );
                        break;
                    }

                    if( Checker.isNull( parserSubRet.parsingItemK8s ) ){
                        logger.LogError( "[makeMsgResoucePathK8s] param parserSubRet.parsingItemK8s is null" );
                        break;
                    }

                    temp = String.Format("{0} - - [{1} {2}] \"{3} {4}\"", 
                    parserSubRet.parsingItemK8s.ip,
                    parserSubRet.parsingItemK8s.date,
                    parserSubRet.parsingItemK8s.time,
                    parserSubRet.parsingItemK8s.httpMethod,
                    parserSubRet.parsingItemK8s.api + String.Format( "?sa_id={0}&code={1}", parserSubRet.parsingItemK8s.saId, parserSubRet.parsingItemK8s.errorCode )                                        
                    );

                    result = temp;

                    logger.LogInformation( String.Format("[makeMsgResoucePathK8s] result : {0}", result) );

                }while(false);
            }catch( Exception ex ){
                Util.LogError( logger, ex );
            }

            return result;
        }
    }
}
