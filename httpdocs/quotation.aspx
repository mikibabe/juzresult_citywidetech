<!-- #include file="config.cs" -->
<!-- #include file="cs\quotation.cs" -->

<script runat=server>
void Page_Load(Object Src, EventArgs E ) 
{
	TS_PageLoad(); //do common things, LogVisit etc...
	RememberLastPage();
	PrintHeaderAndMenu("");

	if(!PrintPage())
		return;

	PrintFooter();
}

</script>