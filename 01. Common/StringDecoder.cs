using System;
using Microsoft.Extensions.Logging;

namespace nmsAbmsWeb {
    public class StringDecoder {

        public static string base64( string data )
        {
            string result = null;

            try{
                System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
                System.Text.Decoder utf8Decode = encoder.GetDecoder();

                byte[] todecode_byte = Convert.FromBase64String(data);
                int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);

                char[] decoded_char = new char[charCount];
                utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);

                result = new String(decoded_char);

            }catch (System.Exception ex){
                throw ex;
            }

            return result;
        }

    }
}
