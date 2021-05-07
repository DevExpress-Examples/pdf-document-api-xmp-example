Imports DevExpress.Pdf
Imports DevExpress.Pdf.Xmp
Imports System.IO

Namespace XmpMetadataExamples
	Friend Class Program
		Shared Sub Main(ByVal args() As String)
			EmbedLoadedMetadata()
			EmbedEditedMetadata()
			EmbedGeneratedMetadata()
		End Sub

		Private Shared Sub EmbedEditedMetadata()
			Using pdfDocumentProcessor As New PdfDocumentProcessor()

				'Load a document:
				pdfDocumentProcessor.LoadDocument("Documents//Invoice.pdf")
				Dim document As PdfDocument = pdfDocumentProcessor.Document

				' Retrieve metadata:
				Dim editedMetadata As XmpDocument = XmpDocument.FromString(document.Metadata.Data)

				' Add items to the Creator array:
				Dim creators As XmpArray = editedMetadata.GetArray("dc:creator")
				If creators IsNot Nothing Then
					creators.Add("PDF Document API")
					creators.Add("Office File API")
				End If

				'Change the CreatorTool node value:
				Dim creatorTool As XmpSimpleNode = editedMetadata.GetSimpleValue("xmp:CreatorTool")
				creatorTool.SetValue("PDF Document API")

				'Add MaxPageSize structure:
				Dim structureName As XmpName = XmpName.Get("MaxPageSize", "http://ns.adobe.com/xap/1.0/t/pg/")
				Dim dimensions As XmpStructure = editedMetadata.CreateStructure(structureName)
				editedMetadata.RegisterNamespace("http://ns.adobe.com/xap/1.0/sType/Dimensions#", "stDim")
				dimensions.Add("stDim:h", 11)
				dimensions.Add("stDim:w", 8.5F)
				dimensions.Add("stDim:Unit", "inch")

				editedMetadata.Remove("dc:subject")

				'Embed modified metadata to the document:
				document.SetMetadata(editedMetadata)

				'Save the result:
				pdfDocumentProcessor.SaveDocument("Invoice_Upd.pdf")
			End Using
		End Sub

		Private Shared Sub EmbedGeneratedMetadata()
			Using pdfDocumentProcessor As New PdfDocumentProcessor()
				'Load a document:
				pdfDocumentProcessor.LoadDocument("Documents//Invoice_blank.pdf")
				Dim document As PdfDocument = pdfDocumentProcessor.Document

				Dim generatedMetadata As XmpDocument = GenerateXmpMetadata()

				'Embed modified metadata to the document:
				document.SetMetadata(generatedMetadata)

				'Save the result:
				pdfDocumentProcessor.SaveDocument("Invoice_new.pdf")
			End Using
		End Sub

		Private Shared Function GenerateXmpMetadata() As XmpDocument
			Dim document As New XmpDocument()

			' Register namespaces:
			document.RegisterNamespace("http://ns.adobe.com/xap/1.0/", "xmp")
			document.RegisterNamespace("http://ns.adobe.com/xap/1.0/sType/Dimensions#", "stDim")
			document.RegisterNamespace("http://ns.adobe.com/pdf/1.3/", "pdf")
			document.RegisterNamespace("http://ns.adobe.com/xap/1.0/t/pg/", "xmpTPg")

			'Add items with "xmp" prefix:
			Dim array As XmpArray = document.CreateArray("xmp:Identifier", XmpArrayType.Unordered)
			array.Add("identifier1")
			array.Add("identifier2")
			array.Add("identifier3")
			document.CreateSimpleValue("xmp:Label", "Demo")

			'Add items with the "pdf" prefix:
			document.Add("pdf:Keywords", "Invoice,Northwind,PDF,XMP")
			document.Add("pdf:Producer", "Developer Express Inc.DXperience(tm)")
			document.CreateSimpleValue("pdf:PDFVersion", "1.3")

			' Add items with the "xmpTPg" prefix:
			Dim dimensions As XmpStructure = document.CreateStructure("xmpTPg:MaxPageSize")
			dimensions.Add("stDim:h", 750)
			dimensions.Add("stDim:w", 500)
			dimensions.Add("stDim:Unit", "pixel")
			document.Add("xmpTPg:NPages", 6)

			Return document
		End Function

		Private Shared Sub EmbedLoadedMetadata()
			ExportSchemas()
			Using pdfDocumentProcessor As New PdfDocumentProcessor()
				' Set metadata loaded from a stream:
				pdfDocumentProcessor.LoadDocument("Documents//Invoice_signed.pdf")
				Dim metadata As XmpDocument
				Using xmlStream As New FileStream("Documents//metadata_new.xml", FileMode.Open, FileAccess.Read)
					metadata = XmpDocument.FromStream(xmlStream)
					pdfDocumentProcessor.Document.SetMetadata(metadata)
				End Using

				pdfDocumentProcessor.SaveDocument("Invoice_metadata-from_file.pdf")
			End Using
		End Sub
		Private Shared Sub ExportSchemas()
			Dim metadata As New XmpDocument()

			' Add items form the XMP basic schema:
			Dim basicSchema As XmpProperties = metadata.XmpProperties
			basicSchema.CreatorTool = "PDF Document API"
			basicSchema.Label = "Sample"
			basicSchema.Identifier.Add("Id")
			basicSchema.Rating = "0"


			' Add items form the Dublin Core schema:
			Dim dublinCoreProperties As DublinCoreProperties = metadata.DublinCoreProperties
			dublinCoreProperties.Creator.Add("DevExpress")
			dublinCoreProperties.Description.AddString("This document has emdedded XMP metadata", "en-us")
			dublinCoreProperties.Title.AddString("Invoice", "x-default")
			dublinCoreProperties.Type.Add("PDF")
			dublinCoreProperties.Publisher.Add("PDF Document API")

			' Add items form the Adobe PDF schema:
			Dim adobePdfProperties As AdobePdfProperties = metadata.PdfProperties
			adobePdfProperties.Keywords = "Invoice, Northwind, PDF, XMP"
			adobePdfProperties.PDFVersion = "1.3"
			adobePdfProperties.Producer = "PDF Document API"
			adobePdfProperties.Trapped = False

			'Add items form the Rights Management schema:
			Dim rightsManagementSchema As XmpRightsManagementProperties = metadata.XmpRightsManagementProperties
			rightsManagementSchema.Certificate = "https://www.devexpress.com/"
			rightsManagementSchema.Owner.Add("DevExpress")
			rightsManagementSchema.Marked = True
			rightsManagementSchema.WebStatement = "https://www.devexpress.com/support/eulas/"
			rightsManagementSchema.UsageTerms.AddString("Copyright(C) 2021 DevExpress.All Rights Reserved.", "x-default")

			'Export generated metadata to the file:
			metadata.Serialize(New FileStream("Documents//metadata_new.xml", FileMode.CreateNew, FileAccess.ReadWrite))
		End Sub
	End Class
End Namespace
