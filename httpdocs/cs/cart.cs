<!-- #include file="q_functions.cs" -->
<script runat=server>
bool m_bSales = false; //if true print sales menu
DataTable dtCart = new DataTable();
Boolean bCartAlterColor = false;
string shoppingCartString = "";
string shoppingCartItemsString = "";
string sShippingFee = "5";
string m_system = "0";	//system quotation?

double dTotalPrice = 0;
double dTotalGST = 0;
double dAmount = 0;
double dTotalSaving = 0;
double m_dSessionFreight = 0;

bool m_bWithSystem = false;
bool m_bWithSystemOld = false;
bool m_bOrder = false; //true for use cart to order from wholesale

int m_cols_cart = 6;

bool CheckShoppingCart()
{
	if(Session[m_sCompanyName + "_ordering"] != null)
		m_bOrder = true;

	string ssid = "";
	if(Request.QueryString["ssid"] != null && Request.QueryString["ssid"] != "")
		ssid = Request.QueryString["ssid"];

	if(Session["ShoppingCart" + ssid] == null) 
	{
		dtCart = new DataTable();
		dtCart.Columns.Add(new DataColumn("site", typeof(String)));	//site identifier, m_sCompanyName
//		dtCart.Columns.Add(new DataColumn("id", typeof(String)));	//class identifier, for sales, purchase etc.
		dtCart.Columns.Add(new DataColumn("kid", typeof(String)));	//purchase/sales order item kid
		dtCart.Columns.Add(new DataColumn("code", typeof(String)));	//product code
		dtCart.Columns.Add(new DataColumn("name", typeof(String)));	//product code
		dtCart.Columns.Add(new DataColumn("quantity", typeof(String)));
		dtCart.Columns.Add(new DataColumn("system", typeof(String)));
		dtCart.Columns.Add(new DataColumn("kit", typeof(String)));
		dtCart.Columns.Add(new DataColumn("used", typeof(String)));
		dtCart.Columns.Add(new DataColumn("supplierPrice", typeof(String)));
		dtCart.Columns.Add(new DataColumn("salesPrice", typeof(String)));
		dtCart.Columns.Add(new DataColumn("supplier", typeof(String)));
		dtCart.Columns.Add(new DataColumn("supplier_code", typeof(String)));
		dtCart.Columns.Add(new DataColumn("s_serialNo", typeof(String)));
		Session["ShoppingCart" + ssid] = dtCart;
		return false;
	}
	else
	{
		dtCart = (DataTable)Session["ShoppingCart" + ssid];
	}
	return true;
}

void EmptyCart()
{
	string ssid = "";
	if(Request.QueryString["ssid"] != null && Request.QueryString["ssid"] != "")
		ssid = Request.QueryString["ssid"];

	string site = "";
	if(Session["ShoppingCart" + ssid] == null)
		return;
	dtCart = (DataTable)Session["ShoppingCart" + ssid];
	for(int i=dtCart.Rows.Count-1; i>=0; i--)
	{
		site = dtCart.Rows[i]["site"].ToString();
		if(site == m_sCompanyName)
			dtCart.Rows.RemoveAt(i);
	}
}

bool IsCartEmpty()
{
	for(int i=dtCart.Rows.Count-1; i>=0; i--)
	{
		if(dtCart.Rows[i]["site"].ToString() == m_sCompanyName)
			return false;
	}
	return true;
}

bool CartOnPageLoad()
{
	if(Request.QueryString["reset"] == "1")
		EmptyCart();

	if(Session[m_sCompanyName + "_ordering"] != null)
	{
		string buyType = "sales";
		if(Session[m_sCompanyName + "_salestype"] != null)
			buyType = Session[m_sCompanyName + "_salestype"].ToString();
		if(buyType == "purchase")
			Response.Redirect("purchase.aspx?ssid=" + Request.QueryString["ssid"]);
		else
			Response.Redirect("pos.aspx?ssid=" + Request.QueryString["ssid"]);
		return false;
	}

	TS_PageLoad(); //do common things, LogVisit etc...
//	CheckUserTable();
	string action = Request.QueryString["t"];
//DEBUG("a=", action);
	if(action == "b")
	{
		bool bAdded = false;
		if(Request.QueryString["s"] == "1") //system quotation
		{
			m_system = "1";
			if(CreateSystemOrder())
				bAdded = true;
//			else
//				DEBUG("createsystemorder failed", "");

		}
		else if(Request.QueryString["s"] != null)
		{
			string code = Request.QueryString["c"];
			string supplier = Request.QueryString["s"];
			string supplier_code = Request.QueryString["sc"];
			if(AddToCart(code, supplier, supplier_code, "1", ""))
				bAdded = true;
		}
		else if(Request.QueryString["c"] != null)
		{
			string code = Request.QueryString["c"];
			if(TSIsDigit(code))
			{
				if(Request.QueryString["used"] == "1")
				{
					if(Used_AddToCart(code))
						bAdded = true;
				}
				else
				{
					string qty = "1";
					if(Request.QueryString["qty"] != null && Request.QueryString["qty"] != "")
					{
						qty = Request.QueryString["qty"];
						try
						{
							qty = int.Parse(qty).ToString();
						}
						catch(Exception e)
						{
						}
					}
					if(AddToCart(code, qty, "")){
                        bAdded = true;
                        //DEBUG("add new item to cart code=" , code);
                    }
						
				}
			}
		}else if(Request.QueryString["id"] != null)
		{
			string code = Request.QueryString["id"];
			if(TSIsDigit(code))
			{
				if(Request.QueryString["used"] == "1")
				{
					if(Used_AddToCart(code))
						bAdded = true;
				}
				else
				{
					string qty = "1";
                    int nQty = 1;
					if(Request.QueryString["qty"] != null && Request.QueryString["qty"] != "")
					{
						qty = Request.QueryString["qty"];
						try
						{
							nQty = int.Parse(qty);
						}
						catch(Exception e)
						{
						}
					}
					if(DoAddKit(code, nQty))
                    {
                        //DEBUG("add new item to cart code=" , code);
                        bAdded = true;
                    }
                    //if(AddToCart(code, qty, ""))
						
				}
			}
		}
		if(!bAdded)
		{
			Response.Write("Product Not Found");
			return true;
		}
		Session["OrderCreated"] = "false"; //recreate the order
//		Session["bargain_final_price"] = null; 
		if(m_bOrder)
		{
			if(Session[m_sCompanyName + "_salestype"] != null)
			{
				if(Session[m_sCompanyName + "_salestype"].ToString() == "quote")
					Response.Write("<meta http-equiv=\"refresh\" content=\"0; URL=q.aspx?r=" + DateTime.UtcNow.AddHours(12).ToOADate() + "\">");
				else
					BackToLastPage();
			}
			else
				BackToLastPage();
		}
		else
			Response.Write("<meta http-equiv=\"refresh\" content=\"0; URL=cart.aspx?r=" + DateTime.UtcNow.AddHours(12).ToOADate() + "\">");
		return false;
	}
	else if(action == "delete")
	{
		string row = Request.QueryString["row"];
		if(!DeleteItem(row))
		{
			Response.Write("Error Remove Item");
			return true;
		}
		Session["bargain_final_price"] = null;
		Response.Write("<meta http-equiv=\"refresh\" content=\"0; URL=cart.aspx?r=" + DateTime.UtcNow.AddHours(12).ToOADate() + "\">");
		Session["OrderCreated"] = "false"; //recreate the order
		return false;
	}
	else if(action == "update")
	{
		UpdateQuantity();
		Session["OrderCreated"] = "false"; //recreate the order
		Session["bargain_final_price"] = null;
  }else if(!String.IsNullOrEmpty(Request.Form["CouponCode"])){
		ApplyCouponCode(Request.Form["CouponCode"]);
	}
	return true;
}

void ApplyCouponCode(string couponCode){
	//get coupon 
    string sqlConnString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["b2aSQLConnection"].ToString();
    double couponValue = 0;
    string couponName = null;
    int couponType = 1;
    int couponId = 0;
    string card_id = "0";
    Session["CouponType"] = null;
	if(Session[m_sCompanyName + "_ordering"] != null)
	{
		if(Session[m_sCompanyName + "_dealer_card_id" + m_ssid] != null && Session[m_sCompanyName + "_dealer_card_id" + m_ssid] != "")
			card_id = Session[m_sCompanyName + "_dealer_card_id" + m_ssid].ToString();
	}
	else if(Session["card_id"] != null && Session["card_id"] != "")
	{
		card_id = Session["card_id"].ToString();
	}
    //check same name.
    string sc = " SELECT * FROM Coupons where CouponName = '"+ couponCode.ToUpper() +"'";
    try
    {
    	DataSet dstcom = new DataSet();
        SqlDataAdapter myCommand = new SqlDataAdapter(sc, sqlConnString);
        myCommand.Fill(dstcom, "coupon");
        myCommand.Dispose();
        if(dstcom.Tables["coupon"].Rows.Count > 0){
        	couponValue = double.Parse(dstcom.Tables["coupon"].Rows[0]["CouponValue"].ToString()) / 100;
        	couponName = dstcom.Tables["coupon"].Rows[0]["CouponName"].ToString();
        	couponType = int.Parse(dstcom.Tables["coupon"].Rows[0]["CouponType"].ToString());
        	couponId = int.Parse(dstcom.Tables["coupon"].Rows[0]["CouponId"].ToString());
        }
    }
    catch (Exception ex)
    {
        //DEBUG("LINE275=", ex.Message);
    }
	//check coupon available
	bool isAvailable = true;
    if(couponType == 1){
    	sc = "SELECT CouponId FROM UserCoupons WHERE CardId = "+ card_id +" AND CouponId = " + couponId;
    	try
	    {
	    	DataSet dstcom = new DataSet();
	        SqlDataAdapter myCommand = new SqlDataAdapter(sc, sqlConnString);
	        myCommand.Fill(dstcom, "coupon");
	        myCommand.Dispose();
	        if(dstcom.Tables[0].Rows.Count > 0){
	        	isAvailable = false;
	        }
	        if(couponType == 1){
	        	Session["CouponType"] = couponType;
	        }
	    }
	    catch (Exception ex)
	    {
	        //DEBUG("LINE301=", ex.Message);
	    }
    }


	//apply coupon
	if(isAvailable){
		Session["CouponValue"] = couponValue;
		Session["CouponName"] = couponName;
		Session["CouponId"] = couponId;
	}else{
		Session["CouponValue"] = null;
		Session["CouponName"] = null;
		Session["CouponId"] = null;
		Session["CouponType"] = null;
	}
	
}

bool UpdateQuantity()
{
	string ssid = "";
	if(Request.QueryString["ssid"] != null && Request.QueryString["ssid"] != "")
		ssid = Request.QueryString["ssid"];

	if(Session["ShoppingCart" + ssid] == null)
		return true;

	dtCart = (DataTable)Session["ShoppingCart" + ssid];

	dtCart.AcceptChanges(); //Commits all the changes made to this row since the last time AcceptChanges was called
	int quantity = 0;
	double dPrice = 0;
	double dTotal = 0;
	for(int i=0; i<dtCart.Rows.Count; i++)
	{
		if(dtCart.Rows[i]["site"].ToString() == m_sCompanyName)
		{
			if(dtCart.Rows[i]["system"] == "1")
				continue;

			string sqty = Request.QueryString["Qty"+i.ToString()];
			if(!TSIsDigit(sqty))
				quantity = 0;
			else
			{
				double dqty = double.Parse(sqty);
				quantity = (int)dqty;
			}
			if(quantity <= 0)
			{
				dtCart.Rows.RemoveAt(i);
			}
			else
			{
				dtCart.Rows[i].BeginEdit();
				dtCart.Rows[i]["quantity"] = quantity;
				if(dtCart.Rows[i]["kit"] == "0")
				{
					string card_id = "0";
					if(Session[m_sCompanyName + "_ordering"] != null)
					{
						if(Session[m_sCompanyName + "_dealer_card_id" + m_ssid] != null && Session[m_sCompanyName + "_dealer_card_id" + m_ssid] != "")
							card_id = Session[m_sCompanyName + "_dealer_card_id" + m_ssid].ToString();
					}
					else if(Session["card_id"] != null && Session["card_id"] != "")
					{
						card_id = Session["card_id"].ToString();
					}
					double dQtyPrice = GetSalesPriceForDealer(dtCart.Rows[i]["code"].ToString(), sqty, Session[m_sCompanyName + "dealer_level"].ToString(), card_id);
					dtCart.Rows[i]["SalesPrice"] = dQtyPrice.ToString();
				}
				dtCart.Rows[i].EndEdit();			
			}
		}
	}
	dtCart.AcceptChanges(); //Commits all the changes made to this row since the last time AcceptChanges was called

	return true;
}

Boolean DeleteItem(string row)
{
	string ssid = "";
	if(Request.QueryString["ssid"] != null && Request.QueryString["ssid"] != "")
		ssid = Request.QueryString["ssid"];

	if(Session["ShoppingCart" + ssid] == null)
		return true;

	dtCart = (DataTable)Session["ShoppingCart" + ssid];

	int nRow = int.Parse(row);
	if(nRow >= dtCart.Rows.Count)
		return true;
	string sSystem = dtCart.Rows[nRow]["system"].ToString();
//DEBUG("here", sSystem);
	if(sSystem != "1")
		dtCart.Rows.RemoveAt(nRow);
	else
	{
		for(int i=dtCart.Rows.Count-1; i>=0; i--)
		{
			sSystem = dtCart.Rows[i]["system"].ToString();
			if(sSystem == "1")
				dtCart.Rows.RemoveAt(i);
		}
	}
	if(dtCart.Rows.Count <= 0) //enable credit card
		Session[m_sCompanyName + "no_credit_card"] = null;

	return true;
}

int GetCartItemForThisSite()
{
	int count = 0;
	for(int i=0; i<dtCart.Rows.Count; i++)
	{
//DEBUG("site=", dtCart.Rows[i]["site"].ToString());
		if(dtCart.Rows[i]["site"].ToString() == m_sCompanyName)
			count++;
	}
//DEBUG("count=", count);
	return count;
}

bool AlreadyExists(string code)
{
	for(int i=0; i<dtCart.Rows.Count; i++)
	{
		DataRow dr = dtCart.Rows[i];
		if(dr["system"].ToString() == "1")
			continue;
		if(dr["code"].ToString() == code && dr["site"].ToString() == m_sCompanyName)
		{
			dtCart.AcceptChanges();
			dr.BeginEdit();
			dr["quantity"] = (int.Parse(dr["quantity"].ToString()) + 1).ToString();
			dr.EndEdit();
			dtCart.AcceptChanges();
			return true;
		}
	}
	return false;
}

bool AlreadyExists(string supplier, string supplier_code)
{
	for(int i=0; i<dtCart.Rows.Count; i++)
	{
		DataRow dr = dtCart.Rows[i];
		if(dr["site"].ToString() == m_sCompanyName && dr["supplier"].ToString() == supplier && dr["supplier_code"].ToString() == supplier_code)
		{
			dtCart.AcceptChanges();
			dr.BeginEdit();
			dr["quantity"] = (int.Parse(dr["quantity"].ToString()) + 1).ToString();
			dr.EndEdit();
			dtCart.AcceptChanges();
			return true;
		}
	}
	return false;
}

bool AddToCart(string code, string qty, string sprice)
{

	if(GetStockDetails(code).Contains("delete.gif")){
		Response.Write("<script>");
		Response.Write("alert('Sorry this product out of stock.');");
		Response.Write("</");
		Response.Write("script>");
		return false;
	}

	string ssid = "";
	if(Request.QueryString["ssid"] != null && Request.QueryString["ssid"] != "")
		ssid = Request.QueryString["ssid"];

	if(!IsInteger(code))
		return false;
	CheckShoppingCart();
	if(AlreadyExists(code)) //already exists, update quantity
		return true;

	DataRow dr = dtCart.NewRow();

	DataRow drp = null;
	if(!GetProduct(code, ref drp)){
       // if(!GetKit(code, ref drp)){
            return false;
        //}
    }
		

	bool bCreditReturn = false;
	string orderType = "purchase";
	if(Session[m_sCompanyName + "_salestype"] != null)
		orderType = Session[m_sCompanyName + "_salestype"].ToString();
	if(m_bOrder && orderType == "purchase")
	{
		Session["purchase_currency"] = drp["currency"].ToString();
		Session["purchase_exrate"] = GetSiteSettings("exchange_rate_" + GetEnumValue("currency", drp["currency"].ToString()));
		dr["salesPrice"] = drp["foreign_supplier_price"].ToString();
	}
	else if(orderType == "sales")
	{
		if(Session["sales_type_credit"] != null)
			bCreditReturn = (bool)Session["sales_type_credit"];
		if(sprice != ""){
            double sppp = 0;
            double.TryParse(sprice, out sppp);
			dr["salesPrice"] = sppp.ToString();//(sppp - (sppp * 3 / 23)).ToString();
        }
		else
		{
			string level = "1";
			if(Session[m_sCompanyName + "_dealer_level_for_pos" + ssid] != null)
				level = Session[m_sCompanyName + "_dealer_level_for_pos" + ssid].ToString();
			string card_id = "0";
			if(Session[m_sCompanyName + "_ordering"] != null)
			{
				if(Session[m_sCompanyName + "_dealer_card_id" + m_ssid] != null && Session[m_sCompanyName + "_dealer_card_id" + m_ssid] != "")
					card_id = Session[m_sCompanyName + "_dealer_card_id" + m_ssid].ToString();
			}
			else if(Session["card_id"] != null && Session["card_id"] != "")
			{
				card_id = Session["card_id"].ToString();
			}
			dr["salesPrice"] = GetSalesPriceForDealer(code, qty, level, card_id); //drp["price"].ToString();
		}
	}
	else
	{
		if(sprice != ""){
             
              double sppp = 0;
              double.TryParse(sprice, out sppp);
              dr["salesPrice"] = sppp.ToString();//(sppp - (sppp * 3 / 23)).ToString();
        }
			
		else
		{
			string level = "1";
			if(Session[m_sCompanyName + "_dealer_level_for_pos" + ssid] != null)
				level = Session[m_sCompanyName + "_dealer_level_for_pos" + ssid].ToString();
			string card_id = "0";
			if(Session[m_sCompanyName + "_ordering"] != null)
			{
				if(Session[m_sCompanyName + "_dealer_card_id" + m_ssid] != null && Session[m_sCompanyName + "_dealer_card_id" + m_ssid] != "")
					card_id = Session[m_sCompanyName + "_dealer_card_id" + m_ssid].ToString();
			}
			else if(Session["card_id"] != null && Session["card_id"] != "")
			{
				card_id = Session["card_id"].ToString();
			}

            double sppp = 0;
            double.TryParse(drp["rrp"].ToString(), out sppp);
			dr["salesPrice"] = sppp.ToString();//(sppp - (sppp * 3 / 23)).ToString();
		}
		if(Session["bargain_price_" + code] != null)
			dr["salesPrice"] = Session["bargain_price_" + code].ToString();
	}

//	int nqty = MyIntParse(qty);
//	if(bCreditReturn)
//		nqty = 0 - nqty;

	dr["site"] = m_sCompanyName;
	dr["quantity"] = qty;//nqty.ToString();
	dr["code"] = code;
	dr["name"] = drp["name"].ToString();
	dr["supplier"] = drp["supplier"].ToString();
	dr["supplier_code"] = drp["supplier_code"].ToString();
	dr["supplierPrice"] = drp["supplier_price"].ToString();
	dr["system"] = m_system;
	dr["used"] = "0";
	dtCart.Rows.Add(dr);
	return true;	
}

bool AddToCart(string code, string supplier, string supplier_code, string qty, string supplier_price)
{
	bool bCreditReturn = false;
	if(Session["sales_type_credit"] != null)
		bCreditReturn = (bool)Session["sales_type_credit"];

	int nqty = MyIntParse(qty);
//	if(bCreditReturn)
//		nqty = 0 - nqty;
	return AddToCart(code, supplier, supplier_code, nqty.ToString(), supplier_price, "", "");
}

bool AddToCart(string code, string supplier, string supplier_code, string qty, string supplier_price, string name, string s_serialNo)
{
	return AddToCart("", code, supplier, supplier_code, qty, supplier_price, "", name, s_serialNo);
}

bool AddToCart(string code, string supplier, string supplier_code, string qty, string supplier_price, string salesPrice, string name, string s_serialNo)
{
	return AddToCart("", code, supplier, supplier_code, qty, supplier_price, salesPrice, name, s_serialNo);
}

bool AddToCart(string code, string supplier, string supplier_code, string qty, string supplier_price, string salesPrice, string name, string s_serialNo, bool isKit)
{
		CheckShoppingCart();
//	if(AlreadyExists(supplier, supplier_code)) //already exists, update quantity
//		return true;

	DataRow drp = null;
	if(!GetProductWithSpecialPrice(code, ref drp))
		return false;

	DataRow dr = dtCart.NewRow();

	if(supplier_price != "")
		dr["supplierPrice"] = supplier_price;
	else
	{
		if(drp != null)
		{
			dr["supplierPrice"] = drp["supplier_price"].ToString();
		}
		else
			dr["supplierPrice"] = "0";
	}
	if(drp != null)
	{
		if(salesPrice == "")
			dr["salesPrice"] = drp["price"].ToString();
		else
			dr["salesPrice"] = salesPrice;
		if(name == "")
			dr["name"] = drp["name"].ToString();
	}
	else
		dr["salesPrice"] = salesPrice;

	dr["kid"] = "";
	dr["site"] = m_sCompanyName;
	dr["code"] = code;
	if(name != "")
		dr["name"] = name;
//DEBUG("qty=", qty);
	dr["quantity"] = qty;
	dr["supplier"] = supplier;
	dr["supplier_code"] = supplier_code;
	dr["used"] = "0";
	dr["s_serialNo"] = s_serialNo;
    dr["kit"] = "1";
	dtCart.Rows.Add(dr);
	return true;	
}

bool AddToCart(string kid, string code, string supplier, string supplier_code, string qty, string supplier_price, string salesPrice, string name, string s_serialNo)
{
	CheckShoppingCart();
//	if(AlreadyExists(supplier, supplier_code)) //already exists, update quantity
//		return true;

	DataRow drp = null;
	if(!GetProductWithSpecialPrice(code, ref drp))
		return false;

	DataRow dr = dtCart.NewRow();

	if(supplier_price != "")
		dr["supplierPrice"] = supplier_price;
	else
	{
		if(drp != null)
		{
			dr["supplierPrice"] = drp["supplier_price"].ToString();
		}
		else
			dr["supplierPrice"] = "0";
	}
	if(drp != null)
	{
		if(salesPrice == "")
			dr["salesPrice"] = drp["price"].ToString();
		else
			dr["salesPrice"] = salesPrice;
		if(name == "")
			dr["name"] = drp["name"].ToString();
	}
	else
		dr["salesPrice"] = salesPrice;

	dr["kid"] = kid;
	dr["site"] = m_sCompanyName;
	dr["code"] = code;
	if(name != "")
		dr["name"] = name;
//DEBUG("qty=", qty);
	dr["quantity"] = qty;
	dr["supplier"] = supplier;
	dr["supplier_code"] = supplier_code;
	dr["used"] = "0";
	dr["s_serialNo"] = s_serialNo;
	dtCart.Rows.Add(dr);
	return true;	
}

bool CreateSystemOrder()
{
	if(!CheckQTable())
	{
		Response.Write("Quotation table error.\r\n");
		return false;
	}
	
	for(int i=0; i<m_qfields; i++)
	{
		string code = dtQ.Rows[0][i].ToString(); 
		string qty = dtQ.Rows[0][fn[i]+"_qty"].ToString(); 
		if(IsInteger(code))
		{
			if(int.Parse(code) > 0)
			{
				if(!AddToCart(code, qty, ""))
				{
					Response.Write("code " + code + " error\r\n");
					return false;
				}
			}
		}
	}
	return true;
}
////-------------------------------------------------------------------------------------===============start print shopping cart
////-------------------------------------------------------------------------------------===============start print shopping cart
////-------------------------------------------------------------------------------------===============start print shopping cart
////-------------------------------------------------------------------------------------===============start print shopping cart
////-------------------------------------------------------------------------------------===============start print shopping cart
////-------------------------------------------------------------------------------------===============start print shopping cart
bool _isKit = false;


string PrintCart(bool bButton, bool bInvoice, bool isDealer) //if bButton then print buttons(update, continue, shipping etc..)
{
    shoppingCartString = ReadSitePage("public_order_list");
	CheckShoppingCart();
    shoppingCartString = shoppingCartString.Replace("@@shipping_method_drop_list",PrintFreightOptions(bButton));
//	CheckUserTable();
//	if(TS_UserLoggedIn())
//	{
//		sShippingFee = dtUser.Rows[0]["shipping_fee"].ToString();
//	}
//	else if(Session["ShippingFee"] != null)
//		sShippingFee = Session["ShippingFee"].ToString();

	int i = 0;
//DEBUG("sf=", sShippingFee);
	StringBuilder sb = new StringBuilder();
	bool bDisableCheckOut = true;
//DEBUG("dtacart =", dtCart.Rows.Count);
	if(dtCart.Rows.Count>0)
		bDisableCheckOut = false;
	//header
	if(bButton)
	{
		//sb.Append("\r\n\r\n<table width=100% cellpadding=4 cellspacing=1 border=0>");
		//sb.Append("<tr ><td colspan=" + m_cols_cart + " valign=top>");
	}
	else
	{

//confirm table start
    //    sb.Append("<div class=\"confirOrderPage\">");
    //    sb.Append("              <div class=\"container\">");
    //    sb.Append("              <div class=\"panel\">");
    //    sb.Append("                 <div class=\"t-o b1\">");
    //    sb.Append("                  </div>");
    //    sb.Append("                  <div class=\"t-o b2\">");
    //    sb.Append("                  </div>");
    //    sb.Append("                 <div class=\"t-o b3\">");
    //    sb.Append("                 </div>");
    //    sb.Append("                 <div class=\"t-o b4\">");
    //    sb.Append("                 </div>");
    //    sb.Append("                 <div class=\"content\">");
    //    sb.Append("\r\n\r\n<table width=100% align=center border=0>");
    //    sb.Append("\r\n<tr><td>");

    //    sb.Append("\r\n\r\n<table width=100% align=center cellpadding=2 cellspacing=1 border=0>");
    //    if(!bInvoice)
    //    {
    //        sb.Append("\r\n<tr><td colspan=8><b>");
    //        if(m_bOrder)
    //            sb.Append("Order List");
    //        else
    //            sb.Append("Items in your cart");
    //        sb.Append("</b>\r\n</td></tr>");
    //    }
    //    sb.Append("\r\n<tr><td colspan=" + m_cols_cart + " valign=top>");
    }
    //sb.Append("\r\n\r\n<table border=0 cellpadding=0 width=100% cellspacing=0>");
    //sb.Append("\r\n<tr><td width=100% align=right>");
	if(bButton && m_sSite != "admin")
	{
		//sb.Append("<input type=button class=\"continueShopButton\" OnClick=window.location=('default.aspx') value='Continue Shopping'>");
		//sb.Append("<input type=button  OnClick=window.location=('");
		string checkOutGolink = "";
        string checkOutButtonName = "";
		if(m_bOrder)
        {
			checkOutGolink = "purchase.aspx?r=" + DateTime.UtcNow.AddHours(12).ToOADate();
        } 
        else if(m_bSales)
        {
			checkOutGolink = "pos.aspx?r=" + DateTime.UtcNow.AddHours(12).ToOADate();
        }
        else
        {
			checkOutGolink = "checkout.aspx?r=" + DateTime.UtcNow.AddHours(12).ToOADate();
        }
		if(Request.QueryString["ssid"] != null && Request.QueryString["ssid"] != "")
        {
			checkOutGolink += "&ssid=" + Request.QueryString["ssid"];
        }
		if(m_bOrder)
        {
			checkOutButtonName = "Purchase";
        }
		else
		{
			checkOutButtonName = "Check Out";
			if(bDisableCheckOut)
				checkOutGolink = "#";
			//sb.Append(">");//Continue to checkout</button>");
		}
        shoppingCartString = shoppingCartString.Replace("@@checkOutGoLink",checkOutGolink);
        shoppingCartString = shoppingCartString.Replace("@@checkoutButtonName", checkOutButtonName);

        //when step 1 -- summary 
        shoppingCartString = shoppingCartString.Replace("@@step_summary","step_current");
        shoppingCartString = shoppingCartString.Replace("@@step_login","step_todo");
        shoppingCartString = shoppingCartString.Replace("@@step_confirm","step_todo");
        shoppingCartString = shoppingCartString.Replace("@@step_finish","step_todo");        
	}
    shoppingCartString = shoppingCartString.Replace("@@display_delete_button","none");
    //sb.Append("\r\n</td></tr></table>\r\n</td></tr>");

    //sb.Append("<tr >");
	
//	sb.Append("<td align=center colspan=1 nowrap><b>Quantity</b></td>");

    //sb.Append("<td align=center nowrap><b></b></td>");
	if(m_bOrder)
	{
        ////sb.Append("<td align=center nowrap><b>SUPPLIER</b></td>");
        ////sb.Append("<td align=center nowrap><b>SUPPLIEW CODE</b></td>");
	}
    ////sb.Append("<td align=center nowrap><b></b></td>");
    ////sb.Append("<td width=100% nowrap><b></b></td>");
//	sb.Append("<td align=center nowrap><b>SHIPS</b></td>");
    //if(bButton)
    //        //sb.Append("<td align=center nowrap><b></b></td>");
    //if(m_bOrder)
    //    sb.Append("<td align=center nowrap><b></b></td>");
    //else
    //    sb.Append("<td align=center nowrap><b>Price</b></td>");
    //if(!bButton)
    //    sb.Append("<td align=center nowrap><b>QTY</b></td>");
	if(!m_bOrder)
	{
//		sb.Append("<td align=center nowrap><b>GST</b></td>");
        //sb.Append("<td align=center nowrap><b>TOTAL</b></td>\r\n</tr>");
	}

	dTotalPrice = 0;
	dTotalGST = 0; //used by confirm.cs
	dAmount = 0;
	dTotalSaving = 0;

	double dCost = 0;

	double dRowPrice = 0;
	double dRowGST = 0;
	double dRowTotal = 0;
	double dRowSaving = 0;
//DEBUG("cartrows=", dtCart.Rows.Count);
	//build up row list
    string proCode = "";
    bool isKit = false;
	for(i=0; i<dtCart.Rows.Count; i++)
	{
		DataRow dr = dtCart.Rows[i];
        
		if(dr["site"].ToString() != m_sCompanyName)
			continue;

		bool bKit = MyBooleanParse(dtCart.Rows[i]["kit"].ToString());
        if(bKit){
            isKit = true;
            _isKit = true;
        }else{
            isKit = false;
            _isKit = false;
        }
//DEBUG("698bKit=", bKit);
		DataRow drp = null;
		string code = dtCart.Rows[i]["code"].ToString();
        proCode = code;

		if(bKit)
		{
            string _cartRow = PrintOneKit(bButton, i, ref dRowPrice, ref dRowGST, ref dRowTotal, ref dRowSaving);
			dTotalPrice += dRowTotal;
			dTotalSaving += dRowSaving;
            if(i < dtCart.Rows.Count - 1){
                _cartRow += "@@list_item";
            }
            shoppingCartString = shoppingCartString.Replace("@@number_of_product",dtCart.Rows.Count.ToString());
            shoppingCartString = shoppingCartString.Replace("@@list_item", _cartRow);
			continue;
		}
		else if(dr["used"].ToString() == "1")
		{
			drp = Used_GetProduct(code);
			if(drp == null)
				return "No such used product";
		}
		else
		{
			if(!GetProductWithSpecialPrice(code, ref drp))
				return "Price Error";
		}

		if(drp == null && m_bOrder)
		{
			if(!GetRawProduct(dr["supplier"].ToString(), dr["supplier_code"].ToString(), ref drp))
				return "GetRawProduct Error";
		}
		double dSupplierPrice = 0;
		if(dr["used"].ToString() != "1")
		{
			if(drp != null && drp["supplier_price"] != null)
				dSupplierPrice = double.Parse(drp["supplier_price"].ToString());

			dCost += dSupplierPrice;
		}
		//sb.Append(PrintOneRow(bButton, drp, dr["system"].ToString(), i, ref dRowPrice, ref dRowGST, ref dRowTotal, ref dRowSaving));
        string cartRow = PrintOneRow(bButton, drp, dr["system"].ToString(), i, ref dRowPrice, ref dRowGST, ref dRowTotal, ref dRowSaving);
        if(i < dtCart.Rows.Count - 1){
            cartRow += "@@list_item";
        }
         shoppingCartString = shoppingCartString.Replace("@@number_of_product",dtCart.Rows.Count.ToString());
        shoppingCartString = shoppingCartString.Replace("@@list_item",cartRow);
		dTotalPrice += dRowTotal;
//		dTotalGST += dRowGST;
//		dAmount += dRowTotal;
		dTotalSaving += dRowSaving;
	}

    string l = "p.aspx?"+proCode;
    if(String.IsNullOrEmpty(proCode)){
         shoppingCartString = shoppingCartString.Replace("@@continue_shopping_link", "c.aspx");
    }else{
         shoppingCartString = shoppingCartString.Replace("@@continue_shopping_link", l);
    }
   
 
    
    shoppingCartString = shoppingCartString.Replace("@@list_item","");
    shoppingCartString = shoppingCartString.Replace("@@number_of_product","0");

	//update quantity

    //sb.Append("<tr height=20px><td></td></tr>");
    if(bButton)
	{
            //sb.Append("<tr><td colspan=5 align=right>");
            string updateQTYButton = "";
            updateQTYButton += "<input type=submit ";
	        updateQTYButton += " value='Update Quantity & Price' class=\"button\">";
            shoppingCartString = shoppingCartString.Replace("@@update_qty_button",updateQTYButton);
            //sb.Append("</td></tr>");
            //sb.Append("<tr >"); 
	}else{
        shoppingCartString = shoppingCartString.Replace("@@update_qty_button","");
    }
    //sb.Append("<tr height=50px><td></td></tr>");
    //sb.Append("<td colspan=2 valign=top >");

    //sb.Append("<td colspan=2 align=right>");
	//Freight options
	//sb.Append(PrintFreightOptions(bButton));
    setBButton(bButton);
    //sb.Append("</tr>");
    if(Request.QueryString["f"] != null)
	{
		try
		{
			m_dSessionFreight = double.Parse(Request.QueryString["f"].ToString());
		}
		catch(Exception e)
		{
			m_dSessionFreight = 0;
		}
	}else if(Session["freight"] != null || m_dSessionFreight < 0){
		try
			{
				m_dSessionFreight = double.Parse(Session["freight"].ToString());
			}
			catch(Exception e)
			{
				m_dSessionFreight = 0;
			}
	}


	//Session["ShippingFee"] = m_dSessionFreight;


	double couponValue = 1;
	double discountValue = 0;
    bool showCouponBox = true;
    double beforeCoupon = dTotalPrice;
	if(Session["CouponValue"] != null){
		couponValue = double.Parse(Session["CouponValue"].ToString());
		discountValue = dTotalPrice * couponValue;
		Session["CouponTotal"] = discountValue.ToString("c");
		dTotalPrice = dTotalPrice * (1- couponValue);	
		//showCouponBox = false;	
	}else{
		//showCouponBox = true;
	}

	if(Session["card_id"] == null){
		showCouponBox = false;
	}
	Session["ShippingFee"] = m_dSessionFreight;
	
	//for display on other pages
	dTotalPrice += m_dSessionFreight;
	beforeCoupon += m_dSessionFreight;

	Session["cart_total_no_gst"] = dTotalPrice;

	dCost = dCost * 1.03; //plus bank fee and dps fee
	double dGstRate = MyDoubleParse(GetSiteSettings("gst_rate_percent", "10.0")) / 100;
    if(isDealer){
        dAmount = Math.Round(dTotalPrice * (1 + dGstRate), 3);
        dTotalGST = dTotalPrice * dGstRate - dTotalPrice;
    }else{
        dAmount = dTotalPrice;//Math.Round(dTotalPrice * (1 + dGstRate), 3);
        dTotalGST = dTotalPrice * 3 / 23;
    }
	
    dTotalGST = Math.Round(dTotalGST, 4);    

    //dTotalGST = CheckPriceIf99(dTotalGST);
    //dAmount = CheckPriceIf99(dAmount);
    //if(dTotalGST > 0){
    //    dTotalGST += 0.001;
    //    dAmount += 0.001;
    //}
	//the only place to set Session["Amount"]


	string sAmount = dAmount.ToString();
	if(sAmount.IndexOf('.') < 0)
		sAmount += ".00";	//for dps reports "Invalid Amount Format" withou ".00"

	Session["Amount"] = sAmount;
	Session["Cost"] = dCost.ToString(); //later for bargain
	Session["BeforeCoupon"] = beforeCoupon.ToString("c");
//DEBUG("amount=", dAmount.ToString("C"));

	//sub total
    //sb.Append("<tr >");
    //sb.Append("<td colspan=" + (m_cols_cart - 1).ToString() + " align=right  nowrap>");
    //sb.Append("<b>Sub Total&nbsp;&nbsp;</b></td>");
    //sb.Append("<td align=right  nowrap>");
	sb.Append("<font size=1 face=verdana,helvtica color=\"red\"> Sub Total");
	sb.Append(dTotalPrice.ToString("c"));
    sb.Append("</font>");
    string subTotal = "";
    double theGST = MyDoubleParse(GetSiteSettings("gst_rate_percent", "10.0")) / 100;
    double subTotalIncGST = dTotalPrice;//Math.Round(dTotalPrice * (1 + theGST) , 4);
    subTotalIncGST = CheckPriceIf99(subTotalIncGST  -  m_dSessionFreight);
    //if(subTotalIncGST > 0){
    //    subTotalIncGST += 0.001;
    //}
    subTotal = dTotalPrice.ToString("c");
    shoppingCartString = shoppingCartString.Replace("@@sub_total",(subTotalIncGST + discountValue).ToString("c"));
    //sb.Append("</font></td>\r\n</tr>");

	//Total GST
    //sb.Append("<tr >");
    //sb.Append("<td colspan=" + (m_cols_cart - 1).ToString() + " valign=top align=right  nowrap>");
    //sb.Append("<b>Total Tax&nbsp;&nbsp;</b></td>");
    //sb.Append("<td align=right valign=top nowrap>");
	sb.Append("<font size=1 face=verdana,helvtica color=\"green\">Incldes GST of");
	sb.Append(dTotalGST.ToString("c"));
    sb.Append("</font>");
    shoppingCartString = shoppingCartString.Replace("@@total_tax",dTotalGST.ToString("c"));
	//sb.Append("</font></td>\r\n</tr>");

    //sb.Append("<tr >");
	if(bButton)
	{
        //sb.Append("<td colspan=" + (m_cols_cart-1).ToString() + " valign=top align=right  nowrap>");
        shoppingCartString = shoppingCartString.Replace("@@edit_cart","");
	}
	else
	{
        //sb.Append("<td colspan=2 align=right valign=top  nowrap>");
		if(!bInvoice)
			shoppingCartString = shoppingCartString.Replace("@@edit_cart","<a href=cart.aspx?r=" + DateTime.UtcNow.AddHours(12).ToOADate() + " class='button_large button'>EDIT CART</a>");
        else{
            shoppingCartString = shoppingCartString.Replace("@@edit_cart","");
        }
        //sb.Append("</td><td colspan=3 valign=top align=right  nowrap>");
	}
	//sb.Append("<b>Total Amount Due&nbsp;&nbsp;</b></td>");
    //sb.Append("<td align=right valign=top  nowrap>");
	sb.Append("<font size=1 face=verdana,helvtica color=pink>Total Amount Due");
	sb.Append(dAmount.ToString("c"));
    sb.Append("</font>");
    shoppingCartString = shoppingCartString.Replace("@@total_amount", dAmount.ToString("c"));
    shoppingCartString = shoppingCartString.Replace("@@total_discount@@", discountValue.ToString("c"));
    if(showCouponBox && bButton)
    	shoppingCartString = shoppingCartString.Replace("@@ShowDiscountFeild@@", "");
    else 
    	shoppingCartString = shoppingCartString.Replace("@@ShowDiscountFeild@@", "hide");
    //sb.Append("</font></td>\r\n</tr>");

	if(Session["bargain_final_price"] != null)
	{
		if(TSIsDigit(Session["bargain_final_price"].ToString()))
		{
			double dBargainFinalPrice = (double)Session["bargain_final_price"];
			//sb.Append("\r\n<tr><td colspan=4 valign=top align=right ><b>Final Bargain Price</b></td>");
			////sb.Append("<td align=right valign=top  nowrap>");
			sb.Append("<font size=1 face=verdana,helvtica color=blue>Final Bargain Price");
			sb.Append(dBargainFinalPrice.ToString("c"));
            sb.Append("</font>");
            shoppingCartString = shoppingCartString.Replace("@@begin_final_price",
                                                                                                            "<tr class=\"cart_total_price\">"+
                                                                                                                "<td colspan=\"6\">Final Bargain Price</td>"+
                                                                                                                "<td class=\"price\" id=\"total_price_without_tax\">"+
                                                                                                                "   <span class=\"price\">"+dBargainFinalPrice.ToString("c")+"</span>"+
                                                                                                                "</td>"+
                                                                                                            "</tr>");
			//sb.Append("</font></td>\r\n</tr>");
		}else{
            shoppingCartString = shoppingCartString.Replace("@@begin_final_price","");
        }
	}else{
            shoppingCartString = shoppingCartString.Replace("@@begin_final_price","");
    }

	//sb.Append("<tr ><td>");
	shoppingCartString = shoppingCartString.Replace("@@reset_cart_button", "<input class=\"button\" type=button value='Reset Cart'  onclick="+
	                                                                                             "window.location=('cart.aspx?reset=1')>");
	//sb.Append("</td><td colspan=" + m_cols_cart + " valign=middle align=right  nowrap>");
	if(bButton && m_sSite != "admin")
	{
		//sb.Append("<input type=button  class=\"continueShopButton\" OnClick=window.location=('default.aspx') value='Continue Shopping'>");
		string checkOutGolink = "";
        string checkOutButtonName = "";
        if(g_bEnableQuotation)
		{
			sb.Append("<input type=button  OnClick=window.location=('");
			sb.Append("q.aspx') value='Move To System Quotation'>");
            checkOutGolink = "q.aspx";
            checkOutButtonName = "Move To System Quotation";
		}
		sb.Append("<input type=button  OnClick=window.location=('");
		if(m_bOrder)
        {
			sb.Append("purchase.aspx?r=" + DateTime.UtcNow.AddHours(12).ToOADate());
            checkOutGolink = "purchase.aspx?r=" + DateTime.UtcNow.AddHours(12).ToOADate();
        }
		else if(m_bSales)
        {
			sb.Append("pos.aspx?r=" + DateTime.UtcNow.AddHours(12).ToOADate());
            checkOutGolink = "pos.aspx?r=" + DateTime.UtcNow.AddHours(12).ToOADate();
        }
		else
        {
			sb.Append("checkout.aspx?r=" + DateTime.UtcNow.AddHours(12).ToOADate());
            checkOutGolink = "checkout.aspx?r=" + DateTime.UtcNow.AddHours(12).ToOADate();
        }

		if(m_bOrder)
        {
			sb.Append("') value=Purchase>");
            checkOutButtonName = "Purchase";
        }
		else
		{
			sb.Append("') value='Continue Check Out'  class=\"continueCheckOut\"");
            checkOutButtonName = "Check out";
			if(bDisableCheckOut)
            {
				sb.Append(" disabled ");
                checkOutGolink = "#";
                shoppingCartString = shoppingCartString.Replace("@@display_delete_button","none");
             }else{
             	shoppingCartString = shoppingCartString.Replace("@@display_delete_button","block");
             }
			sb.Append(">");//Continue to checkout</button>");
            shoppingCartString = shoppingCartString.Replace("@@checkOutGoLink",checkOutGolink);
            shoppingCartString = shoppingCartString.Replace("@@checkoutButtonName", checkOutButtonName);
		}
	}
	//sb.Append("\r\n</td></tr>");

	//sb.Append("<tr><td  colspan=" + m_cols_cart + ">");
	sb.Append(GetSiteSettings("shopping_cart_notice", "", true));
    shoppingCartString = shoppingCartString.Replace("@@shopping_cart_notice",GetSiteSettings("shopping_cart_notice", "", true));
	//sb.Append("</td></tr>");
	
	//sb.Append("\r\n</table>");
	if(!bButton){
		//sb.Append("\r\n</td></tr></table>");
        shoppingCartString = shoppingCartString.Replace("@@display_checkout_button","none");
        shoppingCartString = shoppingCartString.Replace("@@display_continue_button","none");

    }
    //DEBUG("Session[edensales]=", Session["edensales"].ToString());
   
    double shippingFeeD = 0;
    double.TryParse(shippingFee, out shippingFeeD);
    shippingFeeD = shippingFeeD ;//* (1 + dGstRate);
    shoppingCartString = shoppingCartString.Replace("@@shipping_fee",shippingFeeD.ToString("c") );
    shoppingCartString = shoppingCartString.Replace("@@payCreditCard@@", "");
    shoppingCartString = shoppingCartString.Replace("@@payCreditCardCharge@@", "");
	return shoppingCartString;
}
////-------------------------------------------------------------------------------------===============end print shopping cart



string PrintCart(bool bButton, bool bInvoice, bool isDealer, bool payByCreditCard) //if bButton then print buttons(update, continue, shipping etc..)
{
    shoppingCartString = ReadSitePage("public_order_list");
	CheckShoppingCart();
    shoppingCartString = shoppingCartString.Replace("@@shipping_method_drop_list",PrintFreightOptions(bButton));
//	CheckUserTable();
//	if(TS_UserLoggedIn())
//	{
//		sShippingFee = dtUser.Rows[0]["shipping_fee"].ToString();
//	}
//	else if(Session["ShippingFee"] != null)
//		sShippingFee = Session["ShippingFee"].ToString();

	int i = 0;
//DEBUG("sf=", sShippingFee);
	StringBuilder sb = new StringBuilder();
	bool bDisableCheckOut = true;
//DEBUG("dtacart =", dtCart.Rows.Count);
	if(dtCart.Rows.Count>0)
		bDisableCheckOut = false;
	//header
	if(bButton)
	{
		//sb.Append("\r\n\r\n<table width=100% cellpadding=4 cellspacing=1 border=0>");
		//sb.Append("<tr ><td colspan=" + m_cols_cart + " valign=top>");
	}
	else
	{

//confirm table start
    //    sb.Append("<div class=\"confirOrderPage\">");
    //    sb.Append("              <div class=\"container\">");
    //    sb.Append("              <div class=\"panel\">");
    //    sb.Append("                 <div class=\"t-o b1\">");
    //    sb.Append("                  </div>");
    //    sb.Append("                  <div class=\"t-o b2\">");
    //    sb.Append("                  </div>");
    //    sb.Append("                 <div class=\"t-o b3\">");
    //    sb.Append("                 </div>");
    //    sb.Append("                 <div class=\"t-o b4\">");
    //    sb.Append("                 </div>");
    //    sb.Append("                 <div class=\"content\">");
    //    sb.Append("\r\n\r\n<table width=100% align=center border=0>");
    //    sb.Append("\r\n<tr><td>");

    //    sb.Append("\r\n\r\n<table width=100% align=center cellpadding=2 cellspacing=1 border=0>");
    //    if(!bInvoice)
    //    {
    //        sb.Append("\r\n<tr><td colspan=8><b>");
    //        if(m_bOrder)
    //            sb.Append("Order List");
    //        else
    //            sb.Append("Items in your cart");
    //        sb.Append("</b>\r\n</td></tr>");
    //    }
    //    sb.Append("\r\n<tr><td colspan=" + m_cols_cart + " valign=top>");
    }
    //sb.Append("\r\n\r\n<table border=0 cellpadding=0 width=100% cellspacing=0>");
    //sb.Append("\r\n<tr><td width=100% align=right>");
	if(bButton && m_sSite != "admin")
	{
		//sb.Append("<input type=button class=\"continueShopButton\" OnClick=window.location=('default.aspx') value='Continue Shopping'>");
		//sb.Append("<input type=button  OnClick=window.location=('");
		string checkOutGolink = "";
        string checkOutButtonName = "";
		if(m_bOrder)
        {
			checkOutGolink = "purchase.aspx?r=" + DateTime.UtcNow.AddHours(12).ToOADate();
        } 
        else if(m_bSales)
        {
			checkOutGolink = "pos.aspx?r=" + DateTime.UtcNow.AddHours(12).ToOADate();
        }
        else
        {
			checkOutGolink = "checkout.aspx?r=" + DateTime.UtcNow.AddHours(12).ToOADate();
        }
		if(Request.QueryString["ssid"] != null && Request.QueryString["ssid"] != "")
        {
			checkOutGolink += "&ssid=" + Request.QueryString["ssid"];
        }
		if(m_bOrder)
        {
			checkOutButtonName = "Purchase";
        }
		else
		{
			checkOutButtonName = "Check Out";
			if(bDisableCheckOut)
				checkOutGolink = "#";
			//sb.Append(">");//Continue to checkout</button>");
		}
        shoppingCartString = shoppingCartString.Replace("@@checkOutGoLink",checkOutGolink);
        shoppingCartString = shoppingCartString.Replace("@@checkoutButtonName", checkOutButtonName);

        //when step 1 -- summary 
        shoppingCartString = shoppingCartString.Replace("@@step_summary","step_current");
        shoppingCartString = shoppingCartString.Replace("@@step_login","step_todo");
        shoppingCartString = shoppingCartString.Replace("@@step_confirm","step_todo");
        shoppingCartString = shoppingCartString.Replace("@@step_finish","step_todo");        
	}
    shoppingCartString = shoppingCartString.Replace("@@display_delete_button","none");
    //sb.Append("\r\n</td></tr></table>\r\n</td></tr>");

    //sb.Append("<tr >");
	
//	sb.Append("<td align=center colspan=1 nowrap><b>Quantity</b></td>");

    //sb.Append("<td align=center nowrap><b></b></td>");
	if(m_bOrder)
	{
        ////sb.Append("<td align=center nowrap><b>SUPPLIER</b></td>");
        ////sb.Append("<td align=center nowrap><b>SUPPLIEW CODE</b></td>");
	}
    ////sb.Append("<td align=center nowrap><b></b></td>");
    ////sb.Append("<td width=100% nowrap><b></b></td>");
//	sb.Append("<td align=center nowrap><b>SHIPS</b></td>");
    //if(bButton)
    //        //sb.Append("<td align=center nowrap><b></b></td>");
    //if(m_bOrder)
    //    sb.Append("<td align=center nowrap><b></b></td>");
    //else
    //    sb.Append("<td align=center nowrap><b>Price</b></td>");
    //if(!bButton)
    //    sb.Append("<td align=center nowrap><b>QTY</b></td>");
	if(!m_bOrder)
	{
//		sb.Append("<td align=center nowrap><b>GST</b></td>");
        //sb.Append("<td align=center nowrap><b>TOTAL</b></td>\r\n</tr>");
	}

	dTotalPrice = 0;
	dTotalGST = 0; //used by confirm.cs
	dAmount = 0;
	dTotalSaving = 0;

	double dCost = 0;

	double dRowPrice = 0;
	double dRowGST = 0;
	double dRowTotal = 0;
	double dRowSaving = 0;
//DEBUG("cartrows=", dtCart.Rows.Count);
	//build up row list
    string proCode = "";
    bool isKit = false;
	for(i=0; i<dtCart.Rows.Count; i++)
	{
		DataRow dr = dtCart.Rows[i];
        
		if(dr["site"].ToString() != m_sCompanyName)
			continue;

		bool bKit = MyBooleanParse(dtCart.Rows[i]["kit"].ToString());
        if(bKit){
            isKit = true;
            _isKit = true;
        }else{
            isKit = false;
            _isKit = false;
        }
//DEBUG("698bKit=", bKit);
		DataRow drp = null;
		string code = dtCart.Rows[i]["code"].ToString();
        proCode = code;

		if(bKit)
		{
            string _cartRow = PrintOneKit(bButton, i, ref dRowPrice, ref dRowGST, ref dRowTotal, ref dRowSaving);
			dTotalPrice += dRowTotal;
			dTotalSaving += dRowSaving;
            if(i < dtCart.Rows.Count - 1){
                _cartRow += "@@list_item";
            }
            shoppingCartString = shoppingCartString.Replace("@@number_of_product",dtCart.Rows.Count.ToString());
            shoppingCartString = shoppingCartString.Replace("@@list_item", _cartRow);
			continue;
		}
		else if(dr["used"].ToString() == "1")
		{
			drp = Used_GetProduct(code);
			if(drp == null)
				return "No such used product";
		}
		else
		{
			if(!GetProductWithSpecialPrice(code, ref drp))
				return "Price Error";
		}

		if(drp == null && m_bOrder)
		{
			if(!GetRawProduct(dr["supplier"].ToString(), dr["supplier_code"].ToString(), ref drp))
				return "GetRawProduct Error";
		}
		double dSupplierPrice = 0;
		if(dr["used"].ToString() != "1")
		{
			if(drp != null && drp["supplier_price"] != null)
				dSupplierPrice = double.Parse(drp["supplier_price"].ToString());

			dCost += dSupplierPrice;
		}
		//sb.Append(PrintOneRow(bButton, drp, dr["system"].ToString(), i, ref dRowPrice, ref dRowGST, ref dRowTotal, ref dRowSaving));
        string cartRow = PrintOneRow(bButton, drp, dr["system"].ToString(), i, ref dRowPrice, ref dRowGST, ref dRowTotal, ref dRowSaving);
        if(i < dtCart.Rows.Count - 1){
            cartRow += "@@list_item";
        }
         shoppingCartString = shoppingCartString.Replace("@@number_of_product",dtCart.Rows.Count.ToString());
        shoppingCartString = shoppingCartString.Replace("@@list_item",cartRow);
		dTotalPrice += dRowTotal;
//		dTotalGST += dRowGST;
//		dAmount += dRowTotal;
		dTotalSaving += dRowSaving;
	}

    string l = "p.aspx?"+proCode;
    if(String.IsNullOrEmpty(proCode)){
         shoppingCartString = shoppingCartString.Replace("@@continue_shopping_link", "c.aspx");
    }else{
         shoppingCartString = shoppingCartString.Replace("@@continue_shopping_link", l);
    }
   
 
    
    shoppingCartString = shoppingCartString.Replace("@@list_item","");
    shoppingCartString = shoppingCartString.Replace("@@number_of_product","0");

	//update quantity

    //sb.Append("<tr height=20px><td></td></tr>");
    if(bButton)
	{
            //sb.Append("<tr><td colspan=5 align=right>");
            string updateQTYButton = "";
            updateQTYButton += "<input type=submit ";
	        updateQTYButton += " value='Update Quantity & Price' class=\"button\">";
            shoppingCartString = shoppingCartString.Replace("@@update_qty_button",updateQTYButton);
            //sb.Append("</td></tr>");
            //sb.Append("<tr >"); 
	}else{
        shoppingCartString = shoppingCartString.Replace("@@update_qty_button","");
    }
    //sb.Append("<tr height=50px><td></td></tr>");
    //sb.Append("<td colspan=2 valign=top >");

    //sb.Append("<td colspan=2 align=right>");
	//Freight options
	//sb.Append(PrintFreightOptions(bButton));
    setBButton(bButton);
    //sb.Append("</tr>");
    if(Request.QueryString["f"] != null)
	{
		try
		{
			m_dSessionFreight = double.Parse(Request.QueryString["f"].ToString());
		}
		catch(Exception e)
		{
			m_dSessionFreight = 0;
		}
	}else if(Session["freight"] != null || m_dSessionFreight < 0){
		try
			{
				m_dSessionFreight = double.Parse(Session["freight"].ToString());
			}
			catch(Exception e)
			{
				m_dSessionFreight = 0;
			}
	}


	//Session["ShippingFee"] = m_dSessionFreight;


	double couponValue = 1;
	double discountValue = 0;
    bool showCouponBox = true;
    double beforeCoupon = dTotalPrice;
	if(Session["CouponValue"] != null){
		couponValue = double.Parse(Session["CouponValue"].ToString());
		discountValue = dTotalPrice * couponValue;
		Session["CouponTotal"] = discountValue.ToString("c");
		dTotalPrice = dTotalPrice * (1- couponValue);	
		//showCouponBox = false;	
	}else{
		//showCouponBox = true;
	}

	if(Session["card_id"] == null){
		showCouponBox = false;
	}
	Session["ShippingFee"] = m_dSessionFreight;
	
	//for display on other pages
	dTotalPrice += m_dSessionFreight;
	beforeCoupon += m_dSessionFreight;

	Session["cart_total_no_gst"] = dTotalPrice;

	dCost = dCost * 1.03; //plus bank fee and dps fee
	double dGstRate = MyDoubleParse(GetSiteSettings("gst_rate_percent", "10.0")) / 100;
    if(isDealer){
        dAmount = Math.Round(dTotalPrice * (1 + dGstRate), 3);
        dTotalGST = dTotalPrice * dGstRate - dTotalPrice;
    }else{
        dAmount = dTotalPrice;//Math.Round(dTotalPrice * (1 + dGstRate), 3);
        dTotalGST = dTotalPrice * 3 / 23;
    }
	
    dTotalGST = Math.Round(dTotalGST, 4);    

    //dTotalGST = CheckPriceIf99(dTotalGST);
    //dAmount = CheckPriceIf99(dAmount);
    //if(dTotalGST > 0){
    //    dTotalGST += 0.001;
    //    dAmount += 0.001;
    //}
	//the only place to set Session["Amount"]


	string sAmount = dAmount.ToString();
	if(sAmount.IndexOf('.') < 0)
		sAmount += ".00";	//for dps reports "Invalid Amount Format" withou ".00"

	Session["Amount"] = sAmount;
	Session["Cost"] = dCost.ToString(); //later for bargain
	Session["BeforeCoupon"] = beforeCoupon.ToString("c");
//DEBUG("amount=", dAmount.ToString("C"));

	//sub total
    //sb.Append("<tr >");
    //sb.Append("<td colspan=" + (m_cols_cart - 1).ToString() + " align=right  nowrap>");
    //sb.Append("<b>Sub Total&nbsp;&nbsp;</b></td>");
    //sb.Append("<td align=right  nowrap>");
	sb.Append("<font size=1 face=verdana,helvtica color=\"red\"> Sub Total");
	sb.Append(dTotalPrice.ToString("c"));
    sb.Append("</font>");
    string subTotal = "";
    double theGST = MyDoubleParse(GetSiteSettings("gst_rate_percent", "10.0")) / 100;
    double subTotalIncGST = dTotalPrice;//Math.Round(dTotalPrice * (1 + theGST) , 4);
    subTotalIncGST = CheckPriceIf99(subTotalIncGST  -  m_dSessionFreight);
    //if(subTotalIncGST > 0){
    //    subTotalIncGST += 0.001;
    //}
    subTotal = dTotalPrice.ToString("c");
    shoppingCartString = shoppingCartString.Replace("@@sub_total",(subTotalIncGST + discountValue).ToString("c"));
    //sb.Append("</font></td>\r\n</tr>");

	//Total GST
    //sb.Append("<tr >");
    //sb.Append("<td colspan=" + (m_cols_cart - 1).ToString() + " valign=top align=right  nowrap>");
    //sb.Append("<b>Total Tax&nbsp;&nbsp;</b></td>");
    //sb.Append("<td align=right valign=top nowrap>");
	sb.Append("<font size=1 face=verdana,helvtica color=\"green\">Incldes GST of");
	sb.Append(dTotalGST.ToString("c"));
    sb.Append("</font>");
    shoppingCartString = shoppingCartString.Replace("@@total_tax",dTotalGST.ToString("c"));
	//sb.Append("</font></td>\r\n</tr>");

    //sb.Append("<tr >");
	if(bButton)
	{
        //sb.Append("<td colspan=" + (m_cols_cart-1).ToString() + " valign=top align=right  nowrap>");
        shoppingCartString = shoppingCartString.Replace("@@edit_cart","");
	}
	else
	{
        //sb.Append("<td colspan=2 align=right valign=top  nowrap>");
		if(!bInvoice)
			shoppingCartString = shoppingCartString.Replace("@@edit_cart","<a href=cart.aspx?r=" + DateTime.UtcNow.AddHours(12).ToOADate() + " class='button_large button'>EDIT CART</a>");
        else{
            shoppingCartString = shoppingCartString.Replace("@@edit_cart","");
        }
        //sb.Append("</td><td colspan=3 valign=top align=right  nowrap>");
	}
	//sb.Append("<b>Total Amount Due&nbsp;&nbsp;</b></td>");
    //sb.Append("<td align=right valign=top  nowrap>");
	sb.Append("<font size=1 face=verdana,helvtica color=pink>Total Amount Due");
	sb.Append(dAmount.ToString("c"));
    sb.Append("</font>");

	
	
    if(payByCreditCard == true){
		
		//miki 20171129, old code.
    	/*shoppingCartString = shoppingCartString.Replace("@@total_amount", (dAmount * 1.025).ToString("c"));
    	string creditCardCharge = "<tr> <td><strong style='float: none;''>Credit Card Surcharge:</strong></td> <td>"+ (dAmount * 0.025).ToString("c") +"</td></tr>";
    	shoppingCartString = shoppingCartString.Replace("@@payCreditCardCharge@@", creditCardCharge);
    	shoppingCartString = shoppingCartString.Replace("@@payCreditCard@@", "Note: a 2.5% credit card surcharge applies"); */
		
		//start: miki 20171129
		shoppingCartString = shoppingCartString.Replace("@@total_amount", (dAmount).ToString("c"));
    	shoppingCartString = shoppingCartString.Replace("@@payCreditCardCharge@@", string.Empty);
		shoppingCartString = shoppingCartString.Replace("@@payCreditCard@@", string.Empty);
		//end: miki 20171129
		
    }else{
    	shoppingCartString = shoppingCartString.Replace("@@total_amount", dAmount.ToString("c"));
    	shoppingCartString = shoppingCartString.Replace("@@payCreditCard@@", "");
    	shoppingCartString = shoppingCartString.Replace("@@payCreditCardCharge@@", "");
    }

    
    shoppingCartString = shoppingCartString.Replace("@@total_discount@@", discountValue.ToString("c"));
    
    if(showCouponBox && bButton)
    	shoppingCartString = shoppingCartString.Replace("@@ShowDiscountFeild@@", "");
    else 
    	shoppingCartString = shoppingCartString.Replace("@@ShowDiscountFeild@@", "hide");
    //sb.Append("</font></td>\r\n</tr>");

	if(Session["bargain_final_price"] != null)
	{
		if(TSIsDigit(Session["bargain_final_price"].ToString()))
		{
			double dBargainFinalPrice = (double)Session["bargain_final_price"];
			//sb.Append("\r\n<tr><td colspan=4 valign=top align=right ><b>Final Bargain Price</b></td>");
			////sb.Append("<td align=right valign=top  nowrap>");
			sb.Append("<font size=1 face=verdana,helvtica color=blue>Final Bargain Price");
			sb.Append(dBargainFinalPrice.ToString("c"));
            sb.Append("</font>");
            shoppingCartString = shoppingCartString.Replace("@@begin_final_price",
                                                                                                            "<tr class=\"cart_total_price\">"+
                                                                                                                "<td colspan=\"6\">Final Bargain Price</td>"+
                                                                                                                "<td class=\"price\" id=\"total_price_without_tax\">"+
                                                                                                                "   <span class=\"price\">"+dBargainFinalPrice.ToString("c")+"</span>"+
                                                                                                                "</td>"+
                                                                                                            "</tr>");
			//sb.Append("</font></td>\r\n</tr>");
		}else{
            shoppingCartString = shoppingCartString.Replace("@@begin_final_price","");
        }
	}else{
            shoppingCartString = shoppingCartString.Replace("@@begin_final_price","");
    }

	//sb.Append("<tr ><td>");
	shoppingCartString = shoppingCartString.Replace("@@reset_cart_button", "<input class=\"button\" type=button value='Reset Cart'  onclick="+
	                                                                                             "window.location=('cart.aspx?reset=1')>");
	//sb.Append("</td><td colspan=" + m_cols_cart + " valign=middle align=right  nowrap>");
	if(bButton && m_sSite != "admin")
	{
		//sb.Append("<input type=button  class=\"continueShopButton\" OnClick=window.location=('default.aspx') value='Continue Shopping'>");
		string checkOutGolink = "";
        string checkOutButtonName = "";
        if(g_bEnableQuotation)
		{
			sb.Append("<input type=button  OnClick=window.location=('");
			sb.Append("q.aspx') value='Move To System Quotation'>");
            checkOutGolink = "q.aspx";
            checkOutButtonName = "Move To System Quotation";
		}
		sb.Append("<input type=button  OnClick=window.location=('");
		if(m_bOrder)
        {
			sb.Append("purchase.aspx?r=" + DateTime.UtcNow.AddHours(12).ToOADate());
            checkOutGolink = "purchase.aspx?r=" + DateTime.UtcNow.AddHours(12).ToOADate();
        }
		else if(m_bSales)
        {
			sb.Append("pos.aspx?r=" + DateTime.UtcNow.AddHours(12).ToOADate());
            checkOutGolink = "pos.aspx?r=" + DateTime.UtcNow.AddHours(12).ToOADate();
        }
		else
        {
			sb.Append("checkout.aspx?r=" + DateTime.UtcNow.AddHours(12).ToOADate());
            checkOutGolink = "checkout.aspx?r=" + DateTime.UtcNow.AddHours(12).ToOADate();
        }

		if(m_bOrder)
        {
			sb.Append("') value=Purchase>");
            checkOutButtonName = "Purchase";
        }
		else
		{
			sb.Append("') value='Continue Check Out'  class=\"continueCheckOut\"");
            checkOutButtonName = "Check out";
			if(bDisableCheckOut)
            {
				sb.Append(" disabled ");
                checkOutGolink = "#";
                shoppingCartString = shoppingCartString.Replace("@@display_delete_button","none");
             }else{
             	shoppingCartString = shoppingCartString.Replace("@@display_delete_button","block");
             }
			sb.Append(">");//Continue to checkout</button>");
            shoppingCartString = shoppingCartString.Replace("@@checkOutGoLink",checkOutGolink);
            shoppingCartString = shoppingCartString.Replace("@@checkoutButtonName", checkOutButtonName);
		}
	}
	//sb.Append("\r\n</td></tr>");

	//sb.Append("<tr><td  colspan=" + m_cols_cart + ">");
	sb.Append(GetSiteSettings("shopping_cart_notice", "", true));
    shoppingCartString = shoppingCartString.Replace("@@shopping_cart_notice",GetSiteSettings("shopping_cart_notice", "", true));
	//sb.Append("</td></tr>");
	
	//sb.Append("\r\n</table>");
	if(!bButton){
		//sb.Append("\r\n</td></tr></table>");
        shoppingCartString = shoppingCartString.Replace("@@display_checkout_button","none");
        shoppingCartString = shoppingCartString.Replace("@@display_continue_button","none");

    }
    //DEBUG("Session[edensales]=", Session["edensales"].ToString());
   
    double shippingFeeD = 0;
    double.TryParse(shippingFee, out shippingFeeD);
    shippingFeeD = shippingFeeD ;//* (1 + dGstRate);
    shoppingCartString = shoppingCartString.Replace("@@shipping_fee",shippingFeeD.ToString("c") );
	return shoppingCartString;
}
////-------------------------------------------------------------------------------------===============end print shopping cart


////-------------------------------------------------------------------------------------===============start print shopping cart line
string PrintOneRow(bool bButton, DataRow drp, string sSystem, int nRow, ref double dRowPrice, 
	ref double dRowGST, ref double dRowTotal, ref double dRowSaving)
{
    string cartLine = ReadSitePage("public_order_list_item");
	DataRow dr = dtCart.Rows[nRow];
	double dPrice = 0; 
	double dRetailPrice = double.Parse(drp["price"].ToString());

	if(dr["used"].ToString() != "1")
	{
		if(m_bOrder)
			dPrice = MyDoubleParse(drp["supplier_price"].ToString());
		else if(m_bSales) // normal retail price for shopsale
			dPrice = dRetailPrice;
		else 
		{
			dPrice = MyDoubleParse(dr["SalesPrice"].ToString());
			dRowSaving = dRetailPrice - dPrice;
		}
	}
	else
	{
		dPrice = MyDoubleParse(dr["SalesPrice"].ToString());
	}

    double dPriceIncGST = dPrice;
    double theGST = MyDoubleParse(GetSiteSettings("gst_rate_percent", "10.0")) / 100;
    dPriceIncGST = dPriceIncGST ;//* (1 + theGST);
    dPriceIncGST = CheckPriceIf99(dPriceIncGST);
	dPrice = Math.Round(dPrice, 3);
    dPrice = CheckPriceIf99(dPrice);

	//write salesPrice
	if(!m_bSales)
	{
		dtCart.AcceptChanges();
		dr.BeginEdit();
		dr["salesPrice"] = dPrice.ToString();
		dr.EndEdit();
		dtCart.AcceptChanges();
	}
	int quantity = MyIntParse(dr["quantity"].ToString());
	double dTotal = dPrice * quantity;
    double dTotalIncGST = dPriceIncGST * quantity;
    dTotalIncGST = Math.Round(dTotalIncGST, 3);
    dTotalIncGST = CheckPriceIf99(dTotalIncGST);
	dTotal = Math.Round(dTotal, 3);
    dTotal = CheckPriceIf99(dTotal);
	dRowSaving *= quantity;
	
	StringBuilder sb = new StringBuilder();

	//sb.Append("\r\n<tr ");
    //if(bCartAlterColor && bButton)
    //    sb.Append("");
    //else
    //    sb.Append("");
    //bCartAlterColor = !bCartAlterColor;

	//sb.Append(">");

	//delete button
    //sb.Append("<td align=center valign=middle nowrap>");

	m_bWithSystem = (sSystem == "1");

	m_bWithSystemOld = m_bWithSystem;

	//code
    //sb.Append("<td align=center valign=middle>");
    //sb.Append(dr["code"].ToString());
    //sb.Append("</td>\r\n");

	if(m_bOrder)
	{
		//sb.Append("<td>" + drp["supplier"].ToString() + "</td>");
        sb.Append("supplier: " + drp["supplier"].ToString() + "");
		//sb.Append("<td>" + drp["supplier_code"].ToString() + "</td>");
        sb.Append("supplier_code" + drp["supplier_code"].ToString() + "");
	}
    //photo
	//sb.Append("</td><td valign=middle><a href=p.aspx?");
    sb.Append("<a href=p.aspx?");
	sb.Append(dr["code"].ToString());
	sb.Append(" class=d target=_blank>");
    sb.Append("<img width=\"100px\" height=\"100px\" alt=\"\" src=\"");
	sb.Append(GetProductImgSrc(dr["code"].ToString()));
    sb.Append("\" />");
	sb.Append("</a>");//</td>\r\n");
    string productLink = "p.aspx?" +  dr["code"].ToString();
    cartLine = cartLine.Replace("@@product_link", productLink);
    cartLine = cartLine.Replace("@@product_image_link", GetProductImgSrc(dr["code"].ToString(), _isKit));

	//description
	//sb.Append("</td><td valign=middle><a href=p.aspx?");
    sb.Append("<a href=p.aspx?");
	sb.Append(dr["code"].ToString());
	sb.Append(" class=d target=_blank>");
	sb.Append(drp["name"].ToString());
	sb.Append("</a>");//</td>\r\n");

    cartLine = cartLine.Replace("@@product_name", drp["name"].ToString());

	//quantity
	if(bButton)
	{
		if(m_bWithSystem)
		{
			//sb.Append("<td>"+dr["quantity"].ToString());
            sb.Append("quantity:"+dr["quantity"].ToString());

            if(!m_bWithSystem || (m_bWithSystem && !m_bWithSystemOld))
		    {
			    sb.Append("<br /><input type=button  OnClick=window.location=('");
			    sb.Append("cart.aspx?t=delete&row=" + nRow.ToString() + "&r=" + DateTime.UtcNow.AddHours(12).ToOADate());
			    sb.Append("') value='");
                cartLine = cartLine.Replace("@@delete_link","cart.aspx?t=delete&row=" + nRow.ToString() + "&r=" + DateTime.UtcNow.AddHours(12).ToOADate());
			    if(m_bWithSystem)
				    sb.Append("DELETE SYSTEM'>");
			    else
				    sb.Append("DELETE'>");
		    }
            //sb.Append( "</td>");
		}
		else
		{
            //sb.Append("<td><input type=text size=2 maxlength=3 width=\"100px\" name=Qty" + nRow.ToString() + " value=");
			sb.Append("<input type=text size=2 maxlength=3 width=\"100px\" name=Qty" + nRow.ToString() + " value=");
			sb.Append(dr["quantity"].ToString() +" />");
            string quantityBox = "<input  class=\"cart_quantity_input text\" type=text size=2 maxlength=2  name=Qty" + nRow.ToString() + " value=" +
                                                dr["quantity"].ToString() +" style='margin: 0;padding: 0;' />";
            cartLine = cartLine.Replace("@@quantity_box",quantityBox);
            cartLine = cartLine.Replace("@@product_detail_instock_status@@", GetStockDetails(dr["code"].ToString()));
            if(!m_bWithSystem || (m_bWithSystem && !m_bWithSystemOld))
		    {
			    sb.Append("<br /><br /><input type=button class=\"deleteButton3\" OnClick=window.location=('");
			    sb.Append("cart.aspx?t=delete&row=" + nRow.ToString() + "&r=" + DateTime.UtcNow.AddHours(12).ToOADate());
			    sb.Append("') value='");
                cartLine = cartLine.Replace("@@delete_link", "cart.aspx?t=delete&row=" + nRow.ToString() + "&r=" + DateTime.UtcNow.AddHours(12).ToOADate());
//DEBUG("cartLine=", cartLine);
			    if(m_bWithSystem)
				    sb.Append("DELETE SYSTEM'>");
			    else
				    sb.Append("DELETE'>");
		    }
            //sb.Append( "</td>");
		}
	}

	//price
	//sb.Append("<td align='right' valign=middle nowrap>");
	sb.Append("Price : "+dPrice.ToString("c"));
    cartLine = cartLine.Replace("@@unit_price", dPriceIncGST.ToString("c"));
	//sb.Append("</td>\r\n");

	//quantity
	if(!bButton)
	{
		//sb.Append("<td align=center>" + dr["quantity"].ToString());
        sb.Append("quantity" + dr["quantity"].ToString());
        cartLine = cartLine.Replace("@@quantity_box",dr["quantity"].ToString());
        cartLine = cartLine.Replace("@@display_delete_button", "none");
		//sb.Append("</td>\r\n");
	}

	if(!m_bOrder)
	{
		//sb.Append("<td align=right valign=middle nowrap>");
		sb.Append("D Total:" + dTotal.ToString("c"));
        cartLine = cartLine.Replace("@@total_price", dTotalIncGST.ToString("c"));
		//sb.Append("</td>\r\n");
	}

	//sb.Append("\r\n</tr>\r\n");

	dRowPrice = dPrice;
	dRowTotal = dTotal;
    //sb.Append("<tr><td colspan=\"6\"><hr /></td></tr>");

	return cartLine;
}
////-------------------------------------------------------------------------------------===============end print shopping cart line
bool GetSupplierPrice(string code, ref double dPrice)
{
	DataSet dso = new DataSet();
	int rows = 0;

	string sc = "SELECT supplier_price FROM product WHERE code=" + code;
	try
	{
		SqlDataAdapter myCommand = new SqlDataAdapter(sc, myConnection);
		rows = myCommand.Fill(dso);
	}
	catch(Exception e) 
	{
		ShowExp(sc, e);
		return false;
	}
	if(rows <= 0)
	{
		sc = "SELECT k.supplier_price FROM product_skip k JOIN code_relations c ON k.id=c.id WHERE c.code=" + code;
		try
		{
			SqlDataAdapter myCommand = new SqlDataAdapter(sc, myConnection);
			if(myCommand.Fill(dso) <= 0)
				return false;
		}
		catch(Exception e) 
		{
			ShowExp(sc, e);
			return false;
		}
	}

	dPrice = double.Parse(dso.Tables[0].Rows[0]["supplier_price"].ToString());
	return true;
}

//used itme functions

bool Used_AddToCart(string code)
{
	if(!IsInteger(code))
		return false;
	CheckShoppingCart();
	if(AlreadyExists(code)) //already exists, update quantity
		return true;

	DataRow dr = dtCart.NewRow();

	DataRow drp = Used_GetProduct(code);
	if(drp == null)
		return false;

	dr["site"] = m_sCompanyName;
	dr["quantity"] = "1";
	dr["code"] = code;
	dr["system"] = "0";
	dr["used"] = "1";
	dr["salesPrice"] = drp["price"].ToString();

//DEBUG("code=", code);
	dtCart.Rows.Add(dr);
	return true;	
}

DataRow Used_GetProduct(string code)
{
	DataSet dsup = new DataSet();
	int rows = 0;

	string sc = "SELECT * FROM used_product WHERE id=" + code;
	try
	{
		SqlDataAdapter myCommand = new SqlDataAdapter(sc, myConnection);
		rows = myCommand.Fill(dsup);
	}
	catch(Exception e) 
	{
		ShowExp(sc, e);
		return null;
	}
	if(rows > 0)
		return dsup.Tables[0].Rows[0];
	return null;
}

string PrintOneKit(bool bButton, int nRow, ref double dRowPrice, 
	ref double dRowGST, ref double dRowTotal, ref double dRowSaving)
{
    string cartLine = ReadSitePage("public_order_list_item");
	DataRow dr = dtCart.Rows[nRow];
	if(!GetKit(dr["code"].ToString()))
		return "Get Kit Error";

    double dPrice = 0; 
	dPrice = m_dKitPrice;
	double dRetailPrice = dPrice;//double.Parse(drp["price"].ToString());
    dRetailPrice = CheckPriceIf99(dRetailPrice);
    double dPriceIncGST = dPrice;
    double theGST = MyDoubleParse(GetSiteSettings("gst_rate_percent", "10.0")) / 100;
    dPriceIncGST = dPriceIncGST ;//* (1 + theGST);
    dPriceIncGST = CheckPriceIf99(dPriceIncGST);

	int quantity = MyIntParse(dr["quantity"].ToString());
	double dTotal = dPrice * quantity;
    double dTotalIncGST = dPriceIncGST * quantity;
    dTotalIncGST = Math.Round(dTotalIncGST, 3);
    dTotalIncGST = CheckPriceIf99(dTotalIncGST);
	dTotal = Math.Round(dTotal, 3);
    dTotal = CheckPriceIf99(dTotal);
	dRowSaving *= quantity;
    string kitName = dr["name"].ToString();

    string productLink = "p.aspx?" +  dr["code"].ToString();
    cartLine = cartLine.Replace("@@product_link", productLink);
    cartLine = cartLine.Replace("@@product_image_link", GetProductImgSrc(dr["code"].ToString(), _isKit));
    cartLine = cartLine.Replace("@@product_name", kitName);
    cartLine = cartLine.Replace("@@delete_link", "cart.aspx?t=delete&row=" + nRow.ToString() + "&r=" + DateTime.UtcNow.AddHours(12).ToOADate());
    cartLine = cartLine.Replace("@@unit_price", dTotalIncGST.ToString("c"));
    string quantityBox = "<input  class=\"cart_quantity_input text\" type=text size=2 maxlength=2  name=Qty" + nRow.ToString() + " value=" +
                                                dr["quantity"].ToString() +" style='margin: 0;  padding: 0;text-align: center;' />";
	cartLine = cartLine.Replace("@@quantity_box",quantityBox);
    cartLine = cartLine.Replace("@@display_delete_button", "");
    cartLine = cartLine.Replace("@@total_price", dTotalIncGST.ToString("c"));


	
	StringBuilder sb = new StringBuilder();

	//sb.Append("\r\n<tr bgcolor=aliceblue>");

	//delete button
	//sb.Append("<td align=center valign=middle nowrap>");

	//delete button
	if(bButton)
	{
		sb.Append("<input type=button  OnClick=window.location=('");
		sb.Append("cart.aspx?t=delete&row=" + nRow.ToString() + "&r=" + DateTime.UtcNow.AddHours(12).ToOADate());
		sb.Append("') value='");
		sb.Append("DELETE " + m_sKitTerm );//+ "'></td>");
	}

	//quantity
	if(bButton)
	{
		//sb.Append("<td><input type=text size=2 maxlength=3 name=Qty" + nRow.ToString() + " value=");
        sb.Append("<input type=text size=2 maxlength=3 name=Qty" + nRow.ToString() + " value=");
		sb.Append("quantity"+dr["quantity"].ToString() );//+ "</td>");
	}

	//code
	//sb.Append("<td align=center valign=middle>");
	sb.Append("Code"+dr["code"].ToString());
	//sb.Append("</td>\r\n");

	if(m_bOrder)
	{
		//sb.Append("<td>&nbsp;</td>");
		//sb.Append("<td>&nbsp;</td>");
	}

	bool bShowDetails = false;
	string sTitle = "Click to show details";
	if(Request.QueryString["sd"] == nRow.ToString())
	{
		bShowDetails = true;
		sTitle = "Click to hide details";
	}

	//description
	//sb.Append("<td valign=middle><a href=cart.aspx");
    sb.Append("<a href=cart.aspx");
	if(!bShowDetails)
		sb.Append("?sd=" + nRow.ToString());
	sb.Append(" class=o title='" + sTitle + "'>");
	sb.Append(m_sKitName);
	sb.Append("</a>");//</td>\r\n");
	
	//price
	//sb.Append("<td align='right' valign=middle nowrap>");
	sb.Append("dPrice"+dPrice.ToString("c"));
	//sb.Append("</td>\r\n");

	//quantity
	if(!bButton)
	{
		//sb.Append("<td align=center>" + dr["quantity"].ToString());
       sb.Append("quantity" + dr["quantity"].ToString());
		//sb.Append("</td>\r\n");
	}

	if(!m_bOrder)
	{
		//sb.Append("<td align=right valign=middle nowrap>");
		sb.Append("dTotal" + dTotal.ToString("c"));
		//sb.Append("</td>\r\n");
	}

	//sb.Append("\r\n</tr>\r\n");

	dRowPrice = dPrice;
	dRowTotal = dTotal;

	if(!bShowDetails){
       //return sb.ToString();
//DEBUG("cartLine=", cartLine);
	   return cartLine;//sb.ToString();
    }
		

	for(int i=0; i<dskit.Tables["kit_item"].Rows.Count; i++)
	{
		DataRow drk = dskit.Tables["kit_item"].Rows[i];
		string code = drk["code"].ToString();
		string name = drk["name"].ToString();
		string qty = drk["qty"].ToString();
		//sb.Append("<tr bgcolor=aliceblue><td>&nbsp;</td>");
		//sb.Append("<td align=right>&nbsp;</td>");
		//sb.Append("<td>&nbsp;</td>");
		//sb.Append("<td colspan=3> x " + qty + " &nbsp&nbsp; " + name + "</td>");
        sb.Append("x " + qty + " &nbsp&nbsp; " + name + "");
		//sb.Append("</tr>");
	}
//DEBUG("cartLine=", cartLine);
	return cartLine;//sb.ToString();
}

bool _bButton = false;
void setBButton(bool bButton){
    _bButton = bButton;
}

string PrintFreightOptions(){
    return PrintFreightOptions(_bButton);
}

string shippingFee = "0";
string PrintFreightOptions(bool bOptions)
{
	string sBlank ="";// "&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td>";
	if(m_sSite != "www")
		return sBlank;

	int nOptions = 0;
	string s = GetSiteSettings("number_of_freight_options", "0");
	try
	{
		nOptions = int.Parse(s);
	}
	catch(Exception e)
	{
	}

	if(nOptions <= 0)
		return sBlank;

	StringBuilder sb = new StringBuilder();
	StringBuilder sbo = new StringBuilder();
	string freightNameMd5 = "";
	//sbo.Append("<option value=0>Please Select</option>");

	if(Request.QueryString["f"] != null && Request.QueryString["fid"] != null)
	{
		try
		{
			m_dSessionFreight = double.Parse(Request.QueryString["f"].ToString());
			freightNameMd5 = Request.QueryString["fid"];
		}
		catch(Exception e)
		{
			m_dSessionFreight = 0;
		}
	}else if(Session["freight"] != null || m_dSessionFreight < 0){
		try
			{
				m_dSessionFreight = double.Parse(Session["freight"].ToString());
				freightNameMd5 = Session["freightId"].ToString();
			}
			catch(Exception e)
			{
				m_dSessionFreight = 0;
			}
	}

	Session["freight"] = m_dSessionFreight.ToString();
	Session["freightId"] = freightNameMd5;
	Session["ShippingFee"] = m_dSessionFreight.ToString();
	bool _tchanged = false;
	double _firstFreight = 0;
	for(int i=1; i<=nOptions && i<16; i++)
	{
		string sname = GetSiteSettings("freight_option_name" + i.ToString(), "option" + i.ToString());
		double dfreight = 0;
		s = GetSiteSettings("freight_option_price" + i.ToString(), "price" + i.ToString());
		try
		{
			dfreight = double.Parse(s);
            dfreight = dfreight * 1.15;
		}
		catch(Exception e)
		{
		}
		sbo.Append("<option value=" + dfreight.ToString());
		if(freightNameMd5 == GetMd5Hash(sname)){
			sbo.Append( " selected");
			_tchanged = false;
			_firstFreight = dfreight;
			m_dSessionFreight = dfreight;
		}
		if(!_tchanged){
			_tchanged = true;
			_firstFreight = dfreight;
			m_dSessionFreight = dfreight;
		}
		//output name
		sbo.Append(" data-value=" + GetMd5Hash(sname) );

		sbo.Append(">" + sname + "</option>");
	}
	
	if(bOptions)
	{
		sb.Append("<div class='select-option span5'><label>Shipping By:</label>");
		sb.Append("<select name=freight onchange='changeFreight(this)'>");
		sb.Append(sbo.ToString());
		sb.Append("</select></div>");
    if(_firstFreight == 0){
			sb.Append(PrintBranchSelector(_firstFreight,freightNameMd5));
		}

		//output js function to support freight selector
		sb.Append("<script>");
		sb.Append("function changeFreight(obj){");
		sb.Append(" var f = $(obj).val();");
		sb.Append(" var fid = $('option:selected', $(obj)).attr('data-value');");
		sb.Append(" window.location='cart.aspx?f=' + f + '&fid=' + fid;");
		sb.Append(" return;");
		sb.Append("}");
		sb.Append("<");
		sb.Append("/script>");
	}
	else
	{
		sb.Append("<b>Freight : </b>");
	}
	//sb.Append("</td>");
	//sb.Append("<td align=right valign=middle  nowrap>");
	if(bOptions){
            //sb.Append(m_dSessionFreight.ToString("c"));
    }
		
	//sb.Append("</td>");
	//sb.Append("<td align=right valign=middle  nowrap>");
	//sb.Append(m_dSessionFreight.ToString("c"));
    shippingFee = m_dSessionFreight.ToString();
    Session["ShippingFee"] = m_dSessionFreight.ToString();
    Session["freight"] = m_dSessionFreight.ToString();
	//sb.Append("</td>");
	return sb.ToString();

}


string PrintBranchSelector(double f, string fid){
	DataSet dssd = new DataSet();
	StringBuilder sb = new StringBuilder();
	StringBuilder sbo = new StringBuilder();
	string branchName = "";
	string sc = " SELECT name ";
	sc += " FROM branch  ";
//DEBUG("sc = ",sc);
	if(Request.QueryString["pbid"] != null){
		branchName = Request.QueryString["pbid"];
	}else if(Session["pbid"] != null && Session["pbid"].ToString() != ""){
		branchName = Session["pbid"].ToString();
	}

	Session["pbid"] = branchName;

	try
	{
		myAdapter = new SqlDataAdapter(sc, myConnection);
		if(myAdapter.Fill(dssd, "branch") > 0)
		{
		//	return "Error getting stock details";

			for(int i=0; i<dssd.Tables["branch"].Rows.Count; i++)
			{
				DataRow dr = dssd.Tables["branch"].Rows[i];
				string name = dr["name"].ToString();
				sbo.Append("<option value=" + name);
				if(branchName == GetMd5Hash(name)){
					sbo.Append( " selected");
					Session["PickUpBranchName"] = name;
				}
				//output name
				sbo.Append(" data-value=" + GetMd5Hash(name));
				sbo.Append(">" + name + "</option>");						
			}

			if(Session["PickUpBranchName"] == null){
				Session["PickUpBranchName"] = dssd.Tables["branch"].Rows[0]["name"].ToString();
			}
											
		}
		

		sb.Append("<div class='select-option span5 alignright'><label>Pickup From:</label>");
		sb.Append("<select name=pickupBranch onchange='changePickupBranch(this)'>");
		sb.Append(sbo.ToString());
		sb.Append("</select></div>");

		sb.Append("<script>");
		sb.Append("function changePickupBranch(obj){");
		sb.Append(" var pbid = $('option:selected', $(obj)).attr('data-value');");
		sb.Append(" window.location='cart.aspx?f="+ f +"&fid="+ fid +"&pbid=' + pbid ;");
		sb.Append(" return;");
		sb.Append("}");
		sb.Append("<");
		sb.Append("/script>");

	}
	catch(Exception e) 
	{
		//ShowExp(sc, e);
	}

	return sb.ToString();
}


string GetStockDetails(string code)
{
	DataSet dssd = new DataSet();
	string sc = " SELECT ISNULL(q.qty,0) AS qty, ISNULL(q.allocated_stock,0) AS allocated_stock  ";
	sc += " FROM stock_qty q  ";
	sc += " WHERE q.code=" + code;
//DEBUG("sc = ",sc);
	try
	{
		myAdapter = new SqlDataAdapter(sc, myConnection);
		if(myAdapter.Fill(dssd, "stock") <= 0)
		{
		//	return "Error getting stock details";
			return "/themes/Images/delete.gif";
		}
	}
	catch(Exception e) 
	{
		//ShowExp(sc, e);
		return "/themes/Images/delete.gif";
	}

	StringBuilder sb = new StringBuilder();
	sb.Append("<table class=\"dealerStockTable\">");
	sb.Append("<tr><th>Stock On Hand</th>"+
                                "<th>Commited</th>"+
                                "<th>Available</th></tr>");

	int nAllocated = 0;
	int nQty = 0;
	for(int i=0; i<dssd.Tables["stock"].Rows.Count; i++)
	{
		DataRow dr = dssd.Tables["stock"].Rows[i];
		//string branch_name = dr["name"].ToString();
		string qty = dr["qty"].ToString();
		string allocated = dr["allocated_stock"].ToString();
		nQty += MyIntParse(qty);
		nAllocated += MyIntParse(allocated);

		
	}

	if(nQty > 0)
		return "/themes/Images/available.png";
	else
		return "/themes/Images/delete.gif";
}
</script>