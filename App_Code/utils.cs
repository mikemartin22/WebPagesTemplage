using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.WebControls;

/// <summary>
/// Summary description for utils
/// </summary>


namespace Utilities
{


    public class utils
    {
        public utils()
        {
            
        }

        public static string Hello()
        {
            return "Hello: " + DateTime.Now.ToString("yyyy-MM-dd hh:mm");
        }

        public static string TraceSqlCmd(SqlCommand sCmd)
        {
            string traceStr = "";
            traceStr += "SqlCommand: " + sCmd.CommandText + "\n";
            traceStr += "Parameters:\n";
            int count = 0;
            foreach (SqlParameter param in sCmd.Parameters)
            {
                traceStr += String.Format("({0})###>>> {1}|{2}\n", count++, param, param.Value);
            }
            traceStr += "\n";
            return traceStr;
        }

        public static string TraceExcept(Exception e)
        {
            return String.Format("Exception: {0}", e);
        }


        public static string StripTagsRegex(string source)
        {
            return Regex.Replace(source, "<.*?>", string.Empty);
        }


        public static string UploadFile(FileUpload newFile, string department, int deptID, string uname, string folder, string table)
        {
            //HttpServerUtility server = new HttpServerUtility();
            string success = "";
            string connStr = ConfigurationManager.ConnectionStrings["MainConnectionString"].ConnectionString;
            Trace.Write("Uploading to table: " + table);
            string newFileStr = String.Format("{0}-{1}{2}",
                Path.GetFileNameWithoutExtension(newFile.FileName).Replace(' ', '_'),
                DateTime.Now.ToString("MM-dd-yyyy"),
                Path.GetExtension(newFile.FileName));
            newFile.SaveAs(HttpContext.Current.Server.MapPath(folder) + newFileStr);
            string sqliCmd = String.Format("INSERT INTO {0} (f_dept_id, f_filename, f_upload_date, f_upload_user) VALUES (@dept_id, @fileName, @upload_date, @upload_user);", table);
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand sqli = conn.CreateCommand())
            {
                sqli.CommandText = sqliCmd;
                //sqli.Parameters.AddWithValue("@table", table);
                sqli.Parameters.AddWithValue("@dept_id", deptID);
                sqli.Parameters.AddWithValue("@fileName", newFileStr);
                sqli.Parameters.AddWithValue("@upload_date", DateTime.Now);
                sqli.Parameters.AddWithValue("@upload_user", uname);
                try
                {
                    conn.Open();
                    sqli.ExecuteNonQuery();
                    //Trace.Write(utils.TraceSqlCmd(sqli));
                    success = utils.TraceSqlCmd(sqli);
                }
                catch (Exception ex)
                {
                    success = String.Format("SQL Exception: {0}", ex);

                }

            }
            return success;               
        }
        
        public static void SendMessage(String body, String subject = "", String to = "", String cc = "", String bcc = "webmaster@salisbury.edu", String attachment = null)
        {
            //Create message
            MailMessage message = new MailMessage();
            message.Bcc.Add(bcc);
            if(cc != "")
            {
                message.CC.Add(cc);
            }
            message.To.Add(to);        
            message.Subject = subject + " (" + DateTime.Now.ToString("yyyy-MM-dd hh:mm") + ")";
            message.IsBodyHtml = true;
            StringBuilder bodyText = new StringBuilder("<html><body style=\"font-family: arial, sans-serif; font-size: 14px; line-height: 160%\">");
            bodyText.Append("<table style='width:100%'><tr><td style='padding:12px;background:#800000'><img style='margin:20px' src='https://webapps.salisbury.edu/common/images/su_logo_white.gif'/></td></tr></table>");          
            bodyText.Append("<div>" + body + "</div><p>This is an automated email, please do not reply.</p>");
            bodyText.Append("<div style='background:#800000;width:100%'>&nbsp;</div></body></html>");
            message.Body = bodyText.ToString();
            //create attachment if available
            if(attachment != null)
            {
                Attachment xlAttach = new Attachment(attachment);
                message.Attachments.Add(xlAttach);
            }

            //send message
            SmtpClient client = new SmtpClient();
            client.Send(message);
            client.Dispose();
            message.Dispose();
            if (attachment != null && File.Exists(attachment))
            {
                File.Delete(attachment);
            }
        }

        
    }
}