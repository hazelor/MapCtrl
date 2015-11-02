using Hazelor.Infrastructure.DataBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace Hazelor.MapCtrl
{
    internal static class SQLiteDBStore
    {
        private static Regex xFilter = new Regex(@"x=([0-9]*)");
        private static Regex yFilter = new Regex(@"y=([0-9]*)");
        private static Regex zFilter = new Regex(@"z=([0-9]*)");
        private static IniConfig conf = null;
        private const string TABLENAME = "MapInfo";
        private static SQLiteDBHelper _SQLiteDBHelper = new SQLiteDBHelper();
        private static int LastID = 0;
        private const int MAXIMAGENUMBER = 10000;
        public static void InitDB(string DBPath)
        {
            InitConf();
            if (!System.IO.File.Exists(DBPath))
            {
                //create db
                _SQLiteDBHelper.CreateDB(DBPath);
                //create table
                string sql = string.Format("CREATE TABLE {0}(id integer NOT NULL PRIMARY KEY UNIQUE, z integer NOT NULL, x integer NOT NULL, y integer NOT NULL, tile blob)", TABLENAME);
                _SQLiteDBHelper.ExecuteNonQuery(sql, null);
            }
            else
            {
                _SQLiteDBHelper = new SQLiteDBHelper(DBPath);
            }
            
        }

        public static void DestructDB()
        {
            SaveConf();
        }
        private static void InitConf()
        {
            if (conf == null)
            {
                conf = new IniConfig("MapDB.ini");
                string tmpstr = conf.get("LastID");
                if (tmpstr == null)
                {
                    conf.set("LastID", LastID.ToString());
                }
                else
                {
                    LastID = int.Parse(tmpstr);
                }
            }
        }

        private static void SaveConf()
        {
            if (conf != null)
            {
                conf.set("LastID", LastID.ToString());
                conf.save();
            }
        }
        public static void SaveTile(Stream stream, Uri uri)
        {
            string x = xFilter.Match(uri.ToString()).Groups[1].Value;
            string y = yFilter.Match(uri.ToString()).Groups[1].Value;
            string z = zFilter.Match(uri.ToString()).Groups[1].Value;

            SQLiteParameter[] paras = new SQLiteParameter[1];
            paras[0]= new SQLiteParameter("@data", DbType.Binary);

            byte[] buffer = StreamUtil.ReadFully(stream);
            paras[0].Value = buffer;
            string sql = string.Format("replace into {0} (id, z, x, y, tile) values({1}, {2}, {3}, {4}, @data)", TABLENAME, LastID++, z, x, y);
            //string sql = string.Format("update {0} set z={2}, x={3}, y={4}, tile=@data where id={1}", TABLENAME, LastID++, z, x, y);
            if (LastID > MAXIMAGENUMBER)
            {
                LastID = 0;
            }
            int rows = _SQLiteDBHelper.ExecuteNonQuery(sql, paras);
            if (rows <= 0)
            {
                
                //cannot save the tile into db
                throw new Exception("cannot save the tile into db");
            }

        }

        public static BitmapImage GetImage(Uri uri)
        {
            BitmapImage bitmap = null;

            string sql = ConverterToSQL(uri);
            using(var reader = _SQLiteDBHelper.ExecuteReader(sql, null))
            {
                while(reader.Read())
                {
                    byte[] buffer = GetBytes(reader);
                    if (buffer == null || buffer.Length == 0)
                    {
                        return bitmap;
                    }
                    bitmap = ToImage(buffer);
                }
            }
            
            return bitmap;
        }

        private static byte[] GetBytes(SQLiteDataReader reader)
        {
            const int CHUNK_SIZE = 2 * 1024;
            byte[] buffer = new byte[CHUNK_SIZE];
            long bytesRead;
            long fieldOffset = 0;
            using (MemoryStream stream = new MemoryStream())
            {
                while ((bytesRead = reader.GetBytes(0, fieldOffset, buffer, 0, buffer.Length)) > 0)
                {
                    stream.Write(buffer, 0, (int)bytesRead);
                    fieldOffset += bytesRead;
                }
                return stream.ToArray();
            }
        }

        private static BitmapImage ToImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();
                return image;
            }
        }
        private static string ConverterToSQL(Uri uri)
        {
            string x = xFilter.Match(uri.ToString()).Groups[1].Value;
            string y = yFilter.Match(uri.ToString()).Groups[1].Value;
            string z = zFilter.Match(uri.ToString()).Groups[1].Value;
            return string.Format("select tile from {0} where z={1} and x={2} and y={3}", TABLENAME, z, x, y);
        }
    }

    public static class StreamUtil
    {
        const int BufferSize = 8192;

        public static void CopyTo(Stream input, Stream output)
        {
            byte[] buffer = new byte[BufferSize];

            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }
        }

        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream tempStream = new MemoryStream())
            {
                CopyTo(input, tempStream);
                return tempStream.ToArray();
            }
        }

    }


}
