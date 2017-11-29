<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Paymentback.aspx.cs" Inherits="Paymentback" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Processing Your Payment Now...</title>
</head>
<body>
	<form id="form2" runat="server">
        <asp:HiddenField ID="PaymentType" runat="server"   />
        <asp:HiddenField ID="Amount" runat="server"  />
        <asp:HiddenField ID="note" runat="server"   />
    </form>
    <h3>Processing your order now. Please wait a moment.</3>
<asp:Literal ID="Literal1" runat="server"></asp:Literal>
</body>
</html>
