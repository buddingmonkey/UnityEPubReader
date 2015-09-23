using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using System.Linq;

namespace UEPub{
	public class UEPubReader {

		public string epubFolderLocation { get; private set; }

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

			var nodes = doc.SelectNodes ("/ns:package/ns:manifest/ns:item", xmlnsManager);

			foreach (XmlNode node in nodes) {
				bookItems[node.Attributes["id"].InnerText] = node.Attributes["href"].InnerText;
			}

			nodes = doc.SelectNodes("/ns:package/ns:spine/ns:itemref", xmlnsManager);

			foreach (XmlNode node in nodes){
				spine.Add(node.Attributes["idref"].InnerText);
			}

			var toc = spine.Select (x => bookItems [x])
					.ToList ();

			chapters = new List<string> ();

			foreach (string s in toc) {
				StreamReader sr = new StreamReader(Path.GetDirectoryName(opfFile) + "/" + s);
				chapters.Add(sr.ReadToEnd());
				sr.Close();
			}
		}
	}
}
