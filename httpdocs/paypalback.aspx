<%@ Page Language="C#" AutoEventWireup="true" CodeFile="paypalback.aspx.cs" Inherits="paypalback" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="themes/js/jquery-1.8.0.min.js"></script>
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
