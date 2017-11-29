using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


    public partial class Payment : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //check input  value
            //order number:
            string orderNumber = Request.Form["OrderNumber"];
            string payAmount = Request.Form["PayAmount"];


            // If the page was posted then check entered value is valid and if so then begin the payment.
            decimal Amount;

            // Try parsing the amount to check it is a valid number.
            if (Decimal.TryParse(payAmount, out Amount) && !String.IsNullOrEmpty(orderNumber))
            {
                // Create new instance of the payment object.
                PaystationPayment payment = new PaystationPayment();

                // Set bare minimum properties.
                payment.amount = (int)(Amount * 100);                   // The amount must be in cents (i.e. a whole number).
               payment.otherData = "&var="+ orderNumber +"&anything=" + Amount.ToString();    // You can send other params in the request if desired (optional).
                payment.merchantReference = "ref01";

                // Call function to initiate the payment to paystation.
                payment.initiate();

                // If the initiation was successful then the digitalOrder will be set.
                if (payment.digitalOrder != null)
                {
                    // *****
                    // Here you would save the details of the transaction in you database along with the
                    // transactionId from paystation's so that in the postback you can update the correct record.
                    // payment.transactionId;
                    // *****
                    
                    // Redirect the user to the "digitalOrder" url so that they go off to the payment screen.
                    HttpContext.Current.Response.Redirect(payment.digitalOrder);
                }
                else
                {
                    // There must have been an error to set text of the lblError on the page.
                    lblError.Text = payment.errorMessage;
                }
            }
            else
            {
                lblError.Text = "The entered value is not a valid decimal number.";
            }
        }
    }
