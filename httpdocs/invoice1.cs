<script runat=server>

double m_dOrderTotal = 0;
bool m_bDoSN = false;
DataSet dsi = new DataSet();
DataSet dstt = new DataSet();

string m_sInvType = "";

bool m_bShowPayment = false;
double m_invoice_total = 0;
int m_nItems = 0;
int m_nRowsToBottom = 25;

string m_account_number = "";
string m_account_term = "";
string m_account_name = "";

string m_inv_sNumber = "";

string m_itemCols = "7"; //item table columns

string InvoicePrintHeader(string sType, string sNumber, string sDate)
{
	return InvoicePrintHeader(sType, "", sNumber, sDate, "");
}

string InvoicePrintHeader(string sType, string sSales, string sNumber, string sDate)
{
	return InvoicePrintHeader(sType, sSales, sNumber, sDate, "");
}

string InvoicePrintHeader(string sType, string sSales, string sNumber, string sDate, string sPO_number)
{
	return InvoicePrintHeader(sType, sSales, sNumber, sDate, sPO_number, "", "");
}

string InvoicePrintHeader(string sType, string sSales, string sNumber, string sDate, string sPO_number, string card_id, string card_name)
{
	return InvoicePrintHeader(sType, sSales, sNumber, sDate, sPO_number, card_id, card_name, "");
}

string InvoicePrintHeader(string sType, string sSales, string sNumber, string sDate, string sPO_number, string card_id, string card_name, string supplier_invoice)
{
	string header = ReadSitePage("invoice_header");
	m_sInvType = sType;

	m_bDoSN = (sType == "invoice");

	string title = sType.ToUpper(); 

	if(m_bOrder)
		title = "PURCHASE ";
	if(sType == "invoice")
		title = "TAX INVOICE";
	if(sType.ToLower() == "quote")
		title = "QUOTE";
	if(sType.ToLower() == "order")
		title = "ORDER";

	header = header.Replace("@@title", title);

	string sTickets = "";
	
	//header = header.Replace("@@ticket", sTickets);

	StringBuilder sb = new StringBuilder();
	if(!m_bOrder)
	{
		sType = sType.ToUpper();
		sb.Append("<tr><td align=right><b>");
		sb.Append(sType + " Number : &nbsp;");
		sb.Append("</b></td><td>" + sNumber + "</td></tr>");

		if(sSales != "")
		{
			sb.Append("<tr><td align=right><b>Sales : &nbsp;</b></td><td>");
			sb.Append(sSales);
			sb.Append("</td></tr>");
		}
		sb.Append("<tr><td align=right><b>P.O.Number : &nbsp;</b></td><td>");
		sb.Append(sPO_number);
		sb.Append("</td></tr>");
	}
	else
	{
		sb.Append("<tr><td align=right><b>");
		sb.Append("P.O. Number : &nbsp;");
		sb.Append("</b></td><td>");
		sb.Append(sNumber);
		sb.Append("</td></tr>");

		sb.Append("<tr><td align=right><b>");
		sb.Append("Supplier INV# : &nbsp;");
		sb.Append("</b></td><td>");
		sb.Append(supplier_invoice);
		sb.Append("</td></tr>");
	}

	if(m_account_number == "")
	{
		m_account_number = card_id;
		if(Request.QueryString["t"] == "order")
			m_account_number = sSales;
		m_account_name = card_name;
	}
	if(title.IndexOf("PURCHASE") >= 0)
		m_account_number = m_account_name;
	header = header.Replace("@@account_number", m_account_number);
	header = header.Replace("@@account_term", m_account_term);
	header = header.Replace("@@invoice_number", sb.ToString());
	header = header.Replace("@@date", sDate);
	return header;
}

bool DoGetTicketNo(string sInvNo)
{
	string sc = "SELECT ship_name, ship_desc, price, ticket";
		   sc += " FROM invoice_freight WHERE invoice_number=" + sInvNo;
//		   sc += " GROUP BY ship_name, ship_desc, price, ticket";
//DEBUG("sc = ", sc);
	try
	{
		myAdapter = new SqlDataAdapter(sc, myConnection);
		myAdapter.Fill(dstt, "tickets");
	}
	catch(Exception e) 
	{
		ShowExp(sc, e);
		return false;
	}
	return true;
}

string InvoicePrintShip(DataRow dr, string shipto)
{
	StringBuilder sb = new StringBuilder();
	sb.Append("<table width=75% border=0><tr><td width=" + GetSiteSettings("envelope_left_margin", "10") + ">&nbsp;</td>");
	sb.Append("<td valign=top>");

	//bill to
	sb.Append("<table border=0><tr><td>");
	sb.Append("<b>Bill To:</b><br>\r\n");
	if(dr == null)
	{
		sb.Append("Cash Sales<br><br>");
		sb.Append("</td></tr></table>\r\n");
		sb.Append("</td></tr></table>\r\n");
		return sb.ToString();
	}

	string sCompany = "";
	string sAddr = "";
	string sContact = "";

	if(dr["postal1"].ToString() != "")
	{
		sAddr = dr["postal1"].ToString();
		sAddr += "<br>";
		sAddr += dr["postal2"].ToString();
		sAddr += "<br>";
		sAddr += dr["postal3"].ToString();
	}
	else
	{
		sAddr = dr["Address1"].ToString();
		sAddr += "<br>";
		sAddr += dr["Address2"].ToString();
		sAddr += "<br>";
		sAddr += dr["address3"].ToString();
	}

	sCompany = dr["trading_name"].ToString();

	if(sCompany == "")
	{
		sCompany = dr["Name"].ToString();
	}
	else //if we have company name, then put contact person name here as well
	{
		sContact = dr["Name"].ToString();
	}

	sb.Append(sCompany);
	sb.Append("<br>\r\n");
	sb.Append(sAddr);
	sb.Append("<br>\r\n");
//	sb.Append(dr["Email"].ToString());
//	sb.Append("<br>\r\n");
	if(dr["phone"] != null)
		sb.Append(dr["phone"].ToString());

	sb.Append("</td></tr></table></td><td valign=top align=right><table border=0><tr><td>");

	//ship to, if "not pick up  --- 1"
	if(dr["shipping_method"].ToString() != "1")
	{
		sb.Append("<b><font size=3>Ship To:</font></b><br>\r\n");
		if(bool.Parse(dr["special_shipto"].ToString()))
		{
			sb.Append("<font size=3>" + dr["shipto"].ToString().Replace("\r\n", "\r\n<br>") + "</font>");
		}
		else
		{
			if(dr["Address1"].ToString() != "")
			{
				sAddr = dr["Address1"].ToString();
				sAddr += "<br>";
				sAddr += dr["Address2"].ToString();
				sAddr += "<br>";
				sAddr += dr["address3"].ToString();
			}
			else
			{
				sAddr = dr["postal1"].ToString();
				sAddr += "<br>";
				sAddr += dr["postal2"].ToString();
				sAddr += "<br>";
				sAddr += dr["postal3"].ToString();
			}

			sb.Append("<font size=2>" + sCompany + "</font>");
			sb.Append("<br>\r\n");
			sb.Append("<font size=2>" + sAddr + "</font>");
			sb.Append("<br>\r\n");
	//		sb.Append(dr["Email"].ToString());
	//		sb.Append("<br>\r\n");
			if(dr["phone"] != null)
			{
				sb.Append("<font size=2>" + dr["phone"].ToString() + "</font>");
				sb.Append("<br>\r\n");
			}
			if(dr["name"].ToString() != "")
			{
				sb.Append("<font size=2>ATTN : " + dr["name"].ToString() + "</font>");
				sb.Append("<br>\r\n");
			}
		}
	}
	sb.Append("</td></tr></table>\r\n");
	//end of ship to

	sb.Append("</td></tr></table>\r\n");
	return sb.ToString();
}

string InvoicePrintBottom()
{
	return InvoicePrintBottom("");
}

string InvoicePrintBottom(String sComment)
{
	StringBuilder sb = new StringBuilder();
	string bottom = ReadSitePage("invoice_footer");
	// Note/Comment table
	if(sComment != "" && sComment != null)
	{
		sb.Append("<br>  Comment :<br>");
		sb.Append(sComment.Replace("\r\n", "\r\n<br>"));
	}

	bottom = bottom.Replace("@@comment", sb.ToString());
	for(int i=m_nRowsToBottom; i>m_nItems; i--)
		sb.Append("<br>");
	
	return bottom;
}

string check_IsNumber(string s_text)
{
	bool isNum = true;
	if(s_text == null)
		return "false";
	int ptr = 0;
	while (ptr < s_text.Length)
	{
		if (!char.IsDigit(s_text, ptr++))
		{
			isNum = false;
			break;
		}
	}
	return isNum.ToString();
}

string BuildInvoice(string sInvoiceNumber)
{
	m_inv_sNumber = sInvoiceNumber;
	string m_sKitTerm = GetSiteSettings("package_bundle_kit_name", "Kit", true);

	string CheckDigit = check_IsNumber(sInvoiceNumber);
	if(CheckDigit == "False")
		return "<h3>Invoice# " + sInvoiceNumber + " Not Found</h3>";

	DataSet dsi = new DataSet();
	string sc = "SELECT i.invoice_number, i.type, i.payment_type, i.commit_date, i.price ";
	sc += ", i.tax, i.total, i.sales, i.cust_ponumber, ";
	sc += " i.freight, i.sales_note, c.*, s.code, s.supplier_code, s.quantity, s.name as item_name ";
	sc += ", s.commit_price, i.type, i.special_shipto, i.shipto, shipping_method, ";
	sc += " s.status AS si_status, s.note, s.shipby, s.ship_date, s.ticket, s.processed_by ";
	sc += ", s.system AS bSystem, i.system AS iSystem, cr.is_service, i.no_individual_price AS iNoIndividual ";
	sc += ", s.kit, s.krid, k.kit_id, k.qty AS kit_qty, k.name AS kit_name, k.commit_price AS kit_price ";
	sc += " FROM sales s JOIN invoice i ON s.invoice_number=i.invoice_number LEFT OUTER JOIN card c ON c.id=i.card_id ";
	sc += " JOIN code_relations cr ON cr.code = s.code ";
	sc += " LEFT OUTER JOIN sales_kit k ON k.invoice_number=i.invoice_number AND k.krid = s.krid ";
	sc += " WHERE i.invoice_number=";
	sc += sInvoiceNumber;
	if(!SecurityCheck("sales", false))
		sc += " AND i.card_id=" + Session["card_id"];
	sc += " ORDER BY s.id ";
//DEBUG("sc = ", sc);
	int rows = 0;
	try
	{
		myAdapter = new SqlDataAdapter(sc, myConnection);
		rows = myAdapter.Fill(dsi);
	}
	catch(Exception e) 
	{
		ShowExp(sc, e);
		return "";
	}

	if(rows <= 0)
		return "<h3>Invoice# " + sInvoiceNumber + " Not Found</h3>";

	DataRow dr = dsi.Tables[0].Rows[0];
	m_account_number = dr["id"].ToString();
	m_account_term = dr["credit_term"].ToString();
	m_account_term = GetEnumValue("credit_terms", m_account_term);
	m_account_name = dr["trading_name"].ToString();

//	if(dr["email"].ToString() != Session["email"].ToString() && !SecurityCheck("manager"))
//		return "<h3>ACCESS DENIED</h3>";
	
	string sPaymentType = GetEnumValue("payment_method", dr["payment_type"].ToString());
	string sDate = dr["commit_date"].ToString();
	DateTime tDate = DateTime.Parse(sDate);
	string status = dr["si_status"].ToString();
	string sType = GetEnumValue("receipt_type", dr["type"].ToString());
	if(status == "Back Order")
		sType = status;

	string sales = dr["sales"].ToString();
	string po_number = dr["cust_ponumber"].ToString();

	StringBuilder sb = new StringBuilder();

	sb.Append("<html><style type=\"text/css\">\r\n");
	sb.Append("td{FONT-WEIGHT:300;FONT-SIZE:8PT;FONT-FAMILY:verdana;}\r\n");
	sb.Append("body{FONT-WEIGHT:300;FONT-SIZE:8PT;FONT-FAMILY:verdana;}</style>\r\n");
	
	//print header
	sb.Append(InvoicePrintHeader(sType, sales, sInvoiceNumber, tDate.ToString("dd/MM/yyyy"), po_number));

	//print shippment address
	sb.Append("<hr>");
	sb.Append(InvoicePrintShip(dr, ""));
	sb.Append("<hr>");
	
	//print invoiced items
	sb.Append(BuildItemTable(dsi.Tables[0], false));

	sb.Append("<br>");

	m_nRowsToBottom = MyIntParse(GetSiteSettings("invoice_blank_rows_added", "25"));

	//print bottom
	sb.Append(InvoicePrintBottom(dr["sales_note"].ToString()));

	sb.Append("</body></html>");

	return sb.ToString();
}

string GetError(string sInvoiceNumber)
{
	int rows = 0;
	DataSet dserr = new DataSet();
	string sc = "SELECT trans_failed_reason, debug_info FROM invoice WHERE invoice_number=" + sInvoiceNumber;
	try
	{
		SqlDataAdapter myCommand = new SqlDataAdapter(sc, myConnection);
		rows = myCommand.Fill(dserr);
//DEBUG("rows=", rows);
	}
	catch(Exception e) 
	{
		ShowExp(sc, e);
		return "";
	}
	if(rows < 0)
		return "";
	string failed_reason = dserr.Tables[0].Rows[0]["trans_failed_reason"].ToString();
	string debug_info = dserr.Tables[0].Rows[0]["debug_info"].ToString();
	string sRet = "<br><b>failed Reason : </b><br>" + failed_reason;
	if(m_sSite == "admin")
		sRet += "<br><br><b>Debug Info : </b><br>" + debug_info;
	return sRet;
}

string BuildItemTable(DataTable dt, bool bOrder)
{
	string m_sKitTerm = GetSiteSettings("package_bundle_kit_name", "Kit", true);

	bool bSystem = false;
	bool bIndividual_Price = false;
	//DEBUG("border -= ", bOrder.ToString());
	//if(!bOrder)
	bSystem = bool.Parse(dt.Rows[0]["iSystem"].ToString());
	bIndividual_Price = bool.Parse(dt.Rows[0]["iNoIndividual"].ToString()); //tee for display price when no individual price is untick
	
//DEBUG("bysteym = ", bSystem.ToString());
	StringBuilder sb = new StringBuilder();

	sb.Append("<br>\r\n\r\n");
	sb.Append("<table cellpadding=0 cellspacing=0 width=100%");
	if(bSystem)
		sb.Append(" bgcolor=#FFFFEE><tr><td><b>SYSTEM</b></td></tr><tr>");
	else
	{
		sb.Append(">\r\n");
		sb.Append("<tr style=\"color:black;background-color:#EEEEEE;font-weight:bold;\">");
	}
	sb.Append("<td width=70>CODE</td>\r\n");
	sb.Append("<td width=130>M_PN#</td>\r\n");
	sb.Append("<td>DESCRIPTION</td>\r\n");
	sb.Append("<td width=130>&nbsp;</td>\r\n");
//DEBUG("bIndividual_Price = ", bIndividual_Price.ToString());
//	if(bSystem && bIndividual_Price)
	if(bIndividual_Price)
	{
		sb.Append("<td width=70 align=right>&nbsp;</td>\r\n");
		sb.Append("<td width=40 align=right>QTY</td>\r\n");
		sb.Append("<td width=70 align=right>&nbsp;</td></tr>\r\n");
	}
	else
	{
		sb.Append("<td width=70 align=right>PRICE</td>\r\n");
		sb.Append("<td width=40 align=right>QTY</td>\r\n");
		sb.Append("<td width=70 align=right>AMOUNT</td></tr>\r\n");
	}

	sb.Append("<tr><td colspan=" + m_itemCols + "><hr></td></tr>\r\n");
	//build up list
	int i = 0;
	int j = 0;
	string kit_id = "";
	string kit_id_old = "";
	for(i=0; i<dt.Rows.Count; i++)
	{
		if(bSystem)
		{
			bool bSys = bool.Parse(dt.Rows[i]["bsystem"].ToString());
			if(!bSys)
				continue;
		}
		//bool bService = bool.Parse(dt.Rows[i]["is_service"].ToString());
		bool bService = false;
		if(dt.Rows[i]["is_service"].ToString() != null && dt.Rows[i]["is_service"].ToString() != "")
			bService = bool.Parse(dt.Rows[i]["is_service"].ToString());
		
		if(bService)
			continue; //put them to the end
		if(MyBooleanParse(dt.Rows[i]["kit"].ToString()))
		{
			kit_id = dt.Rows[i]["kit_id"].ToString();
			string kit_name = dt.Rows[i]["kit_name"].ToString();
			string kit_qty = dt.Rows[i]["kit_qty"].ToString();
			double dKitPrice = MyDoubleParse(dt.Rows[i]["kit_price"].ToString());
			double dKitTotal = dKitPrice * MyDoubleParse(kit_qty);
			if(kit_id != kit_id_old)
			{
				sb.Append("<tr bgcolor=aliceblue>");
				sb.Append("<td nowrap>" + m_sKitTerm + "#" + kit_id + " &nbsp&nbsp;</td>");
				sb.Append("<td>&nbsp;</td>"); //m_pn
				sb.Append("<td>" + kit_name + "</td>");
				sb.Append("<td>&nbsp;</td>");
				sb.Append("<td align=right>" + dKitPrice.ToString("c") + "</td>");
				sb.Append("<td align=right>" + kit_qty + "</td>");
				sb.Append("<td align=right>" + dKitTotal.ToString("c") + "</td></tr>");
				kit_id_old = kit_id;
			}
			sb.Append(InvoicePrintOneRow(dt.Rows[i], bSystem, bIndividual_Price, true));
		}
		else
		{
			sb.Append(InvoicePrintOneRow(dt.Rows[i], bSystem, bIndividual_Price));
		}
		j++;
	}

	//m_nItems = j;
	//now print service items
	for(i=0; i<dt.Rows.Count; i++)
	{
		if(bSystem)
		{
			bool bSys = bool.Parse(dt.Rows[i]["bsystem"].ToString());
			if(!bSys)
				continue;
		}
		
		//bool bService = bool.Parse(dt.Rows[i]["is_service"].ToString());
		bool bService = false;
		if(dt.Rows[i]["is_service"].ToString() != null && dt.Rows[i]["is_service"].ToString() != "")
			bService = bool.Parse(dt.Rows[i]["is_service"].ToString());

		if(!bService)
			continue; //put them to the end
		sb.Append(InvoicePrintOneRow(dt.Rows[i], bSystem, bIndividual_Price));
	}

//DEBUG("rows="+dt.Rows.Count, " j="+j);

	if(bSystem && j < dt.Rows.Count)
	{
		sb.Append("<tr><td colspan=" + m_itemCols + "><hr></td></tr>\r\n");
		sb.Append("</table><br>");
		sb.Append("<table cellpadding=0 cellspacing=0 width=100%>\r\n");
		sb.Append("<tr><td colspan=5><b>OTHER PARTS</b></td></tr>");
		sb.Append("<tr style=\"color:black;background-color:#FFFFFF;font-weight:bold;\">");
		sb.Append("<td width=70>PART#</td>\r\n");
		sb.Append("<td width=130>M_PN#</td>\r\n");
		sb.Append("<td>DESCRIPTION</td>\r\n");
		if(bSystem && bIndividual_Price)
		{
			sb.Append("<td width=70 align=right>&nbsp;</td>\r\n");
			sb.Append("<td width=40 align=right>QTY</td>\r\n");
			sb.Append("<td width=70 align=right>&nbsp;</td></tr>\r\n");
		}
		else
		{
			sb.Append("<td width=70 align=right>PRICE</td>\r\n");
			sb.Append("<td width=40 align=right>QTY</td>\r\n");
			sb.Append("<td width=70 align=right>AMOUNT</td></tr>\r\n");
		}
	
		sb.Append("<tr><td colspan=" + m_itemCols + "><hr></td></tr>\r\n");

		//build up list
		for(i=0; i<dt.Rows.Count; i++)
		{
			j += 1;
			if(bool.Parse(dt.Rows[i]["bsystem"].ToString()))
				continue;
			sb.Append(InvoicePrintOneRow(dt.Rows[i], bSystem, bIndividual_Price));
		}
	}
	m_nItems = j; 
	double dTotal = 0;
	double dTAX = 0;
	double dAmount = 0;
	double dFreight = MyDoubleParse(dt.Rows[0]["freight"].ToString());

	if(bOrder)
	{
		dTotal = m_dOrderTotal;
		double dGST = MyDoubleParse(GetSiteSettings("gst_rate_percent", "12.5")) / 100; //Get GST rate for Site Sitting //Modified by NEO
		if(dt.Rows[0]["quote_total"].ToString() != "" && dt.Rows[0]["quote_total"].ToString() != "0")
			dTotal = double.Parse(dt.Rows[0]["quote_total"].ToString(), NumberStyles.Currency, null);
//		dTAX = (dTotal + dFreight) * 0.15; //MyDoubleParse(Session["gst_rate"].ToString());
//		dTAX = (dTotal + dFreight) * dGST;							//Modified by NEO
		dTAX = (dTotal + dFreight) * MyDoubleParse(dt.Rows[0]["gst_rate"].ToString()); 
		dTAX = Math.Round(dTAX, 2);
		dAmount = dTotal + dFreight + dTAX;
	}
	else
	{
		dTotal = double.Parse(dt.Rows[0]["price"].ToString(), NumberStyles.Currency, null);
		dTAX = double.Parse(dt.Rows[0]["tax"].ToString(), NumberStyles.Currency, null);
		dAmount = double.Parse(dt.Rows[0]["total"].ToString(), NumberStyles.Currency, null);
	}

	sb.Append("<tr><td colspan=" + m_itemCols + ">&nbsp;</td></tr>\r\n");
	sb.Append("<tr><td colspan=" + m_itemCols + "><hr></td></tr>\r\n");

	if(m_sInvType == "invoice")// && dFreight > 0)
	{
		sb.Append(PrintFreightTicket());
	}

	//sub-total
	sb.Append("<tr><td colspan=" + (MyIntParse(m_itemCols) - 1).ToString() + " align=right><b>Sub-Total : </b></td><td align=right>");
	sb.Append(dTotal.ToString("c"));
	sb.Append("</td></tr>\r\n");

	sb.Append("<tr><td colspan=" + (MyIntParse(m_itemCols) - 1).ToString() + " align=right valign=top nowrap>");
	sb.Append("<b>Freight : </b></td><td align=right>" + dFreight.ToString("c") + "</font></td></tr>");
	
	sb.Append("<tr><td colspan=" + (MyIntParse(m_itemCols) - 1).ToString() + " align=right><b>Tax : </b></td><td align=right>");
	sb.Append(dTAX.ToString("c"));
	sb.Append("</td></tr>\r\n");

	sb.Append("<tr><td colspan=" + (MyIntParse(m_itemCols) - 1).ToString() + " align=right><b>Total Amount : </b></td><td align=right>");
	sb.Append(dAmount.ToString("c"));

	if(m_bShowPayment)
	{
		sb.Append("<tr><td colspan=" + (MyIntParse(m_itemCols) - 1).ToString() + ">");
		sb.Append(ShowPayment());
		sb.Append("</td></tr>");
	}
	sb.Append("</td></tr></table>\r\n");

	return sb.ToString();
}

string ShowPayment()
{
	DataSet dspay = new DataSet();
	string sc = " SELECT t.amount, i.amount_applied, d.*, c.name AS staff, e.name AS payment ";
	sc += " FROM tran_invoice i JOIN trans t ON t.id = i.tran_id ";
	sc += " JOIN tran_detail d ON d.id = i.tran_id ";
	sc += " LEFT OUTER JOIN card c ON c.id = d.staff_id ";
	sc += " LEFT OUTER JOIN enum e ON (e.id = d.payment_method AND e.class='payment_method') ";
	sc += " WHERE i.invoice_number = '" + m_inv_sNumber + "' ";
	int rows = 0;
	try
	{
		myAdapter = new SqlDataAdapter(sc, myConnection);
		rows = myAdapter.Fill(dspay, "payment");
	}
	catch(Exception e) 
	{
		ShowExp(sc, e);
		return "";
	}

	if(rows <= 0)
		return "<font color=red><b>UNPAID</b></font>";

	double dAppliedTotal = 0;

	StringBuilder sb = new StringBuilder();

	sb.Append("<table cellspacing=1 cellpadding=1 bordercolor=#EEEEEE bgcolor=white");
	sb.Append(" style=\"font-family:Verdana;font-size:8pt;border-width:1px;border-style:Solid;border-collapse:collapse;fixed\">");
	sb.Append("<tr><td colspan=10><font size=+1>Payment Information</font></td></tr>");
	sb.Append("<tr style=\"color:black;background-color:#EEEEEE;font-weight:bold;\">");
	sb.Append("<th>Date &nbsp; </th>");
	sb.Append("<th>Type &nbsp; </th>");
	sb.Append("<th>Total &nbsp; </th>");
	sb.Append("<th>Ref &nbsp; </th>");
	sb.Append("<th>PaidBy &nbsp; </th>");
	sb.Append("<th>Bank &nbsp; </th>");
	sb.Append("<th>Branch &nbsp; </th>");
	sb.Append("<th>Accountant &nbsp; </th>");
	sb.Append("<th>Note &nbsp; </th>");
	sb.Append("<th>Applied &nbsp; </th>");
	sb.Append("</tr>");

	for(int i=0; i<rows; i++)
	{
		DataRow dr = dspay.Tables["payment"].Rows[i];
		string payment_method = dr["payment"].ToString().ToUpper();
		string payment_date = DateTime.Parse(dr["trans_date"].ToString()).ToString("dd-MM-yyyy HH:mm");
		double dApplied = MyDoubleParse(dr["amount_applied"].ToString());
		string staff = dr["staff"].ToString();
		string payment_ref = dr["payment_ref"].ToString();
		string paid_by = dr["paid_by"].ToString();
		string bank = dr["bank"].ToString();
		string branch = dr["branch"].ToString();
		double dAmount = MyDoubleParse(dr["amount"].ToString());
		string note = dr["note"].ToString();
		
		dAppliedTotal += dApplied;
		sb.Append("<tr>");
		sb.Append("<td>" + payment_date + "</td>");
		sb.Append("<td>" + payment_method + "</td>");
		sb.Append("<td>" + dAmount.ToString("c") + "</td>");
		sb.Append("<td>" + payment_ref + "</td>");
		sb.Append("<td>" + paid_by + "</td>");
		sb.Append("<td>" + bank + "</td>");
		sb.Append("<td>" + branch + "</td>");
		sb.Append("<td>" + staff + "</td>");
		sb.Append("<td>" + note + "</td>");
		sb.Append("<td align=right>" + dApplied.ToString("c") + "</td>");
		sb.Append("</tr>");
	}
	sb.Append("<tr><td colspan=9 align=right><b>Total Applied : </b></td>");
	sb.Append("<td align=right><b>" + dAppliedTotal.ToString("c") + "<b></td></tr>");

	double dLeft = m_invoice_total - dAppliedTotal;

	sb.Append("<tr><td colspan=9 align=right><b>Amount Owed : </b></td>");
	sb.Append("<td align=right><font color=red><b>" + dLeft.ToString("c") + "<b></font></td></tr>");
	
	sb.Append("</table>");

	return sb.ToString();
}

string PrintFreightTicket()
{
	StringBuilder sb = new StringBuilder();
	double ship_dtotal = 0;
	if(!DoGetTicketNo(m_inv_sNumber))
		return sb.ToString();
	else
	{
		int tickets = dstt.Tables["tickets"].Rows.Count;
		if(tickets <= 0)
			return "";

		sb.Append("<tr><td colspan=6 align=right>\r\n");

		DataRow drt = null;
		//List shipping tickets;
		for(int f=0; f<tickets; f++)
		{
			drt = dstt.Tables["tickets"].Rows[f];
			sb.Append("<tr><td>&nbsp;</td>");
			sb.Append("<td>" + drt["ticket"].ToString() + "</td>");
			sb.Append("<td>" + drt["ship_name"].ToString() + " - ");// + "</td>");
			sb.Append(drt["ship_desc"].ToString() + "</td>");
			sb.Append("<td colspan=3 align=right>" + MyMoneyParse(drt["price"].ToString()).ToString("c") + "</td></tr>");
			ship_dtotal += MyMoneyParse(drt["price"].ToString());
		}
		sb.Append("</td></tr>");
		sb.Append("<tr><td colspan=6><hr></td></tr>\r\n");
	}
	return sb.ToString();
}
/*
string PrintKits(bool bIndividual_Price, bool bOrder)
{
	StringBuilder sb = new StringBuilder();

	DataSet dsi = new DataSet();
	
	string sc = "SELECT k.kit_id, k.krid, k.qty AS quantity, '" + m_sKitTerm + " #" + "' + STR(kit_id) AS code ";
	sc += ", k.name AS item_name, k.commit_price, 'false' AS is_service ";
	if(bOrder)
		sc += " FROM order_kit k JOIN orders o ON k.id=o.id WHERE o.id=" + m_inv_sNumber;
	else
		sc += " FROM sales_kit k JOIN invoice o ON o.invoice_number = k.invoice_number WHERE o.invoice_number = " + m_inv_sNumber;
	sc += " ORDER BY k.krid ";
	int rows = 0;
	try
	{
		myAdapter = new SqlDataAdapter(sc, myConnection);
		rows = myAdapter.Fill(dsi, "kit");
	}
	catch(Exception e) 
	{
		ShowExp(sc, e);
		return "";
	}

	if(rows <= 0)
		return "";

	for(int i=0; i<dsi.Tables["kit"].Rows.Count; i++)
	{
		DataRow dr = dsi.Tables["kit"].Rows[i];

		//print kit name
		sb.Append(InvoicePrintOneRow(dr, false, bIndividual_Price, true));
		
		//print kit items
		string kit_id = dr["kit_id"].ToString();
		string krid = dr["krid"].ToString();

		DataSet dsKitInv = new DataSet();
		//sc = " SELECT k.*, c.name FROM kit_item k JOIN code_relations c ON c.code=k.code WHERE k.id = " + kit_id;
		if(bOrder)
			sc = " SELECT k.*, o.item_name FROM order_kit k JOIN order_item o ON o.id = k.id WHERE k.kit_id = "+ kit_id +" AND k.id = "+ m_inv_sNumber;
		else
			sc = " SELECT k.*, s.name AS item_name FROM sales_kit k JOIN sales s ON s.invoice_number = k.invoice_number WHERE k.kit_id = "+ kit_id +" AND k.invoice_number = "+ m_inv_sNumber;

		try
		{
			SqlDataAdapter myCommand = new SqlDataAdapter(sc, myConnection);
			myCommand.Fill(dsKitInv, "kit_item");
		}
		catch(Exception e) 
		{
			ShowExp(sc, e);
			return "";
		}
		for(int j=0; j<dsKitInv.Tables["kit_item"].Rows.Count; j++)
		{
			dr = dsKitInv.Tables["kit_item"].Rows[j];
			sb.Append("<tr bgcolor=aliceblue>");
			sb.Append("<td>&nbsp;</td>"); //no code
			sb.Append("<td> &nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp; x " + dr["qty"].ToString() + " " + dr["item_name"].ToString());
			sb.Append("</td><td colspan=4>&nbsp;</td></tr>\r\n");
		}
	}
	return sb.ToString();
}
*/
string InvoicePrintOneRow(DataRow dr, bool bSystem, bool bIndividual_Price)
{
	return InvoicePrintOneRow(dr, bSystem, bIndividual_Price, false);
}

string InvoicePrintOneRow(DataRow dr, bool bSystem, bool bIndividual_Price, bool bKit)
{
//	string m_sKitTerm = GetSiteSettings("package_bundle_kit_name", "Kit", true);
	double dPrice = double.Parse(dr["commit_price"].ToString(), NumberStyles.Currency, null);
	double quantity = MyDoubleParse(dr["quantity"].ToString());

	dPrice = Math.Round(dPrice, 2);
	double dTotal = dPrice * quantity;
	dTotal = Math.Round(dTotal, 2);

	m_dOrderTotal += dTotal;

	StringBuilder sb = new StringBuilder();

	sb.Append("<tr");
	if(bKit)
		sb.Append(" bgcolor=aliceblue");
	sb.Append(">");

//	bool bService = bool.Parse(dr["is_service"].ToString());
	bool bService = false;
		if(dr["is_service"].ToString() != null && dr["is_service"].ToString() != "")
			bService = bool.Parse(dr["is_service"].ToString());
	if(bService)
	{
		sb.Append("<td>&nbsp;</td>");
		sb.Append("<td>&nbsp;</td>");
		sb.Append("<td>" + dr["item_name"].ToString());
	}
	else if(bKit)
	{
		sb.Append("<td>&nbsp;</td>");
		sb.Append("<td>&nbsp;</td>");
		sb.Append("<td>x" + dr["quantity"].ToString() + " " + dr["code"].ToString() + " " + dr["item_name"].ToString());
	}
	else
	{
		sb.Append("<td>" + dr["code"].ToString() + "</td>");
		sb.Append("<td>" + dr["supplier_code"].ToString() + "</td>");
		sb.Append("<td>" + dr["item_name"].ToString());
	}
	sb.Append("</td><td>&nbsp;</td><td align=right>");
//	if((bSystem && bIndividual_Price) || bKit)
	if((bIndividual_Price) || bKit)
		sb.Append("&nbsp;");
	else
		sb.Append(MyDoubleParse(dr["commit_price"].ToString()).ToString("c"));
	sb.Append("</td>");
	if(!bKit)
		sb.Append("<td align=right>" + dr["quantity"].ToString() + "</td>");
	else
		sb.Append("<td>&nbsp;</td>");
	sb.Append("<td align=right>");
//	if((bSystem && bIndividual_Price) || bKit)
	if((bIndividual_Price) || bKit)
		sb.Append("&nbsp;");
	else
		sb.Append(dTotal.ToString("c"));
	sb.Append("</td></tr>\r\n");

//	m_bDoSN = false; //no S/N print on inoivce, required by Robert DW 12.01.2003
	if(m_bDoSN && !bKit)
	{
		DataSet dst = new DataSet();
		
		string inv_No = m_inv_sNumber;//Request.QueryString[0];
		string sc = "SELECT sn FROM sales_serial WHERE invoice_number=" + inv_No;
			sc += " AND code=" + dr["code"].ToString();
		try
		{
			myAdapter = new SqlDataAdapter(sc, myConnection);
			myAdapter.Fill(dst,"serials");
		}
		catch(Exception e) 
		{
			ShowExp(sc, e);
			return "";
		}	
		string sn = "";
		for(int i=0; i < dst.Tables["serials"].Rows.Count; i++)
		{
			sn += dst.Tables["serials"].Rows[i]["sn"].ToString() + "; ";
		}
		/*if(sn.Length > 0)  //do not display SN# 7-4-03
		{
			sb.Append("<tr><td>S/N# :</td><td>");
			sb.Append(sn);
			sb.Append("</td></tr>\r\n");
		}*/  
	}

	return sb.ToString();
}

string BuildOrderMail(string sOrderNumber)
{
	string m_sKitTerm = GetSiteSettings("package_bundle_kit_name", "Kit", true);

	m_inv_sNumber = sOrderNumber;

	DataSet dsi = new DataSet();
	
	string sc = "SELECT o.system AS iSystem, o.no_individual_price AS iNoIndividual, o.*, c.*, i.code ";
	sc += ", i.supplier_code, i.item_name, i.quantity, i.commit_price, cr.is_service, i.system AS bsystem, i.kit, i.krid ";
	sc += ", k.kit_id, k.qty AS kit_qty, k.name AS kit_name, k.commit_price AS kit_price ";
	sc += " FROM order_item i JOIN orders o ON i.id=o.id ";
	sc += " LEFT OUTER JOIN order_kit k ON k.id=o.id AND k.krid=i.krid ";
	sc += " LEFT OUTER JOIN card c ON c.id=o.card_id ";
	sc += " LEFT OUTER JOIN code_relations cr ON cr.code = i.code ";
	sc += " WHERE o.id=" + sOrderNumber;
	sc += " ORDER BY i.kid ";
//DEBUG("sc=", sc);
	int rows = 0;
	try
	{
		myAdapter = new SqlDataAdapter(sc, myConnection);
		rows = myAdapter.Fill(dsi);
	}
	catch(Exception e) 
	{
		ShowExp(sc, e);
		return "";
	}

	if(rows <= 0)
		return "<h3>Order# " + sOrderNumber + " Not Found</h3>";

	DataRow dr = dsi.Tables[0].Rows[0];

	string sDate = dr["record_date"].ToString();
	DateTime tDate = DateTime.Parse(sDate);
	string sales = dr["sales"].ToString();
	string po_number = dr["po_number"].ToString();
	string sType = GetEnumValue("receipt_type", dr["type"].ToString());
	StringBuilder sb = new StringBuilder();

	sb.Append("<html><style type=\"text/css\">\r\n");
	sb.Append("td{FONT-WEIGHT:300;FONT-SIZE:8PT;FONT-FAMILY:verdana;}\r\n");
	sb.Append("body{FONT-WEIGHT:300;FONT-SIZE:8PT;FONT-FAMILY:verdana;}</style>\r\n");
	
	//print header
	sb.Append(InvoicePrintHeader(sType, sales, sOrderNumber, tDate.ToString("dd/MM/yyyy"), po_number));

	//print shippment address
	string shipto = "";
	if(bool.Parse(dr["special_shipto"].ToString()))
	{
		shipto = dr["shipto"].ToString();
		shipto = shipto.Replace("\r\n", "<br>\r\n");
	}
	sb.Append(InvoicePrintShip(dr, ""));
	
	//print invoiced items
	sb.Append(BuildItemTable(dsi.Tables[0], true));

	sb.Append("<br>");

	//conditions table
	sb.Append(InvoicePrintBottom());

	sb.Append("</body></html>");

	return sb.ToString();
}
</script>
