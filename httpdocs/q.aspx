<!-- #include file="config.cs" -->
<!-- #include file="invoice.cs" -->
<!-- #include file="cs\q.cs" -->

<script runat=server>
void Page_Load(Object Src, EventArgs E ) 
{
	TS_PageLoad(); //do common things, LogVisit etc...

	if(!QPage_Load())
		return;
	PrintHeaderAndMenu("");
	PrintQForm();
	PrintFooter();
}

void PrintPageHeader()
{
	PrintHeaderAndMenu("");
}

</script>