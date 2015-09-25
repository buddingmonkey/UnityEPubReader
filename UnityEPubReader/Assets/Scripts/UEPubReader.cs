using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using System.Linq;
using System;
using UnityEngine;

namespace UEPub{
	public class UEPubReader {

		public string epubFolderLocation { get; private set; }
		public string htmlRoot { get; private set; }
		public UEPubMetadata metadata { get; private set; }

		public List<string> chapters;

		private Dictionary<string, string> bookItems;
		private List<string> spine;

		private string opfFileString;

		public UEPubReader(string file){
			bookItems = new Dictionary<string, string> ();
			spine = new List<string> ();

			var fileArray = file.Split('.');

			if (fileArray [fileArray.Length - 1].ToLower() != "epub") {
				Debug.LogErrorFormat ("The file {0} is not a .epub file", file);
				return;
			}

			fileArray = string.Join (".", fileArray, 0, fileArray.Length - 1).Split (System.IO.Path.DirectorySeparatorChar);
			var folderName = fileArray[fileArray.Length - 1];

			epubFolderLocation = Application.temporaryCachePath + "/" + folderName;

			ZipUtil.Unzip ( file, epubFolderLocation);

			ParseContainer ();
			ParseOPF ();
		}

		private void ParseContainer(){
			var containerFile = epubFolderLocation + "/META-INF/container.xml";

			XmlDocument doc = new XmlDocument ();
			doc.Load (containerFile);

			var xmlnsManager = new System.Xml.XmlNamespaceManager(doc.NameTable);
			xmlnsManager.AddNamespace("ns", "urn:oasis:names:tc:opendocument:xmlns:container");

			XmlNodeList rootFiles = doc.SelectNodes("/ns:container/ns:rootfiles/ns:rootfile", xmlnsManager);
			opfFileString = rootFiles[0].Attributes ["full-path"].InnerText;
		}

		private void ParseOPF(){
			var opfFile = epubFolderLocation + "/" + opfFileString;
			
			XmlDocument doc = new XmlDocument ();
			doc.Load (opfFile);

			var xmlnsManager = new System.Xml.XmlNamespaceManager(doc.NameTable);
			xmlnsManager.AddNamespace("ns", "http://www.idpf.org/2007/opf");
			xmlnsManager.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
			xmlnsManager.AddNamespace("dcterms", "http://purl.org/dc/terms/");

			ReadMetadata (doc, xmlnsManager);

			ReadChapters (opfFile, doc, xmlnsManager);
		}

		private void ReadChapters (string opfFile, XmlDocument doc, XmlNamespaceManager xmlnsManager)
		{
			var nodes = doc.SelectNodes ("/ns:package/ns:manifest/ns:item", xmlnsManager);
			foreach (XmlNode node in nodes) {
				bookItems [node.Attributes ["id"].InnerText] = node.Attributes ["href"].InnerText;
			}

			nodes = doc.SelectNodes ("/ns:package/ns:spine/ns:itemref", xmlnsManager);
			foreach (XmlNode node in nodes) {
				spine.Add (node.Attributes ["idref"].InnerText);
			}

			var toc = spine.Select (x => bookItems [x]).ToList ();
			chapters = new List<string> ();

			htmlRoot = Path.GetDirectoryName (opfFile) + "/";

			foreach (string s in toc) {
				StreamReader sr = new StreamReader(htmlRoot + s);
				chapters.Add (sr.ReadToEnd ());
				sr.Close ();
			}
		}

		private void ReadMetadata(XmlDocument doc, XmlNamespaceManager xmlnsManager){
			metadata = new UEPubMetadata ();

			// Required properties
			metadata.title = doc.SelectSingleNode ("/ns:package/ns:metadata/dc:title", xmlnsManager).InnerText;
			metadata.language = doc.SelectSingleNode ("/ns:package/ns:metadata/dc:language", xmlnsManager).InnerText;
			metadata.identifier = doc.SelectSingleNode ("/ns:package/ns:metadata/dc:identifier", xmlnsManager).InnerText;

			// Optional props
			SetMetadataProperty (ref metadata.creator, "/ns:package/ns:metadata/dc:creator", doc, xmlnsManager);
			SetMetadataProperty (ref metadata.contributor, "/ns:package/ns:metadata/dc:contributor", doc, xmlnsManager);
			SetMetadataProperty (ref metadata.coverage, "/ns:package/ns:metadata/dc:coverage", doc, xmlnsManager);
			SetMetadataProperty (ref metadata.description, "/ns:package/ns:metadata/dc:description", doc, xmlnsManager);
			SetMetadataProperty (ref metadata.format, "/ns:package/ns:metadata/dc:format", doc, xmlnsManager);
			SetMetadataProperty (ref metadata.publisher, "/ns:package/ns:metadata/dc:publisher", doc, xmlnsManager);
			SetMetadataProperty (ref metadata.relation, "/ns:package/ns:metadata/dc:relation", doc, xmlnsManager);
			SetMetadataProperty (ref metadata.rights, "/ns:package/ns:metadata/dc:rights", doc, xmlnsManager);
			SetMetadataProperty (ref metadata.source, "/ns:package/ns:metadata/dc:source", doc, xmlnsManager);
			SetMetadataProperty (ref metadata.subject, "/ns:package/ns:metadata/dc:subject", doc, xmlnsManager);
			SetMetadataProperty (ref metadata.type, "/ns:package/ns:metadata/dc:type", doc, xmlnsManager);

			// Date

			var node = doc.SelectSingleNode ("/ns:package/ns:metadata/dc:date", xmlnsManager);

			if (node != null) {
				metadata.date = DateTime.ParseExact(node.InnerText, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
			}
		}

		private void SetMetadataProperty(ref string property, string XPath, XmlDocument doc, XmlNamespaceManager xmlnsManager){
			var node = doc.SelectSingleNode (XPath, xmlnsManager);

			if (node != null) {
				property = node.InnerText;
			}
		}
	}
}
