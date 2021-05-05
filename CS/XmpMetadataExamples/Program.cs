using DevExpress.Pdf;
using DevExpress.Pdf.Xmp;
using System.IO;

namespace XmpMetadataExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            EmbedLoadedMetadata();
            EmbedEditedMetadata();
            EmbedGeneratedMetadata();
        }

        static void EmbedEditedMetadata()
        {
            using (PdfDocumentProcessor pdfDocumentProcessor = new PdfDocumentProcessor())
            {

                //Load a document:
                pdfDocumentProcessor.LoadDocument("Documents//Invoice.pdf");
                PdfDocument document = pdfDocumentProcessor.Document;

                // Retrieve metadata:
                XmpDocument editedMetadata = XmpDocument.FromString(document.Metadata.Data);

                // Add items to the Creator array:
                XmpArray creators = editedMetadata.GetArray("dc:creator");
                if (creators != null)
                {
                    creators.Add("PDF Document API");
                    creators.Add("Office File API");
                }

                //Change the CreatorTool node value:
                XmpSimpleNode creatorTool = editedMetadata.GetSimpleValue("xmp:CreatorTool");
                creatorTool.SetValue("PDF Document API");

                //Add MaxPageSize structure:
                XmpName structureName = XmpName.Get("MaxPageSize", "http://ns.adobe.com/xap/1.0/t/pg/");
                XmpStructure dimensions = editedMetadata.CreateStructure(structureName);
                editedMetadata.RegisterNamespace("http://ns.adobe.com/xap/1.0/sType/Dimensions#", "stDim");
                dimensions.Add("stDim:h", 11);
                dimensions.Add("stDim:w", 8.5f);
                dimensions.Add("stDim:Unit", "inch");

                editedMetadata.Remove("dc:subject");

                //Embed modified metadata to the document:
                document.SetMetadata(editedMetadata);

                //Save the result:
                pdfDocumentProcessor.SaveDocument("Invoice_Upd.pdf");
            }
        }

        static void EmbedGeneratedMetadata()
        {
            using (PdfDocumentProcessor pdfDocumentProcessor = new PdfDocumentProcessor())
            {
                //Load a document:
                pdfDocumentProcessor.LoadDocument("Documents//Invoice_blank.pdf");
                PdfDocument document = pdfDocumentProcessor.Document;

                XmpDocument generatedMetadata = GenerateXmpMetadata();

                //Embed modified metadata to the document:
                document.SetMetadata(generatedMetadata);

                //Save the result:
                pdfDocumentProcessor.SaveDocument("Invoice_new.pdf");
            }
        }

        private static XmpDocument GenerateXmpMetadata()
        {
            XmpDocument document = new XmpDocument();

            // Register namespaces:
            document.RegisterNamespace("http://ns.adobe.com/xap/1.0/", "xmp");
            document.RegisterNamespace("http://ns.adobe.com/xap/1.0/sType/Dimensions#", "stDim");
            document.RegisterNamespace("http://ns.adobe.com/pdf/1.3/", "pdf");
            document.RegisterNamespace("http://ns.adobe.com/xap/1.0/t/pg/", "xmpTPg");

            //Add items with "xmp" prefix:
            XmpArray array = document.CreateArray("xmp:Identifier", XmpArrayType.Unordered);
            array.Add("identifier1");
            array.Add("identifier2");
            array.Add("identifier3");
            document.CreateSimpleValue("xmp:Label", "Demo");

            //Add items with the "pdf" prefix:
            document.Add("pdf:Keywords", "Invoice,Northwind,PDF,XMP");
            document.Add("pdf:Producer", "Developer Express Inc.DXperience(tm)");
            document.CreateSimpleValue("pdf:PDFVersion", "1.3");

            // Add items with the "xmpTPg" prefix:
            XmpStructure dimensions = document.CreateStructure("xmpTPg:MaxPageSize");
            dimensions.Add("stDim:h", 750);
            dimensions.Add("stDim:w", 500);
            dimensions.Add("stDim:Unit", "pixel");
            document.Add("xmpTPg:NPages", 6);

            return document;
        }

        static void EmbedLoadedMetadata()
        {
            ExportSchemas();
            using (PdfDocumentProcessor pdfDocumentProcessor = new PdfDocumentProcessor())
            {
                // Set metadata loaded from a stream:
                pdfDocumentProcessor.LoadDocument("Documents//Invoice_signed.pdf");
                XmpDocument metadata;
                using (FileStream xmlStream = new FileStream("Documents//metadata_new.xml", FileMode.Open, FileAccess.Read))
                {
                    metadata = XmpDocument.FromStream(xmlStream);
                    pdfDocumentProcessor.Document.SetMetadata(metadata);
                }

                pdfDocumentProcessor.SaveDocument("Invoice_metadata-from_file.pdf");
            }
        }
        private static void ExportSchemas()
        {
            XmpDocument metadata = new XmpDocument();

            // Add items form the XMP basic schema:
            XmpProperties basicSchema = metadata.XmpProperties;
            basicSchema.CreatorTool = "PDF Document API";
            basicSchema.Label = "Sample";
            basicSchema.Identifier.Add("Id");
            basicSchema.Rating = "0";


            // Add items form the Dublin Core schema:
            DublinCoreProperties dublinCoreProperties = metadata.DublinCoreProperties;
            dublinCoreProperties.Creator.Add("DevExpress");
            dublinCoreProperties.Description.AddString("This document has emdedded XMP metadata", "en-us");
            dublinCoreProperties.Title.AddString("Invoice", "x-default");
            dublinCoreProperties.Type.Add("PDF");
            dublinCoreProperties.Publisher.Add("PDF Document API");

            // Add items form the Adobe PDF schema:
            AdobePdfProperties adobePdfProperties = metadata.PdfProperties;
            adobePdfProperties.Keywords = "Invoice, Northwind, PDF, XMP";
            adobePdfProperties.PDFVersion = "1.3";
            adobePdfProperties.Producer = "PDF Document API";
            adobePdfProperties.Trapped = false;

            //Add items form the Rights Management schema:
            XmpRightsManagementProperties rightsManagementSchema = metadata.XmpRightsManagementProperties;
            rightsManagementSchema.Certificate = "https://www.devexpress.com/";
            rightsManagementSchema.Owner.Add("DevExpress");
            rightsManagementSchema.Marked = true;
            rightsManagementSchema.WebStatement = "https://www.devexpress.com/support/eulas/";
            rightsManagementSchema.UsageTerms.AddString("Copyright(C) 2021 DevExpress.All Rights Reserved.", "x-default");

            //Export generated metadata to the file:
            metadata.Serialize(new FileStream("Documents//metadata_new.xml", FileMode.CreateNew, FileAccess.ReadWrite));
        }
    }
}
