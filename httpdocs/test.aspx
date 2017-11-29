<!-- #include file="cs\sqlstring.cs" -->
<%@Language=C# Debug="true" %>
<%@Import Namespace="System.Web.Caching" %>
<%@Import Namespace="System.Web.Mail" %>
<%@Import Namespace="System.Data" %>
<%@Import Namespace="System.Data.SqlClient" %>
<script runat=server language=c#>

protected void Page_Load(Object Src, EventArgs E ) 
{
	Response.Write("ASP.NET is running ok.<br>");

        DateTime startTime = DateTime.Now;
	SqlConnection myConnection = new SqlConnection("Initial Catalog=" + m_sCompanyName + m_sDataSource + m_sSecurityString);
	SqlDataAdapter myAdapter;
	DataSet ds = new DataSet();
	
	string sc = " SELECT * FROM product ";
	try
	{
		myAdapter = new SqlDataAdapter(sc, myConnection);
		myAdapter.Fill(ds);
                
		Response.Write("SQL connection is ok.<br> " );
	}
	catch(Exception e) 
	{
		Response.Write("SQL connection is bad.<br>");
		Response.Write(e.ToString() + "<br><br>");
	}

	
	string mTo = "alert@eznz.com";
	string mFrom = "alert@eznz.com";
	string mSubject = "test.aspx : " + Request.ServerVariables["SERVER_NAME"];
	string strBody = "";

	try
	{
		 SendEmail(mFrom, mFrom, mTo, mSubject, strBody);
		Response.Write("smtp is working.<br>");
	}
	catch(Exception e)
	{
		Response.Write("smtp is bad, sendmail failed.<br>");
		Response.Write(e.ToString() + "<br><br>");
	}

TimeSpan interval = DateTime.Now - startTime;
Response.Write( interval.TotalSeconds + "seconds");

}

   public void SendEmail(String from, String fromTitle, String to, String subject, String body)
    {
        System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
        msg.To.Add(to);

        msg.From = new System.Net.Mail.MailAddress(from, fromTitle, System.Text.Encoding.UTF8);

        msg.Subject = subject;
        msg.SubjectEncoding = System.Text.Encoding.UTF8;
        msg.Body = body;
        msg.BodyEncoding = System.Text.Encoding.UTF8;
        msg.IsBodyHtml = true;
        msg.Priority = System.Net.Mail.MailPriority.High;

        System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient();

        object userState = msg;
        try
        {
            //client.Send(msg);
        }
        catch (System.Net.Mail.SmtpException ex)
        {
            //client.SendAsyncCancel();
        }
    }

    public void SendEmail(String from, String fromTitle, String to, String subject, String body, System.Net.Mail.Attachment attach)
{
    System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
    msg.To.Add(to);

    msg.From = new System.Net.Mail.MailAddress(from, fromTitle, System.Text.Encoding.UTF8);

    msg.Subject = subject;
    msg.SubjectEncoding = System.Text.Encoding.UTF8;
    msg.Body = body;
    msg.BodyEncoding = System.Text.Encoding.UTF8;
    msg.IsBodyHtml = true;
    msg.Priority = System.Net.Mail.MailPriority.High;
    if (attach != null)
    {
        msg.Attachments.Add(attach);
    }

    System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient();

    object userState = msg;
    try
    {
        //client.Send(msg);
    }
    catch (System.Net.Mail.SmtpException ex)
    {
        //client.SendAsyncCancel();
    }
}
</script>