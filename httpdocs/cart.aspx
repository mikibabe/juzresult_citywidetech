<!-- #include file="config.cs" -->
<!-- #include file="cs\kit_fun.cs" -->
<script runat="server">

protected void Page_Load(Object Src, EventArgs E ) 
{
	if(!CartOnPageLoad())
		return;

	InitKit();
	
	PrintHeaderAndMenu("Shopping Cart");
	RememberLastPage();

    string shoppingCart = PrintCart(true, false, false);
	Response.Write(shoppingCart); //true to printer buttons

	PrintFooter();
}

</script>
