using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UEPub;

public class EbookRenderer : MonoBehaviour {
	public Text displayText;
	public TextAsset testBook;

	//private EpubBook epubBook;

	// Use this for initialization
	void Start () {
		OpenEbookFile ();
	}
	
	void OpenEbookFile(){
		// Opening a book
		//epubBook = EpubReader.OpenBook("austen-pride-and-prejudice-illustrations.epub");
		//EpubChapter chapter = epubBook.Chapters [0];
		//displayText.text = chapter.HtmlContent;
		var epub = new UEPubReader ("Assets/Books/austen-pride-and-prejudice-illustrations.epub");
		Debug.Log (epub.epubFolderLocation);

		displayText.text = epub.chapters[10];
	}
}
