
using Ionic.Zip;
using MSCLoader;
using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace DonnerTech_ECU_Mod.Reporter
{
    class Reporter
    {
        public static void GenerateReport(DonnerTech_ECU_Mod mod, Report[] reports, string reportType, bool showMessage = true)
        {
            string password = GenerateRandomPassword(6);
            string id = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "").ToLower();
            id = id.Substring(0, id.Length / 2);
            //Element with same key already exists
            DateTime dateTime = DateTime.Now;
            string formatedDateTime = dateTime.ToString("G", CultureInfo.CreateSpecificCulture("de-DE"));
            formatedDateTime = formatedDateTime.Replace(".", "-");
            formatedDateTime = formatedDateTime.Replace(" ", "_");
            formatedDateTime = formatedDateTime.Replace(":", "-");
            string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), String.Format("[{0} {1} {2}]_{3}.zip", id, reportType, mod.Version, formatedDateTime));

            

            using (ZipFile zip = new ZipFile())
            {
                zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
                zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                zip.Password = password;

                foreach (Report report in reports)
                {
                    int fileCounter = 0;
                    foreach (string file in report.files)
                    {
                        if (report.directory)
                        {
                            if(report.files.Length > 1)
                            {
                                zip.AddDirectory(file, report.name + fileCounter.ToString());
                                fileCounter++;
                            }
                            else
                            {
                                zip.AddDirectory(file, report.name);
                            }
                        }
                        else
                        {
                            zip.AddFile(file, report.name);
                        }
                    }
                }
                zip.Save(outputPath);
            }

            if (showMessage)
            {
                ModUI.ShowYesNoMessage(
                    "a .zip file has been saved to your Desktop\n" +
                    "The password is: " + "'" + password + "' (only known to you)\n" +
                    "The id is: " + "'" + id + "' (SEND ME BOTH with the zip)\n" + 
                    "Press Yes to upload to my server (you first need to agree to this in Mod Settings"
                    , "Report saved to desktop", delegate()
                    {
                        UploadReportToServer(mod, id, mod.Version, reportType, outputPath);
                    });
            }
        }

        private static void UploadReportToServer(DonnerTech_ECU_Mod mod, string id, string modVersion, string reportType, string fileToUpload)
        {
            if (!File.Exists(fileToUpload))
            {
                ModUI.ShowMessage("FIle could not be found under: \n" + fileToUpload);
                return;
            }
            
            string url = "http://test.msc.mbeym.de/report_upload.php";

            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("id", id);
            nvc.Add("modVersion", modVersion);
            nvc.Add("reportType", reportType);

            string response = HttpUploadFile(url, fileToUpload, "file", "application/json", nvc);

            ReportResponse reportResponse = new ReportResponse();
            if (response == "")
            {
                reportResponse.error = "Server response was empty. Something went wrong with the connection.\n Please try again at a later time";
                mod.logger.New(reportResponse.error);
            }
            else
            {
                
                try
                {
                    response = response.Replace("Array", ""); //Array{\"success\":\"Report has been saved to the Server\"}
                    response = response.Replace(@"\", "");
                    reportResponse = JsonConvert.DeserializeObject<ReportResponse>(response);
                }
                catch (Exception ex)
                {
                    reportResponse.error = "Something went wrong while trying to deserialize the server response\n Exception: " + ex.Message;
                    mod.logger.New("Something went wrong while trying to deserialize the server response", "", ex);
                }
            }

            if(reportResponse.error != ""){
                ModUI.ShowMessage(reportResponse.error);
            }
            else if(reportResponse.success != "")
            {
                ModUI.ShowMessage(reportResponse.success);
            }


        }

        private static string HttpUploadFile(string url, string file, string paramName, string contentType, NameValueCollection nvc)
        {
            string response = "";

            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            WebRequest wr = WebRequest.Create(url);

            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            Stream rs = wr.GetRequestStream();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, file, contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            try
            {
                rs.Close();
            }
            catch(Exception ex)
            {
                string exc = ex.Message;
            }

            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                response = reader2.ReadToEnd();
            }
            catch (Exception ex)
            {
                response = "";
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }
            return response;
        }


        private static string GenerateRandomPassword(int length)
        {
            RNGCryptoServiceProvider cryptRNG = new RNGCryptoServiceProvider();
            byte[] tokenBuffer = new byte[length];
            cryptRNG.GetBytes(tokenBuffer);
            return Convert.ToBase64String(tokenBuffer);
        }
    }
}
