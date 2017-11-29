using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DataExcLib;

public partial class paypalback : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
    	String txn_status = Request.Form["txn_status"];
    	String response_text = Request.Form["response_text"];
    	String amount = Request.Form["amount"];
        String particular = Request.Form["particular"];
    	PaymentType.Value = "Credit Card Online";
    	Amount.Value = amount;

    	String op = "";
    	if(txn_status != "2"){
    		op = "<h3>"+ response_text +" Please try again</h3>"+
    			 "	    <form name='checkout_confirmation' action='https://my.fetchpayments.co.nz/webpayments/default.aspx' method='post'>  <!-- payment gateway required fields -->"+
				"	        <input type='hidden' name='cmd' value='_xclick'>"+
				"	        <input type='hidden' name='account_id' value='11216'>"+
				"	        <input type='hidden' name='return_url' value='http://"+ HttpContext.Current.Request.Url.Host +"/paypalback.aspx'>"+
				"	        <input type='hidden' name='amount' value='"+ amount +"'>"+
				"	        <input type='hidden' name='item_name' value='Surmantw Online Order'>"+
				"	        <input type='hidden' name='store_card' value='1'>"+
				"	        <input type='hidden' name='csc_required' value='1'>"+
				"	        <input type='hidden' name='reference' value='Surmantiw Online Shop'>"+
				"	        <input type='hidden' name='particular' value='"+ particular +"'>"+
				"	        <input type='submit' Value='Pay now' />"+
				"	    </form>";
    	}else{
    		op = "    <form id='form1' action='result.aspx' method='POST' >"+
				  "      <input type='hidden' id='rPaymentType' name='PaymentType'/>"+
				  "      <input type='hidden' id='rAmount' name='Amount'/>"+
				  "      <input type='hidden' id='rnote' name='note'/>"+
				  //"  <h3>Processing your order now. Please wait a moment.</h3> "+
				 "       <script>"+
				 //"           $(document).ready(function () {"+
				 //"               $('#rPaymentType').val($('#PaymentType').val());"+
				 //"               $('#rAmount').val($('#Amount').val());"+
				 //"               $('#rnote').val($('#note').val());"+
				 //"               //$('#form1').submit();"+
				 "               window.location = 'result.aspx?pay=succ';"+
				 //"           });"+
				 "       </"+
				 "script>"+
				 "   </form>";


				 //set order paid
				String sqlConnString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["b2aSQLConnection"].ToString();
            	DataExc de = new DataExc(sqlConnString);
            	String sc = "UPDATE ORDERs SET sales_note = convert(nvarchar(max),sales_note) + '- Paid by Credit Card Online' WHERE id=" + particular.Replace("Order ID ",""); ;
            	de.ExcSQLCommand(sc);

    	}

    	Literal1.Text = op;
    }
}