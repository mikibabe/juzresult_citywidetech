<script runat=server>

double m_dOrderTotal = 0;
bool m_bDoSN = false;
DataSet dsi = new DataSet();
DataSet dstt = new DataSet();

string m_sInvType = "";

bool m_bShowPayment = false;
bool m_bShowSN = false;
double m_invoice_total = 0;
int m_nItems = 0;
int m_nRowsToBottom = 25;

string m_account_number = "";
string m_account_term = "";
string m_account_name = "";

string m_inv_sNumber = "";

string account_paystatus = "";

string m_itemCols = "7"; //item table columns
string sPaymentType = "";


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
	return InvoicePrintHeader(sType, sSales, sNumber, sDate, sPO_number, card_id, card_name, supplier_invoice, "");
}
string InvoicePrintHeader(string sType, string sSales, string sNumber, string sDate, string sPO_number, string card_id, string card_name, string supplier_invoice, string p_invoice_date)
{
	return InvoicePrintHeader(sType, sSales, sNumber, sDate, sPO_number, card_id, card_name, supplier_invoice, p_invoice_date, true);
}
string InvoicePrintHeader(string sType, string sSales, string sNumber, string sDate, string sPO_number, string card_id, string card_name, string supplier_invoice, string p_invoice_date, bool bSalesORPurchase)
{
//DEBUG("sType=", sType);
//**** bSalesORPurchase sales and purchase type: true is sales, false is purchase *****
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
	//if(sType.ToLower() == "order")
	//	title = "ORDER";

	if(sType.ToLower() == "order" && bSalesORPurchase){
            title = "SALES ORDER";
    }
		
	else if(sType.ToLower() == "order" && !bSalesORPurchase)
		title = "PURCHASE ORDER";
	if(Request.QueryString["t"] == "pp" && sType.ToLower() == "order")
		title = "PURCHASE ORDER";
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
		sb.Append("<tr><td align=right><b>");
		sb.Append("INV DATE# : &nbsp;");
		sb.Append("</b></td><td>");
		sb.Append(p_invoice_date);
		sb.Append("</td></tr>");
	}

	if(m_account_number == "")
	{
		m_account_number = card_id;
		//if(Request.QueryString["t"] == "order")
		//	m_account_number = sSales;
		m_account_name = card_name;

	}
	if(title.IndexOf("PURCHASE") >= 0)
		m_account_number = m_account_name;
	header = header.Replace("@@account_number", m_account_number);
	header = header.Replace("@@account_term", m_account_term);
	header = header.Replace("@@invoice_number", sb.ToString());
	header = header.Replace("@@date", sDate);
    header = header.Replace("@@account_paystatus", account_paystatus);
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
	sb.Append("<table width=95% border=0><tr><td width=" + GetSiteSettings("envelope_left_margin", "10") + ">&nbsp;</td>");
	sb.Append("<td valign=top>");

	//bill to
	sb.Append("<div id='invoice'><table border=0><tr><td>");
	sb.Append("<b><font size=3><strong>Bill To:</strong></font></b><br><font size=2>\r\n");
	if(dr == null)
	{
		sb.Append("Cash Sales<br><br>");
		sb.Append("</td></tr></table></div>\r\n");
		sb.Append("</td></tr></table></div>\r\n");
		return sb.ToString();
	}

	string sCompany = "";
	string sAddr = "";
	string sContact = "";

	if(dr["Address1"].ToString() != "")
	{
		sAddr = dr["Address1"].ToString();
		sAddr += "<br>";
		sAddr += dr["Address2"].ToString();
		sAddr += "<br>";
		sAddr += dr["address3"].ToString();
	}

//DEBUG("192=", dr["Address1"].ToString());
	if(dr["postal1"] != null && dr["postal1"].ToString() != "")
	{
		try 
	    {	        
		    sAddr = dr["postal1"].ToString();
		    sAddr += "<br>";
		    sAddr += dr["postal2"].ToString();
		    sAddr += "<br>";
		    sAddr += dr["postal3"].ToString();
	    }
	    catch (global::System.Exception)
	    {
		
	    }
	}
//DEBUG("208=", sAddr  + "--");

     if(String.IsNullOrEmpty(sAddr)){
        try{
            sAddr = dr["addr1B"].ToString();
		    sAddr += "<br>";
	        sAddr += dr["addr2B"].ToString();
		    sAddr += "<br>";
		    sAddr += dr["addr3B"].ToString();
        }catch(Exception ex){
            sAddr = "";
        }
        
    }   

    if(String.IsNullOrEmpty(sAddr)){
        try{
            sAddr = dr["addr1"].ToString();
		    sAddr += "<br>";
	        sAddr += dr["addr2"].ToString();
		    sAddr += "<br>";
		    sAddr += dr["addr3"].ToString();
        }catch(Exception ex){
            sAddr = "";
        }
    }   
//DEBUG("215=", sAddr  + "--");
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
		sb.Append("TEL: " + dr["phone"].ToString());

	sb.Append("</td></tr></table></div></td><td valign=top align=right><table border=0><tr><td>");

	//ship to, if "not pick up  --- 1"
	if(dr["shipping_method"].ToString() != "1")
	{
		sb.Append("<b><font size=3><strong>Ship To:</strong></font></b><br>\r\n");
        if(bool.Parse(dr["special_shipto"].ToString()))
        {
            sb.Append("<font size=2>" + dr["shipto"].ToString().Replace(",", "<br>") + "</font>");
        }
        else
		{
            try 
	        {	        
		            sAddr = dr["address1"].ToString();
                    sAddr += "<br />";
                    sAddr += dr["address2"].ToString();
                    sAddr += "<br />";
                    sAddr += dr["address3"].ToString();
                    sAddr += "<br />";
                    sAddr += dr["CityB"].ToString();
                    sAddr += "<br />";
                    //sAddr += dr["CountryB"].ToString();
	        }
	        catch
	        {
		            
	        }

			sb.Append("<font size=2>" + sCompany + "</font>");
			sb.Append("<br>\r\n");
			sb.Append("<font size=2>" + sAddr + "</font>");
			sb.Append("<br>\r\n");
	//		sb.Append(dr["Email"].ToString());
	//		sb.Append("<br>\r\n");
			if(dr["phone"] != null)
			{
				sb.Append("<font size=2>TEL: " + dr["phone"].ToString() + "</font>");
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

    //bottom = bottom.Replace("@@comment", sb.ToString());
	return "";
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
	sc += ", i.tax, i.total, i.sales, i.cust_ponumber, i.address1, i.address2, i.address3 ";
	sc += ", i.freight, i.sales_note, c.*, s.code, s.supplier_code, s.quantity, s.name as item_name ";
	sc += ", s.commit_price, i.type, i.special_shipto, i.shipto, shipping_method, i.amount_paid AS amount_paid ,";
	sc += " s.status AS si_status, s.note, s.shipby, s.ship_date, s.ticket, s.processed_by, c.address1 AS addr1, c.address2 AS addr2, c.address3 AS addr3,   c.address1B AS addr1B, c.address2B AS addr2B, c.cityB AS addr3B ";
	sc += ", s.system AS bSystem, i.system AS iSystem, cr.is_service, i.no_individual_price AS iNoIndividual ";
	sc += ", s.kit, s.krid, k.kit_id, k.qty AS kit_qty, k.name AS kit_name, k.commit_price AS kit_price, i.paid AS account_paystatus ";
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
	
	sPaymentType = GetEnumValue("payment_method", dr["payment_type"].ToString());
	string sDate = dr["commit_date"].ToString();
	DateTime tDate = DateTime.Parse(sDate);
	string status = dr["si_status"].ToString();
    string paystatus = "";
    try{
        paystatus = dr["account_paystatus"].ToString();
    }catch{
        paystatus = "";
    }
    if(paystatus == "True"){
        account_paystatus = "Paid";
    }else{
        account_paystatus = "Unpaid";
    }
//DEBUG("status", account_paystatus);

	string sType = GetEnumValue("receipt_type", dr["type"].ToString());
	if(status == "Back Order")
		sType = status;

	string sales = dr["sales"].ToString();
	string po_number = dr["cust_ponumber"].ToString();

	StringBuilder sb = new StringBuilder();

	sb.Append("<html><style type=\"text/css\">\r\n");
	sb.Append("td{FONT-WEIGHT:300;FONT-SIZE:8PT;FONT-FAMILY:verdana;}\r\n");
	sb.Append("body{FONT-WEIGHT:300;FONT-SIZE:8PT;FONT-FAMILY:verdana;}</style>\r\n");
	m_bShowSN = MyBooleanParse(GetSiteSettings("show_serial_in_tax_invoice", "1", true));
	//print header
//DEBUG("392sType=", sType);
	sb.Append(InvoicePrintHeader(sType, sales, sInvoiceNumber, tDate.ToString("dd/MM/yyyy"), po_number));

	//print shippment address
	sb.Append("<hr>");
	sb.Append(InvoicePrintShip(dr, ""));
	sb.Append("<hr>");
	
	//print invoiced items
	sb.Append(BuildItemTable(dsi.Tables[0], false));
    string footer = ReadSitePage("invoice_footer");
    footer = footer.Replace("@@invoice_number", m_inv_sNumber);
    footer = footer.Replace("@@comment", dr["sales_note"].ToString() + "<br /><br />");
    sb.Append(footer);
	sb.Append("<br>");

	m_nRowsToBottom = MyIntParse(GetSiteSettings("invoice_blank_rows_added", "25"));

	//miki, 20171129
    /*if(sPaymentType == "credit card"){
    	sb.Append("<br /> <br /> Note: a 2.5% credit card surcharge applies"); 
    }*/

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

	bool bPrintSubTotal = true;
	bool bPrintOrderHeader = true;
	bPrintOrderHeader = MyBooleanParse(GetSiteSettings("print_header_in_order", "1", true));
	if(!bPrintOrderHeader)
		bPrintSubTotal = false;
	if(bOrder && Request.QueryString["email"] != null && Request.QueryString["email"] != "")
		bPrintSubTotal = true;
//DEBUG("brpitn =", bPrintSubTotal);

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
	sb.Append("<td width=130>&nbsp;</td>\r\n");
	sb.Append("<td>DESCRIPTION</td>\r\n");
	sb.Append("<td width=>&nbsp;</td>\r\n");
	if(!bPrintSubTotal)
		bIndividual_Price = true;
//DEBUG("bIndividual_Price = ", bIndividual_Price.ToString());
//	if(bSystem && bIndividual_Price)
	if(bIndividual_Price)
	{
		sb.Append("<td align=right>&nbsp;</td>\r\n");
		sb.Append("<td width=40 align=right>QTY</td>\r\n");
		sb.Append("<td width=70 align=right>&nbsp;</td></tr>\r\n");
	}
	else
	{
		if(!bPrintOrderHeader)
			sb.Append("<td width=70 align=right>&nbsp;</td>\r\n");
		else
			sb.Append("<td width=70 align=right>PRICE</td>\r\n");

		sb.Append("<td width=40 align=right>QTY</td>\r\n");
		if(!bPrintOrderHeader)
			sb.Append("<td align=right>&nbsp;</td>\r\n");
		else
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
			sb.Append("<td align=right>&nbsp;</td>\r\n");
			sb.Append("<td width=40 align=right>QTY</td>\r\n");
			sb.Append("<td width=70 align=right>&nbsp;</td></tr>\r\n");
		}
		else
		{
			if(!bPrintOrderHeader)
				sb.Append("<td width=70 align=right>&nbsp;</td>\r\n");
			else
				sb.Append("<td width=70 align=right>PRICE</td>\r\n");
			sb.Append("<td width=40 align=right>QTY</td>\r\n");
			if(!bPrintOrderHeader)
				sb.Append("<td width=70 align=right>&nbsp;</td>\r\n");
			else
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
    double dPaid = 0;
    double dBalanceDue = 0;

	if(bOrder)
	{

		dTotal = m_dOrderTotal;
		double dGST = MyDoubleParse(GetSiteSettings("gst_rate_percent", "15.0")) / 100; //Get GST rate for Site Sitting //Modified by NEO
        double dd = double.Parse(dt.Rows[0]["quote_total"].ToString(), NumberStyles.Currency, null);
        //DEBUG("dd", dd);
		//if(dt.Rows[0]["quote_total"].ToString() != "" && dt.Rows[0]["quote_total"].ToString() != "0.0000")
        if(dd > 0)
			dTotal = dd;//double.Parse(dt.Rows[0]["quote_total"].ToString(), NumberStyles.Currency, null);
//		dTAX = (dTotal + dFreight) * 0.15; //MyDoubleParse(Session["gst_rate"].ToString());
//		dTAX = (dTotal + dFreight) * dGST;							//Modified by NEO
		dTAX = (dTotal + dFreight) * MyDoubleParse(dt.Rows[0]["gst_rate"].ToString()); 
		dTAX = Math.Round(dTAX, 3);

		dAmount = dTotal + dFreight + dTAX;
		try{
        //DEBUG("615amount_paid =",  dt.Rows[0]["amount_paid"].ToString());
                dPaid = MyDoubleParse(dt.Rows[0]["amount_paid"].ToString()); 

        }catch(Exception){
                dPaid = 0;
        }
        dBalanceDue = dAmount - dPaid;
	}
	else
	{

		dTotal = double.Parse(dt.Rows[0]["price"].ToString());
		dTAX = double.Parse(dt.Rows[0]["tax"].ToString());
    
        //dTAX = dTotal * MyDoubleParse(dt.Rows[0]["gst_rate"].ToString()); 
		//dTAX = Math.Round(dTAX, 3);

		dAmount = double.Parse(dt.Rows[0]["total"].ToString());
        try{
                dPaid = MyDoubleParse(dt.Rows[0]["amount_paid"].ToString()); 
        }catch(Exception){
                dPaid = 0;
        }
        dBalanceDue = dAmount - dPaid;
//DEBUG("dTax=", dt.Rows[0]["tax"].ToString());
	}

	sb.Append("<tr><td colspan=" + m_itemCols + ">&nbsp;</td></tr>\r\n");
	sb.Append("<tr><td colspan=" + m_itemCols + "><hr></td></tr>\r\n");

	if(m_sInvType == "invoice")// && dFreight > 0)
	{
		sb.Append(PrintFreightTicket());
	}


	if(bPrintSubTotal)
	{
		sb.Append("<td colspan=" + (MyIntParse(m_itemCols) ).ToString() + " align=right><table><tr><td align=right><b>Sub-Total : </b></td><td align=right>");
		sb.Append(dTotal.ToString("c"));
		sb.Append("</td></tr>\r\n");

		sb.Append("<tr><td align=right valign=top nowrap>");
		sb.Append("<b>Freight : </b></td><td align=right>" + dFreight.ToString("c") + "</font></td></tr>");
		
		sb.Append("<tr><td align=right><b>Tax : </b></td><td align=right>");
		sb.Append(dTAX.ToString("c"));
		sb.Append("</td></tr>\r\n");

		sb.Append("<tr><td align=right><b>Total Amount : </b></td><td align=right>");
		sb.Append(dAmount.ToString("c"));

        sb.Append("<tr><td align=right><b>Paid : </b></td><td align=right>");
		sb.Append(dPaid.ToString("c"));

		if(sPaymentType == "credit card"){
	    	sb.Append("<tr><td align=right><b>Balance Due : </b></td><td align=right>");
			sb.Append((dBalanceDue * 1.025).ToString("c"));

	    }else{
	    	sb.Append("<tr><td align=right><b>Balance Due : </b></td><td align=right>");
			sb.Append(dBalanceDue.ToString("c"));
	    }
        
	}
	if(m_bShowPayment)
	{
		sb.Append("<tr><td colspan=" + (MyIntParse(m_itemCols) - 1).ToString() + ">");
		sb.Append(ShowPayment());
		sb.Append("</td></tr>");
	}
	//sub-total
    sb.Append("</td></tr></table></td></tr></table>\r\n");
    //sb.Append("<tr><td colspan=" + (MyIntParse(m_itemCols) ).ToString() + "><br /><hr /></td></tr>");
    //sb.Append("<tr><td colspan=" + (MyIntParse(m_itemCols) ).ToString() + " align=\"center\"  ><div style='text-align:center;'>-Remittance Advice-</div></td></tr>");
    //sb.Append("<tr><td colspan=" + (MyIntParse(m_itemCols) - 1).ToString() + " ><table>");
    //sb.Append("<tr><td>Please send remittance to:-</td></tr>");
    //sb.Append("<tr><td><br /></td></tr>");
    //sb.Append("<tr><td><b>HAULAND LIMITED</b></td></tr>");
    //sb.Append("<tr><td><b>PO Box 305 308</b></td></tr>");
    //sb.Append("<tr><td><b>Triton Plaza Auckland 0757</b></td></tr>");
    //sb.Append("<tr><td><br /></td></tr>");
    //sb.Append("<tr><td>To pay by Direct Credit, Bank Details are as follows</td></tr>");
    //sb.Append("<tr><td>Account No. : 12-3050-0494110-50</td></tr>");
    //sb.Append("<tr><td>Account Name: Hauland Limited</td></tr>");
    //sb.Append("<tr><td>Bank: ASB Bank</td></tr>");
    sb.Append("</table></td>");
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
        if(m_sInvType == "invoice"){

        }else{
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

	dPrice = Math.Round(dPrice, 3);
	double dTotal = dPrice * quantity;
	dTotal = Math.Round(dTotal, 3);

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
		//sb.Append("<td>" + dr["supplier_code"].ToString() + "</td>");
		sb.Append("<td>&nbsp;</td>");
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
//DEBUG("m_bDoSN =",  m_bDoSN);
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
		if(m_bShowSN)
		{
			if(sn.Length > 0)  //do not display SN# 7-4-03
			{
				sb.Append("<tr><td>S/N# :</td><td>");
				sb.Append(sn);
				sb.Append("</td></tr>\r\n");
			}  
		}
	}

	return sb.ToString();
}
string BuildOrderMail(string sOrderNumber)
{
	return BuildOrderMail(sOrderNumber, false);
}
string BuildOrderMail(string sOrderNumber, bool bPrintHeader)
{

	string m_sKitTerm = GetSiteSettings("package_bundle_kit_name", "Kit", true);

	m_inv_sNumber = sOrderNumber;

	DataSet dsi = new DataSet();
	
	string sc = "SELECT o.system AS iSystem, o.no_individual_price AS iNoIndividual, c1.name AS sales_person, e1.name AS credit_terms, e2.name AS shipping, o.*, c.*, i.code ";
	sc += ", i.supplier_code, i.item_name, i.quantity, i.commit_price, cr.is_service, i.system AS bsystem, i.kit, i.krid ";
	sc += ", k.kit_id, k.qty AS kit_qty, k.name AS kit_name, k.commit_price AS kit_price, inc.amount_paid AS amount_paid, inc.paid AS account_paystatus , inc.payment_type";
	sc += " FROM order_item i JOIN orders o ON i.id=o.id ";
	sc += " LEFT OUTER JOIN order_kit k ON k.id=o.id AND k.krid=i.krid ";
	sc += " LEFT OUTER JOIN card c ON c.id=o.card_id ";
	sc += " LEFT OUTER JOIN code_relations cr ON cr.code = i.code ";
	sc += " LEFT OUTER JOIN card c1 ON c1.id = o.sales ";
    sc += " LEFT OUTER JOIN enum e1 ON e1.id = c.credit_term AND e1.class = 'credit_terms' ";
    sc += " LEFT OUTER JOIN enum e2 ON e2.id = o.shipping_method AND e2.class = 'shipping_method' ";
    sc += " LEFT OUTER JOIN invoice inc ON inc.invoice_number = o.invoice_number ";

	sc += " WHERE o.id=" + sOrderNumber;
	sc += " ORDER BY i.kid ";
//DEBUG("1029 sc=", sc);
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
    string sTypeId = dr["type"].ToString();
    string orderStatusId = dr["status"].ToString();
    if(orderStatusId == "4"){
        sTypeId = "5";
    }
	string sType = GetEnumValue("receipt_type", sTypeId);

	string sales_person = dr["sales_person"].ToString();
	string terms = dr["credit_terms"].ToString();
    m_account_term = terms;
	string account_id = dr["card_id"].ToString();
	string order_number = dr["number"].ToString();
	string order_date = dr["record_date"].ToString();
	string shipping = dr["shipping"].ToString();
	string freight = dr["freight"].ToString();
	string sSystem = dr["system"].ToString();
	string sales_note = dr["sales_note"].ToString();
	bool bisbuild = MyBooleanParse(dr["isbuild"].ToString());
	bool bSpecialShip = MyBooleanParse(dr["special_shipto"].ToString());
	string sAddr = "";
	string sp_shipto = dr["shipto"].ToString();
    string paystatus = "";
    sPaymentType = GetEnumValue("payment_method", dr["payment_type"].ToString());
    try{
        paystatus = dr["account_paystatus"].ToString();
    }catch{
        paystatus = "";
    }
    if(paystatus == "True"){
        account_paystatus = "Paid";
    }else{
        account_paystatus = "Unpaid";
    }
	sp_shipto = sp_shipto.Replace("\r\n", "\r\n<br>");
	if(!bSpecialShip && sp_shipto != "")
	{
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
			sAddr += dr["city"].ToString();
		}
	}
	else
	{
		sAddr = sp_shipto;
	}
	
	StringBuilder sb = new StringBuilder();
	
	sb.Append("<html><style type=\"text/css\">\r\n");
	sb.Append("td{FONT-WEIGHT:300;FONT-SIZE:8PT;FONT-FAMILY:verdana;}\r\n");
	sb.Append("body{FONT-WEIGHT:300;FONT-SIZE:8PT;FONT-FAMILY:verdana;}</style>\r\n");
	
	bool bPrintOrderHeader = true;

	bPrintOrderHeader = MyBooleanParse(GetSiteSettings("print_header_in_order", "1", true));

	if(Request.QueryString["email"] == null || Request.QueryString["email"] == "")
	{
		if(Request.QueryString["t"] != null && Request.QueryString["t"] != "")
		{
			if(Request.QueryString["t"].ToString().ToLower() == "order")
				bPrintHeader = false;
			else
				bPrintHeader = true;
		}
		else
			bPrintHeader = true;
	}
	else
		bPrintHeader = true;

	string order_header = "<!-- please use this varaibles to display your header ";
	order_header += "@@ORDER_NUMBER, @@SALES_PERSON, @@PO_NUMBER, @@ORDER_DATE, @@TERMS, @@SHIPPING, @@CUST_ID, @@PRINT_DATE, @@ADDRESS ";
	order_header += "@@ISSYSTEM (1=system, 0=not system) ";
	order_header += " --> ";
	order_header += ReadSitePage("order_header");

	string order_footer = "<!-- please use this varaibles to display your header ";
	order_footer += "@@FREIGHT to write your footer detail ";
	order_footer += " --> ";
	order_footer += ReadSitePage("order_footer");
	string order_middle = ReadSitePage("order_middle_for_system");
	//print header
//DEBUG("1148 sales = " , sales_person);
	if(bPrintHeader || bPrintOrderHeader)
		sb.Append(InvoicePrintHeader(sType, sales_person, sOrderNumber, tDate.ToString("dd/MM/yyyy"), po_number, account_id, ""));
	else
	{
		order_header = order_header.Replace("@@ORDER_NUMBER", order_number);
		order_header = order_header.Replace("@@SALES_PERSON", sales_person);
		order_header = order_header.Replace("@@PO_NUMBER", po_number.ToUpper());
		order_header = order_header.Replace("@@ORDER_DATE", order_date);
		order_header = order_header.Replace("@@TERMS", terms);
		order_header = order_header.Replace("@@SHIPPING", shipping);
		order_header = order_header.Replace("@@CUST_ID", account_id);
		order_header = order_header.Replace("@@ADDRESS", sAddr);
		order_header = order_header.Replace("@@FREIGHT", freight);
		order_header = order_header.Replace("@@PRINT_DATE", DateTime.UtcNow.AddHours(12).ToString());
		order_header = order_header.Replace("@@ISSYSTEM", sSystem);
		sb.Append(order_header);
	
		if(sSystem == "1" || bisbuild)
			sb.Append(order_middle);
		else
			sb.Append("<br><br><br><br><br><br><br><br><br><br><br>");
	}
	
	//print shippment address
	string shipto = "";
	if(bool.Parse(dr["special_shipto"].ToString()))
	{
		shipto = dr["shipto"].ToString();
		shipto = shipto.Replace("\r\n", "<br>\r\n");
	}
	if(bPrintHeader || bPrintOrderHeader)
		sb.Append(InvoicePrintShip(dr, ""));
	
	//print invoiced items
	sb.Append(BuildItemTable(dsi.Tables[0], true));

	//miki, 20171129
	/*if(sPaymentType == "credit card"){
    	sb.Append("<br /> <br /> Note: a 2.5% credit card surcharge applies.<br /><br />");
    }*/

	string footer = ReadSitePage("invoice_footer");
    footer = footer.Replace("@@invoice_number", m_inv_sNumber);
    footer = footer.Replace("@@comment", dr["sales_note"].ToString() + "<br /><br />");
    sb.Append(footer);
	sb.Append("<br>");

	
	//conditions table
	if(bPrintHeader || bPrintOrderHeader)
		sb.Append(InvoicePrintBottom());
	else
	{
		order_footer = order_footer.Replace("@@FREIGHT", freight );
		order_footer = order_footer.Replace("@@COMPANY_NAME", m_sCompanyName);
		sb.Append(order_footer);
	}

	

	sb.Append("</body></html>");

	return sb.ToString();
}
</script>
