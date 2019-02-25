using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using OpenXmlPowerTools;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using Xceed.Words.NET;
using Font = Xceed.Words.NET.Font;
using OpenXmlPowerTools;
using DocumentFormat.OpenXml.Wordprocessing;
using Spire.Doc;
using Spire.Doc.Documents;
using PdfSharp.Pdf.IO;

namespace Word_Pdf
{
    public partial class Form1 : Form
    {


        public static void ConvertToHtml(string file, string outputDirectory)
        {
            var fi = new FileInfo(file);
            Console.WriteLine(fi.Name);
            byte[] byteArray = File.ReadAllBytes(fi.FullName);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                memoryStream.Write(byteArray, 0, byteArray.Length);
                using (WordprocessingDocument wDoc = WordprocessingDocument.Open(memoryStream, true))
                {
                    var destFileName = new FileInfo(fi.Name.Replace(".docx", ".html"));
                    if (outputDirectory != null && outputDirectory != string.Empty)
                    {
                        DirectoryInfo di = new DirectoryInfo(outputDirectory);
                        if (!di.Exists)
                        {
                            throw new OpenXmlPowerToolsException("Output directory does not exist");
                        }
                        destFileName = new FileInfo(Path.Combine(di.FullName, destFileName.Name));
                    }
                    var imageDirectoryName = destFileName.FullName.Substring(0, destFileName.FullName.Length - 5) + "_files";
                    int imageCounter = 0;

                    var pageTitle = fi.FullName;
                    var part = wDoc.CoreFilePropertiesPart;
                    if (part != null)
                    {
                        pageTitle = (string)part.GetXDocument().Descendants(DC.title).FirstOrDefault() ?? fi.FullName;
                    }

                    // TODO: Determine max-width from size of content area.
                    HtmlConverterSettings settings = new HtmlConverterSettings()
                    {
                        AdditionalCss = "body { margin: 1cm auto; max-width: 20cm; padding: 0; }",
                        PageTitle = pageTitle,
                        FabricateCssClasses = true,
                        CssClassPrefix = "pt-",
                        RestrictToSupportedLanguages = false,
                        RestrictToSupportedNumberingFormats = false,
                        ImageHandler = imageInfo =>
                        {
                            DirectoryInfo localDirInfo = new DirectoryInfo(imageDirectoryName);
                            if (!localDirInfo.Exists)
                                localDirInfo.Create();
                            ++imageCounter;
                            string extension = imageInfo.ContentType.Split('/')[1].ToLower();
                            ImageFormat imageFormat = null;
                            if (extension == "png")
                                imageFormat = ImageFormat.Png;
                            else if (extension == "gif")
                                imageFormat = ImageFormat.Gif;
                            else if (extension == "bmp")
                                imageFormat = ImageFormat.Bmp;
                            else if (extension == "jpeg")
                                imageFormat = ImageFormat.Jpeg;
                            else if (extension == "tiff")
                            {
                                // Convert tiff to gif.
                                extension = "gif";
                                imageFormat = ImageFormat.Gif;
                            }
                            else if (extension == "x-wmf")
                            {
                                extension = "wmf";
                                imageFormat = ImageFormat.Wmf;
                            }

                            // If the image format isn't one that we expect, ignore it,
                            // and don't return markup for the link.
                            if (imageFormat == null)
                                return null;

                            string imageFileName = imageDirectoryName + "/image" +
                                imageCounter.ToString() + "." + extension;
                            try
                            {
                                imageInfo.Bitmap.Save(imageFileName, imageFormat);
                            }
                            catch (System.Runtime.InteropServices.ExternalException)
                            {
                                return null;
                            }
                            string imageSource = localDirInfo.Name + "/image" +
                                imageCounter.ToString() + "." + extension;

                            XElement img = new XElement(Xhtml.img,
                                new XAttribute(NoNamespace.src, imageSource),
                                imageInfo.ImgStyleAttribute,
                                imageInfo.AltText != null ?
                                    new XAttribute(NoNamespace.alt, imageInfo.AltText) : null);
                            return img;
                        }
                    };
                    XElement htmlElement = HtmlConverter.ConvertToHtml(wDoc, settings);

                    // Produce HTML document with <!DOCTYPE html > declaration to tell the browser
                    // we are using HTML5.
                    var html = new XDocument(
                        new XDocumentType("html", null, null, null),
                        htmlElement);

                    // Note: the xhtml returned by ConvertToHtmlTransform contains objects of type
                    // XEntity.  PtOpenXmlUtil.cs define the XEntity class.  See
                    // http://blogs.msdn.com/ericwhite/archive/2010/01/21/writing-entity-references-using-linq-to-xml.aspx
                    // for detailed explanation.
                    //
                    // If you further transform the XML tree returned by ConvertToHtmlTransform, you
                    // must do it correctly, or entities will not be serialized properly.

                    var htmlString = html.ToString(SaveOptions.DisableFormatting);
                    File.WriteAllText(destFileName.FullName, htmlString, Encoding.UTF8);
                }
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pl-PL");
            using (DocX document = DocX.Load(@"C:\Users\Ernest\source\repos\Word_Pdf\Word_Pdf\Mój1.docx"))
            {
                // Check if all the replace patterns are used in the loaded document.

               
                document.ReplaceText("@PrzykładowyTekstDoZmiany0", "PierwszaWstawkaBezSPacji_Znakmi!#$%^)&()", false, System.Text.RegularExpressions.RegexOptions.None, new Formatting() { Bold = true  }) ;
                document.ReplaceText("@PrzykładowyTekstDoZmiany1", "Zwykły tekst ", false, System.Text.RegularExpressions.RegexOptions.None, new Formatting() { Bold = true });
                document.ReplaceText("@PrzykładowyTekstDoZmiany2", "Zwykły tekst, dodany przez aplikację (      )   ", false, System.Text.RegularExpressions.RegexOptions.None, new Formatting() { Bold = true });

                var p = document.InsertParagraph();
                p.Append("Przykładowy czerwony tekst dodany na końcu,")
                .Font(new Font("Arial"))
                .FontSize(25)
                .Color(System.Drawing.Color.Red)
                .Bold()
                .Append(" zawierający dodatkowe inne niebieskie formatowanie").Font(new Font("Times New Roman")).Color(System.Drawing.Color.Blue).Italic()
                .SpacingAfter(40);
                // Save this document to disk.
                document.SaveAs(@"C:\Users\Ernest\source\repos\Word_Pdf\Word_Pdf\Mój2.docx");



                using (DocX document1 = DocX.Load(@"C:\Users\Ernest\source\repos\Word_Pdf\Word_Pdf\Mój1.docx"))
                {
                    using (DocX document3 = DocX.Load(@"C:\Users\Ernest\source\repos\Word_Pdf\Word_Pdf\Mój2.docx"))
                    {
                        document1.InsertDocument(document3, true);
                        document1.SaveAs(@"C:\Users\Ernest\source\repos\Word_Pdf\Word_Pdf\Mój3.docx");
                    }
                }


                var n = DateTime.Now;
                var tempDi = new DirectoryInfo(string.Format("ExampleOutput-{0:00}-{1:00}-{2:00}-{3:00}{4:00}{5:00}", n.Year - 2000, n.Month, n.Day, n.Hour, n.Minute, n.Second));
                tempDi.Create();

                /*
                 * This example loads each document into a byte array, then into a memory stream, so that the document can be opened for writing without
                 * modifying the source document.
                 */
                //foreach (var file in Directory.GetFiles("../../", "*.docx"))
                //{
                //    ConvertToHtml(file, tempDi.FullName);

                //}

                byte[] byteArray = File.ReadAllBytes(@"C:\Users\Ernest\source\repos\Word_Pdf\Word_Pdf\Mój2.docx");
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    memoryStream.Write(byteArray, 0, byteArray.Length);
                    using (WordprocessingDocument doc = WordprocessingDocument.Open(memoryStream, true))
                    {
                        HtmlConverterSettings settings = new HtmlConverterSettings()
                        {
                            PageTitle = "My Page Title"
                        };
                        XElement html = HtmlConverter.ConvertToHtml(doc, settings);

                        File.WriteAllText(@"C:\Users\Ernest\source\repos\Word_Pdf\Word_Pdf\Mój2.html", html.ToStringNewLineOnAttributes());
                    }
                }

                string test = File.ReadAllText(@"C:\Users\Ernest\source\repos\Word_Pdf\Word_Pdf\Mój2.hmtl");

                //Byte[] res = null;
                //using (MemoryStream ms = new MemoryStream())
                //{
                //    var pdf = TheArtOfDev.HtmlRenderer.PdfSharp.PdfGenerator.GeneratePdf(test, PdfSharp.PageSize.A4);
                //    pdf.Save(@"C:\Users\Ernest\source\repos\Word_Pdf\Word_Pdf\Mój2.pdf");
                //    res = ms.ToArray();
                //}


                //Load Document
                Spire.Doc.Document document2 = new Spire.Doc.Document();
                document2.LoadFromFile(@"C:\Users\Ernest\source\repos\Word_Pdf\Word_Pdf\Mój2.docx");
                //Convert Word to PDF
                document2.SaveToFile("toPDF.PDF", FileFormat.PDF);
                //Launch Document
                System.Diagnostics.Process.Start("toPDF.PDF");


                string[] pdfFiles = { @"C:\Users\Ernest\source\repos\Word_Pdf\Word_Pdf\bin\Debug\toPDF.PDF", @"C:\Users\Ernest\source\repos\Word_Pdf\Word_Pdf\bin\Debug\toPDF.PDF" , @"C:\Users\Ernest\source\repos\Word_Pdf\Word_Pdf\bin\Debug\toPDF.PDF", @"C:\Users\Ernest\source\repos\Word_Pdf\Word_Pdf\bin\Debug\toPDF.PDF" , @"C:\Users\Ernest\source\repos\Word_Pdf\Word_Pdf\bin\Debug\toPDF.PDF", @"C:\Users\Ernest\source\repos\Word_Pdf\Word_Pdf\bin\Debug\toPDF.PDF" };
                PdfDocument outputPDFDocument = new PdfDocument();
                foreach (string pdfFile in pdfFiles)
                {
                    PdfDocument inputPDFDocument = PdfReader.Open(pdfFile, PdfDocumentOpenMode.Import);
                    outputPDFDocument.Version = inputPDFDocument.Version;
                    foreach (PdfPage page in inputPDFDocument.Pages)
                    {
                        outputPDFDocument.AddPage(page);
                    }
                }
                outputPDFDocument.Save(@"C:\Users\Ernest\source\repos\Word_Pdf\Word_Pdf\bin\Debug\toPDF2.PDF");
            }
        }
    }
}
