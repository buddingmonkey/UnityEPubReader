using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;

namespace UEPub{
	public class UEPubReader {

		public string epubFolderLocation { get; private set; }

		public Dictionary<string, string> chapters;

		private string opfFileString;

		public UEPubReader(string file){
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
	}
}
