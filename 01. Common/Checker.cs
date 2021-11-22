using System;

namespace nmsAbmsWeb
{
    public class Checker
    {
		public static bool isNull( Object objParam )
		{
			bool bRet = false;
			do {
				if( objParam != null ) {
					break;
				}
				
				bRet = true;			
			}while( false );
			
			return bRet;
		}
		
		public static bool isEmpty( string strParam )
		{
			bool bRet = false;
			do {
				if( strParam != null && strParam != String.Empty ) {
					break;
				}
				
				bRet = true;			
			}while( false );
			
			return bRet;
		}

		public static bool compare(string strValSrc, string strValTarget ) {
			bool result = false;

			do{
				if (strValSrc != strValTarget) { break; }
				result = true;
			}while(false);

			return result;
		}

		public static bool contains(string strValSrc, string strValTarget ) {
			bool result = false;

			do{
				if (!strValSrc.Contains(strValTarget)) { break; }
				result = true;
			}while(false);

			return result;
		}
		
	}
}