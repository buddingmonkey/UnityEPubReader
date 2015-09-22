using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;

namespace UEPub{
	public class UEPubReader {

		public string epubFolderLocation { get; private set; }

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
		}

		private void ParseContainer(){
			var containerFile = epubFolderLocation + "/META-INF/container.xml";

			XmlDocument doc = new XmlDocument ();
			doc.Load (containerFile);

			XmlNode rootFile =
				doc.SelectSingleNode("/container/rootfiles/rootfile");
			opfFileString = rootFile.Attributes ["full-path"].InnerText;
		}

		private void ParseOPF(){
			var opfFile = epubFolderLocation + "/" + opfFileString;
			
			XmlDocument doc = new XmlDocument ();
			doc.Load (opfFile);

			var nodes = doc.SelectNodes ("/package/manifest");

			foreach (XmlNode node in nodes) {
				bookItems[node.Attributes["id"].InnerText] = node.Attributes["href"].InnerText;
			}

			nodes = doc.SelectNodes("/package/spine");

			foreach (XmlNode node in nodes){
				spine.Add(node.Attributes["idref"].InnerText);
			}
		}
	}
}
