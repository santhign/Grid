using System;
using System.Text;
using Serilog;
using Core.Enums;

namespace Core.Helpers
{
    public class Base64Helper
    {
        public string base64Decode(string toDecode)
        {
            try
            {
                UTF8Encoding encoder = new UTF8Encoding();

                Decoder utf8Decode = encoder.GetDecoder();

                byte[] todecode_byte = Convert.FromBase64String(toDecode);

                int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);

                char[] decoded_char = new char[charCount];

                utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);

                string result = new String(decoded_char);

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
        }


        public string base64Encode(string toEncode)
        {
            try
            {
                byte[] encData_byte = new byte[toEncode.Length];

                encData_byte = System.Text.Encoding.UTF8.GetBytes(toEncode);

                string encodedData = Convert.ToBase64String(encData_byte);

                return encodedData;
            }
            catch (Exception ex)
            {
                Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
        }
    }
}
