<!-- #include file="cs\common.cs" -->
<!-- #include file="cs\autoftp.cs" -->
<!-- #include file="cs\schedule.cs" -->
 
<script runat=server>
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//site settings

const int m_nFirstCode = 1001;		//first available code in code_relations table, init a default value here

string m_sCompanyTitle = "";
bool g_bRetailVersion = true;
bool g_bOrderOnlyVersion = true;
bool g_bEnableQuotation = false;
bool g_bSysQuoteAddHardwareLabourCharge = false;
bool g_bSysQuoteAddSoftwareLabourCharge = false;
bool g_bDemo = true;
bool g_bUseSystemQuotation = false;

void TS_Init()
{
	myConnection = new SqlConnection("Initial Catalog=" + m_sCompanyName + m_sDataSource + m_sSecurityString);

	AppySiteSettings();

	if(!CheckBlockIPOK())
	{
		Response.End();
		return; //say nothing
	}

	string sc = "INSERT INTO web_uri_log (id, target, parameters) ";
	sc += " VALUES('" + Session["session_log_id"].ToString() + "', '";
	sc += EncodeQuote(Request.ServerVariables["URL"].ToString()) + "', '";
	sc += EncodeQuote(Request.ServerVariables["QUERY_STRING"].ToString()) + "') ";
	try
	{
		myCommand = new SqlCommand(sc);
		myCommand.Connection = myConnection;
		myCommand.Connection.Open();
		myCommand.ExecuteNonQuery();
		myCommand.Connection.Close();
	}
	catch(Exception e) 
	{
		AlertAdmin("Error LogVisit", e.ToString() + "\r\n\r\nQuery = \r\n" + sc);
//		ShowExp(sc, e);
	}

	Response.CacheControl = "NO-CACHE";

	Response.AppendHeader("Pragma", "no-cache");
	Response.Cache.SetExpires(DateTime.UtcNow.AddHours(12).AddSeconds(1));
	Response.Cache.SetNoServerCaching();

	if(Application[Session.SessionID.ToString() + "_msg"] != null)
	{
		string msg = Application[Session.SessionID.ToString() + "_msg"].ToString();
		bool bKill = false;
		if(msg.IndexOf("kill") == 0)
		{
			bKill = true;
			if(msg.IndexOf(Session["name"].ToString()) >= 0 && msg.IndexOf(Session["rip"].ToString()) >= 0)
				msg = "You killed yourself.";
			else
				msg = "Sorry, your session was killed by system administrator.";
		}
		Response.Write("<script Language=javascript");
		Response.Write(">\r\n");
		Response.Write("window.alert('" + msg + "')\r\n");
//		Response.Write(" rmsg = window.prompt('Any Return Message?')\r\n");
//		Response.Write("window.confirm(rmsg)\r\n");
		Response.Write("</script");
		Response.Write(">\r\n ");
		Application[Session.SessionID.ToString() + "_msg"] = null;
		if(bKill)
		{
			Response.Write("<script Language=javascript");
			Response.Write(">\r\n");
			Response.Write("window.close();\r\n");
			Response.Write("</script");
			Response.Write(">\r\n ");
			Session.Abandon();
		}
	}
}

void TS_PageLoad()
{
	TS_Init();
	CheckAutoFtp(false);
	CheckScheduleTasks();
	if(m_bDealerArea || m_bCheckLogin)
	{
		if(!SecurityCheck("normal"))
			return;
		if(!CustomerAccessCheck())
			return;
	}
}

bool CustomerAccessCheck()
{
	String uri = Request.ServerVariables["URL"];
	int p = uri.IndexOf("/");
	while(p >= 0)
	{
		uri = uri.Substring(p+1, uri.Length - p - 1);
		p = uri.IndexOf("/");
	}
	if(Session["no_access_" + uri] != null)
	{
		if(uri == "c.aspx")
			Response.Write("<meta http-equiv=\"refresh\" content=\"0; URL=sp.aspx?about\">");
		else
			Response.Write("<h3>ACCESS DENIED</h3>");
		Response.End();
		return false;
	}
	if(Request.QueryString["color_set"] != null)
	{
		if(Request.QueryString["color_set"] == "next")
		{
			int i = MyIntParse(Session["color_set"].ToString()) + 1;
			if(i > GetColorSets())
				i = 0;
			Session["color_set"] = i.ToString();
		}
		else
		{
			Session["color_set"] = Request.QueryString["color_set"];
		}
	}
	return true;
}

bool CheckPatent(string name)
{
	return true;
}

int GetColorSets()
{
	string sc = " SELECT * FROM color_set ORDER BY id ";
	try
	{
		myAdapter = new SqlDataAdapter(sc, myConnection);
		return myAdapter.Fill(dstcom, "colorsets");
	}
	catch(Exception e) 
	{
		ShowExp(sc, e);
		return 0;
	}
	return 0;
}


bool AppySiteSettings()
{
	if(MyBooleanParse(GetSiteSettings("branch_support", "0")))
		Session["branch_support"] = true;
	else
		Session["branch_support"] = null;

	if(Session[m_sCompanyName + "_retail_version"] == null)
		Session[m_sCompanyName + "_retail_version"] = MyBooleanParse(GetSiteSettings("system_retail_version", "0", true));
	g_bRetailVersion = (bool)Session[m_sCompanyName + "_retail_version"];
	
	if(Session[m_sCompanyName + "_orderonly_version"] == null)
		Session[m_sCompanyName + "_orderonly_version"] = MyBooleanParse(GetSiteSettings("system_orderonly_version", "1", true));
	g_bOrderOnlyVersion = (bool)Session[m_sCompanyName + "_orderonly_version"];

	if(m_sCompanyName != "b2a")
		g_bDemo = false;

	if(m_sSite == "www")
		m_bDealerArea = false;

	if(Session["session_log_id"] == null)
		Session["session_log_id"] = "";

	if(Session[m_sCompanyName + "AccessLevel"] == null)
		Session[m_sCompanyName + "AccessLevel"] = "0";

	if(Session["simple_freight"] == null)
	{
		if(GetSiteSettings("simple_freight_charge", "1") != "1")
			Session["simple_freight"] = false;
		else
			Session["simple_freight"] = true;
	}
	if(Session["SalesEmail"] == null)
	{
		m_sSalesEmail = GetSiteSettings("sales_email", "alert@ezsoft.com");
		if(g_bDemo)
			m_sCompanyTitle = Capital(GetSiteSettings("company_name", "Wholesale NZ Limited"));
		else
			m_sCompanyTitle = GetSiteSettings("company_name", "EZ Soft Corp.");
		Session["SalesEmail"] = m_sSalesEmail;
		Session["CompanyName"] = m_sCompanyTitle;
	}
	m_sSalesEmail = Session["SalesEmail"].ToString();
	m_sCompanyTitle = Session["CompanyName"].ToString();

	if(Session["button_style"] == null)
		Session["button_style"] = GetSiteSettings("button_style", EncodeQuote(" class=b"));

	if(Session["color_set"] == null)
		Session["color_set"] = "1";

	return true;
}

bool CheckBlockIPOK()
{
	int i = 0;
	int j = 0;
	string[] abip = new string[1024];
	string oneip = "";

	if(Session["block_ip"] == null)
	{
		string block_ip = GetSiteSettings("block_ip", "");
		for(i=0; i<block_ip.Length; i++)
		{
			if(block_ip[i] == ' ' || block_ip[i] == ',' || block_ip[i] == ';')
			{
				Trim(ref oneip);
				if(oneip != "")
				{
					abip[j++] = oneip;
					oneip = "";
				}
			}
			else
			{
				oneip += block_ip[i];
			}
		}
		if(oneip != "") //the last one
		{
			abip[j++] = oneip;
			oneip = "";
		}

		Session["block_ip"] = abip;
	}
	else
	{
		abip = (string[])Session["block_ip"];
	}
	string ip = "";
	if(Session["ip"] != null)
		ip = Session["ip"].ToString();
	if(ip == "")
		return true;
	for(i=0; i<abip.Length; i++)
	{
		oneip = abip[i];
		if(oneip == null)
			break;

//DEBUG("oneip=", oneip);
//DEBUG("ip=", ip);
		if(ip.IndexOf(oneip) == 0 && ip != "127.0.0.1")
			return false;
	}
	return true;
}

</script>