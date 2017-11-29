using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using DataExcLib;


    public partial class Paymentback: System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            String errorCode = Request["ec"];
	    	String errorMessage = Request["em"];
	    	String orderNumber = Request["var"];
	        String amount = Request["anything"];
	    	PaymentType.Value = "Credit Card Online";

	    	String op = "";
	    	if (errorCode == "0"){
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
            	String sc = "UPDATE ORDERs SET sales_note = convert(nvarchar(max),sales_note) + '- Paid by Credit Card Online' WHERE id=" + orderNumber; 
            	de.ExcSQLCommand(sc);
	    	}else{
	    		op = "<h3>"+ errorMessage +" <br /> Please try again</h3>"+
    			 "	    <form name='checkout_confirmation' action='Payment.aspx' method='post'>  <!-- payment gateway required fields -->"+
				"          <input type='hidden' name='OrderNumber' value='"+ orderNumber +"'>"+
				"          <input type='hidden' name='PayAmount' value='"+ amount +"'>"+
				"	        <input type='submit' Value='Pay now' />"+
				"	    </form>";
	    	}

	    	Literal1.Text = op;
        }
    }
