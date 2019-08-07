using log4net;
using log4net.Config;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace ImgCopFile
{
    class Program
    {

        static void Main(string[] args)
        {

            // 监视代码运行时间
            Stopwatch stopwatch = new Stopwatch();
            // 开始监视
            stopwatch.Start();

            //定义logger
            var logger = LogManager.GetLogger(typeof(Program));
           

            // 调用日志
            InitLog4net();

            logger.Info("-------- 程序Start！ --------");
            logger.Info("Start-DataTime：" + DateTime.Now.ToShortDateString().ToString() + "  " + DateTime.Now.TimeOfDay.ToString());

            Thread.Sleep(3000);
            logger.InfoFormat("BuidImges：" + DateTime.Now.ToShortDateString().ToString() + "  " + DateTime.Now.TimeOfDay.ToString());

            //Imgcop
            BuidImges();
            string DirCount = ConfigurationManager.AppSettings["imagesDir"];
            System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(DirCount);
            logger.Info("文件总数：" + GetFilesCount(dirInfo));

            //delete
            //DeleteImges();

            logger.Info("--------- 程序End！------------");
            // 停止监视
            stopwatch.Stop();
            //获得当前实例测量得出的总时间
            TimeSpan timeSpan = stopwatch.Elapsed;
            logger.Info("Stop-DataTime：" + DateTime.Now.ToShortDateString().ToString() + "  " + DateTime.Now.TimeOfDay.ToString());
            logger.Info("SUM-DataTime：" + timeSpan);

            Console.ReadKey();
        }



        #region ImgCop
        /// <summary>
        /// ImgCop - OpenSQL 图片COP
        /// </summary>
        public static void BuidImges()
        {
            string connectionStr = ConfigurationManager.ConnectionStrings["master"].ConnectionString;

            var log = LogManager.GetLogger(typeof(Program));

            using (SqlConnection conn = new SqlConnection(connectionStr))
            {
                conn.Open();
                DataTable ImageTable = new DataTable();
                SqlDataAdapter Adapter = new SqlDataAdapter("SELECT * FROM  ImageSouce", conn);
                lock (Adapter)
                {
                    Adapter.Fill(ImageTable);
                }

                foreach (DataRow row in ImageTable.Rows)
                {
                    //获取appSetings配置的key获取value值
                    string imgdate = ConfigurationManager.AppSettings["imagesRootDir"];
                    //图像复制后保存的地址
                    string imgdir = ConfigurationManager.AppSettings["imagesDir"];

                    string dest = imgdir + row["ImageSouce"].ToString();

                    //System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(imgdir);
                    try
                    {
                        //创建复制后保存的文件夹
                        Directory.CreateDirectory(Path.GetDirectoryName(dest));
                        // 图片存放的原文件夹
                        File.Copy(imgdate + row["ImageSouce"].ToString(), dest); //剪切采用Move方法
                        log.InfoFormat(dest);
                        //当前路径下的文件总数
                        //log.Info("总文件数：" + GetFilesCount(dirInfo));

                    }
                    catch (Exception ex)
                    {
                        log.Info(ex.Message.ToString());
                        log.InfoFormat("Finished: FAILURE！");
                    }
                }
                conn.Close();
                log.InfoFormat("Finished: SUCCESS！");

            }
        }

        #endregion


        #region DeleteDir
        /// <summary>
        /// 根据数据库提供的表删除对应配置文件夹下的文件夹
        /// </summary>
        public static void DeleteImges()
        {
            string connectionStr = ConfigurationManager.ConnectionStrings["master"].ConnectionString;

            var log = LogManager.GetLogger(typeof(Program));

            using (SqlConnection conn = new SqlConnection(connectionStr))
            {
                conn.Open();
                DataTable ImageTable = new DataTable();
                SqlDataAdapter Adapter = new SqlDataAdapter("SELECT * FROM  ImageSouce", conn);
                lock (Adapter)
                {
                    Adapter.Fill(ImageTable);
                }

                foreach (DataRow row in ImageTable.Rows)
                {
                    //获取appSetings配置的key获取value值
                    string imgdate = ConfigurationManager.AppSettings["DeleteDir"];
                    string deletedir = imgdate + row["ImageSouce"].ToString();
                    System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(imgdate);
                    log.Info("要删除的文件总数：" + GetFilesCount(dirInfo));
                    try
                    {
                        string folderToBeDeleted = deletedir;
                        DirectoryInfo folder = new DirectoryInfo(folderToBeDeleted);
                        if (folder.Exists)
                        {
                            folder.Delete(true);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Info(ex.Message.ToString());
                        log.InfoFormat("Finished: FAILURE！");
                    }
                }
                conn.Close();
                log.InfoFormat("Finished: SUCCESS！");

            }
        }
        #endregion


        #region 获取当前目录下的文件总数

        public static int GetFilesCount(System.IO.DirectoryInfo dirInfo)
        {
            int totalFile = 0;
            totalFile += dirInfo.GetFiles().Length;
            foreach (System.IO.DirectoryInfo subdir in dirInfo.GetDirectories())
            {
                totalFile += GetFilesCount(subdir);
            }
            return totalFile;
        }

        #endregion


        #region 构造日志方法
        /// <summary>
        /// 定义日志
        /// </summary>
        private static void InitLog4net()
        {
            var log = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "Log4net.config");
            XmlConfigurator.ConfigureAndWatch(log);
        }
        #endregion

    }
}
