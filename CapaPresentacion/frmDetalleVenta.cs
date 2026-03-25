using CapaEntidad;
using CapaNegocio;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QRCoder;

namespace CapaPresentacion
{
    public partial class frmDetalleVenta : Form
    {
        public frmDetalleVenta()
        {
            InitializeComponent();
        }

        private void frmDetalleVenta_Load(object sender, EventArgs e)
        {
            txtbusqueda.Select();
        }

        private void btnbuscar_Click(object sender, EventArgs e)
        {
            Venta oVenta = new CN_Venta().ObtenerVenta(txtbusqueda.Text);

            if (oVenta.IdVenta != 0)
            {
                txtnumerodocumento.Text = oVenta.NumeroDocumento;

                txtfecha.Text = oVenta.FechaRegistro;
                txttipodocumento.Text = oVenta.TipoDocumento;
                txtusuario.Text = oVenta.oUsuario.NombreCompleto;

                txtdoccliente.Text = oVenta.DocumentoCliente;
                txtnombrecliente.Text = oVenta.NombreCliente;

                dgvdata.Rows.Clear();
                foreach (Detalle_Venta dv in oVenta.oDetalle_Venta)
                {
                    dgvdata.Rows.Add(new object[] { dv.oProducto.Nombre, dv.PrecioVenta, dv.Cantidad, dv.SubTotal });
                }

                txtmontototal.Text = oVenta.MontoTotal.ToString("0.00");
                txtmontopago.Text = oVenta.MontoPago.ToString("0.00");
                txtmontocambio.Text = oVenta.MontoCambio.ToString("0.00");
            }
            else
            {
                MessageBox.Show("No se encontró la venta", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnborrar_Click(object sender, EventArgs e)
        {
            txtfecha.Text = "";
            txttipodocumento.Text = "";
            txtusuario.Text = "";
            txtdoccliente.Text = "";
            txtnombrecliente.Text = "";

            dgvdata.Rows.Clear();
            txtmontototal.Text = "0.00";
            txtmontopago.Text = "0.00";
            txtmontocambio.Text = "0.00";
            txtnumerodocumento.Text = "";
        }

       
        private string GenerarCufe(
            string numFac,
            DateTime fechaFactura,
            decimal valFac,
            decimal valImp1,
            decimal valImp2,
            decimal valImp3,
            decimal valImpTotal,
            string nitOfe,
            string tipoAdq,
            string numAdq,
            string claveTecnica)
        {
            string fecFac = fechaFactura.ToString("yyyyMMddHHmmss");

            string sValFac = valFac.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            string sValImp1 = valImp1.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            string sValImp2 = valImp2.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            string sValImp3 = valImp3.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            string sValImpTotal = valImpTotal.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);

            string sNitOfe = nitOfe.Replace(".", "").Replace("-", "").Trim();
            string sNumAdq = numAdq.Replace(".", "").Replace("-", "").Trim();

            string codImp1 = "01";
            string codImp2 = "02";
            string codImp3 = "03";

            string cadena = string.Join(";", new string[]
            {
                numFac,
                fecFac,
                sValFac,
                codImp1,
                sValImp1,
                codImp2,
                sValImp2,
                codImp3,
                sValImp3,
                sValImpTotal,
                sNitOfe,
                tipoAdq,
                sNumAdq,
                claveTecnica
            });

            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(cadena);
                byte[] hash = sha1.ComputeHash(bytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("X2")); 
                }
                return sb.ToString();
            }
        }

        private byte[] GenerarQrBytes(string contenido)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(contenido, QRCodeGenerator.ECCLevel.Q);
                using (QRCode qrCode = new QRCode(qrCodeData))
                using (Bitmap qrBitmap = qrCode.GetGraphic(20))
                using (MemoryStream ms = new MemoryStream())
                {
                    qrBitmap.Save(ms, ImageFormat.Png);
                    return ms.ToArray();
                }
            }
        }

        private void btndescargar_Click(object sender, EventArgs e)
        {
            if (txttipodocumento.Text == "")
            {
                MessageBox.Show("No se encontraron resultados", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            string Texto_Html = Properties.Resources.PlantillaVenta.ToString();
            Negocio odatos = new CN_Negocio().ObtenerDatos();

            Texto_Html = Texto_Html.Replace("@nombrenegocio", odatos.Nombre.ToUpper());
            Texto_Html = Texto_Html.Replace("@docnegocio", odatos.RUC);
            Texto_Html = Texto_Html.Replace("@direcnegocio", odatos.Direccion);

            Texto_Html = Texto_Html.Replace("@tipodocumento", txttipodocumento.Text.ToUpper());
            Texto_Html = Texto_Html.Replace("@numerodocumento", txtnumerodocumento.Text);

            Texto_Html = Texto_Html.Replace("@doccliente", txtdoccliente.Text);
            Texto_Html = Texto_Html.Replace("@nombrecliente", txtnombrecliente.Text);
            Texto_Html = Texto_Html.Replace("@fecharegistro", txtfecha.Text);
            Texto_Html = Texto_Html.Replace("@usuarioregistro", txtusuario.Text);

            DateTime fechaFactura;
            if (!DateTime.TryParse(txtfecha.Text, out fechaFactura))
            {
                fechaFactura = DateTime.Now;
            }

            decimal totalFactura = 0m;
            decimal.TryParse(
                txtmontototal.Text.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out totalFactura
            );

            const decimal TASA_IVA = 0.19m;
            decimal valFac = 0m;
            decimal valImp1 = 0m;

            if (totalFactura > 0)
            {
                valFac = Math.Round(totalFactura / (1 + TASA_IVA), 2);
                valImp1 = Math.Round(totalFactura - valFac, 2);
            }

            decimal valImp2 = 0m;
            decimal valImp3 = 0m;
            decimal valImpTotal = totalFactura;

            string nitOfe = odatos.RUC;
            string tipoAdq = "13";              
            string numAdq = txtdoccliente.Text;

            string claveTecnica = "CLAVE_TECNICA_DIAN_AQUI";

            string cufe = GenerarCufe(
                txtnumerodocumento.Text,
                fechaFactura,
                valFac,
                valImp1,
                valImp2,
                valImp3,
                valImpTotal,
                nitOfe,
                tipoAdq,
                numAdq,
                claveTecnica
            );

            string cufeHtmlSafe = System.Net.WebUtility.HtmlEncode(cufe);
            Texto_Html = Texto_Html.Replace("@cufe", cufeHtmlSafe);

            string urlDian = "https://catalogo-vpfe.dian.gov.co/Document/Document?documentKey=" + cufe;

            byte[] qrBytes = GenerarQrBytes(urlDian);

            string filas = string.Empty;
            foreach (DataGridViewRow row in dgvdata.Rows)
            {
                string prod = System.Net.WebUtility.HtmlEncode(row.Cells["Producto"].Value.ToString());
                string prec = row.Cells["Precio"].Value.ToString();
                string cant = row.Cells["Cantidad"].Value.ToString();
                string sub = row.Cells["SubTotal"].Value.ToString();

                filas += "<tr>";
                filas += $"<td>{prod}</td>";
                filas += $"<td>{prec}</td>";
                filas += $"<td>{cant}</td>";
                filas += $"<td>{sub}</td>";
                filas += "</tr>";
            }

            Texto_Html = Texto_Html.Replace("@filas", filas);
            Texto_Html = Texto_Html.Replace("@montototal", txtmontototal.Text);
            Texto_Html = Texto_Html.Replace("@pagocon", txtmontopago.Text);
            Texto_Html = Texto_Html.Replace("@cambio", txtmontocambio.Text);

            SaveFileDialog savefile = new SaveFileDialog();
            savefile.FileName = string.Format("Venta_{0}.pdf", txtnumerodocumento.Text);
            savefile.Filter = "Pdf Files|*.pdf";

            if (savefile.ShowDialog() == DialogResult.OK)
            {
                using (FileStream stream = new FileStream(savefile.FileName, FileMode.Create))
                {
                    Document pdfDoc = new Document(PageSize.A4, 25, 25, 25, 25);
                    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                    pdfDoc.Open();

                    bool obtenido = true;
                    byte[] byteImage = new CN_Negocio().ObtenerLogo(out obtenido);

                    if (obtenido)
                    {
                        iTextSharp.text.Image imgLogo = iTextSharp.text.Image.GetInstance(byteImage);
                        imgLogo.ScaleToFit(60, 60);
                        imgLogo.Alignment = iTextSharp.text.Image.UNDERLYING;
                        imgLogo.SetAbsolutePosition(pdfDoc.Left, pdfDoc.GetTop(51));
                        pdfDoc.Add(imgLogo);
                    }

                    using (StringReader sr = new StringReader(Texto_Html))
                    {
                        XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                    }

                    iTextSharp.text.Image qrImg = iTextSharp.text.Image.GetInstance(qrBytes);
                    qrImg.ScaleToFit(80f, 80f);

                    float x = pdfDoc.Right - 170; 
                    float y = pdfDoc.Top - 125;    
                    qrImg.SetAbsolutePosition(x, y);

                    pdfDoc.Add(qrImg);

                    pdfDoc.Close();
                    stream.Close();
                    MessageBox.Show("Documento Generado", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}
