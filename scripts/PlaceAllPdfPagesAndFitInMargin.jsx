//PlaceMultipagePDF.jsx
//An InDesign JavaScript
/*  
@@@BUILDINFO@@@ "PlaceMultipagePDF.jsx" 3.0.0 15 December 2009
*/
//Places all of the pages of a multi-page PDF.
//
//For more on InDesign/InCopy scripting see the documentation included in the Scripting SDK 
//available at http://www.adobe.com/devnet/indesign/sdk.html
//or visit the InDesign Scripting User to User forum at http://www.adobeforums.com
//
main();
function main() {
	//Make certain that user interaction (display of dialogs, etc.) is turned on.
	app.scriptPreferences.userInteractionLevel = UserInteractionLevels.interactWithAll;
	//Display a standard Open File dialog box.
	var myPDFFile = File.openDialog("Choose a PDF File");
	if ((myPDFFile != "") && (myPDFFile != null)) {
		var myDocument, myPage;
		if (app.documents.length != 0) {
			var myTemp = myChooseDocument();
			myDocument = myTemp[0];
			myNewDocument = myTemp[1];
		}
		else {
			myDocument = app.documents.add();
			myNewDocument = false;
		}

		if (myNewDocument == false) {
			myPage = myChoosePage(myDocument);
		}
		else {
			myPage = myDocument.pages.item(0);
		}
		myPlacePDF(myDocument, myPage, myPDFFile);
	}
}
function myChooseDocument() {
	var myDocumentNames = new Array;
	myDocumentNames.push("New Document");
	//Get the names of the documents
	for (var myDocumentCounter = 0; myDocumentCounter < app.documents.length; myDocumentCounter++) {
		myDocumentNames.push(app.documents.item(myDocumentCounter).name);
	}
	var myChooseDocumentDialog = app.dialogs.add({ name: "Choose a Document", canCancel: false });
	with (myChooseDocumentDialog.dialogColumns.add()) {
		with (dialogRows.add()) {
			with (dialogColumns.add()) {
				staticTexts.add({ staticLabel: "Place PDF in:" });
			}
			with (dialogColumns.add()) {
				var myChooseDocumentDropdown = dropdowns.add({ stringList: myDocumentNames, selectedIndex: 0 });
			}
		}
	}
	var myResult = myChooseDocumentDialog.show();
	if (myResult == true) {
		if (myChooseDocumentDropdown.selectedIndex == 0) {
			myDocument = app.documents.add();
			myNewDocument = true;
		}
		else {
			myDocument = app.documents.item(myChooseDocumentDropdown.selectedIndex - 1);
			myNewDocument = false;
		}
		myChooseDocumentDialog.destroy();
	}
	else {
		myDocument = "";
		myNewDocument = "";
		myChooseDocumentDialog.destroy();
	}
	return [myDocument, myNewDocument];
}
function myChoosePage(myDocument) {
	var myPageNames = new Array;
	//Get the names of the pages in the document
	for (var myCounter = 0; myCounter < myDocument.pages.length; myCounter++) {
		myPageNames.push(myDocument.pages.item(myCounter).name);
	}
	var myChoosePageDialog = app.dialogs.add({ name: "Choose a Page", canCancel: false });
	with (myChoosePageDialog.dialogColumns.add()) {
		with (dialogRows.add()) {
			with (dialogColumns.add()) {
				staticTexts.add({ staticLabel: "Place PDF on:" });
			}
			with (dialogColumns.add()) {
				var myChoosePageDropdown = dropdowns.add({ stringList: myPageNames, selectedIndex: 0 });
			}
		}
	}
	myChoosePageDialog.show();
	var myPage = myDocument.pages.item(myChoosePageDropdown.selectedIndex);
	myChoosePageDialog.destroy();
	return myPage;
}
function myPlacePDF(myDocument, myPage, myPDFFile) {
	var myPDFPage;
	app.pdfPlacePreferences.pdfCrop = PDFCrop.cropMedia;
	var myCounter = 1;
	var myBreak = false;
	while (myBreak == false) {
		if (myCounter > 1) {
			myPage = myDocument.pages.add(LocationOptions.after, myPage);
		}
		app.pdfPlacePreferences.pageNumber = myCounter;
		myPDFPage = myPage.place(File(myPDFFile), [0, 0])[0];

		var page = myPage;
		var leftMargin = page.marginPreferences.left;
		var rightMargin = page.marginPreferences.right;
		var topMargin = page.marginPreferences.top;
		var bottomMargin = page.marginPreferences.bottom;
		var pageWidth = page.bounds[3] - page.bounds[1];
		var pageHeight = page.bounds[2] - page.bounds[0];
		var viewWidth = pageWidth - leftMargin - rightMargin; // Available width within margins
		var viewHeight = pageHeight - topMargin - bottomMargin; // Available height within margins

		var o = myPDFPage;

		// Move the PDF down by the top margin
		o.move(undefined, [0, topMargin]);

		// Fit frame to content before resizing
		o.fit(FitOptions.FRAME_TO_CONTENT);

		var currY = o.geometricBounds[0]; // New top
		var currX = o.geometricBounds[1]; // Left
		var currY2 = o.geometricBounds[2]; // New bottom
		var currX2 = o.geometricBounds[3]; // Right

		var currWidth = currX2 - currX;
		var currHeight = currY2 - currY;

		// Calculate scale factor to fit width within margins
		var scaleFactor = viewWidth / currWidth;
		var scaledHeight = currHeight * scaleFactor;

		// If scaled height exceeds available height, adjust scale factor
		if (scaledHeight > viewHeight) {
			scaleFactor = viewHeight / currHeight;
		}

		// Apply scaling
		o.resize(CoordinateSpaces.INNER_COORDINATES, AnchorPoint.TOP_LEFT_ANCHOR, ResizeMethods.MULTIPLYING_CURRENT_DIMENSIONS_BY, [scaleFactor, scaleFactor]);

		// Calculate the target position for movement
		currY2 = o.geometricBounds[2]; // New bottom
		var newBottom = pageHeight - bottomMargin;

		// Move the PDF to align its bottom with the bottom margin
		o.move(undefined, [leftMargin, newBottom - currY2]);

		o.fit(FitOptions.FRAME_TO_CONTENT);
		//alert(scale + "\n" + newHeight + "\n" + currHeight)
		if (myCounter == 1) {
			var myFirstPage = myPDFPage.pdfAttributes.pageNumber;
		}
		else {
			if (myPDFPage.pdfAttributes.pageNumber == myFirstPage) {
				myPage.remove();
				myBreak = true;
			}
		}
		myCounter = myCounter + 1;
	}
}
