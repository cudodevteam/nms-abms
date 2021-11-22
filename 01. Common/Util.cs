using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace nmsAbmsWeb
{
    public class Util
    {
        public static void LogError( ILogger logger, System.Exception ex )
        {
            logger.LogError( ex.StackTrace );
            logger.LogError( ex.Message );            
        }

        public static bool isExistFile( string str_url )
        {
            bool bRet = false;

            do {
                FileInfo file_info = new FileInfo( str_url );

                bRet = file_info.Exists;
            } while( false );

            return bRet;
        }

        // 폴더 확인
        public static bool isExistDir( string str_url )
        {
            bool bRet = false;

            do {
                System.IO.DirectoryInfo directory_info = new System.IO.DirectoryInfo( str_url );

                bRet = directory_info.Exists;
            } while( false );

            return bRet;
        }

        public static bool CreateDir( string strDirPath )
        {
            bool bRet = false;

            do {
                DirectoryInfo di = new DirectoryInfo( strDirPath );
                if( di.Exists == false ) {
                    di.Create( );
                }

                bRet = true;
            } while( false );

            return bRet;
        }

        public static bool DeleteFile( string strPath )
        {
            bool bRet = false;

            try {
                do {
                    DirectoryInfo di = new DirectoryInfo( strPath );
                    if( di.Exists ) {
                        di.Delete( true );
                    }

                    bRet = true;
                } while( false );
            } catch( Exception ex ) { }
            
            return bRet;
        }

        public static long GetFileSize( string strPath )
        {
            FileInfo f = new FileInfo( strPath );
            return f.Length;
        }
        
        public static string GetCurDirPath()
        {
            return System.IO.Path.GetDirectoryName( Assembly.GetEntryAssembly( ).Location );
        }

        public static bool IsNumber( string strParam )
        {
            bool bRet = false;

            try {
                do {
                    if( strParam == null || strParam == string.Empty ) {
                        break;
                    }

                    double convVal = Convert.ToDouble( strParam );

                    bRet = true;

                } while( false );

            } catch( Exception ex ) { }

            return bRet;
        }

        public static IEnumerable<int> AllIndexesOf( string str, string value )
        {
            if( String.IsNullOrEmpty( value ) )
                throw new ArgumentException( "the string to find may not be empty", "value" );
            for( int index = 0; ; index += value.Length ) {
                index = str.IndexOf( value, index );
                if( index == -1 )
                    break;
                yield return index;
            }
        }

        public static ArrayList RemoveDuplicateArrayList( ArrayList sourceList )
        {
            ArrayList list = new ArrayList( );
            foreach( var item in sourceList ) {
                if( !list.Contains( item ) ) {
                    list.Add( item );
                }
            }
            return list;
        }

        public static string GetCurTimeStr()
        {
            return DateTime.Now.ToString( "HH:mm:ss.fff" );
        }

        public static long dateTimeToMillisec( DateTime datetime )
        {
            long result = 0;

            do{
                if( datetime == null ){
                    break;
                }

                long hour       =  datetime.Hour * ( 1000 * 60 * 60 );            
                long minute     =  datetime.Minute * ( 1000 * 60 );
                long second     =  datetime.Second * ( 1000 );
                long milliecond =  datetime.Millisecond;

                result = ( hour + minute + second + milliecond );
            }while( false );

            return result;
        }
    }
}