using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace FTPClient
{
    public static class FTPClient
    {



        public static void SendFile(Context context ,string file)
        {
            //string ftpHost = "ftp.eyetracki3a4.1gb.ru";
            string ftpUser = "w_eyetracki3a4_fc5b973e";
            string ftpPassword = "f2a565737xv";
            string ftpfullpath = "ftp://eyetracki3a4.1gb.ru/locations.txt";

            FtpWebRequest ftp = (FtpWebRequest)FtpWebRequest.Create(ftpfullpath);

            //userid and password for the ftp server  
            ftp.Credentials = new NetworkCredential(ftpUser, ftpPassword);

            ftp.KeepAlive = true;
            ftp.UseBinary = true;
            ftp.Method = WebRequestMethods.Ftp.UploadFile;

            FileStream fs = File.OpenRead(file);

            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);

            fs.Close();

            Stream ftpstream = ftp.GetRequestStream();
            ftpstream.Write(buffer, 0, buffer.Length);
            ftpstream.Close();



            FtpWebResponse response = (FtpWebResponse)ftp.GetResponse();

            Android.Util.Log.Debug("Upload File Complete, status {0}", response.StatusDescription);

            string status = "File Upload status " + response.StatusDescription;

            Toast.MakeText(context, status, ToastLength.Long).Show();

            response.Close();


            
        }


    }
}