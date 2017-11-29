<!-- #include file="config.cs" -->

<script runat=server>
void Page_Load(Object Src, EventArgs E ) 
{
	SecurityCheck("manager");
	TS_PageLoad(); //do common things, LogVisit etc...
//	if(Request.QueryString["cn"] == null)
//		return;
//	if(Request.QueryString["url"] == null)
//		return;
	string url = Request.QueryString["url"];
	string cn = Request.QueryString["cn"];

//	Response.Write("<h3>Refresh cache ...</h3>");
//	Response.Write("<h3>menu cache " + cn + " ... removed.</h3>");
	TSRemoveCache(m_sCompanyName + "_" + m_sHeaderCacheName);

	if(Request.QueryString["url"] != null)
	{
		Response.Write("<meta http-equiv=\"refresh\" content=\"0; URL=");
		Response.Write(url);
		Response.Write("\">");
	}
}
</script>