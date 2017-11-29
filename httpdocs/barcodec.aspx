
<%@Import Namespace="System.Drawing" %>
<%@Import Namespace="System.IO" %>
<%@Import Namespace="System.Drawing.Imaging" %>
<%@Import Namespace="J4L.RBarcode" %>
<%@ OutputCache Duration="100" VaryByParam="none" %>
<script runat=server>



protected void Page_Load(Object Src, EventArgs E ) 
{
Rbarcode1DWeb bc =  new Rbarcode1DWeb();


bc.Code="12345678";
bc.BarType=Rbarcode1DWeb.tbarType.CODE128;


Bitmap inMemoryImage = new Bitmap( 200,200);
Graphics g = Graphics.FromImage(inMemoryImage);

bc.paintBarcode(g);

MemoryStream tempStream = new MemoryStream();


inMemoryImage.Save(tempStream,ImageFormat.Gif);

Response.ClearContent();
Response.Cache.SetCacheability(HttpCacheability.NoCache);
Response.ContentType = "image/gif";
Response.BinaryWrite(tempStream.ToArray());
Response.End();

}
</script>

