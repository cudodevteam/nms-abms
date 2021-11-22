using System;
using Newtonsoft.Json.Linq;

namespace nmsAbmsWeb
{
    public class JsonValue
    {
        public static string getString( JObject json, String key, bool isThrowEx )
        {
            string result = String.Empty;

            try{
                result = (string)json[key];
            }catch( System.Exception ex ){
                if( isThrowEx ){
                    throw ex;
                }
            }

            return result;
        }

        public static string getString( JObject json, String key, string initResult )
        {
            string result = initResult;

            try{
                result = (string)json[key];
            }catch( System.Exception ex ){                
            }

            return result;
        }

        public static int getInteger( JObject json, String key, bool isThrowEx )
        {
            int result = 0;

            try{
                result = (int)json[key];
            }catch( System.Exception ex ){
                if( isThrowEx ){
                    throw ex;
                }
            }

            return result;
        }

        public static int getInteger( JObject json, String key, int initResult )
        {
            int result = initResult;

            try{
                result = (int)json[key];
            }catch( System.Exception ex ){                
            }

            return result;
        }

        public static JObject getJObjct( JObject json, String key, bool isThrowEx )
        {
            JObject result = null;

            try{
                result = (JObject)json[key];
            }catch( System.Exception ex ){                
                if( isThrowEx ){
                    throw ex;
                }
            }

            return result;
        }
    }
}